using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Serilog;
using FileParser.Servicies.Interfaces;

namespace FileParser.Implementations
{
    public class Parser : IParser
    {
        private static readonly Random random = new Random();
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly ILogger logger;

        public Parser(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<XmlDocument> LoadXmlDocumentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var xmlDoc = new XmlDocument();
                await Task.Run(() =>
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        xmlDoc.Load(fileStream);
                    }
                }, cancellationToken);

                return xmlDoc;
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading XML document: {ex.Message}");
                throw; 
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<JObject> GetInstrumentStatusAsync(XmlDocument xmlDoc, CancellationToken cancellationToken = default)
        {
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
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
            catch (Exception ex)
            {
                logger.Error($"Error processing instrument status: {ex.Message}");
                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
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
                string[] possibleStates = { "Online", "Run", "NotReady", "Offline" };
                string currentModuleState = moduleStateToken.Value<string>();
                string newModuleState = GetRandomModuleState(possibleStates, currentModuleState);

                moduleStateToken.Replace(newModuleState);
            }
            else
            {
                logger.Warning($"Token '{moduleStatePath}' not found in the RapidControlStatus JObject.");
            }
        }

        private string GetRandomModuleState(string[] possibleStates, string currentModuleState)
        {
            List<string> availableStates = new List<string>(possibleStates);
            string newModuleState = availableStates[random.Next(availableStates.Count)];
            return newModuleState;
        }
    }
}
