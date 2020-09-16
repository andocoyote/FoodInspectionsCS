using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientTest.HttpHelpers
{
    public class HttpHelper
    {
        public HttpClient _client = new HttpClient();
        private Uri _uri { get; set; } = null;
        private string _app_token { get; set; } = null;

        public HttpHelper(string url, HttpConfiguration config)
        {
            _uri = new Uri(url);
            _app_token = config.AppToken;

            // Configure the HttpClient with the app token and data type
            _client.BaseAddress = _uri;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(config.MediaType));
            _client.DefaultRequestHeaders.Add("X-App-Token", config.AppToken);
        }

        public async Task<TOutput> DoGetAsync<TOutput>(string relativeUri, WebHeaderCollection customHeaders = null, int retryCount = 1)
        {
            HttpResponseMessage response = await GetServerResponseMessageAsync(relativeUri, customHeaders, retryCount);
            return await GetResponseObjectAsync<TOutput>(response).ConfigureAwait(false);
        }

        protected async virtual Task<HttpResponseMessage> GetServerResponseMessageAsync(string relativeUri, WebHeaderCollection customHeaders, int retryCount)
        {
            // Call HttpClient.SendAsync(...) with retry and return the HttpResponseMessage which contains the status code and data
            return await PerformHttpClientActionAsync(
                taskAction: async () => await _client.SendAsync(CreateRequestMessage(HttpMethod.Get, relativeUri, (object)null, customHeaders)).ConfigureAwait(false),
                customHeaders: customHeaders,
                retryCount: retryCount).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> PerformHttpClientActionAsync(Func<Task<HttpResponseMessage>> taskAction, WebHeaderCollection customHeaders, int retryCount)
        {
            HttpResponseMessage response = await
                Retry.Do(
                    action: async () => await taskAction().ConfigureAwait(false),
                    retryInterval: TimeSpan.FromSeconds(1),
                    attemptCount: retryCount + 1)
                .ConfigureAwait(false);

            return response;
        }

        private async Task<TOutput> GetResponseObjectAsync<TOutput>(HttpResponseMessage response)
        {
            // Deserialize the JSON content obtained from the API call
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            TOutput result = JsonConvert.DeserializeObject<TOutput>(content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, SerializationBinder = new DefaultSerializationBinder() });

            return result;
        }

        private HttpRequestMessage CreateRequestMessage<TInput>(
            HttpMethod method,
            string relativeUri,
            TInput sendObject,
            WebHeaderCollection customHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, relativeUri);

            // HttpRequestMessage.Content is for POST requests, not GET
            if (method != HttpMethod.Get)
            {
                request.Content = ConvertSendObjectToContent(sendObject);
            }

            AddCustomHeadersToRequest(request, customHeaders);

            return request;
        }

        private StringContent ConvertSendObjectToContent<TInput>(TInput sendObject)
        {
            string jsonSendBody = string.Empty;

            if (sendObject != null)
            {
                jsonSendBody = JsonConvert.SerializeObject(
                    sendObject,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, SerializationBinder = new DefaultSerializationBinder() });
            }

            return new StringContent(jsonSendBody, Encoding.UTF8);
        }

        private void AddCustomHeadersToRequest(HttpRequestMessage request, WebHeaderCollection customHeaders)
        {
            // The System.Net.Http APIs try to validate HTTP headers. It turns out the 
            // APIs get pretty upset and yell at you if you try to add content headers
            // into the the request headers instead of the content headers (even though
            // the HTTP standard has no such distinction). Because HTTP calls are perfectly
            // fine without this validation, we use TryAddWithoutValidation to avoid this
            // scenario.
            customHeaders?.AllKeys.ToList().ForEach(key =>
            {
                if (!request.Headers.TryAddWithoutValidation(key, customHeaders[key]))
                {
                    Console.WriteLine($"While calling {nameof(CreateRequestMessage)}, " +
                                            $"{nameof(HttpHelper)} failed to add the header: '{key}'.");
                }
            });
        }
    }
}
