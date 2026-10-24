using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DictionaryTool.Core.Clients;
using DictionaryTool.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class DictionaryApiDevClientTests
    {
        [TestMethod]
        public async Task GetAsync_AudioEntryMissingOwnText_FallsBackToSiblingText()
        {
            // dictionaryapi.dev thường trả 1 mục chỉ có "text" (không audio) và 1 mục
            // khác chỉ có "audio" (không text) cho cùng 1 giọng — client phải gộp IPA
            // dự phòng vào mục có audio, không được bỏ mất.
            const string json = @"[{
                ""word"": ""boyfriend"",
                ""phonetic"": ""/ˈbɔɪˌfɹɛnd/"",
                ""phonetics"": [
                    { ""text"": ""/ˈbɔɪˌfɹɛnd/"", ""audio"": """" },
                    { ""audio"": ""https://api.dictionaryapi.dev/media/pronunciations/en/boyfriend-us.mp3"" }
                ],
                ""meanings"": [{
                    ""partOfSpeech"": ""noun"",
                    ""definitions"": [{ ""definition"": ""A male partner."", ""example"": null }],
                    ""synonyms"": [], ""antonyms"": []
                }]
            }]";

            var httpClient = new HttpClient(new StubHttpMessageHandler(HttpStatusCode.OK, json));
            var client = new DictionaryApiDevClient(httpClient);

            var result = await client.GetAsync("boyfriend");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Phonetics.Count);
            Assert.AreEqual("/ˈbɔɪˌfɹɛnd/", result.Phonetics[0].Text);
            Assert.AreEqual("https://api.dictionaryapi.dev/media/pronunciations/en/boyfriend-us.mp3", result.Phonetics[0].AudioUrl);
            Assert.AreEqual("US", result.Phonetics[0].AccentLabel);
        }

        [TestMethod]
        public async Task GetAsync_NoAudioAtAll_StillReturnsIpaTextWithoutAudioUrl()
        {
            const string json = @"[{
                ""word"": ""example"",
                ""phonetic"": ""/example-ipa/"",
                ""phonetics"": [
                    { ""text"": ""/example-ipa/"", ""audio"": """" }
                ],
                ""meanings"": [{
                    ""partOfSpeech"": ""noun"",
                    ""definitions"": [{ ""definition"": ""def."", ""example"": null }],
                    ""synonyms"": [], ""antonyms"": []
                }]
            }]";

            var httpClient = new HttpClient(new StubHttpMessageHandler(HttpStatusCode.OK, json));
            var client = new DictionaryApiDevClient(httpClient);

            var result = await client.GetAsync("example");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Phonetics.Count);
            Assert.AreEqual("/example-ipa/", result.Phonetics[0].Text);
            Assert.AreEqual(string.Empty, result.Phonetics[0].AudioUrl);
        }
    }
}
