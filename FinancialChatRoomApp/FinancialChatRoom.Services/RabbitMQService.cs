using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using FinancialChatRoom.Hubs;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using Microsoft.Extensions.Logging;

namespace FinancialChatRoomApp.FinancialChatRoom.Services
{
    public class RabbitMQService
    {
        private readonly IModel _model;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IHubContext<FinancialChatRoomHub> _hubContext;

        public RabbitMQService(RabbitMQConnection rbConnection,
                                ILogger<RabbitMQService> logger,
                                IHubContext<FinancialChatRoomHub> hubContext)
        {
            _model = rbConnection.GetModel();
            _logger = logger;
            _hubContext = hubContext;
            ReceiveMessages();
        }

        public void SendMessage(string jsonMessage)
        {
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            _model.BasicPublish(exchange: "",
                                routingKey: "CommandToBotQueue",
                                basicProperties: null,
                                body: body);
            _logger.LogInformation($"RabbitMQ: Sent {jsonMessage.ToString()}");
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
                    await ProcessReceivedMessage(message);
                });

                _logger.LogInformation($"RabbitMQ-App: Received {message}");
            };

            _model.BasicConsume(queue: "ResponseFromBotQueue",
                                 autoAck: true,
                                 consumer: consumer);
        }

        private async Task ProcessReceivedMessage(string message)
        {
            var commandResult = JsonSerializer.Deserialize<CommandResult>(message);
            //precisa configurar a autenticação
            await _hubContext.Clients.User(commandResult.Caller).SendAsync("ReceiveMessage", "BOT", commandResult.Message);
        }
    }
}
