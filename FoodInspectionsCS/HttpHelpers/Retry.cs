using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace HttpClientTest.HttpHelpers
{
    public static class Retry
    {
        private const int DefaultAttemptCount = 1;

        /// <summary>
        /// Run action until it succeeds or up to attemptCount times, waiting retryInterval between attempts (linear retry).
        /// </summary>
        /// <param name="action">Action to run.</param>
        /// <param name="retryInterval">duration to wait between retries</param>
        /// <param name="attemptCount">Maximum number of times to attempt to execute action.</param>
        public static async Task<HttpResponseMessage> Do(Func<Task<HttpResponseMessage>> action, TimeSpan retryInterval, int attemptCount = DefaultAttemptCount)
        {
            if (attemptCount < 1)
            {
                attemptCount = 1;
            }

            List<Exception> exceptions = new List<Exception>();

            for (int attempt = 1; attempt <= attemptCount; attempt++)
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await action().ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                    var retErr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(retErr))
                    {
                        throw new HttpResponseException(response.StatusCode);
                    }
                    else
                    {
                        HttpResponseMessage errorMessage = new HttpResponseMessage(response.StatusCode);
                        errorMessage.Content = new StringContent(retErr);
                        throw new HttpResponseException(errorMessage);
                    }
                }
                catch (HttpResponseException ex)
                {
                    if (attemptCount > attempt && IsRetriable(response))
                    {
                        exceptions.Add(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (HttpRequestException ex)
                {
                    // We have been seeing HttpRequestExceptions due to not being able to get a socket.  We will treat this as a retriable error.
                    if (attemptCount > attempt)
                    {
                        exceptions.Add(ex);
                    }
                    else
                    {
                        throw;
                    }

                }

                // If we got here, it must mean we are going to retry the action.  But we need to wait for the specified interval before we try it again.
                Thread.Sleep(retryInterval);
            }
            throw new AggregateException(exceptions.Last());
        }

        private static readonly HttpStatusCode[] HttpStatusCodesToRetry =
        {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        private static bool IsRetriable(HttpResponseMessage response)
        {
            return HttpStatusCodesToRetry.Contains(response.StatusCode);
        }
    }
}
