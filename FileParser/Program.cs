using FileParser.Implementations;

class Program
{
    static void Main()
    {
        string directoryPath = "";/////your pass
        string rabbitMQConnectionString = "amqp://guest:guest@localhost/";
        string exchangeName = "exchange_name";
        string routingKey = "routing_key";

        Console.WriteLine($"Directory Path: {directoryPath}");
        Console.WriteLine($"RabbitMQ Connection String: {rabbitMQConnectionString}");
        Console.WriteLine($"Exchange Name: {exchangeName}");
        Console.WriteLine($"Routing Key: {routingKey}");

        var fileParser = new Parser();
        var rabbitMQCommunication = new RabbitMQCommunication(rabbitMQConnectionString, exchangeName, routingKey);
        var dataProcessor = new DataProcessor(rabbitMQCommunication, fileParser);
        while (true)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath, "*.xml"))
            {
                try
                {
                    dataProcessor.ProcessFile(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
            }
        }

    }
}