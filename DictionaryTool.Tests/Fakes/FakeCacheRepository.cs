using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Cache;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeCacheRepository : ICacheRepository
    {
        private readonly Dictionary<string, WordLookupResult> _store = new Dictionary<string, WordLookupResult>();
        private readonly List<string> _viWords = new List<string>();
        private readonly List<string> _zhWords = new List<string>();

        public int SaveCallCount { get; private set; }

        public void Seed(string normalizedWord, WordLookupResult result)
        {
            _store[normalizedWord] = result;
        }

        public void SeedViWord(string word) => _viWords.Add(word);
        public void SeedZhWord(string word) => _zhWords.Add(word);

        public Task<WordLookupResult> TryGetAsync(string normalizedWord)
        {
            WordLookupResult result;
            if (_store.TryGetValue(normalizedWord, out result))
            {
                result.FromCache = true;
                return Task.FromResult(result);
            }

            return Task.FromResult<WordLookupResult>(null);
        }

        public Task SaveAsync(string normalizedWord, WordLookupResult result)
        {
            SaveCallCount++;
            _store[normalizedWord] = result;
            return Task.CompletedTask;
        }

        public Task<List<string>> GetAllCachedWordsAsync()
        {
            return Task.FromResult(new List<string>(_store.Keys));
        }

        public Task<List<string>> GetAllCachedViWordsAsync()
        {
            return Task.FromResult(new List<string>(_viWords));
        }

        public Task<List<string>> GetAllCachedZhWordsAsync()
        {
            return Task.FromResult(new List<string>(_zhWords));
        }
    }
}
