using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeTatoebaRepository : ITatoebaRepository
    {
        private readonly Dictionary<string, List<ExampleSentence>> _examples = new Dictionary<string, List<ExampleSentence>>();

        public void Seed(string englishWord, List<ExampleSentence> examples)
        {
            _examples[englishWord] = examples;
        }

        public Task EnsureImportedAsync(IProgress<string> progress = null) => Task.CompletedTask;

        public Task<List<ExampleSentence>> GetExamplesForEnglishAsync(string englishWord, int limit = 20)
        {
            if (_examples.TryGetValue(englishWord, out var list))
            {
                return Task.FromResult(list);
            }
            return Task.FromResult(new List<ExampleSentence>());
        }
    }
}
