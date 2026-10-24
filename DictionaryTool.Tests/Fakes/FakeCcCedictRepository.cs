using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeCcCedictRepository : ICcCedictRepository
    {
        private readonly Dictionary<string, ChineseResult> _byChinese = new Dictionary<string, ChineseResult>();
        private readonly Dictionary<string, ChineseResult> _byEnglish = new Dictionary<string, ChineseResult>();

        public void SeedByChinese(string word, ChineseResult result)
        {
            _byChinese[word] = result;
        }

        public void SeedByEnglish(string word, ChineseResult result)
        {
            _byEnglish[word] = result;
        }

        public Task EnsureImportedAsync(IProgress<string> progress = null) => Task.CompletedTask;

        public Task<ChineseResult> LookupByChineseAsync(string word)
        {
            if (_byChinese.TryGetValue(word, out var result))
            {
                return Task.FromResult(result);
            }
            return Task.FromResult(new ChineseResult { NotFound = true });
        }

        public Task<ChineseResult> LookupByEnglishAsync(string englishWord)
        {
            if (_byEnglish.TryGetValue(englishWord, out var result))
            {
                return Task.FromResult(result);
            }
            return Task.FromResult(new ChineseResult { NotFound = true });
        }
    }
}
