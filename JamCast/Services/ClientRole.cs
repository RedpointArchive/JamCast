using System;
using JamCast.Models;

namespace JamCast.Services
{
    public class ClientRole : IClientRole
    {
        private readonly IJamHostApiService _jamHostApiService;

        private DateTime _nextUpdate = DateTime.MinValue;
        private StreamInfo _streamInfo;

        public ClientRole(IJamHostApiService jamHostApiService)
        {
            _jamHostApiService = jamHostApiService;

            Status = "Waiting...";
        }

        public void Update()
        {
            if (DateTime.UtcNow > _nextUpdate)
            {
                _streamInfo = _jamHostApiService.ClientPing();
                _nextUpdate = DateTime.UtcNow.AddMinutes(1);
            }

            if (_streamInfo?.ShouldStream ?? false)
            {
                Status = "Streaming";
            }
            else
            {
                Status = "Not Streaming";
            }
        }

        public void End()
        {
            
        }

        public string Status { get; set; }
    }
}