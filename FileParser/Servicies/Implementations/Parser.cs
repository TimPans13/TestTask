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
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
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
                string packageID = xmlDoc.SelectSingleNode("/InstrumentStatus/PackageID")?.InnerText ?? throw new ArgumentNullException();

                JArray deviceStatusArray = new JArray();
                XmlNodeList deviceStatusNodes = xmlDoc.SelectNodes("/InstrumentStatus/DeviceStatus") ?? throw new ArgumentNullException(); ;
                foreach (XmlNode deviceStatusNode in deviceStatusNodes)
                {
                    string moduleCategoryID = deviceStatusNode.SelectSingleNode("ModuleCategoryID")?.InnerText ?? throw new ArgumentNullException(); ;
                    int indexWithinRole = Convert.ToInt32(deviceStatusNode.SelectSingleNode("IndexWithinRole")?.InnerText);

                    XmlNode rapidControlStatusNode = deviceStatusNode.SelectSingleNode("RapidControlStatus") ?? throw new ArgumentNullException(); ;

                    JObject rapidControlStatus = ParseRapidControlStatus(rapidControlStatusNode, moduleCategoryID);

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
            { "InstrumentStatus", new JObject
                {
                    { "PackageID", packageID },
                    { "DeviceStatus", deviceStatusArray }
                }
            }
        };

                return instrumentStatus;
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing tool status: {ex.Message}");
                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private JObject ParseRapidControlStatus(XmlNode rapidControlStatusNode, string moduleType)
        {
            try
            {
                string rapidControlStatusXml = rapidControlStatusNode.InnerXml;
                string decodedXml = System.Net.WebUtility.HtmlDecode(rapidControlStatusXml);
                XmlDocument rapidControlXmlDoc = new XmlDocument();
                rapidControlXmlDoc.LoadXml(decodedXml);

                JObject rapidControlStatus = JObject.FromObject(rapidControlXmlDoc.DocumentElement) ?? throw new ArgumentNullException(); ;

                RemoveAttributes(rapidControlStatus, "@xmlns:xsi", "@xmlns:xsd");

                ModifyModuleStateInRapidControlStatus(rapidControlStatus, moduleType);

                return rapidControlStatus;
            }
            catch (Exception ex)
            {
                logger.Error($"Error parsing XML RapidControlStatus: {ex.Message}");
                logger.Error($"Problematic XML content: {rapidControlStatusNode.InnerXml}");
                throw;
            }
        }

        private void RemoveAttributes(JObject jObject, params string[] attributeNames)
        {
            foreach (var attributeName in attributeNames)
            {
                JToken attribute = jObject.DescendantsAndSelf().FirstOrDefault(a => a.Type == JTokenType.Property && ((JProperty)a).Name == attributeName) ?? throw new ArgumentNullException(); ;
                if (attribute != null)
                {
                    attribute.Remove();
                }
            }
        }

        private void ModifyModuleStateInRapidControlStatus(JObject rapidControlStatus, string moduleType)
        {
            try
            {
                switch (moduleType)
                {
                    case "SAMPLER":
                        ModifyModuleState(rapidControlStatus, "CombinedSamplerStatus.ModuleState");
                        break;
                    case "QUATPUMP":
                        ModifyModuleState(rapidControlStatus, "CombinedPumpStatus.ModuleState");
                        break;
                    case "COLCOMP":
                        ModifyModuleState(rapidControlStatus, "CombinedOvenStatus.ModuleState");
                        break;
                    default:
                        logger.Warning($"Unknown module type: {moduleType}. Skipping modification.");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error modifying module states in RapidControlStatus: {ex}");
            }
        }


        private void ModifyModuleState(JObject rapidControlStatus, string moduleStatePath)
        {
            try
            {
                JToken moduleStateToken = rapidControlStatus.SelectToken(moduleStatePath) ?? throw new ArgumentNullException(); ;

                if (moduleStateToken != null)
                {
                    string[] possibleStates = { "Online", "Run", "NotReady", "Offline" };
                    string currentModuleState = moduleStateToken.Value<string>() ?? throw new ArgumentNullException(); ;
                    string newModuleState = GetRandomModuleState(possibleStates, currentModuleState) ?? throw new ArgumentNullException(); ;

                    moduleStateToken.Replace(newModuleState);
                }
                else
                {
                    logger.Warning($"Token '{moduleStatePath}' not found in the RapidControlStatus JObject.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error modifying module state for token '{moduleStatePath}': {ex}");
            }
        }




        private string GetRandomModuleState(string[] possibleStates, string currentModuleState)
        {
            try
            {
                List<string> availableStates = new List<string>(possibleStates);
                string newModuleState = availableStates[random.Next(availableStates.Count)];
                return newModuleState;
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting random module state: {ex}");
                throw;
            }
        }
    }
}
