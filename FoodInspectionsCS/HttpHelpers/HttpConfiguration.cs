using System;

namespace HttpClientTest.HttpHelpers
{
    // Contains configuration settings for the HttpClient object
    public class HttpConfiguration
    {
        public string AppToken { get; set; } = null;
        public string MediaType { get; set; } = null;

        public HttpConfiguration(string appToken, string mediaType)
        {
            AppToken = appToken;
            MediaType = mediaType;
        }
    }
}
