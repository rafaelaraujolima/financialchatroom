namespace FinancialChatRoomBot.Models
{
    public class Stock
    {
        public string Name { get; set; }
        public DateOnly? DateOfQuote { get; set; }
        public TimeOnly? TimeOfQuote { get; set; }
        public double OpenQuote { get; set; }
        public double HighQuote { get; set; }
        public double LowQuote { get; set; }
        public double CloseQuote { get; set; }
        public int VolumeOfNegotiations { get; set; }
    }
}
