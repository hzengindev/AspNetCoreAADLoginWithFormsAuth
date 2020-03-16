using Newtonsoft.Json;
using System;

namespace CustomLogin.Models.Auth
{
    public class MeModel
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("givenName")]
        public string GivenName { get; set; }
        [JsonProperty("surname")]
        public string Surname { get; set; }
        [JsonProperty("mail")]
        public string Mail { get; set; }
        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("error")]
        public AADError Error { get; set; }
    }
}