using HttpServiceServer.MessageProcessing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpServiceServer.MessageQueue
{
    internal class MessageQueueProcessingService : BackgroundService
    {
        private readonly ILogger<MessageQueueProcessingService> _logger;
        private readonly IMessageQueue _messageQueue;
        private readonly IMessageProcessingService _messageProcessingService;

        public MessageQueueProcessingService(ILogger<MessageQueueProcessingService> logger,
            IMessageQueue messageQueue, IMessageProcessingService messageProcessingService)
        {
            _logger = logger;
            _messageQueue = messageQueue;
            _messageProcessingService = messageProcessingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(MessageQueueProcessingService)} is running");

            var messages = _messageQueue.DequeueAllMessagesAsync(stoppingToken);
            await foreach (var (handler, receivedMessage) in messages.WithCancellation(stoppingToken))
            {
                await _messageProcessingService.ProcessReceivedMessage(handler, receivedMessage);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(MessageQueueProcessingService)} is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
