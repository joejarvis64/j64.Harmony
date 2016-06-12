using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.Harmony.Web.Models
{
    public class j64HarmonyGateway
    {
        #region Harmony Hub Info
        public string Email { get; set; }

        public string Password { get; set; }

        public string HubAddress { get; set; } = "HarmonyHub.attlocal.net";

        public int HubPort { get; set; } = 5222;
        #endregion

        #region SmartThings Hub Info

        public string STHubAddress { get; set; } = "TBD during smartapp install";

        public int STHubPort { get; set; } = 39500;

        public string j64AppId { get; set; }
        #endregion

        #region j64 Server Info
        public string j64Address { get; set; } = "TBD during smartapp install";

        public int j64Port { get; set; } = 2065;

        #endregion

        #region Volume/Channel Info
        public string VolumeDevice { get; set; } = "";

        public string ChannelDevice { get; set; } = "";

        public int ChanneKeyPauseInterval { get; set; } = 750;

        public string SoundDeviceName { get; set; } = "Sound";

        public string ChannelSurfDeviceName { get; set; } = "Surfing";

        public string LastChannelDeviceName { get; set; } = "Previous Channel";

        public string VcrPauseDeviceName { get; set; } = "D.V.R.";

        public List<FavoriteChannel> FavoriteChannels = new List<FavoriteChannel>();
        #endregion
    }

    public class FavoriteChannel
    {
        public string Name { get; set; }
        public string Channel { get; set; }
    }
}
