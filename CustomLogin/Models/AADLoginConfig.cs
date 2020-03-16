namespace CustomLogin.Models
{
    public class AADLoginConfig
    {
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SignOutRedirectUri { get; set; }
        public string SignOutRedirectUriEncoded { get => string.IsNullOrEmpty(SignOutRedirectUri) ? null : System.Net.WebUtility.UrlEncode(this.SignOutRedirectUri); }
        public string SignInRedirectUri { get; set; }
        public string SignInRedirectUriEncoded { get => string.IsNullOrEmpty(SignInRedirectUri) ? null : System.Net.WebUtility.UrlEncode(this.SignInRedirectUri); }
    }
}