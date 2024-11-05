namespace FinancialChatRoomApp.FinancialChatRoom.Interfaces
{
    public interface IFinancialChatRoomHub
    {
        Task SendMessageAsync(string user, string message);
        Task ReceiveCommandResult(string jsonMessage);
    }
}
