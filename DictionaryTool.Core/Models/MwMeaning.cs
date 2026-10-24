using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    public class MwMeaning
    {
        public string PartOfSpeech { get; set; }
        public List<string> ShortDefinitions { get; set; } = new List<string>();

        /// <summary>MW dictionary product this entry came from: "Collegiate" or "Learner's".</summary>
        public string Source { get; set; }
    }
}
