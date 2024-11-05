using FinancialChatRoom.Constants;
using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces.Repositories;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using FinancialChatRoomApp.Models;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FinancialChatRoom.Hubs
{
    public class FinancialChatRoomHub : Hub, IFinancialChatRoomHub
    {
        private readonly IModel _model;
        private readonly ILogger<FinancialChatRoomHub> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FinancialChatRoomHub(RabbitMQConnection rbConnection,
            ILogger<FinancialChatRoomHub> logger,
            IServiceProvider serviceProvider)
        {
            _model = rbConnection.GetModel();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task SendMessageAsync(string user, string message)
        {         
            if (IsValidMessage(message))
            {
                if (IsCommand(message))
                {
                    if (IsValidCommand(message))
                    {
                        string jsonMessage = JsonSerializer.Serialize(new SendMessage
                        {
                            Caller = user,
                            StockName = StockName(message)
                        });

                        SendMessage(jsonMessage);
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", "SYSTEM", $"The command you entered is not a valid command! Command: {message}");
                    }
                }
                else
                {
                    Post post = new Post
                    {
                        DateOfPost = DateTime.UtcNow,
                        Message = message,
                        UserName = user
                    };

                    // Send messages to the clients logged in the chatroom
                    await Clients.All.SendAsync("ReceiveMessage", post.UserName, post.Message, post.DateOfPost.ToString());

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var postRepository = scope.ServiceProvider.GetRequiredService<IPostRepository>();

                        await postRepository.InsertPostAsync(post);
                    }
                }
            }
        }

        public async Task ReceiveCommandResult(string jsonMessage)
        {
            CommandResult result = JsonSerializer.Deserialize<CommandResult>(jsonMessage);

            await Clients.User(result.Caller).SendAsync(result.Message);
        }

        private static bool IsValidMessage(string message)
        {
            return !string.IsNullOrWhiteSpace(message);
        }
        
        private static bool IsCommand(string message)
        {
            if (message.StartsWith(Commands.commandPatternInit)
                && message.Contains(Commands.commandPatternEnd))
                return true;

            return false;
        }

        private static bool IsValidCommand(string message)
        {
            int indexOfEndOfCommand = message.IndexOf(Commands.commandPatternEnd);

            string command = message.Substring(1, indexOfEndOfCommand - 1);

            return Commands.validCommands.Contains(command);
        }

        private static string StockName(string command)
        {
            return command.Substring(command.IndexOf(Commands.commandPatternEnd) + 1);
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
            await Clients.Caller.SendAsync("ReceiveMessage", "BOT", commandResult.Message, DateTime.UtcNow.ToString());
        }
    }
}
