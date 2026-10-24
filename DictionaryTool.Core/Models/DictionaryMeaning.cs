using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    public class DictionaryMeaning
    {
        public string PartOfSpeech { get; set; }
        public List<DictionaryDefinition> Definitions { get; set; } = new List<DictionaryDefinition>();
        public List<string> Synonyms { get; set; } = new List<string>();
        public List<string> Antonyms { get; set; } = new List<string>();
    }
}
