using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core
{
    public interface IDictionaryService
    {
        Task<WordLookupResult> LookupAsync(string word, CancellationToken ct = default(CancellationToken));
    }
}
