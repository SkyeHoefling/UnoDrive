namespace UnoDrive.Models
{
    public interface IAuthenticationResult
    {
        string AccessToken { get; }
        bool IsSuccess { get; }
        string Message { get; }
        string ObjectId { get; }
    }

    public class AuthenticationResult : IAuthenticationResult
    {
        public string AccessToken { get; set; }
        public virtual bool IsSuccess { get => !string.IsNullOrEmpty(AccessToken); }
        public string Message { get; set; }
        public string ObjectId { get; set; }
    }

    public class OfflineAuthenticationResult : IAuthenticationResult
    {
        public string AccessToken { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ObjectId { get; set; }
    }
}
