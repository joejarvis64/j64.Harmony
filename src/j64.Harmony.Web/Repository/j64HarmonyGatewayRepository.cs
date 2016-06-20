using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IO;
using j64.Harmony.Web.Models;

namespace j64.Harmony.Web.Repository
{
    public class j64HarmonyGatewayRepository
    {
        #region Read/Save
        public static string HarmonyHubConfigurationFile { get; set; } = "HarmonyHubConfiguration.json";
        
        public static j64HarmonyGateway Read()
        {
            j64HarmonyGateway j64Config = new j64HarmonyGateway();

            if (File.Exists(HarmonyHubConfigurationFile))
            {
                using (StreamReader file = System.IO.File.OpenText(HarmonyHubConfigurationFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    j64Config = (j64HarmonyGateway)serializer.Deserialize(file, typeof(j64HarmonyGateway));
                }
            }
            else
            {
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "Fox and Friends", Channel = "1004" });
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "One zero zero five", Channel = "1005" });
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "Ten Oh Eight", Channel = "1008" });
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "Eleven 20", Channel = "1120" });
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "History Channel", Channel = "1256" });
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "Food Network", Channel = "1452" });
                j64Config.FavoriteChannels.Add(new FavoriteChannel() { Name = "Show Time", Channel = "1852" });

                var cc = new CustomCommand()
                {
                    CommandName = "Command 1"
                };
                cc.Actions.Add(new CustomAction()
                {
                    Sequence = 1,
                    Device = "Samsung Amp",
                    Group = "Volume",
                    Function = "mute",
                    Command = "press"
                });
                cc.Actions.Add(new CustomAction()
                {
                    Sequence = 2,
                    Device = "Samsung Amp",
                    Group = "Volume",
                    Function = "VolumeUp",
                    Command = "press"
                });
                cc.Actions.Add(new CustomAction()
                {
                    Sequence = 3,
                    Device = "Samsung Amp",
                    Group = "Volume",
                    Function = "VolumeUp",
                    Command = "press"
                });

                j64Config.CustomCommands.Add(cc);

                Save(j64Config);
            }

            return j64Config;
        }

        public static void Save(j64HarmonyGateway j64Config)
        {
            using (StreamWriter file = System.IO.File.CreateText(HarmonyHubConfigurationFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, j64Config);
            }
        }
        #endregion
    }
}
