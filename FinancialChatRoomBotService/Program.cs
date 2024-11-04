using FinancialChatRoomBotService;
using FinancialChatRoomBotService.FinancialChatRoomBot.Configuration;
using FinancialChatRoomBotService.FinancialChatRoomBot.Interfaces;
using FinancialChatRoomBotService.FinancialChatRoomBot.Services;

var builder = Host.CreateApplicationBuilder(args);

#region Services Configuration

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<IQuoteService, QuoteService>();
// avoid circular dependency
builder.Services.AddSingleton<RabbitMQService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<RabbitMQService>>();
    var rbConnection = provider.GetRequiredService<RabbitMQConnection>();
    var quoteServiceFactory = () => provider.GetRequiredService<IQuoteService>();

    return new RabbitMQService(rbConnection, logger, quoteServiceFactory);
});

#endregion

#region Log Configuration

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

#endregion

var host = builder.Build();
var rabbitMQService = host.Services.GetRequiredService<RabbitMQService>();
// force first execution to start monitoring the queue
//Task.Run(() => rabbitMQService.ReceiveMessages());

host.Run();