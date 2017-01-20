using System;
using JamCast.Models;
using JamCast.Windows;

namespace JamCast.Services
{
    public class ProjectorRole : IProjectorRole
    {
        private readonly IJamHostApiService _jamHostApiService;

        private DateTime _nextUpdate = DateTime.MinValue;
        private StreamInfo _streamInfo;
        private Projector _projector;

        public ProjectorRole(IJamHostApiService jamHostApiService)
        {
            _jamHostApiService = jamHostApiService;

            Status = "Projector";
        }

        public void Update()
        {
            if (DateTime.UtcNow > _nextUpdate)
            {
                var streamInfo = _jamHostApiService.ProjectorPing();

                if (streamInfo != null)
                {
                    _streamInfo = streamInfo;
                }

                _nextUpdate = DateTime.UtcNow.AddMinutes(1);
            }

            if (_streamInfo != null)
            {
                Status = "Streaming From: " + (_streamInfo.ActiveClientFullName ?? "<No Client>");

                if (_projector == null || _projector.IsDisposed)
                {
                    _projector = new Projector();
                }

                _projector.SetRtmpsUrl(_streamInfo.RtmpsUrl);

                if (!_projector.Visible)
                {
                    _projector.Show();
                }
            }
            else
            {
                Status = "Waiting for Streaming Information";
            }
        }

        public void End()
        {
            _projector?.Close();
            _projector = null;
        }

        public string Status { get; set; }
    }'

}