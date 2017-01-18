using System.Drawing;
using System.IO;
using System.Net;

namespace JamCast.Services
{
    public interface IImageService
    {
        byte[] GetImageCoverBytes();

        byte[] GetImageFaviconBytes();
    }

    public class ImageService : IImageService
    {
        private readonly SiteInfoService _siteInfoService;
        private byte[] _imageCoverBytes;
        private byte[] _imageFaviconBytes;

        public ImageService(SiteInfoService siteInfoService)
        {
            _siteInfoService = siteInfoService;
        }

        public byte[] GetImageCoverBytes()
        {
            if (_imageCoverBytes != null)
            {
                return _imageCoverBytes;
            }

            var siteInfo = _siteInfoService.GetSiteInfo();

            using (var client = new WebClient())
            {
                _imageCoverBytes = client.DownloadData(siteInfo.ImageCover);
            }

            return _imageCoverBytes;
        }

        public byte[] GetImageFaviconBytes()
        {
            if (_imageFaviconBytes != null)
            {
                return _imageFaviconBytes;
            }
            
            var siteInfo = _siteInfoService.GetSiteInfo();

            using (var client = new WebClient())
            {
                _imageFaviconBytes = client.DownloadData(siteInfo.ImageFavicon);
            }

            return _imageFaviconBytes;
        }
    }
}
