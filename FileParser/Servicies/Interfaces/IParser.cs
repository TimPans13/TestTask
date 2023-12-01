using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace FileParser.Servicies.Interfaces
{
    public interface IParser
    {
        Task<XmlDocument> LoadXmlDocumentAsync(string filePath, CancellationToken cancellationToken = default);
        Task<JObject> GetInstrumentStatusAsync(XmlDocument xmlDoc, CancellationToken cancellationToken = default);
    }
}
