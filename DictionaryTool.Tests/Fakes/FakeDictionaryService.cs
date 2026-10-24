using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeDictionaryService : IDictionaryService
    {
        private readonly Dictionary<string, WordLookupResult> _results = new Dictionary<string, WordLookupResult>();

        public int CallCount { get; private set; }

        public void Seed(string word, WordLookupResult result)
        {
            _results[word] = result;
        }

        public Task<WordLookupResult> LookupAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            CallCount++;

            if (_results.TryGetValue(word, out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult(new WordLookupResult { Word = word, NotFound = true });
        }
    }
}
