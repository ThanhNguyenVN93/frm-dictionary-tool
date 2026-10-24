using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core
{
    public interface ITrilingualLookupService
    {
        Task<TrilingualResult> LookupAsync(string query, Language? languageHint = null, CancellationToken ct = default(CancellationToken));
    }
}
