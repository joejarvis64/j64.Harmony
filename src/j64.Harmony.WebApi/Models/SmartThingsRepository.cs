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
        public static void InstallDevices(string hostString)
        {
            try
            {
                string[] h = hostString.Split(':');
                string j64Server = h[0];
                int j64Port = 80;
                if (h.Length > 1)
                    j64Port = Convert.ToInt32(h[1]);

                var hostName = System.Net.Dns.GetHostEntryAsync(System.Net.Dns.GetHostName());
                hostName.Wait();
                foreach (var i in hostName.Result.AddressList)
                {
                    if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        j64Server = i.ToString();
                        break;
                    }
                }

                OauthInfo authInfo = OauthRepository.Get();

                if (authInfo == null | authInfo.endpoints == null || authInfo.endpoints.Count == 0)
                {
                    return;
                }
                string url = authInfo.endpoints[0].uri + $"/installDevices";

                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                msg.Headers.Add("Authorization", $"Bearer {authInfo.accessToken}");

                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("j64Server", j64Server));
                parms.Add(new KeyValuePair<string, string>("j64Port", j64Port.ToString()));
                parms.Add(new KeyValuePair<string, string>("j64UserName", "admin"));
                parms.Add(new KeyValuePair<string, string>("j64Password", "Admin_01"));
                msg.Content = new System.Net.Http.FormUrlEncodedContent(parms);
                var response = client.SendAsync(msg);
                response.Wait();

                if (response.Result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                }
            }
            catch (Exception)
            {
            }
        }
        
        /// <summary>
        /// Install or Update Devices in the SmartThings App
        /// </summary>
        public static void InstallDevices2(string hostString)
        {
            try
            {
                var url = "http://192.168.1.105:39500";
                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);

                var json = @"{'Email': 'james@example.com','Active': true,'CreatedDate': '2013-01-20T00:00:00Z','Roles': ['User','Admin'] }";
                msg.Content = new System.Net.Http.FormUrlEncodedContent(json);
                var response = client.SendAsync(msg);

                response.Wait();

                if (response.Result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
