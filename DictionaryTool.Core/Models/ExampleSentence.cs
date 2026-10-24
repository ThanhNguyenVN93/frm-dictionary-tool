namespace DictionaryTool.Core.Models
{
    /// <summary>
    /// Câu ví dụ từ Tatoeba. Vì câu ví dụ được lấy theo 2 cặp ngôn ngữ độc lập
    /// (EN-VI và EN-ZH, xem TatoebaRepository), không phải lúc nào cả 3 trường
    /// cũng có dữ liệu cùng lúc — English luôn có (vai trò cầu nối), Vietnamese
    /// và/hoặc Chinese có thể null nếu câu đó không nằm trong cặp tương ứng.
    /// </summary>
    public class ExampleSentence
    {
        public string English { get; set; }
        public string Vietnamese { get; set; }
        public string Chinese { get; set; }
    }
}
