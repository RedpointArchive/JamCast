namespace JamCast.Services
{
    public interface IUserInfoService
    {
        bool Authenticated { get; set; }
        string FullName { get; set; }
        string Email { get; set; }
    }

    public class UserInfoService : IUserInfoService
    {
        public bool Authenticated { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
