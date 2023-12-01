namespace SQLiteDB.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? ModuleCategoryID { get; set; }
        public string? ModuleState { get; set; }
    }

    public class CombinedSamplerStatus
    {
        public string ModuleState { get; set; }
    }

    public class CombinedPumpStatus
    {
        public string ModuleState { get; set; }
    }

    public class CombinedOvenStatus
    {
        public string ModuleState { get; set; }
    }

    public class RapidControlStatus
    {
        public string ModuleState { get; set; }
        public CombinedSamplerStatus CombinedSamplerStatus { get; set; }
        public CombinedPumpStatus CombinedPumpStatus { get; set; }
        public CombinedOvenStatus CombinedOvenStatus { get; set; }
    }


    public class DeviceStatus
    {
        public string ModuleCategoryID { get; set; }
        public int IndexWithinRole { get; set; }
        public RapidControlStatus RapidControlStatus { get; set; }
    }

    public class InstrumentStatus
    {
        public string PackageID { get; set; }
        public List<DeviceStatus> DeviceStatus { get; set; }
    }
}
