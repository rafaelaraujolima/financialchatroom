namespace FinancialChatRoomApp.FinancialChatRoom.Interfaces
{
    public interface IFinancialChatRoomHub
    {
        Task SendMessage(string user, string message);
        Task ReceiveCommandResult(string jsonMessage);
    }
}
