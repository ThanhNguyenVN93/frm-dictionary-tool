using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    public class TrilingualResult
    {
        public string Query { get; set; }
        public Language DetectedLanguage { get; set; }

        public WordLookupResult EnglishResult { get; set; }
        public VietnameseResult VietnameseResult { get; set; }
        public ChineseResult ChineseResult { get; set; }

        public List<ExampleSentence> ExampleSentences { get; set; } = new List<ExampleSentence>();
    }
}
