using System;
using System.Configuration;
using System.Net.Http;
using System.Windows.Forms;
using DictionaryTool.Core;
using DictionaryTool.Core.Cache;
using DictionaryTool.Core.Clients;
using DictionaryTool.Core.Export;
using DictionaryTool.Core.Repositories;

namespace DictionaryTool.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var httpClient = new HttpClient();
            var dictionaryApiDevClient = new DictionaryApiDevClient(httpClient);

            var apiUsageTracker = new ApiUsageTracker();

            var collegiateApiKey = ConfigurationManager.AppSettings["MerriamWebster:CollegiateApiKey"];
            var learnerApiKey = ConfigurationManager.AppSettings["MerriamWebster:LearnerApiKey"];
            var merriamWebsterClient = new MerriamWebsterClient(collegiateApiKey, learnerApiKey, httpClient, apiUsageTracker);

            var cacheRepository = new CacheRepository();
            var dictionaryService = new DictionaryService(dictionaryApiDevClient, merriamWebsterClient, cacheRepository);

            var audioCacheRepository = new AudioCacheRepository(httpClient);

            var cedictRepository = new CcCedictRepository();
            var stardictRepository = new StardictRepository();
            var tatoebaRepository = new TatoebaRepository();
            var trilingualLookupService = new TrilingualLookupService(
                dictionaryService, cedictRepository, stardictRepository, tatoebaRepository);

            var historyRepository = new HistoryRepository();
            var bookmarkRepository = new BookmarkRepository();
            var bookmarkXlsxExporter = new BookmarkXlsxExporter();

            Application.Run(new MainForm(
                trilingualLookupService,
                audioCacheRepository,
                apiUsageTracker,
                cacheRepository,
                cedictRepository,
                stardictRepository,
                tatoebaRepository,
                historyRepository,
                bookmarkRepository,
                bookmarkXlsxExporter));
        }
    }
}
