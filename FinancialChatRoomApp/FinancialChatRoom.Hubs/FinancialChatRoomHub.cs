using Constants;
using Microsoft.AspNetCore.SignalR;

namespace FinancialChatRoom.Hubs
{
    public class FinancialChatRoomHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            if (IsValidMessage(message))
            {
                if (IsCommand(message))
                {
                    if (IsValidCommand(message))
                    {
                        
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", "System", $"The command you entered is not a valid command! Command: {message}");
                    }
                }
                else
                {
                    // Send messages to the clients logged in the chatroom
                    await Clients.All.SendAsync("ReceiveMessage", user, message);
                }
            }
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
    }
}
