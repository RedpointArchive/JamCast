using System;
using JamCast.Models;

namespace JamCast.Services
{
    public interface IRoleInfoService
    {
        RoleInfo GetRoleInfo();
        void SwitchRole();
    }

    public class RoleInfoService : IRoleInfoService
    {
        private readonly IJamHostApiService _jamHostApiService;
        private RoleInfo _cachedRoleInfo;
        private DateTime _utcCacheExpiry = DateTime.MinValue;

        public RoleInfoService(IJamHostApiService jamHostApiService)
        {
            _jamHostApiService = jamHostApiService;
        }

        public RoleInfo GetRoleInfo()
        {
            if (_utcCacheExpiry > DateTime.UtcNow)
            {
                return _cachedRoleInfo;
            }
            try
            {
                var newRole = _jamHostApiService.GetSessionRole();
                if (newRole != null)
                {
                    _cachedRoleInfo = newRole.Value;
                    _utcCacheExpiry = DateTime.UtcNow.AddMinutes(5);
                }
            }
            catch (System.Net.WebException) {
                _utcCacheExpiry = DateTime.UtcNow.AddMinutes(1);
            }

            if (_utcCacheExpiry != DateTime.MinValue)
            {
                return _cachedRoleInfo;
            }
            else
            {
                return RoleInfo.Client;
            }
        }

        public void SwitchRole()
        {
            var newRole = GetRoleInfo() == RoleInfo.Client ? "projector" : "client";
            _jamHostApiService.SetSessionRole(newRole);
            _utcCacheExpiry = DateTime.MinValue;
            GetRoleInfo();
        }
    }
}
