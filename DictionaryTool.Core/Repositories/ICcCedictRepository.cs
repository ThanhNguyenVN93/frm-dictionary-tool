using System;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    public interface ICcCedictRepository
    {
        /// <summary>Import dữ liệu CC-CEDICT từ file seed vào SQLite nếu chưa import lần nào. An toàn để gọi nhiều lần.</summary>
        Task EnsureImportedAsync(IProgress<string> progress = null);

        /// <summary>Tra theo từ/chữ Hán (phồn thể hoặc giản thể). Trả về NotFound=true nếu không có.</summary>
        Task<ChineseResult> LookupByChineseAsync(string word);

        /// <summary>Tra ngược: tìm từ Hán có nghĩa tiếng Anh khớp với từ cho trước.</summary>
        Task<ChineseResult> LookupByEnglishAsync(string englishWord);
    }
}
