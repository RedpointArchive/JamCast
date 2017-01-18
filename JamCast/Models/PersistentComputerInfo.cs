using Newtonsoft.Json;

namespace JamCast.Models
{
    public class PersistentComputerInfo
    {
        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("secretKey")]
        public string SecretKey { get; set; }
    }
}
