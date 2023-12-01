using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SQLiteDB.Data;
using SQLiteDB.Models;
using SQLiteDB.Servicies.Interfaces;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLiteDB.Servicies.Implementations
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly AppDbContext _dbContext;

        public RabbitMQService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void ReceiveMessages()
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost/") };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var queueName = "queue_name"; 

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                SaveMessageToDatabase(message);

                Console.WriteLine($"Received from RabbitMQ ({queueName}): {message}");
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine($"Waiting for messages. Press Enter to exit.");
            Console.ReadLine();
        }

        private void SaveMessageToDatabase(string message)
        {
            try
            {
                var instrumentStatus = JsonConvert.DeserializeObject<InstrumentStatus>(
                    message,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                );

                if (instrumentStatus != null && instrumentStatus.DeviceStatus != null)
                {
                    foreach (var deviceStatus in instrumentStatus.DeviceStatus)
                    {
                        if (deviceStatus != null && deviceStatus.RapidControlStatus != null)
                        {
                            var combinedStatus = GetCombinedStatus(deviceStatus);

                            var existingRecord = _dbContext.Message.FirstOrDefault(m => m.ModuleCategoryID == deviceStatus.ModuleCategoryID);

                            if (existingRecord != null)
                            {
                                existingRecord.ModuleState = combinedStatus?.ModuleState;
                            }
                            else
                            {
                                var messageModel = new MessageModel
                                {
                                    Message = message,
                                    ModuleCategoryID = deviceStatus.ModuleCategoryID,
                                    ModuleState = combinedStatus?.ModuleState
                                };

                                _dbContext.Message.Add(messageModel);
                            }
                        }
                    }

                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing and saving message: {ex.Message}");
            }
        }

        private dynamic GetCombinedStatus(DeviceStatus deviceStatus)
        {
            switch (deviceStatus.ModuleCategoryID)
            {
                case "SAMPLER":
                    return deviceStatus.RapidControlStatus.CombinedSamplerStatus;
                case "QUATPUMP":
                    return deviceStatus.RapidControlStatus.CombinedPumpStatus;
                case "COLCOMP":
                    return deviceStatus.RapidControlStatus.CombinedOvenStatus;
                default:
                    return null;
            }
        }



    }
}

