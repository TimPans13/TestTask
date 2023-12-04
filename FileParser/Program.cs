using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileParser.Implementations;

class Program
{
    static async Task Main()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        string directoryPath = configuration["DirectoryPath"] ?? throw new ArgumentNullException(nameof(directoryPath));
        string rabbitMQConnectionString = configuration["RabbitMQ:ConnectionString"] ?? throw new ArgumentNullException(nameof(rabbitMQConnectionString));
        string exchangeName = configuration["RabbitMQ:ExchangeName"] ?? throw new ArgumentNullException(nameof(exchangeName));
        string routingKey = configuration["RabbitMQ:RoutingKey"] ?? throw new ArgumentNullException(nameof(routingKey));
        string queueName = configuration["RabbitMQ:QueueName"] ?? throw new ArgumentNullException(nameof(queueName));

        Console.WriteLine($"Directory Path: {directoryPath}");
        Console.WriteLine($"RabbitMQ Connection String: {rabbitMQConnectionString}");
        Console.WriteLine($"Exchange Name: {exchangeName}");
        Console.WriteLine($"Routing Key: {routingKey}");
        Console.WriteLine($"Queue Name: {queueName}");

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var fileParser = new Parser(logger);
        var rabbitMQCommunication = new RabbitMQCommunication(rabbitMQConnectionString, exchangeName, routingKey, queueName, logger);
        var dataProcessor = new DataProcessor(rabbitMQCommunication, fileParser, logger);

        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath, "*.xml"))
            {
                try
                {
                    await dataProcessor.ProcessFile(filePath, cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error processing file {filePath}: {ex.Message}");
                }
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }
    }
}
