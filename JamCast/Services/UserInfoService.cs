namespace JamCast.Services
{
    public interface IUserInfoService
    {
        string FullName { get; set; }
        string Email { get; set; }
    }

    public class UserInfoService : IUserInfoService
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
