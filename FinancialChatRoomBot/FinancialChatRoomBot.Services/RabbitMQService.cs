using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FinancialChatRoomBot.FinancialChatRoomBot.Services
{
    public class RabbitMQService
    {
        private readonly IModel _model;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly QuoteService _quoteService;

        public RabbitMQService(RabbitMQConnection rbConnection,
                                ILogger<RabbitMQService> logger,
                                QuoteService quoteService)
        {
            _model = rbConnection.GetModel();
            _logger = logger;
            _quoteService = quoteService;
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _model.BasicPublish(exchange: "",
                                routingKey: "ResponseFromBotQueue",
                                basicProperties: null,
                                body: body);
            _logger.LogInformation($"RabbitMQ: Sent {message}");
        }

        public void ReceiveMessages()
        {
            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"RabbitMQ: Received {message}");
            };

            _model.BasicConsume(queue: "CommandToBotQueue",
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
