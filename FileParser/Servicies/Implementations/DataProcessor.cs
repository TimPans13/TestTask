using FileParser.Servicies.Interfaces;
using Newtonsoft.Json.Linq;

namespace FileParser.Implementations
{
    public class DataProcessor : IDataProcessor
    {
        private readonly IRabbitMQCommunication rabbitMQCommunication;
        private readonly IParser fileParser;

        public DataProcessor(IRabbitMQCommunication rabbitMQCommunication, IParser fileParser)
        {
            this.rabbitMQCommunication = rabbitMQCommunication ?? throw new ArgumentNullException(nameof(rabbitMQCommunication));
            this.fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
        }

        public void ProcessFile(string filePath)
        {
            try
            {
                var xmlDoc = fileParser.LoadXmlDocument(filePath);
                JObject jsonData = fileParser.GetInstrumentStatus(xmlDoc);
                rabbitMQCommunication.SendData(jsonData.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }
    }
}
