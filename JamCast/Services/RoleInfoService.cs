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

            _cachedRoleInfo = _jamHostApiService.GetSessionRole();
            _utcCacheExpiry = DateTime.UtcNow.AddMinutes(5);
            return _cachedRoleInfo;
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
