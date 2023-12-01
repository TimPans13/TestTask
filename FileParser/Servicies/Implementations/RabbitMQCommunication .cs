using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileParser.Servicies.Interfaces;
using RabbitMQ.Client;
using Serilog;

namespace FileParser.Implementations
{
    public class RabbitMQCommunication : IRabbitMQCommunication
    {
        private readonly string rabbitMQConnectionString;
        private readonly string exchangeName;
        private readonly string routingKey;
        private readonly string queueName;

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ILogger logger;

        public RabbitMQCommunication(string rabbitMQConnectionString, string exchangeName, string routingKey, ILogger logger)
        {
            this.rabbitMQConnectionString = rabbitMQConnectionString ?? throw new ArgumentNullException(nameof(rabbitMQConnectionString));
            this.exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            this.routingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
            this.queueName = "queue_name";
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(async () => await InitializeRabbitMQAsync());
        }

        public void SendData(string jsonData)
        {
            try
            {
                semaphoreSlim.Wait();

                var factory = new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString) };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(jsonData);

                    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);

                    logger.Information($"Sent to RabbitMQ ({queueName}): {jsonData}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error sending data to RabbitMQ ({queueName}): {ex.Message}");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task InitializeRabbitMQAsync()
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString) };

                using (var connection = await Task.Run(() => factory.CreateConnection()))
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error initializing RabbitMQ ({queueName}): {ex.Message}");
            }
        }
    }
}
