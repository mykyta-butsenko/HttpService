using System.Net.Sockets;
using HttpServiceServer.MessageProcessing;
using HttpServiceServer.SocketWrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HttpServiceServer.Test
{
    public class MessageProcessingServiceTests
    {
        private IMessageProcessingService _messageProcessingService = null!;
        private ISocket _mockHandler = null!;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            
            _messageProcessingService =
                new MessageProcessingService(serviceProvider.GetRequiredService<ILogger<MessageProcessingService>>());
            _mockHandler = MockTcpSocket();
        }

        [Test]
        public void ProcessReceivedMessage_HandlerIsNull_ThrowsArgumentNullException()
        {
            ISocket handler = null!;
            const string receivedMessage = "foo bar";

            Assert.That(async () => await _messageProcessingService.ProcessReceivedMessage(handler, receivedMessage),
                Throws.ArgumentNullException);
        }

        [Test]
        public void ProcessReceivedMessage_ReceivedMessageIsNull_ThrowsArgumentNullException()
        {
            const string receivedMessage = null!;

            Assert.That(async () => await _messageProcessingService.ProcessReceivedMessage(_mockHandler, receivedMessage!),
                Throws.ArgumentNullException);
        }

        [Test]
        public void ProcessReceivedMessage_ReceivedMessageForFavicon_Success()
        {
            const string receivedMessage = "GET /favicon.ico HTTP/1.1\r\nHost: localhost:4200\r\nConnection: keep-alive\r\nsec-ch-ua: \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\", \"Google Chrome\";v=\"108\"\r\nsec-ch-ua-mobile: ?0\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36\r\nsec-ch-ua-platform: \"Windows\"\r\nAccept: image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8\r\nSec-Fetch-Site: same-origin\r\nSec-Fetch-Mode: no-cors\r\nSec-Fetch-Dest: image\r\nReferer: http://localhost:4200/\r\nAccept-Encoding: gzip, deflate, br\r\nAccept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7";
            Assert.That(async () => await _messageProcessingService.ProcessReceivedMessage(_mockHandler, receivedMessage), Throws.Nothing);
        }

        [Test]
        public void ProcessReceivedMessage_ReceivedMessageForHtml_Success()
        {
            const string receivedMessage = "GET / HTTP/1.1\r\nHost: localhost:4200\r\nConnection: keep-alive\r\nCache-Control: max-age=0\r\nsec-ch-ua: \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\", \"Google Chrome\";v=\"108\"\r\nsec-ch-ua-mobile: ?0\r\nsec-ch-ua-platform: \"Windows\"\r\nUpgrade-Insecure-Requests: 1\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9\r\nSec-Fetch-Site: none\r\nSec-Fetch-Mode: navigate\r\nSec-Fetch-User: ?1\r\nSec-Fetch-Dest: document\r\nAccept-Encoding: gzip, deflate, br\r\nAccept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7";
            Assert.That(async () => await _messageProcessingService.ProcessReceivedMessage(_mockHandler, receivedMessage), Throws.Nothing);
        }

        [Test]
        public void ProcessReceivedMessage_ReceivedInvalidMessage_Success()
        {
            const string invalidMessage = "foo bar";
            Assert.That(async () => await _messageProcessingService.ProcessReceivedMessage(_mockHandler, invalidMessage), Throws.Nothing);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
        }

        private static TcpSocket MockTcpSocket()
        {
            var handlerMock = new Mock<TcpSocket>(AddressFamily.InterNetwork);
            handlerMock.Setup(handler => handler.SendAsync(It.IsAny<byte[]>())).ReturnsAsync(It.IsAny<int>());
            handlerMock.Setup(handler => handler.Shutdown()).Verifiable();
            handlerMock.Setup(handler => handler.Close()).Verifiable();
            return handlerMock.Object;
        }
    }
}