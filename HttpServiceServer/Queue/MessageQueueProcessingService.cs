using Microsoft.Extensions.Logging;
using HttpServiceServer.MessageProcessing;
using Microsoft.Extensions.Hosting;

namespace HttpServiceServer.Queue
{
    internal class MessageQueueProcessingService : BackgroundService
    {
        private readonly ILogger<MessageQueueProcessingService> _logger;
        private readonly IProcessMessageTaskQueue _messageQueue;
        private readonly IMessageProcessingService _messageProcessingService;

        public MessageQueueProcessingService(ILogger<MessageQueueProcessingService> logger,
            IProcessMessageTaskQueue messageQueue, IMessageProcessingService messageProcessingService)
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
