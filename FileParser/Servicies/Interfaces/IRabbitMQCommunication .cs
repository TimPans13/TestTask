using System;

namespace FileParser.Servicies.Interfaces
{
    public interface IRabbitMQCommunication
    {
        void SendData(string jsonData);
    }
}
