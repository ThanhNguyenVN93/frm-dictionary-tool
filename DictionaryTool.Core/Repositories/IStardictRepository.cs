using System;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    public interface IStardictRepository
    {
        /// <summary>Import cả 2 bộ từ điển (EN-VI, VI-EN) vào SQLite nếu chưa import. An toàn để gọi nhiều lần.</summary>
        Task EnsureImportedAsync(IProgress<string> progress = null);

        Task<StardictEntry> LookupEnViAsync(string englishWord);
        Task<StardictEntry> LookupViEnAsync(string vietnameseWord);

        /// <summary>
        /// Tra theo headword VI-EN đã bỏ dấu (vd "khong" khớp headword "không").
        /// Nhiều headword thật có thể trùng 1 dạng không dấu (vd "ba"/"bà"/"bá") —
        /// trả về headword đầu tiên theo thứ tự chữ cái, chấp nhận đây là suy đoán
        /// gần đúng cho input vốn đã mất thông tin dấu.
        /// </summary>
        Task<StardictEntry> LookupViEnByNoDiacriticsAsync(string wordNoDiacritics);
    }
}
