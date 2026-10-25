using System;

namespace DictionaryTool.Core.Models
{
    public class BookmarkEntry
    {
        public string Query { get; set; }
        public Language Language { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        // --- Ôn tập giãn cách (SRS) ---
        /// <summary>Thời điểm đến hạn ôn lại. Null = chưa lên lịch (coi như đến hạn ngay).</summary>
        public DateTime? NextReviewUtc { get; set; }
        public int IntervalDays { get; set; }
        public double Ease { get; set; } = SrsDefaults.Ease;
        public int Reps { get; set; }
        public DateTime? LastReviewedUtc { get; set; }
    }

    internal static class SrsDefaults
    {
        public const double Ease = 2.5;
    }
}
