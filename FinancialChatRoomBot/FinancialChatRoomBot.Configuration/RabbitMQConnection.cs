using RabbitMQ.Client;

namespace FinancialChatRoomBot.FinancialChatRoomBot.Configuration
{
    public class RabbitMQConnection : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;

        public RabbitMQConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = RabbitMQSettings.HostName,
                UserName = RabbitMQSettings.UserName,
                Password = RabbitMQSettings.Password
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
