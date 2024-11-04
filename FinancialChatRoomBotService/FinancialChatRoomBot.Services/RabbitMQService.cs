using FinancialChatRoomBotService.FinancialChatRoomBot.Configuration;
using FinancialChatRoomBotService.FinancialChatRoomBot.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FinancialChatRoomBotService.FinancialChatRoomBot.Services
{
    public class RabbitMQService
    {
        private readonly IModel _model;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly Func<IQuoteService> _quoteServiceFactory;

        public RabbitMQService(RabbitMQConnection rbConnection,
                                ILogger<RabbitMQService> logger,
                                Func<IQuoteService> quoteServiceFactory)
        {
            _model = rbConnection.GetModel();
            _logger = logger;
            _quoteServiceFactory = quoteServiceFactory;
            ReceiveMessages();
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

                _ = Task.Run(async () =>
                {
                    var quoteService = _quoteServiceFactory();
                    // call QuoteService for get stock quote
                    await quoteService.GetStockQuote(message);
                });

                _logger.LogInformation($"RabbitMQ: Received {message}");
            };

            _model.BasicConsume(queue: "CommandToBotQueue",
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
