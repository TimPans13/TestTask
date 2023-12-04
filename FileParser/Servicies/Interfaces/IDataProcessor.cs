using System;

namespace FileParser.Servicies.Interfaces
{
    public interface IDataProcessor
    {
        Task ProcessFile(string filePath, CancellationToken cancellationToken = default);
    }
}
