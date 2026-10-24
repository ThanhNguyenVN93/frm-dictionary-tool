using System;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Clients;

namespace DictionaryTool.Tests.Fakes
{
    internal class FakeDictionaryApiDevClient : IDictionaryApiDevClient
    {
        private readonly DictionaryApiDevResult _result;
        private readonly Exception _exceptionToThrow;

        public int CallCount { get; private set; }

        private FakeDictionaryApiDevClient(DictionaryApiDevResult result, Exception exceptionToThrow)
        {
            _result = result;
            _exceptionToThrow = exceptionToThrow;
        }

        public static FakeDictionaryApiDevClient ReturningNotFound()
        {
            return new FakeDictionaryApiDevClient(null, null);
        }

        public static FakeDictionaryApiDevClient Returning(DictionaryApiDevResult result)
        {
            return new FakeDictionaryApiDevClient(result, null);
        }

        public static FakeDictionaryApiDevClient ThrowingException()
        {
            return new FakeDictionaryApiDevClient(null, new InvalidOperationException("Simulated network failure"));
        }

        public Task<DictionaryApiDevResult> GetAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            CallCount++;

            if (_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            }

            return Task.FromResult(_result);
        }
    }
}
