using FinancialChatRoom.Constants;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using FinancialChatRoomApp.FinancialChatRoom.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace FinancialChatRoom.Hubs
{
    public class FinancialChatRoomHub : Hub, IFinancialChatRoomHub
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<FinancialChatRoomHub> _logger;

        public FinancialChatRoomHub(RabbitMQService service,
            ILogger<FinancialChatRoomHub> logger)
        {
            _rabbitMQService = service;
            _logger = logger;
        }

        public async Task SendMessage(string user, string message)
        {
            var username = Context.User?.Identity?.Name;
            
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

                        _rabbitMQService.SendMessage(jsonMessage);
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", "SYSTEM", $"The command you entered is not a valid command! Command: {message}");
                    }
                }
                else
                {
                    // Send messages to the clients logged in the chatroom
                    await Clients.All.SendAsync("ReceiveMessage", user, message);
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
    }
}
