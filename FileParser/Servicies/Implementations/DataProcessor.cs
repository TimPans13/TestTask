using FileParser.Servicies.Interfaces;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FileParser.Implementations
{
    public class DataProcessor : IDataProcessor
    {
        private readonly IRabbitMQCommunication rabbitMQCommunication;
        private readonly IParser fileParser;
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ILogger logger;

        public DataProcessor(IRabbitMQCommunication rabbitMQCommunication, IParser fileParser, ILogger logger)
        {
            this.rabbitMQCommunication = rabbitMQCommunication ?? throw new ArgumentNullException(nameof(rabbitMQCommunication));
            this.fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessFile(string filePath, CancellationToken cancellationToken = default)
        {
            await ProcessFileAsync(filePath, cancellationToken);
        }

        private async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                await semaphoreSlim.WaitAsync();

                var xmlDoc = await fileParser.LoadXmlDocumentAsync(filePath, cancellationToken);
                JObject jsonData = await fileParser.GetInstrumentStatusAsync(xmlDoc, cancellationToken);
                await rabbitMQCommunication.SendDataAsync(jsonData.ToString());

                logger.Information($"File processed successfully: {filePath}");
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing file {filePath}: {ex.Message}");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

    }
}