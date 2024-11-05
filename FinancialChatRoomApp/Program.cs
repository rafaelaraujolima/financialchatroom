using FinancialChatRoom.Hubs;
using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces.Repositories;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using FinancialChatRoomApp.FinancialChatRoom.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

# region Configure Services

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
                            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))));

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// configuration of RabbitMQ service
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<FinancialChatRoomHub>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
});
builder.Services.AddAuthorization();

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

var financialChatRoomHub = app.Services.GetRequiredService<FinancialChatRoomHub>();
// force first execution to start monitoring the queue
Task.Run(() => financialChatRoomHub.ReceiveMessages());

# endregion

app.Run();