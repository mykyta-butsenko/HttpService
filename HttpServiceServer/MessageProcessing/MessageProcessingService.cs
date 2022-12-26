using Microsoft.Extensions.Logging;
using HttpServiceServer.MessageListener;
using HttpServiceServer.SocketWrappers;

namespace HttpServiceServer.MessageProcessing
{
    public class MessageProcessingService : IMessageProcessingService
    {
        private readonly ILogger<MessageProcessingService> _logger;

        public MessageProcessingService(ILogger<MessageProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task ProcessReceivedMessage(ISocket handler, string receivedMessage)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (receivedMessage is null)
            {
                throw new ArgumentNullException(nameof(receivedMessage));
            }

            using (handler)
            {
                var receivedMessageSplit = receivedMessage.Split(Environment.NewLine);
                var requestUrl = receivedMessageSplit[0];
                if (requestUrl.Contains(FilePath.Favicon, StringComparison.OrdinalIgnoreCase))
                {
                    // It means the browser requested the favicon; this request is normally received after sending HTML page as a response
                    await SendFaviconResponse(handler).ConfigureAwait(false);
                }
                else if (requestUrl.Contains("GET / HTTP/1.1", StringComparison.OrdinalIgnoreCase))
                {
                    // It means the browser requested us for the first time, therefore it needs HTML as a response
                    await SendHtmlResponse(handler).ConfigureAwait(false);
                }
                else
                {
                    // It means the browser requested something wrong
                    await SendErrorResponse(handler).ConfigureAwait(false);
                }

                handler.Shutdown();
                handler.Close();
            }
        }

        private async Task SendHtmlResponse(ISocket handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            const string statusLine = "HTTP/1.1 200 OK";
            const string pageResponseHeader = "Content-Type: text/html";
            var indexHtmlContent = await File.ReadAllTextAsync(FilePath.IndexPagePath).ConfigureAwait(false);

            _logger.LogInformation("Sending the Index page = {indexPage} as a response...", FilePath.IndexPagePath);
            await handler.SendAsync(statusLine.AppendNewLine().ToBytes()).ConfigureAwait(false);
            await handler.SendAsync(pageResponseHeader.AppendNewLine().ToBytes()).ConfigureAwait(false);
            await handler.SendAsync(indexHtmlContent.AppendNewLine().ToBytes()).ConfigureAwait(false);
            _logger.LogInformation("Service {service} sent HTML response.", nameof(ListenerService));
        }

        private async Task SendFaviconResponse(ISocket handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            const string statusLine = "HTTP/1.1 200 OK";
            const string faviconResponseHeader = "Content-Type: image/x-icon";
            var faviconContent = await File.ReadAllBytesAsync(FilePath.FaviconPath).ConfigureAwait(false);

            _logger.LogInformation("Sending the Favicon = {favicon} as a response...", FilePath.FaviconPath);
            await handler.SendAsync(statusLine.AppendNewLine().ToBytes()).ConfigureAwait(false);
            await handler.SendAsync(faviconResponseHeader.AppendNewLine().ToBytes()).ConfigureAwait(false);
            await handler.SendAsync(faviconContent).ConfigureAwait(false);
            await handler.SendAsync(Environment.NewLine.ToBytes()).ConfigureAwait(false);
            _logger.LogInformation("Service {service} sent Favicon response.", nameof(ListenerService));
        }

        private async Task SendErrorResponse(ISocket handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            const string statusLine = "HTTP/1.1 400 Bad Request";

            _logger.LogInformation("Sending 400 Bad Request error response");
            await handler.SendAsync(statusLine.AppendNewLine().ToBytes()).ConfigureAwait(false);
            _logger.LogInformation("Service {service} sent 400 Bad Request response.", nameof(ListenerService));
        }
    }
}
