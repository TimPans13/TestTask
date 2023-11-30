using Newtonsoft.Json.Linq;
using System.Xml;
using System;

namespace FileParser.Servicies.Interfaces
{
    public interface IParser
    {
        XmlDocument LoadXmlDocument(string filePath);
        JObject GetInstrumentStatus(XmlDocument xmlDoc);
    }
}
