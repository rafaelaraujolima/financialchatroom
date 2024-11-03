using FinancialChatRoomBot.Constants;
using FinancialChatRoomBot.Models;
using System.Globalization;
using System.Text;

public static class FinancialControlBot
{
    private static readonly HttpClient httpClient = new HttpClient();    
    
    public static async Task<string> GetStockQuote(string stockName)
    {
        try
        {
            string urlApiCall = Endpoint.StockQuote.Replace("STOCK_CODE", stockName.ToLower());

            HttpResponseMessage response = await httpClient.GetAsync(urlApiCall);

            response.EnsureSuccessStatusCode();

            string csvContent = await response.Content.ReadAsStringAsync();

            Stock stock = StockQuoteCsvToStock(csvContent);

            return StockQuoteMessage(stock);
        }
        catch (HttpRequestException ex)
        { 
            Console.WriteLine(ex.Message);
            return $"Error getting the stock quote for {stockName.ToUpper()}";
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
                HighQuote = Double.Parse(columns[4]),
                LowQuote = Double.Parse(columns[5]),
                CloseQuote = Double.Parse(columns[6]),
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
        stringBuilder.Append(stock.OpenQuote.ToString("F2", CultureInfo.InvariantCulture));
        stringBuilder.Append(" per share");

        return stringBuilder.ToString();
    }
}
