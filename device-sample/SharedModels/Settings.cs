namespace Azure.IoT.SharedModels
{
    public class Settings
    {
        public string DeviceId { get; set; }
        public string AccessPolicyName { get; set; }
        public string AccessKey { get; set; }
        public string HubName { get; set; }
        public string? DeviceCertificateFilePath { get; set; }
        public string? DpsIdScope { get; set; }
        public SimulationConfig SimulationConfig { get; set; }
    }
}
