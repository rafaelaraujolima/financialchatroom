﻿using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FinancialChatRoomApp.FinancialChatRoom.Configuration
{
    public class RabbitMQConnection : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;

        public RabbitMQConnection(IOptions<RabbitMQSettings> rabbitMQSettings)
        {
            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQSettings.Value.HostName,
                UserName = rabbitMQSettings.Value.UserName,
                Password = rabbitMQSettings.Value.Password
            };
            
            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();
            _model.QueueDeclare(queue: "CommandToBotQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _model.QueueDeclare(queue: "ResponseFromBotQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Dispose()
        {
            _model?.Close();
            _model?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        public IModel GetModel()
        {
            return _model;
        }

        public IConnection GetConnection()
        {
            return _connection;
        }
    }
}
