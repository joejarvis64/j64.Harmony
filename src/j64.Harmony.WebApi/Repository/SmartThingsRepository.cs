using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using j64.Harmony.WebApi.Models;

namespace j64.Harmony.WebApi.Repository
{
    public class SmartThingsRepository
    {
        /// <summary>
        /// Install or Update Devices in the SmartThings App
        /// </summary>
        public static void PrepTheInstall(j64HarmonyGateway j64Config)
        {
            try
            {
                OauthInfo authInfo = OauthRepository.Get();
                if (authInfo == null | authInfo.endpoints == null || authInfo.endpoints.Count == 0)
                    return;

                // create a new identifier for this app!
                j64Config.j64AppId = Guid.NewGuid().ToString();

                string url = authInfo.endpoints[0].uri + $"/prepInstall";

                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                msg.Headers.Add("Authorization", $"Bearer {authInfo.accessToken}");

                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("j64AppId", j64Config.j64AppId));
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

                    j64Config.STHubAddress = (string)ipInfo["hubIP"];
                    j64Config.STHubPort = Convert.ToInt32((string)ipInfo["hubPort"]);

                    if (j64Config.STHubAddress == "null")
                    {
                        j64Config.STHubAddress = null;
                        j64Config.STHubPort = 0;
                    }
                    j64HarmonyGatewayRepository.Save(j64Config);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Install or Update Devices in the SmartThings App
        /// </summary>
        public static void InstallDevices(j64HarmonyGateway j64Config, string host)
        {
            OauthInfo authInfo = OauthRepository.Get();

            // We can't sync if the IP has not yet been set
            if (String.IsNullOrEmpty(j64Config.STHubAddress) || j64Config.STHubAddress.Contains("TBD"))
                return;

            // Set the IP address of this server if it has not been set yet
            if (String.IsNullOrEmpty(j64Config.j64Address) || j64Config.j64Address.Contains("TBD"))
                SmartThingsRepository.Determinej64Address(host, j64Config);
                
            var url = $"http://{j64Config.STHubAddress}:{j64Config.STHubPort}";
            var client = new System.Net.Http.HttpClient();

            System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);

            List<j64Device> l = new List<j64Device>();

            l.Add(new j64Device() { Name = j64Config.SoundDeviceName, DeviceValue = null, DeviceType = j64DeviceType.Volume });
            l.Add(new j64Device() { Name = j64Config.ChannelSurfDeviceName, DeviceValue = null, DeviceType = j64DeviceType.Surfing });
            l.Add(new j64Device() { Name = j64Config.VcrPauseDeviceName, DeviceValue = "pause", DeviceType = j64DeviceType.VCR });

            foreach (var fc in j64Config.FavoriteChannels)
                l.Add(new j64Device() { Name = fc.Name, DeviceValue = fc.Channel.ToString(), DeviceType = j64DeviceType.Channel });
            l.Add(new j64Device() { Name = j64Config.LastChannelDeviceName, DeviceValue = "previous", DeviceType = j64DeviceType.Channel });

            var request = new MyRequest<List<j64Device>>()
            {
                j64Ip = j64Config.j64Address,
                j64Port = j64Config.j64Port,
                j64AppId = j64Config.j64AppId,
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
        
        public static void Determinej64Address(string host, j64HarmonyGateway j64Config)
        {
            string[] h = host.Split(':');
            if (h.Length > 1)
                j64Config.j64Port = Convert.ToInt32(h[1]);

            var hostName = System.Net.Dns.GetHostEntryAsync(System.Net.Dns.GetHostName());
            hostName.Wait();
            foreach (var i in hostName.Result.AddressList)
            {
                if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    j64Config.j64Address = i.ToString();
                    break;
                }
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
