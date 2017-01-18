using Newtonsoft.Json;

namespace JamCast.Models
{
    public class SiteInfo
    {
        [JsonProperty("sitename")]
        public string SiteName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image_cover")]
        public string ImageCover { get; set; }

        [JsonProperty("image_favicon")]
        public string ImageFavicon { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}