using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using j64.Harmony.Web.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using j64.Harmony.Web.Repository;
using j64.Harmony.Web.ViewModels.Configure;
using j64.Harmony.Xmpp;
using System.Threading.Tasks;

namespace j64.Harmony.Web.Controllers
{
    public class OAuthController : Controller
    {
        j64HarmonyGateway j64Config;
        IHostingEnvironment myEnv;
        Hub myHub;

        public OAuthController(j64HarmonyGateway j64Config, Hub hub, IHostingEnvironment env)
        {
            this.j64Config = j64Config;
            this.myEnv = env;
            this.myHub = hub;
        }

        public IActionResult Index()
        {
            if (myHub.hubConfig == null || String.IsNullOrEmpty(j64Config.ChannelDevice) || String.IsNullOrEmpty(j64Config.VolumeDevice))
                return RedirectToAction("Edit", "FirstTimeConfig");

            var oauth = OauthRepository.Get();
            var ovm = new OauthViewModel()
            {
                ClientKey = oauth.clientKey,
                SecretKey = oauth.secretKey
            };

            return View(ovm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSmartApp(OauthViewModel authInfo)
        {
            if (ModelState.IsValid == false)
                return View(authInfo);

            var conn = new SmartThingsConnection();
            if (await conn.Login(authInfo.STUserId, authInfo.STPassword) == false)
            {
                ModelState.AddModelError("STPassword", "Could not connect to smart things using the supplied credentials");
                return View("Index", authInfo);
            }

            var cs = new DeviceTypeRepository(conn, "j64 Channel Switch", $"{myEnv.WebRootPath}/../SmartThingApps/j64ChannelSwitchDevice.groovy");
            var ss = new DeviceTypeRepository(conn, "j64 Surfing Switch", $"{myEnv.WebRootPath}/../SmartThingApps/j64SurfingSwitchDevice.groovy");
            var vs = new DeviceTypeRepository(conn, "j64 VCR Switch", $"{myEnv.WebRootPath}/../SmartThingApps/j64VcrSwitchDevice.groovy");
            var os = new DeviceTypeRepository(conn, "j64 Volume Switch", $"{myEnv.WebRootPath}/../SmartThingApps/j64VolumeSwitchDevice.groovy");
            var cc = new DeviceTypeRepository(conn, "j64 Custom Command Switch", $"{myEnv.WebRootPath}/../SmartThingApps/j64CustomCommandSwitchDevice.groovy");

            var har = new SmartAppRepository(conn, "j64 Harmony", $"{myEnv.WebRootPath}/../SmartThingApps/j64HarmonySmartApp.groovy");

            // Save the client/secret keys
            var oauth = OauthRepository.Get();
            oauth.clientKey = har.clientKey;
            oauth.secretKey = har.secretKey;
            OauthRepository.Save(oauth);

            var ovm = new OauthViewModel()
            {
                ClientKey = oauth.clientKey,
                SecretKey = oauth.secretKey
            };

            return View("Index", ovm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BeginAuth([Bind("clientKey", "secretKey")] OauthInfo authInfo)
        {
            string authorizedUrl = "http://" + this.Request.Host.Value + this.Url.Content("~/OAuth/Authorized");

            // Reset the token info
            authInfo.accessToken = null;
            authInfo.tokenType = null;
            authInfo.expiresInSeconds = 0;

            OauthRepository.Save(authInfo);

            string Url = $"https://graph.api.smartthings.com/oauth/authorize?response_type=code&scope=app&redirect_uri={authorizedUrl}&client_id={authInfo.clientKey}";

            return Redirect(Url);
        }

        public IActionResult Authorized(string code)
        {
            OauthInfo oai = OauthRepository.Get();

            if (code == null)
                return View(oai);

            oai.authCode = code;

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            var url = "https://graph.api.smartthings.com/oauth/token";

            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            parms.Add(new KeyValuePair<string, string>("code", oai.authCode));
            parms.Add(new KeyValuePair<string, string>("client_id", oai.clientKey));
            parms.Add(new KeyValuePair<string, string>("client_secret", oai.secretKey));
            string authorizedUrl = "http://" + this.Request.Host.Value + this.Url.Content("~/OAuth/Authorized");
            parms.Add(new KeyValuePair<string, string>("redirect_uri", authorizedUrl));

            var content = new System.Net.Http.FormUrlEncodedContent(parms);
            var response = client.PostAsync(url, content);
            response.Wait();

            if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ViewData.Add("GetTokenError", "Get Auth Code Error: " + response.Result.StatusCode.ToString());
                return View(oai);
            }

            // Save the interim result
            var val = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Result.Content.ReadAsStringAsync().Result);
            oai.accessToken = val["access_token"];
            oai.expiresInSeconds = Convert.ToInt32(val["expires_in"]);
            oai.tokenType = val["token_type"];
            OauthRepository.Save(oai);

            // Get the endpoint info
            client = new System.Net.Http.HttpClient();
            url = "https://graph.api.smartthings.com/api/smartapps/endpoints";

            System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            msg.Headers.Add("Authorization", $"Bearer {oai.accessToken}");

            response = client.SendAsync(msg);
            response.Wait();

            if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ViewData.Add("GetTokenError", "Get EndPoints Error: " + response.Result.StatusCode.ToString());
                return View(oai);
            }

            string jsonString = response.Result.Content.ReadAsStringAsync().Result;
            oai.endpoints = JsonConvert.DeserializeObject<List<OauthEndpoint>>(jsonString);

            OauthRepository.Save(oai);

            // Prepare the smart app to be called from the local traffic
            SmartThingsRepository.PrepTheInstall(j64Config);

            // Send all of the default devices to the smart app
            SmartThingsRepository.InstallDevices(j64Config, Request.Host.Value);

            // all done!
            return View(oai);

        }
    }
}
