using Microsoft.AspNet.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace j64.Harmony.WebApi.ViewModels.Config
{
    public class HarmonyHubConfiguration
    {
        #region Harmony Hub Info
        public string Email { get; set; }

        public string Password { get; set; }

        public string HubAddress { get; set; } = "HarmonyHub.attlocal.net";

        public int HubPort { get; set; } = 5222;
        #endregion

        #region SmartThings Hub Info

        public string STHubAddress { get; set; } = "";

        public int STHubPort { get; set; } = 0;

        public string j64AppId { get; set; }
        #endregion

        #region j64 Server Info
        public string j64Address { get; set; } = "";

        public int j64Port { get; set; } = 2065;

        #endregion

        #region Volume/Channel Info

        public string VolumeDevice { get; set; } = "";

        public string ChannelDevice { get; set; } = "";

        public int ChanneKeyPauseInterval { get; set; } = 750;

        public string SoundDeviceName { get; set; } = "Sound";

        public string ChannelSurfDeviceName { get; set; } = "Channel Surfing";

        public string LastChannelDeviceName { get; set; } = "Previous Channel";

        public string VcrPauseDeviceName { get; set; } = "Pause VCR";


        public List<FavoriteChannel> FavoriteChannels = new List<FavoriteChannel>();

        [JsonIgnore]
        public List<SelectListItem> DeviceList { get; set; } = new List<SelectListItem>();
        #endregion

        #region Read/Save
        public static string HarmonyHubConfigurationFile { get; set; } = "HarmonyHubConfiguration.json";
        
        public static HarmonyHubConfiguration Read()
        {
            HarmonyHubConfiguration hubConfig = new HarmonyHubConfiguration();

            if (File.Exists(HarmonyHubConfigurationFile))
            {
                using (StreamReader file = System.IO.File.OpenText(HarmonyHubConfigurationFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    hubConfig = (HarmonyHubConfiguration)serializer.Deserialize(file, typeof(HarmonyHubConfiguration));
                }
            }
            else
            {
                hubConfig.FavoriteChannels.Add(new FavoriteChannel() { Name = "Fox and Friends", Channel = "1004" });
                hubConfig.FavoriteChannels.Add(new FavoriteChannel() { Name = "Eleven 20", Channel = "1120" });
                hubConfig.FavoriteChannels.Add(new FavoriteChannel() { Name = "Food Network", Channel = "1452" });
                Save(hubConfig);
            }

            return hubConfig;
        }

        public static void Save(HarmonyHubConfiguration hc)
        {
            using (StreamWriter file = System.IO.File.CreateText(HarmonyHubConfigurationFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, hc);
            }
        }
        #endregion
    }

    public class FavoriteChannel
    {
        public string Name { get; set; }
        public string Channel { get; set; }
    }
}
