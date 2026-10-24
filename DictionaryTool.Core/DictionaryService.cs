using System;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Cache;
using DictionaryTool.Core.Clients;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IDictionaryApiDevClient _dictionaryApiDevClient;
        private readonly IMerriamWebsterClient _merriamWebsterClient;
        private readonly ICacheRepository _cacheRepository;

        public DictionaryService(
            IDictionaryApiDevClient dictionaryApiDevClient,
            IMerriamWebsterClient merriamWebsterClient,
            ICacheRepository cacheRepository)
        {
            _dictionaryApiDevClient = dictionaryApiDevClient ?? throw new ArgumentNullException(nameof(dictionaryApiDevClient));
            _merriamWebsterClient = merriamWebsterClient ?? throw new ArgumentNullException(nameof(merriamWebsterClient));
            _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
        }

        public async Task<WordLookupResult> LookupAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            var normalized = (word ?? string.Empty).Trim().ToLowerInvariant();

            var cached = await _cacheRepository.TryGetAsync(normalized).ConfigureAwait(false);
            if (cached != null)
            {
                return cached;
            }

            ct.ThrowIfCancellationRequested();

            var dictionaryApiDevTask = SafeGetDictionaryApiDevAsync(normalized, ct);
            var merriamWebsterTask = SafeGetMerriamWebsterAsync(normalized, ct);

            await Task.WhenAll(dictionaryApiDevTask, merriamWebsterTask).ConfigureAwait(false);

            var dictionaryApiDevOutcome = dictionaryApiDevTask.Result;
            var merriamWebsterOutcome = merriamWebsterTask.Result;

            ct.ThrowIfCancellationRequested();

            bool dictionaryApiDevFailed = dictionaryApiDevOutcome.Failed;
            bool merriamWebsterFailed = merriamWebsterOutcome.Failed;

            if (dictionaryApiDevFailed && merriamWebsterFailed)
            {
                return new WordLookupResult
                {
                    Word = normalized,
                    FromCache = false,
                    HasError = true,
                    ErrorMessage = "Không thể kết nối tới các nguồn từ điển. Vui lòng kiểm tra kết nối mạng và thử lại."
                };
            }

            var result = new WordLookupResult
            {
                Word = normalized,
                FromCache = false
            };

            if (dictionaryApiDevOutcome.Data != null)
            {
                result.Phonetics.AddRange(dictionaryApiDevOutcome.Data.Phonetics);
                result.Meanings.AddRange(dictionaryApiDevOutcome.Data.Meanings);
            }

            if (merriamWebsterOutcome.Data != null)
            {
                result.LearnerMeanings.AddRange(merriamWebsterOutcome.Data.Meanings);

                // Thêm phonetics từ MW khi dictionaryapi.dev không có audio
                bool dictApiHasAudio = result.Phonetics.Exists(p => !string.IsNullOrWhiteSpace(p.AudioUrl));
                if (!dictApiHasAudio && merriamWebsterOutcome.Data.Phonetics.Count > 0)
                {
                    result.Phonetics.AddRange(merriamWebsterOutcome.Data.Phonetics);
                }
            }

            bool hasAnyData = result.Phonetics.Count > 0 || result.Meanings.Count > 0 || result.LearnerMeanings.Count > 0;
            if (!hasAnyData)
            {
                // Cả hai nguồn đều trả lời thành công nhưng không có dữ liệu -> từ không tồn tại,
                // khác với lỗi hệ thống. Không cache kết quả rỗng để lần sau vẫn thử lại được
                // (vd. nếu người dùng gõ sai rồi sửa lại, hoặc nguồn thêm dữ liệu sau này).
                result.NotFound = true;
                return result;
            }

            await _cacheRepository.SaveAsync(normalized, result).ConfigureAwait(false);
            return result;
        }

        private async Task<Outcome<DictionaryApiDevResult>> SafeGetDictionaryApiDevAsync(string word, CancellationToken ct)
        {
            try
            {
                var data = await _dictionaryApiDevClient.GetAsync(word, ct).ConfigureAwait(false);
                return Outcome<DictionaryApiDevResult>.Success(data);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return Outcome<DictionaryApiDevResult>.Failure();
            }
        }

        private async Task<Outcome<MwCollegiateResult>> SafeGetMerriamWebsterAsync(string word, CancellationToken ct)
        {
            try
            {
                // Gọi Collegiate trước — nếu có kết quả thì không cần gọi Learner's
                // (tiết kiệm ~50% quota MW khi từ phổ biến đều có trong Collegiate).
                var collegiate = await _merriamWebsterClient.GetCollegiateAsync(word, ct).ConfigureAwait(false);
                if (collegiate.Meanings.Count > 0 || collegiate.Phonetics.Count > 0)
                    return Outcome<MwCollegiateResult>.Success(collegiate);

                // Collegiate không có dữ liệu → thử Learner's làm fallback.
                var learner = await _merriamWebsterClient.GetLearnerAsync(word, ct).ConfigureAwait(false);
                return Outcome<MwCollegiateResult>.Success(new MwCollegiateResult { Meanings = learner });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return Outcome<MwCollegiateResult>.Failure();
            }
        }

        private struct Outcome<T>
        {
            public T Data;
            public bool Failed;

            public static Outcome<T> Success(T data)
            {
                return new Outcome<T> { Data = data, Failed = false };
            }

            public static Outcome<T> Failure()
            {
                return new Outcome<T> { Data = default(T), Failed = true };
            }
        }
    }
}
