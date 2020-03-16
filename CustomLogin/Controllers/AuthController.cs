using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using CustomLogin.Managers;
using CustomLogin.Models;
using CustomLogin.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CustomLogin.Controllers
{
    public class AuthController : Controller
    {
        IAuthManager authManager;
        AADLoginConfig aadLoginConfig;

        public AuthController(IAuthManager _authManager, AADLoginConfig _aadLoginConfig)
        {
            authManager = _authManager;
            aadLoginConfig = _aadLoginConfig;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] LoginModel value)
        {
            if (!ModelState.IsValid)
                return View(value);

            var user = authManager.SignIn(value.Username, value.Password);

            if(user == null)
            {
                ModelState.AddModelError("usernotfound", "User not found!");
                return View(value);
            }

            await CreateAuthCookie(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            var AADLoginActive = User.Claims.Any(z => z.Type == "AADLogin");
            await HttpContext.SignOutAsync();
            
            if (AADLoginActive)
                return Redirect($"https://login.microsoftonline.com/{aadLoginConfig.Tenant}/oauth2/logout?post_logout_redirect_uri={aadLoginConfig.SignOutRedirectUriEncoded}");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult SignInAD()
        {
            var state = Guid.NewGuid();
            var nonce = Guid.NewGuid();
            var loginURL = $"https://login.microsoftonline.com/{aadLoginConfig.Tenant}/oauth2/v2.0/authorize?client_id={aadLoginConfig.ClientId}&response_type=code&redirect_uri={aadLoginConfig.SignInRedirectUriEncoded}&response_mode=query&scope=openid%20user.read&state={state}&nonce={nonce}";
            return Redirect(loginURL);
        }

        [HttpGet]
        public async Task<IActionResult> AADCallback(string code, string state, string error, string error_description)
        {
            if (!string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(error_description))
                throw new Exception($"AADLogin Error: {error} => {error_description.Split('\r').FirstOrDefault()}");

            AccessTokenModel accessToken = await GetAADAccessToken(code);
            if (string.IsNullOrEmpty(accessToken.Error))
                throw new Exception($"{accessToken.Error} => {accessToken.ErrorDescription}");

            MeModel me = await GetAADMe(accessToken.AccessToken);
            if (me.Error != null)
                throw new Exception($"{me.Error.Code} => {me.Error.Message}");

            var user = authManager.SignInAAD(me.UserPrincipalName);
            if (user == null)
                throw new Exception("User not found!");

            await CreateAuthCookie(user, true);

            return RedirectToAction("Index", "Home");
        }
        
        private async Task<AccessTokenModel> GetAADAccessToken(string code)
        {
            AccessTokenModel accessToken = null;

            var _params = new Dictionary<string, string>();
            _params.Add("scope", "openid");
            _params.Add("grant_type", "authorization_code");
            _params.Add("code", code);
            _params.Add("client_id", aadLoginConfig.ClientId);
            _params.Add("client_secret", aadLoginConfig.ClientSecret);
            _params.Add("redirect_uri", aadLoginConfig.SignInRedirectUri);

            using (var client = new HttpClient())
            {
                var tokenURL = $"https://login.microsoftonline.com/{aadLoginConfig.Tenant}/oauth2/v2.0/token";
                var tokenResponse = await client.PostAsync(tokenURL, new FormUrlEncodedContent(_params));
                accessToken = JsonConvert.DeserializeObject<AccessTokenModel>(await tokenResponse.Content.ReadAsStringAsync());
            }
            return accessToken;
        }

        private async Task<MeModel> GetAADMe(string accessToken)
        {
            MeModel me = null;
            using (var client = new HttpClient())
            {
                var meURL = $"https://graph.microsoft.com/v1.0/me";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var meResponse = await client.GetAsync(meURL);
                me = JsonConvert.DeserializeObject<MeModel>(await meResponse.Content.ReadAsStringAsync());
            }
            return me;
        }

        private async Task CreateAuthCookie(User user, bool isAADLogin = false)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Fullname),
                new Claim(ClaimTypes.Email, user.Email)
            };
            if (isAADLogin)
                claims.Add(new Claim("AADLogin", true.ToString()));

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationProperties authenticationProperties = new AuthenticationProperties() { IsPersistent = false };

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
        }
    }
}