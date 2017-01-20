namespace JamCast.Models
{
    public class StreamInfo
    {
        public string RtmpUrl { get; set; }
        public string RtmpsUrl { get; set; }
        public bool ShouldStream { get; set; }
        public string ActiveClientId { get; set; }
        public string ActiveClientFullName { get; set; }
        public string ActiveClientScreenshotHash { get; set; }
        public OperationMode OperationMode { get; set; }
    }

    public enum OperationMode
    {
        Rtmp,

        Jpeg
    }
}
