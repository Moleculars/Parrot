using Bb.Http;
using Bb.Http.Configuration;

namespace Bb.Curls
{
    public class Context
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token to cancel operation.</param>
        public Context(CancellationTokenSource cancellationTokenSource = null)
        {
            _tokenSource = cancellationTokenSource ?? new CancellationTokenSource();
            _token = _tokenSource.Token;
            _parameters = new Dictionary<string, string>();
        }

        public HttpClient HttpClient { get; internal set; }

        public HttpRequestMessage RequestMessage { get; internal set; }

        public IFlurlClient Client { get; internal set; }

        public FlurlRequest Request { get; internal set; }

        public IFlurlClientFactory Factory { get; internal set; }
        public IFlurlResponse Result { get; private set; }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CallAsync()
        {

            var client = Factory.Get(RequestMessage.RequestUri);
            Result = await client.SendAsync(Request, HttpCompletionOption.ResponseHeadersRead);

            var result = await HttpClient.SendAsync(RequestMessage, HttpCompletionOption.ResponseHeadersRead, _token);
            return result;
        }

        public HttpResponseMessage Call()
        {
            return HttpClient.Send(RequestMessage, HttpCompletionOption.ResponseHeadersRead, _token);
        }

        public void Cancel()
        {
            if (_token.CanBeCanceled)
                _tokenSource.Cancel();
        }

        internal void Add(KeyValuePair<string, string> item)
        {
            if (_parameters.ContainsKey(item.Key))
                _parameters[item.Key] = item.Value;
            else
                _parameters.Add(item.Key, item.Value);
        }

        internal bool Has(string key, string value)
        {

            if (_parameters.TryGetValue(key, out var result))
                return result == value;

            return false;

        }

        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

    }

}
