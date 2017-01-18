using JamCast.Windows;

namespace JamCast.Services
{
    public class ProjectorRole : IProjectorRole
    {
        private readonly IJamHostApiService _jamHostApiService;

        private string _rtmpsUrl;
        private Projector _projector;

        public ProjectorRole(IJamHostApiService jamHostApiService)
        {
            _jamHostApiService = jamHostApiService;
        }

        public void Update()
        {
            if (_rtmpsUrl == null)
            {
                _rtmpsUrl = _jamHostApiService.ProjectorStreamingInfo().RtmpsUrl;
            }

            if (_projector == null || _projector.IsDisposed)
            {
                _projector = new Projector(_rtmpsUrl);
            }

            if (!_projector.Visible)
            {
                _projector.Show();
            }
        }

        public void End()
        {
            _projector.Close();
            _projector = null;
        }
    }
}