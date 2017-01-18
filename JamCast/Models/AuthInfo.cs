namespace Client
{
    public class AuthInfo
    {
        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public bool IsValid { get; set; }

        public string Error { get; set; }

        public string SessionId { get; set; }

        public string SecretKey { get; set; }

        public string AccountType { get; set; }
    }
}
