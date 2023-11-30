using FileParser.Servicies.Interfaces;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace FileParser.Implementations
{
    public class Parser : IParser
    {
        private static readonly Random random = new Random();

        public XmlDocument LoadXmlDocument(string filePath)
        {
            var xmlDoc = new XmlDocument();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                xmlDoc.Load(fileStream);
            }
            return xmlDoc;
        }

        public JObject GetInstrumentStatus(XmlDocument xmlDoc)
        {
            string packageID = xmlDoc.SelectSingleNode("/InstrumentStatus/PackageID")?.InnerText;

            JArray deviceStatusArray = new JArray();
            XmlNodeList deviceStatusNodes = xmlDoc.SelectNodes("/InstrumentStatus/DeviceStatus");
            foreach (XmlNode deviceStatusNode in deviceStatusNodes)
            {
                string moduleCategoryID = deviceStatusNode.SelectSingleNode("ModuleCategoryID")?.InnerText;
                int indexWithinRole = Convert.ToInt32(deviceStatusNode.SelectSingleNode("IndexWithinRole")?.InnerText);
                string rapidControlStatusXml = deviceStatusNode.SelectSingleNode("RapidControlStatus")?.InnerText;

                JObject rapidControlStatus = ParseRapidControlStatus(rapidControlStatusXml);

                ModifyModuleStateInRapidControlStatus(rapidControlStatus);

                JObject deviceStatus = new JObject
                {
                    { "ModuleCategoryID", moduleCategoryID },
                    { "IndexWithinRole", indexWithinRole },
                    { "RapidControlStatus", rapidControlStatus }
                };

                deviceStatusArray.Add(deviceStatus);
            }

            JObject instrumentStatus = new JObject
            {
                { "PackageID", packageID },
                { "DeviceStatus", deviceStatusArray }
            };

            return instrumentStatus;
        }

        private JObject ParseRapidControlStatus(string rapidControlStatusXml)
        {
            XmlDocument rapidControlXmlDoc = new XmlDocument();
            rapidControlXmlDoc.LoadXml(rapidControlStatusXml);

            JObject rapidControlStatus = JObject.FromObject(rapidControlXmlDoc);

            return rapidControlStatus;
        }

        private void ModifyModuleStateInRapidControlStatus(JObject rapidControlStatus)
        {
            ModifyModuleState(rapidControlStatus, "CombinedSamplerStatus.ModuleState");
            ModifyModuleState(rapidControlStatus, "CombinedPumpStatus.ModuleState");
            ModifyModuleState(rapidControlStatus, "CombinedOvenStatus.ModuleState");
        }

        private void ModifyModuleState(JObject rapidControlStatus, string moduleStatePath)
        {
            JToken moduleStateToken = rapidControlStatus.SelectToken(moduleStatePath);

            if (moduleStateToken != null)
            {
                string[] possibleStates = { "Online", "Run", "NotReady", "Offline", "Online!" };
                string currentModuleState = moduleStateToken.Value<string>();
                string newModuleState = GetRandomModuleState(possibleStates, currentModuleState);

                moduleStateToken.Replace(newModuleState);
            }
            else
            {
                Console.WriteLine($"Token '{moduleStatePath}' not found in the RapidControlStatus JObject.");
            }
        }

        private string GetRandomModuleState(string[] possibleStates, string currentModuleState)
        {
            List<string> availableStates = possibleStates.ToList();
            string newModuleState = availableStates[random.Next(availableStates.Count)];
            return newModuleState;
        }
    }
}
