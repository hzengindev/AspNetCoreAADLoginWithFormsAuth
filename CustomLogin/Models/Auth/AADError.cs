using Newtonsoft.Json;

namespace CustomLogin.Models.Auth
{
    public class AADError
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}