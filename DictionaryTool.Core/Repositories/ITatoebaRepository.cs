using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    public interface ITatoebaRepository
    {
        /// <summary>Import 2 file câu ví dụ EN-VI và EN-ZH đã bundle sẵn vào SQLite nếu chưa import.</summary>
        Task EnsureImportedAsync(IProgress<string> progress = null);

        /// <summary>
        /// Lấy câu ví dụ chứa <paramref name="englishWord"/> (cầu nối qua tiếng Anh cho
        /// cả 3 mode tra từ). Tối đa <paramref name="limit"/> câu.
        /// </summary>
        Task<List<ExampleSentence>> GetExamplesForEnglishAsync(string englishWord, int limit = 20);
    }
}
