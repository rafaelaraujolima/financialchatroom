using FinancialChatRoomBotService.FinancialChatRoomBot.Constants;
using FinancialChatRoomBotService.FinancialChatRoomBot.Interfaces;
using FinancialChatRoomBotService.FinancialChatRoomBot.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FinancialChatRoomBotService.FinancialChatRoomBot.Services
{
    public class QuoteService : IQuoteService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<QuoteService> _logger;

        public QuoteService(RabbitMQService rabbitMQService,
            ILogger<QuoteService> logger)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task GetStockQuote(string jsonMessage)
        {
            ReceiveMessage message = JsonSerializer.Deserialize<ReceiveMessage>(jsonMessage);

            try
            {
                string urlApiCall = Endpoint.StockQuote.Replace("STOCK_CODE", message.StockName.ToLower());

                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                {
                    HttpResponseMessage response = await httpClient.GetAsync(urlApiCall, cancellationTokenSource.Token);

                    response.EnsureSuccessStatusCode();

                    string csvContent = await response.Content.ReadAsStringAsync();

                    Stock stock = StockQuoteCsvToStock(csvContent);

                    string responseMessage = JsonSerializer.Serialize(new ResponseMessage
                    {
                        Caller = message.Caller,
                        Message = StockQuoteMessage(stock)
                    });

                    _rabbitMQService.SendMessage(responseMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error getting the stock quote for {message.StockName.ToUpper()}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout consulting stock quote");
            }
        }

        private static Stock StockQuoteCsvToStock(string csvContent)
        {
            using (var reader = new StringReader(csvContent))
            {
                string line;
                bool isFirstLine = true;

                line = reader.ReadLine();

                if (isFirstLine)
                {
                    isFirstLine = false;
                    line = reader.ReadLine();
                }

                var columns = line.Split(',');

                // need to validate data before instantiating
                // after a period the market is closed
                // and the response could be different
                // need to handle this
                Stock stock = new Stock
                {
                    Name = columns[0],
                    DateOfQuote = DateOnly.ParseExact(columns[1], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TimeOfQuote = TimeOnly.ParseExact(columns[2], "HH:mm:ss", CultureInfo.InvariantCulture),
                    OpenQuote = Double.Parse(columns[3], CultureInfo.InvariantCulture),
                    HighQuote = Double.Parse(columns[4], CultureInfo.InvariantCulture),
                    LowQuote = Double.Parse(columns[5], CultureInfo.InvariantCulture),
                    CloseQuote = Double.Parse(columns[6], CultureInfo.InvariantCulture),
                    VolumeOfNegotiations = Int32.Parse(columns[7])
                };

                return stock;
            }
        }

        private static string StockQuoteMessage(Stock stock)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(stock.Name.ToUpper());
            stringBuilder.Append(" quote is $");
            stringBuilder.Append(stock.CloseQuote.ToString("F2", CultureInfo.InvariantCulture));
            stringBuilder.Append(" per share");

            return stringBuilder.ToString();
        }
    }
}
