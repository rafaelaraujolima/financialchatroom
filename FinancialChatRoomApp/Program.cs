using FinancialChatRoom.Hubs;
using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces;
using FinancialChatRoomApp.FinancialChatRoom.Services;

var builder = WebApplication.CreateBuilder(args);

# region Configure Services
// configuration of RabbitMQ service
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<IFinancialChatRoomHub, FinancialChatRoomHub>();
builder.Services.AddSingleton<RabbitMQService>();

builder.Services.AddAuthentication();

builder.Services.AddRazorPages();
// Add services for control Websockets with SignalR
builder.Services.AddSignalR();

# endregion

#region Log Configuration

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

#endregion

var app = builder.Build();

# region Configure Pipeline
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
// Maps the endpoint for the chatroom
app.MapHub<FinancialChatRoomHub>("/chatroomhub");

# endregion

app.Run();