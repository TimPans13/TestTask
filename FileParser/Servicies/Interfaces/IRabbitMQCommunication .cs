using System;

namespace FileParser.Servicies.Interfaces
{
    public interface IRabbitMQCommunication
    {
        Task SendDataAsync(string jsonData);
    }
}
