using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using FileParser.Implementations;
using Serilog;
using Serilog.Sinks.File;
using Serilog.Core;

class Program
{
    static async Task Main()
    {
        string directoryPath = "D:\\sas"; /////your pass
        string rabbitMQConnectionString = "amqp://guest:guest@localhost/";
        string exchangeName = "exchange_name";
        string routingKey = "routing_key";

        Console.WriteLine($"Directory Path: {directoryPath}");
        Console.WriteLine($"RabbitMQ Connection String: {rabbitMQConnectionString}");
        Console.WriteLine($"Exchange Name: {exchangeName}");
        Console.WriteLine($"Routing Key: {routingKey}");

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var fileParser = new Parser(logger);
        var rabbitMQCommunication = new RabbitMQCommunication(rabbitMQConnectionString, exchangeName, routingKey, logger);
        var dataProcessor = new DataProcessor(rabbitMQCommunication, fileParser, logger);

        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            // Обработка события Ctrl+C для завершения программы
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath, "*.xml"))
            {
                try
                {
                    await Task.Run(() => dataProcessor.ProcessFile(filePath), cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error processing file {filePath}: {ex.Message}");
                }
                await Task.Delay(1000);
            }
        }
    }
}

