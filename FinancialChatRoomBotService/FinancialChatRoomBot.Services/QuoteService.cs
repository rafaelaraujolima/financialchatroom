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

                    string responseMessage = string.Empty;

                    if (stock != null)
                    {
                        responseMessage = JsonSerializer.Serialize(new ResponseMessage
                        {
                            Caller = message.Caller,
                            Message = StockQuoteMessage(stock)
                        });
                    }
                    else
                    {
                        responseMessage = JsonSerializer.Serialize(new ResponseMessage
                        {
                            Caller = message.Caller,
                            Message = StockNotFoundMessage(message.StockName.ToUpper())
                        });
                    }

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

        private static Stock? StockQuoteCsvToStock(string csvContent)
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

                if (DateOnly.TryParseExact(columns[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOfQuote) &&
                    TimeOnly.TryParseExact(columns[2], "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly timeOfQuote) &&
                    Double.TryParse(columns[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double openQuote) &&
                    Double.TryParse(columns[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double highQuote) &&
                    Double.TryParse(columns[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double lowQuote) &&
                    Double.TryParse(columns[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double closeQuote) &&
                    Int32.TryParse(columns[7], out int volume))
                {
                    Stock stock = new Stock
                    {
                        Name = columns[0],
                        DateOfQuote = dateOfQuote,
                        TimeOfQuote = timeOfQuote,
                        OpenQuote = openQuote,
                        HighQuote = highQuote,
                        LowQuote = lowQuote,
                        CloseQuote = closeQuote,
                        VolumeOfNegotiations = volume
                    };

                    return stock;
                }

                return null;
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

        private static string StockNotFoundMessage(string stockName)
        {
            return "Stock " + stockName + " not found";
        }
    }
}
