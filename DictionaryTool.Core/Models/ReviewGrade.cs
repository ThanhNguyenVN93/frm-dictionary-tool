namespace DictionaryTool.Core.Models
{
    /// <summary>Mức độ nhớ do người dùng tự đánh giá khi ôn tập (giống Anki).</summary>
    public enum ReviewGrade
    {
        Again = 0, // Quên
        Hard = 1,  // Khó
        Good = 2,  // Nhớ
        Easy = 3   // Dễ
    }
}
