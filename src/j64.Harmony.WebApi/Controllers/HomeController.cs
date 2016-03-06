using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using j64.Harmony.WebApi.ViewModels.Config;
using j64.Harmony.Xmpp;
using Microsoft.AspNet.Mvc.Rendering;
using System.IO;
using Newtonsoft.Json;
using j64.Harmony.WebApi.Models;

namespace j64.Harmony.WebApi.Controllers
{
    public class HomeController : Controller
    {
        private HarmonyHubConfiguration hubConfig = null;
        private Hub myHub = null;
        private static int previousLevel { get; set; } = 50;

        public HomeController(HarmonyHubConfiguration hubConfig, Hub hub)
        {
            this.hubConfig = hubConfig;
            this.myHub = hub;
        }

        public IActionResult Index()
        {
            return View(hubConfig);
        }

        public IActionResult Mute()
        {
            myHub.SendCommand(hubConfig.VolumeDevice, "mute", "press");
            return View("Index", hubConfig);
        }

        [HttpGet("VolumeLevel/{level}")]
        public IActionResult VolumeLevel(int level)
        {
            myHub.SetVolume(level, previousLevel, hubConfig.VolumeDevice);
            return View("Index", hubConfig);
        }

        [HttpGet("SetChannel/{channel}")]
        public IActionResult SetChannel(string channel)
        {
            myHub.SendCommand(hubConfig.ChannelDevice, "pause", "press");
            return View("Index", hubConfig);
        }

        [HttpGet("Transport/{command}")]
        public IActionResult Transport(string command)
        {
            myHub.SendCommand(hubConfig.ChannelDevice, command, "press");
            return View("Index", hubConfig);
        }

        public IActionResult StartSurf()
        {
            myHub.StartChannelSurf(hubConfig.ChannelDevice);
            return View("Index", hubConfig);
        }

        public IActionResult StopSurf()
        {
            myHub.StopChannelSurf();
            return View("Index", hubConfig);
        }

        public IActionResult AddChannel()
        {
           hubConfig.FavoriteChannels.Add(new FavoriteChannel() { Name = "New Channel", Channel = "1000"} );
            return View("Index", hubConfig);
        }

        [HttpGet("DeleteChannel/{channel}")]
        public IActionResult DeleteChannel(string channel)
        {
            var fc = hubConfig.FavoriteChannels.FirstOrDefault(x => x.Channel == channel);
            if (fc != null)
                hubConfig.FavoriteChannels.Remove(fc);

            return View("Index", hubConfig);
        }

        public IActionResult SaveChanges(HarmonyHubConfiguration hc)
        {
            hubConfig.Email = hc.Email;
            if (!String.IsNullOrEmpty(hc.Password))
                hubConfig.Password = hc.Password;
            hubConfig.HubAddress = hc.HubAddress;
            hubConfig.HubPort = hc.HubPort;
            hubConfig.ChannelDevice = hc.ChannelDevice;
            hubConfig.VolumeDevice = hc.VolumeDevice;
            hubConfig.ChannelSurfDeviceName = hc.ChannelSurfDeviceName;
            hubConfig.SoundDeviceName = hc.SoundDeviceName;
            hubConfig.ChanneKeyPauseInterval = hc.ChanneKeyPauseInterval;
            hubConfig.LastChannelDeviceName = hc.LastChannelDeviceName;
            hubConfig.VcrPauseDeviceName = hc.VcrPauseDeviceName;
            hubConfig.STHubAddress = hc.STHubAddress;
            hubConfig.STHubPort = hc.STHubPort;

            if (Request.Form["fc.Name"].Count > 0)
            {
                hubConfig.FavoriteChannels.Clear();
                for (int i = 0; i < Request.Form["fc.Name"].Count; i++)
                {
                    string name = Request.Form["fc.Name"][i];
                    hubConfig.FavoriteChannels.Add(new FavoriteChannel()
                    {
                        Name = Request.Form["fc.Name"][i],
                        Channel = Request.Form["fc.Channel"][i]
                    });
                }
            }

            // Refresh the connection
            myHub.StartNewConnection(hc.Email, hc.Password, hc.HubAddress, hc.HubPort);

            // We always have to update the device list on the Hub Configuration after we get the config info
            hubConfig.DeviceList.Clear();
            myHub.hubConfig?.device.ForEach(x => hubConfig.DeviceList.Add(new Microsoft.AspNet.Mvc.Rendering.SelectListItem() { Text = x.label }));

            // Set a default to get the ball rolling
            if (String.IsNullOrEmpty(hubConfig.ChannelDevice))
            {
                hubConfig.ChannelDevice = myHub.hubConfig.device?[0].label;
                hubConfig.VolumeDevice = myHub.hubConfig.device?[0].label;
            }

            // Save the new data
            HarmonyHubConfiguration.Save(hubConfig);

            return View("Index", hubConfig);
        }

        public IActionResult SyncSmartThings()
        {
            SmartThingsRepository.InstallDevices(hubConfig);
            return View("Index", hubConfig);
        }

        public IActionResult Findj64Address()
        {
            string[] h = this.Request.Host.Value.Split(':');
            if (h.Length > 1)
                hubConfig.j64Port = Convert.ToInt32(h[1]);

            var hostName = System.Net.Dns.GetHostEntryAsync(System.Net.Dns.GetHostName());
            hostName.Wait();
            foreach (var i in hostName.Result.AddressList)
            {
                if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    hubConfig.j64Address = i.ToString();
                    break;
                }
            }
            HarmonyHubConfiguration.Save(hubConfig);

            return View("Index", hubConfig);
        }
    }
}
