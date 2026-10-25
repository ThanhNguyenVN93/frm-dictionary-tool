using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Clients;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;


namespace DictionaryTool.Core
{
    /// <summary>
    /// Điều phối tra từ 3 ngôn ngữ (spec: "Logic tra từ — 3 mode"). Cả 2 chain
    /// ZH->EN->VI và VI->EN->ZH đều bắc cầu qua tiếng Anh, dùng IDictionaryService
    /// (Module 2 cũ) làm nguồn EnglishResult chung cho cả 3 mode.
    /// </summary>
    public class TrilingualLookupService : ITrilingualLookupService
    {
        private static readonly Regex BridgeWordRegex = new Regex(@"[A-Za-z]+(?:[-'][A-Za-z]+)*", RegexOptions.Compiled);

        private readonly IDictionaryService _dictionaryService;
        private readonly ICcCedictRepository _cedictRepository;
        private readonly IStardictRepository _stardictRepository;
        private readonly ITatoebaRepository _tatoebaRepository;
        private readonly WiktionaryViEnRepository _wiktionaryRepository;
        private readonly GoogleTranslateClient _googleTranslateClient;

        public TrilingualLookupService(
            IDictionaryService dictionaryService,
            ICcCedictRepository cedictRepository,
            IStardictRepository stardictRepository,
            ITatoebaRepository tatoebaRepository,
            GoogleTranslateClient googleTranslateClient = null,
            WiktionaryViEnRepository wiktionaryRepository = null)
        {
            _dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
            _cedictRepository = cedictRepository ?? throw new ArgumentNullException(nameof(cedictRepository));
            _stardictRepository = stardictRepository ?? throw new ArgumentNullException(nameof(stardictRepository));
            _tatoebaRepository = tatoebaRepository ?? throw new ArgumentNullException(nameof(tatoebaRepository));
            _googleTranslateClient = googleTranslateClient;
            _wiktionaryRepository = wiktionaryRepository;
        }

        public async Task<TrilingualResult> LookupAsync(string query, Language? languageHint = null, CancellationToken ct = default(CancellationToken))
        {
            var trimmed = (query ?? string.Empty).Trim();

            Language detected;
            if (languageHint.HasValue)
            {
                detected = languageHint.Value;
            }
            else
            {
                // Nhánh "quick" (regex Unicode) không nhận ra tiếng Việt gõ không dấu
                // (vd "khong") — DetectAsync thử thêm 1 bước tra ngược index VI-EN qua
                // ResolveVietnameseEntryAsync (cùng logic dùng để tra thật ở dưới, nên
                // bỏ loại từ/bỏ dấu đều được tính luôn, không cần lặp lại ở đây).
                detected = await LanguageDetector.DetectAsync(
                    trimmed,
                    async candidate => !(await ResolveVietnameseEntryAsync(candidate).ConfigureAwait(false)).NotFound
                ).ConfigureAwait(false);
            }

            var result = new TrilingualResult { Query = trimmed, DetectedLanguage = detected };

            ct.ThrowIfCancellationRequested();

            switch (detected)
            {
                case Language.Chinese:
                    await LookupFromChineseAsync(trimmed, result, ct).ConfigureAwait(false);
                    break;

                case Language.Vietnamese:
                    await LookupFromVietnameseAsync(trimmed, result, ct).ConfigureAwait(false);
                    break;

                default:
                    // English hoặc Unknown: coi như tra tiếng Anh (spec "EN: mặc định nếu không phải ZH/VI").
                    result.DetectedLanguage = Language.English;
                    await LookupFromEnglishAsync(trimmed, result, ct).ConfigureAwait(false);

                    // Fallback: nếu MW hoàn toàn không tìm thấy (vd "anh" gõ không
                    // flag VI), kiểm tra xem có phải từ tiếng Việt trong StarDict không.
                    // "run" không trigger vì MW trả về kết quả → NotFound = false.
                    if (result.EnglishResult.NotFound)
                    {
                        var viProbe = await _stardictRepository.LookupViEnAsync(trimmed).ConfigureAwait(false);
                        if (!viProbe.NotFound)
                        {
                            result = new TrilingualResult { Query = trimmed, DetectedLanguage = Language.Vietnamese };
                            await LookupFromVietnameseAsync(trimmed, result, ct).ConfigureAwait(false);
                        }
                    }
                    break;
            }

            return result;
        }

        private async Task LookupFromEnglishAsync(string word, TrilingualResult result, CancellationToken ct)
        {
            result.EnglishResult = await _dictionaryService.LookupAsync(word, ct).ConfigureAwait(false);
            TagEnglishSource(result.EnglishResult);

            var viEntry = await _stardictRepository.LookupEnViAsync(word).ConfigureAwait(false);
            if (!viEntry.NotFound) viEntry.Source = SourceKind.OfflineStarDict;
            result.VietnameseResult = ToVietnameseResult(viEntry);

            result.ChineseResult = await _cedictRepository.LookupByEnglishAsync(word).ConfigureAwait(false);
            await TagAndBreakdownChineseAsync(result.ChineseResult).ConfigureAwait(false);

            result.ExampleSentences = await _tatoebaRepository.GetExamplesForEnglishAsync(word).ConfigureAwait(false);
        }

        private async Task LookupFromVietnameseAsync(string word, TrilingualResult result, CancellationToken ct)
        {
            var viEntry = await ResolveVietnameseEntryAsync(word).ConfigureAwait(false);
            result.VietnameseResult = ToVietnameseResult(viEntry);

            var bridgeWord = viEntry.NotFound ? null : ExtractBridgeWord(viEntry.Definitions);
            if (bridgeWord == null)
                return;

            result.EnglishResult = await _dictionaryService.LookupAsync(bridgeWord, ct).ConfigureAwait(false);

            // Nếu từ cầu nối offline không tìm được trong MW (vd lỗi đánh máy trong
            // StarDict như "shoolmaster"), thử lại bằng Google Translate cho từ VI gốc.
            if (result.EnglishResult.NotFound && _googleTranslateClient != null)
            {
                var translations = await _googleTranslateClient.TranslateViToEnAsync(word).ConfigureAwait(false);
                if (translations.Count > 0)
                {
                    bridgeWord = translations[0];
                    result.EnglishResult = await _dictionaryService.LookupAsync(bridgeWord, ct).ConfigureAwait(false);
                }
            }
            TagEnglishSource(result.EnglishResult);

            result.ChineseResult = await _cedictRepository.LookupByEnglishAsync(bridgeWord).ConfigureAwait(false);
            await TagAndBreakdownChineseAsync(result.ChineseResult).ConfigureAwait(false);
            result.ExampleSentences = await _tatoebaRepository.GetExamplesForEnglishAsync(bridgeWord).ConfigureAwait(false);
        }

        /// <summary>
        /// Tra 1 từ tiếng Việt với 4 bước thử theo thứ tự, dừng ở bước đầu tiên tìm
        /// được: (1) nguyên văn -> (2) bỏ loại từ đứng đầu (vd "con mèo" -> "mèo")
        /// -> (3) bỏ dấu (vd "khong" -> so khớp headword "không" đã bỏ dấu sẵn
        /// trong index) -> (4) vừa bỏ loại từ vừa bỏ dấu. Headword trả về là từ
        /// thật trong từ điển (có thể khác hẳn input nếu qua bước 2-4), không phải
        /// input gốc.
        /// </summary>
        private async Task<StardictEntry> ResolveVietnameseEntryAsync(string rawWord)
        {
            var trimmed = (rawWord ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return new StardictEntry { Word = trimmed, NotFound = true };
            }

            var entry = await _stardictRepository.LookupViEnAsync(trimmed).ConfigureAwait(false);
            if (!entry.NotFound)
            {
                entry.Source = SourceKind.OfflineStarDict;
                return entry;
            }

            var withoutClassifier = VietnameseNormalizer.StripLeadingClassifier(trimmed);
            if (withoutClassifier != null)
            {
                entry = await _stardictRepository.LookupViEnAsync(withoutClassifier).ConfigureAwait(false);
                if (!entry.NotFound)
                {
                    entry.Source = SourceKind.OfflineStarDict;
                    return entry;
                }
            }

            // Bước 3-4: tra theo index đã bỏ dấu (WordNoDiacritics) — CHỈ khi input là
            // ASCII thuần (không có dấu tiếng Việt). Nếu input đã có dấu thì bỏ dấu
            // rồi tra lại có thể nhầm sang từ khác thanh điệu: "bàn học" → "ban hoc"
            // → khớp "bạn học" (classmate) thay vì "bàn học" (desk). Use case đúng
            // của bước này là gõ không dấu: "khong" → "không", "ban" → "bàn/bạn/bán".
            var inputIsAscii = string.Equals(
                VietnameseNormalizer.StripDiacritics(trimmed), trimmed,
                StringComparison.OrdinalIgnoreCase);

            if (inputIsAscii)
            {
                var noDiacritics = trimmed.ToLowerInvariant();
                entry = await _stardictRepository.LookupViEnByNoDiacriticsAsync(noDiacritics).ConfigureAwait(false);
                if (!entry.NotFound)
                {
                    entry.Source = SourceKind.OfflineStarDict;
                    return entry;
                }

                if (withoutClassifier != null)
                {
                    var withoutClassifierNoDiacritics = VietnameseNormalizer.StripDiacritics(withoutClassifier).ToLowerInvariant();
                    entry = await _stardictRepository.LookupViEnByNoDiacriticsAsync(withoutClassifierNoDiacritics).ConfigureAwait(false);
                    if (!entry.NotFound)
                    {
                        entry.Source = SourceKind.OfflineStarDict;
                        return entry;
                    }
                }
            }

            // Bước 5: Wiktionary API (online, có cache 30 ngày). Tra nguyên cụm từ —
            // KHÔNG tách từ cuối vì tiếng Việt đặt head word ở đầu (bàn học, bao lì xì)
            // nên last-word heuristic cho kết quả sai (học → study, xì → hiss).
            // Cụm từ nhiều âm tiết không có trong offline sẽ do Google Translate xử lý.
            if (_wiktionaryRepository != null)
            {
                var wiktEntry = await _wiktionaryRepository.LookupAsync(trimmed).ConfigureAwait(false);
                if (!wiktEntry.NotFound)
                {
                    wiktEntry.Source = SourceKind.OnlineWiktionary;
                    return wiktEntry;
                }
            }

            // Bước 6 (GT): fallback cuối, không cần API key, xử lý tốt cụm từ nhiều âm tiết.
            if (_googleTranslateClient != null)
            {
                var translations = await _googleTranslateClient.TranslateViToEnAsync(trimmed).ConfigureAwait(false);
                if (translations.Count > 0)
                    return new StardictEntry { Word = trimmed, Definitions = translations, NotFound = false, Source = SourceKind.OnlineGoogleTranslate };
            }

            return new StardictEntry { Word = trimmed, NotFound = true };
        }

        private async Task LookupFromChineseAsync(string word, TrilingualResult result, CancellationToken ct)
        {
            var zhResult = await _cedictRepository.LookupByChineseAsync(word).ConfigureAwait(false);
            result.ChineseResult = zhResult;
            await TagAndBreakdownChineseAsync(result.ChineseResult).ConfigureAwait(false);

            var bridgeWord = zhResult.NotFound ? null : ExtractBridgeWord(zhResult.Definitions.OrderBy(d => d.Length));
            if (bridgeWord == null)
            {
                return;
            }

            result.EnglishResult = await _dictionaryService.LookupAsync(bridgeWord, ct).ConfigureAwait(false);
            TagEnglishSource(result.EnglishResult);

            var viEntry = await _stardictRepository.LookupEnViAsync(bridgeWord).ConfigureAwait(false);
            if (!viEntry.NotFound) viEntry.Source = SourceKind.OfflineStarDict;
            result.VietnameseResult = ToVietnameseResult(viEntry);

            result.ExampleSentences = await _tatoebaRepository.GetExamplesForEnglishAsync(bridgeWord).ConfigureAwait(false);
        }

        private static VietnameseResult ToVietnameseResult(StardictEntry entry)
        {
            return new VietnameseResult
            {
                Word = entry.Word,
                Definitions = entry.Definitions,
                NotFound = entry.NotFound,
                Source = entry.Source
            };
        }

        /// <summary>Gán nguồn cho EnglishResult (cache vs online) để UI hiện badge.</summary>
        private static void TagEnglishSource(WordLookupResult en)
        {
            if (en == null || en.NotFound || en.HasError) return;
            en.Source = en.FromCache ? SourceKind.Cache : SourceKind.OnlineDictApi;
        }

        /// <summary>
        /// Tách từ Hán ghép thành từng ký tự rồi tra pinyin + nghĩa của mỗi ký tự
        /// trong CC-CEDICT (#7). Chỉ chạy khi từ có từ 2 ký tự Hán trở lên.
        /// </summary>
        /// <summary>Gán nguồn CC-CEDICT (offline) cho ZH và tách ký tự nếu là từ ghép.</summary>
        private async Task TagAndBreakdownChineseAsync(ChineseResult zh)
        {
            if (zh == null || zh.NotFound) return;
            zh.Source = SourceKind.OfflineCcCedict;
            await PopulateChineseCharactersAsync(zh).ConfigureAwait(false);
        }

        private async Task PopulateChineseCharactersAsync(ChineseResult zh)
        {
            if (zh == null || zh.NotFound || string.IsNullOrEmpty(zh.Simplified)) return;

            var chars = zh.Simplified;
            if (CountHanCharacters(chars) < 2) return;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var ch in EnumerateCharacters(chars))
            {
                if (!IsHanCharacter(ch) || !seen.Add(ch)) continue;

                var charResult = await _cedictRepository.LookupByChineseAsync(ch).ConfigureAwait(false);
                if (charResult == null || charResult.NotFound) continue;

                zh.Characters.Add(new ChineseCharacterInfo
                {
                    Character = ch,
                    Pinyin = charResult.Pinyin,
                    Definitions = charResult.Definitions ?? new List<string>()
                });
            }
        }

        private static int CountHanCharacters(string text)
        {
            var count = 0;
            foreach (var ch in EnumerateCharacters(text))
                if (IsHanCharacter(ch)) count++;
            return count;
        }

        // Dùng StringInfo để không cắt nhầm ký tự Hán nằm ngoài BMP (surrogate pair).
        private static IEnumerable<string> EnumerateCharacters(string text)
        {
            var e = System.Globalization.StringInfo.GetTextElementEnumerator(text ?? string.Empty);
            while (e.MoveNext())
                yield return (string)e.Current;
        }

        private static bool IsHanCharacter(string element)
        {
            if (string.IsNullOrEmpty(element)) return false;
            var cp = char.ConvertToUtf32(element, 0);
            // CJK Unified Ideographs + Ext A + phần Ext B hay gặp.
            return (cp >= 0x4E00 && cp <= 0x9FFF)
                || (cp >= 0x3400 && cp <= 0x4DBF)
                || (cp >= 0x20000 && cp <= 0x2A6DF);
        }

        /// <summary>
        /// Những từ chức năng hay đứng đầu câu định nghĩa kiểu "to greet", "a cat",
        /// "the ..." — nếu lấy nguyên token đầu tiên sẽ ra "to"/"a"/"the" vô nghĩa
        /// thay vì từ mang nghĩa thật. Bỏ qua các từ này khi tìm từ cầu nối.
        /// </summary>
        private static readonly HashSet<string> BridgeWordStopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Articles, prepositions, conjunctions, pronouns
            "a", "an", "the", "to", "of", "in", "on", "at", "is", "be", "this", "that",
            "and", "or", "as", "it", "its", "for", "with", "by", "from", "into", "onto",
            "also", "than", "then", "them", "they", "when", "will", "some", "such", "more",
            "very", "well", "just", "much", "over", "same", "only", "each", "both",
            // Động từ chức năng xuất hiện trong định nghĩa nhưng không mang nghĩa riêng
            // (vd "to have a bath" → "have"; "to say goodbye" → "say")
            "say", "have", "make", "take", "give", "get", "put", "set", "let", "see",
            "try", "ask", "pay", "cut", "eat", "sit", "win", "buy", "run", "add",
            "call", "come", "back", "turn", "show", "keep", "hold", "fall", "feel",
            // Quá chung chung hoặc sai nghĩa khi dùng làm bridge word
            "man", "woman", "boy", "girl", "one", "type", "kind", "form", "way", "used",
            "dame", "miss", "mistress", "lord", "lady", "sir", "madam", "mister"
        };

        /// <summary>
        /// Lấy 1 từ tiếng Anh đại diện từ danh sách định nghĩa để làm cầu nối sang
        /// nguồn còn lại (vd định nghĩa CC-CEDICT "/hello/hi/" -> "hello", hoặc
        /// Stardict VI-EN "To greet, to salute" -> "greet", bỏ qua "to"). Đây là
        /// suy đoán đơn giản dựa trên token chữ cái có nghĩa đầu tiên tìm thấy,
        /// không phải dịch máy — đủ dùng cho phase 1, có thể cải thiện sau.
        /// </summary>
        private static string ExtractBridgeWord(IEnumerable<string> definitions)
        {
            // Score = defIndex * 100 + wordLen: ưu tiên definition đầu tiên,
            // dùng độ dài làm tiebreaker bên trong cùng 1 definition.
            // Vd "cô giáo" → def0:"teacher"(7) score=7, def2:"dame"(4) score=204
            //    → teacher thắng dù dài hơn vì ở definition đầu.
            // Vd "học sinh" → def0:"Schoolboy,schoolgirl,pupil" → pupil(5) score=5 thắng.
            string best = null;
            int bestScore = int.MaxValue;
            string fallback = null; // từ dài hơn 12 nếu không có gì khác

            var defIndex = 0;
            foreach (var definition in definitions)
            {
                foreach (Match match in BridgeWordRegex.Matches(definition))
                {
                    var word = match.Value;
                    if (word.Length < 3 || BridgeWordStopWords.Contains(word))
                        continue;

                    if (word.Length <= 12)
                    {
                        var score = defIndex * 100 + word.Length;
                        if (score < bestScore)
                        {
                            bestScore = score;
                            best = word.ToLowerInvariant();
                        }
                    }
                    else if (fallback == null)
                    {
                        fallback = word.ToLowerInvariant();
                    }
                }
                defIndex++;
            }
            return best ?? fallback;
        }
    }
}
