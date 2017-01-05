using Newtonsoft.Json;

namespace Jamcast
{
    public class DeploymentInfo
    {
        [JsonProperty("sitename")]
        public string Sitename { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}