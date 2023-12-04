using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataProcessor.Data;
using DataProcessor.Models;
using DataProcessor.Servicies.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Serilog;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Servicies.Implementations
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger logger;
        private readonly string rabbitMQConnectionString;
        private readonly string queueName;

        public RabbitMQService(AppDbContext dbContext, string queueName, string rabbitMQConnectionString, ILogger logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.rabbitMQConnectionString = rabbitMQConnectionString ?? throw new ArgumentNullException(nameof(rabbitMQConnectionString));
            this.queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        public async Task StartReceivingMessagesAsync()
        {
            await Task.Run(() => ReceiveMessagesAsync());
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString) };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                try
                {
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error declaring queue ({queueName}): {ex}");
                }

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    await SaveMessageToDatabaseAsync(message);
                    logger.Information($"Received from RabbitMQ ({queueName}): {message}");
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                logger.Information($"Waiting for messages. Press Enter to exit.");
                await Task.Run(() => Console.ReadLine());
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred: {ex}");
            }
        }

        private async Task SaveMessageToDatabaseAsync(string message)
        {
            try
            {
                var container = JsonConvert.DeserializeObject<InstrumentStatusContainer>(message);
                if (container != null && container.InstrumentStatus != null && container.InstrumentStatus.DeviceStatus != null)
                {
                    foreach (var deviceStatus in container.InstrumentStatus.DeviceStatus)
                    {
                        if (deviceStatus != null && deviceStatus.RapidControlStatus != null)
                        {
                            var combinedStatus = await GetCombinedStatusAsync(deviceStatus);

                            var existingRecord = await dbContext.Message.FirstOrDefaultAsync(m => m.ModuleCategoryID == deviceStatus.ModuleCategoryID);

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

                                dbContext.Message.Add(messageModel);
                            }
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    logger.Information("Message saved to the database successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing and saving message: {ex.Message}");
            }
        }

        private async Task<dynamic> GetCombinedStatusAsync(DeviceStatus deviceStatus)
        {
            switch (deviceStatus.ModuleCategoryID)
            {
                case "SAMPLER":
                    return await Task.FromResult(deviceStatus.RapidControlStatus.CombinedSamplerStatus);
                case "QUATPUMP":
                    return await Task.FromResult(deviceStatus.RapidControlStatus.CombinedPumpStatus);
                case "COLCOMP":
                    return await Task.FromResult(deviceStatus.RapidControlStatus.CombinedOvenStatus);
                default:
                    return null;
            }
        }
    }
}