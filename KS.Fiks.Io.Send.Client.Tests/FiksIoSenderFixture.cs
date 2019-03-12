using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace KS.Fiks.Io.Send.Client.Tests
{
    public class FiksIoSenderFixture
    {
        private string _fiksIoScheme;
        private string _fiksIoHost;
        private int _fiksIoPort;
        private HttpStatusCode _statusCode;
        private SentMessageApiModel _returnValue;
        private Dictionary<string, string> _authorizationHeaders;
        private bool _useInvalidReturnValue = false;

        public FiksIoSenderFixture()
        {
            SetDefaultValues();
            AuthenticationStrategyMock = new Mock<IAuthenticationStrategy>();
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
        }

        public Mock<IAuthenticationStrategy> AuthenticationStrategyMock { get; }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }

        public FiksIoSender CreateSut()
        {
            SetupMocks();
            return new FiksIoSender(
                _fiksIoScheme,
                _fiksIoHost,
                _fiksIoPort,
                AuthenticationStrategyMock.Object,
                new HttpClient(HttpMessageHandleMock.Object));
        }

        public FiksIoSenderFixture WithScheme(string scheme)
        {
            _fiksIoScheme = scheme;
            return this;
        }

        public FiksIoSenderFixture WithHost(string host)
        {
            _fiksIoHost = host;
            return this;
        }

        public FiksIoSenderFixture WithPort(int port)
        {
            _fiksIoPort = port;
            return this;
        }

        public FiksIoSenderFixture WithStatusCode(HttpStatusCode code)
        {
            _statusCode = code;
            return this;
        }

        public FiksIoSenderFixture WithReturnValue(SentMessageApiModel value)
        {
            _returnValue = value;
            return this;
        }

        public FiksIoSenderFixture WithAuthorizationHeaders(Dictionary<string, string> value)
        {
            _authorizationHeaders = value;
            return this;
        }

        public FiksIoSenderFixture WithInvalidReturnValue()
        {
            _useInvalidReturnValue = true;
            return this;
        }

        private void SetDefaultValues()
        {
            _fiksIoHost = "test.no";
            _fiksIoScheme = "http";
            _fiksIoPort = 8084;
            _statusCode = HttpStatusCode.Accepted;
            _returnValue = new SentMessageApiModel();
            _authorizationHeaders = new Dictionary<string, string>();
        }

        private void SetupMocks()
        {
            SetHttpResponse();
            SetupAuthenticationStrategyMock();
        }

        private void SetHttpResponse()
        {
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = _statusCode,
                Content = _useInvalidReturnValue ? GenerateInvalidResponse() : GenerateJsonResponse()
            };

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();
        }

        private StringContent GenerateJsonResponse()
        {
            return new StringContent(JsonConvert.SerializeObject(_returnValue));
        }

        private static StringContent GenerateInvalidResponse()
        {
            return new StringContent(">DSFSV#%¤DFGHV___XCXV132<>");
        }

        private void SetupAuthenticationStrategyMock()
        {
            AuthenticationStrategyMock.Setup(x => x.GetAuthorizationHeaders()).ReturnsAsync(_authorizationHeaders);
        }
    }
}