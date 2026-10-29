using System.Net;
using System.Text;

namespace ApiAggregator.API.Tests.Helpers
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;

        public List<HttpRequestMessage> Requests { get; } = new();

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            _respond = respond;
        }

        public static FakeHttpMessageHandler Returning(HttpStatusCode status, string json = "{}")
            => new((_, _) => Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return _respond(request, cancellationToken);
        }
    }
}