using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Clients;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeMerriamWebsterClient : IMerriamWebsterClient
    {
        private readonly List<MwMeaning> _collegiate;
        private readonly List<MwMeaning> _learner;
        private readonly Exception _exceptionToThrow;

        public int CallCount { get; private set; }
        public int CollegiateCallCount { get; private set; }
        public int LearnerCallCount { get; private set; }

        private FakeMerriamWebsterClient(List<MwMeaning> collegiate, List<MwMeaning> learner, Exception exceptionToThrow)
        {
            _collegiate = collegiate;
            _learner = learner;
            _exceptionToThrow = exceptionToThrow;
        }

        public static FakeMerriamWebsterClient ReturningEmpty()
        {
            return new FakeMerriamWebsterClient(new List<MwMeaning>(), new List<MwMeaning>(), null);
        }

        public static FakeMerriamWebsterClient Returning(List<MwMeaning> collegiate, List<MwMeaning> learner)
        {
            return new FakeMerriamWebsterClient(collegiate, learner, null);
        }

        public static FakeMerriamWebsterClient ThrowingException()
        {
            return new FakeMerriamWebsterClient(null, null, new InvalidOperationException("Simulated network failure"));
        }

        public Task<MwCollegiateResult> GetCollegiateAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            CallCount++;
            CollegiateCallCount++;
            if (_exceptionToThrow != null) throw _exceptionToThrow;
            return Task.FromResult(new MwCollegiateResult { Meanings = _collegiate ?? new List<MwMeaning>() });
        }

        public Task<List<MwMeaning>> GetLearnerAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            CallCount++;
            LearnerCallCount++;
            if (_exceptionToThrow != null) throw _exceptionToThrow;
            return Task.FromResult(_learner);
        }
    }
}
