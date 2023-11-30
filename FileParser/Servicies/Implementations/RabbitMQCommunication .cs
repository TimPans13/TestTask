using FileParser.Servicies.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace FileParser.Implementations
{
    public class RabbitMQCommunication : IRabbitMQCommunication
    {
        private readonly string rabbitMQConnectionString;
        private readonly string exchangeName;
        private readonly string routingKey;
        private readonly string queueName;

        public RabbitMQCommunication(string rabbitMQConnectionString, string exchangeName, string routingKey)
        {
            this.rabbitMQConnectionString = rabbitMQConnectionString ?? throw new ArgumentNullException(nameof(rabbitMQConnectionString));
            this.exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            this.routingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
            this.queueName = "queue_name"; 
            InitializeRabbitMQ();
        }

        public void SendData(string jsonData)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString) };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(jsonData);

                    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);

                    Console.WriteLine($"Sent to RabbitMQ ({queueName}): {jsonData}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to RabbitMQ ({queueName}): {ex.Message}");
            }
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString) };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing RabbitMQ ({queueName}): {ex.Message}");
            }
        }
    }
}

