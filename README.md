# Financial Chatroom App

This project is for developing an application that creates a chatroom for users and retrieves stock quotes from an API using a specific command.

This project was developed on the Windows Operating System with Visual Studio 2022.

## Requirements

- Docker
- .NET 8.0
- MySQL Community
- RabbitMQ

## Docker

Install Docker by following the installation instructions of the Docker website for your operating system.

After installation, open PowerShell or Bash and run the following command.

```bash
> docker run -d --hostname rabbitmq --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

This will download the image and run the RabbitMQ container.

Now run the command below in PowerShell or Bash.

```bash
> docker run --name mysqlserver -e MYSQL_ROOT_PASSWORD=YOUR_PASSWORD -p 3306:3306 -d mysql:8.0
```

This will download the image and run the MySQL container. If you already have MySQL installed on your system, the port 3306 may already be in use. You have two options: either use your own MySQL server and ignore this command, or change the port to one that is not in use, as shown below:

```bash
> docker run --name mysqlserver -e MYSQL_ROOT_PASSWORD=YOUR_PASSWORD -p 9999:3306 -d mysql:8.0
```

Don’t forget to change the connection port in the appsettings.json file located in the project directory at financialchatroom\FinancialChatRoomApp.

```json
appsettings.json

"ConnectionStrings": {
  "MySqlConnection": "Server=localhost;Port=9999;Database=financial_chat_room;User=DATABASE_USERNAME;Password=YOUR_PASSWORD;"
}
```

Replace the values of DATABASE_USERNAME and YOUR_PASSWORD with your own values. The YOUR_PASSWORD is the same as the one used in the MySQL installation command.

Run the migrations to create and structure the database. Inside the project folder financialchatroom\FinancialChatRoomApp, execute the following commands.

```bash
# install entity framework executable
> dotnet ef install
# create and configure the database
> dotnet ef database update
```

The application is ready to run. If you open the project in Visual Studio, simply configure both projects in Startup Projects.
I didn’t have time to set up a `docker-compose`, so open two terminals (Windows) and run both applications separately.

```shell
# terminal 1
> cd financialchatroom\FinancialChatRoomApp
> dotnet run 

# terminal 2
> cd financialchatroom\FinancialChatRoomBotService
> dotnet run
```

Both will run, and the application can be accessed via the browser at the address `http://localhost:5220`.

You can open multiple tabs, each independent of the other.
