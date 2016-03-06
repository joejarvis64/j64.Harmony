using j64.Harmony.WebApi.Controllers;
using j64.Harmony.WebApi.ViewModels.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.Harmony.WebApi.Models
{
    public class SmartThingsRepository
    {
        /// <summary>
        /// Install or Update Devices in the SmartThings App
        /// </summary>
        public static void PrepTheInstall(HarmonyHubConfiguration hubConfig)
        {
            try
            {
                OauthInfo authInfo = OauthRepository.Get();
                if (authInfo == null | authInfo.endpoints == null || authInfo.endpoints.Count == 0)
                    return;

                // create a new identifier for this app!
                hubConfig.j64AppId = Guid.NewGuid().ToString();

                string url = authInfo.endpoints[0].uri + $"/prepInstall";

                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                msg.Headers.Add("Authorization", $"Bearer {authInfo.accessToken}");

                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("j64AppId", hubConfig.j64AppId));
                parms.Add(new KeyValuePair<string, string>("j64UserName", "admin"));
                parms.Add(new KeyValuePair<string, string>("j64Password", "Admin_01"));
                msg.Content = new System.Net.Http.FormUrlEncodedContent(parms);
                var response = client.SendAsync(msg);
                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    // Get the address of the local smart things hub
                    var result = response.Result.Content.ReadAsStringAsync().Result;
                    JObject ipInfo = JObject.Parse(result);

                    hubConfig.STHubAddress = (string)ipInfo["hubIP"];
                    hubConfig.STHubPort = Convert.ToInt32((string)ipInfo["hubPort"]);

                    if (hubConfig.STHubAddress == "null")
                    {
                        hubConfig.STHubAddress = null;
                        hubConfig.STHubPort = 0;
                    }
                    HarmonyHubConfiguration.Save(hubConfig);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Install or Update Devices in the SmartThings App
        /// </summary>
        public static void InstallDevices(HarmonyHubConfiguration hubConfig)
        {
            OauthInfo authInfo = OauthRepository.Get();

            // We can't sync if the IP has not yet been set
            if (String.IsNullOrEmpty(hubConfig.STHubAddress))
                return;

            var url = $"http://{hubConfig.STHubAddress}:{hubConfig.STHubPort}";
            var client = new System.Net.Http.HttpClient();

            System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);

            List<j64Device> l = new List<j64Device>();

            l.Add(new j64Device() { Name = hubConfig.SoundDeviceName, DeviceValue = null, DeviceType = j64DeviceType.Volume });
            l.Add(new j64Device() { Name = hubConfig.ChannelSurfDeviceName, DeviceValue = null, DeviceType = j64DeviceType.Surfing });
            l.Add(new j64Device() { Name = hubConfig.VcrPauseDeviceName, DeviceValue = "pause", DeviceType = j64DeviceType.VCR });

            foreach (var fc in hubConfig.FavoriteChannels)
                l.Add(new j64Device() { Name = fc.Name, DeviceValue = fc.Channel, DeviceType = j64DeviceType.Channel });
            l.Add(new j64Device() { Name = hubConfig.LastChannelDeviceName, DeviceValue = "previous", DeviceType = j64DeviceType.Channel });

            var request = new MyRequest<List<j64Device>>()
            {
                j64Ip = hubConfig.j64Address,
                j64Port = hubConfig.j64Port,
                j64AppId = hubConfig.j64AppId,
                Route = "/installAllDevices",
                Payload = l
            };

            string json = JsonConvert.SerializeObject(request, Formatting.None);
            msg.Content = new System.Net.Http.StringContent(json);
            msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = client.SendAsync(msg);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    var result = response.Result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                // TODO: display an appropriate error message!
            }

        }
    }

    public class MyRequest<T>
    {
        public string j64AppId { get; set; }
        public string j64Ip { get; set; }
        public int j64Port { get; set; }
        public string Route { get; set; }
        public T Payload { get; set; }
    }
}
