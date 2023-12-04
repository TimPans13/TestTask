namespace DataProcessor.Servicies.Interfaces
{
    public interface IRabbitMQService
    {
        Task StartReceivingMessagesAsync();
    }
}
