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
        public string Email { get; set; }

        public string Password { get; set; }

        public string HubAddress { get; set; } = "HarmonyHub.attlocal.net";

        public int HubPort { get; set; } = 5222;

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

        public static HarmonyHubConfiguration Read()
        {
            HarmonyHubConfiguration hubConfig = new HarmonyHubConfiguration();

            if (File.Exists("HarmonyHubConfiguration.json"))
            {
                using (StreamReader file = System.IO.File.OpenText("HarmonyHubConfiguration.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    hubConfig = (HarmonyHubConfiguration)serializer.Deserialize(file, typeof(HarmonyHubConfiguration));
                }
            }

            return hubConfig;
        }

        public static void Save(HarmonyHubConfiguration hc)
        {
            using (StreamWriter file = System.IO.File.CreateText("HarmonyHubConfiguration.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, hc);
            }
        }
    }

    public class FavoriteChannel
    {
        public string Name { get; set; }
        public string Channel { get; set; }
    }
}
