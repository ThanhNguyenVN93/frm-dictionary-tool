using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeStardictRepository : IStardictRepository
    {
        private readonly Dictionary<string, StardictEntry> _enVi = new Dictionary<string, StardictEntry>();
        private readonly Dictionary<string, StardictEntry> _viEn = new Dictionary<string, StardictEntry>();
        private readonly Dictionary<string, StardictEntry> _viEnNoDiacritics = new Dictionary<string, StardictEntry>();

        public void SeedEnVi(string word, StardictEntry entry)
        {
            _enVi[word] = entry;
        }

        public void SeedViEn(string word, StardictEntry entry)
        {
            _viEn[word] = entry;
        }

        public void SeedViEnNoDiacritics(string wordNoDiacritics, StardictEntry entry)
        {
            _viEnNoDiacritics[wordNoDiacritics] = entry;
        }

        public Task EnsureImportedAsync(IProgress<string> progress = null) => Task.CompletedTask;

        public Task<StardictEntry> LookupEnViAsync(string englishWord)
        {
            if (_enVi.TryGetValue(englishWord, out var entry))
            {
                return Task.FromResult(entry);
            }
            return Task.FromResult(new StardictEntry { Word = englishWord, NotFound = true });
        }

        public Task<StardictEntry> LookupViEnAsync(string vietnameseWord)
        {
            if (_viEn.TryGetValue(vietnameseWord, out var entry))
            {
                return Task.FromResult(entry);
            }
            return Task.FromResult(new StardictEntry { Word = vietnameseWord, NotFound = true });
        }

        public Task<StardictEntry> LookupViEnByNoDiacriticsAsync(string wordNoDiacritics)
        {
            if (_viEnNoDiacritics.TryGetValue(wordNoDiacritics, out var entry))
            {
                return Task.FromResult(entry);
            }
            return Task.FromResult(new StardictEntry { Word = wordNoDiacritics, NotFound = true });
        }
    }
}
