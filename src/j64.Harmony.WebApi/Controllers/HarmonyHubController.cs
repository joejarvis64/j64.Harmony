using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using j64.Harmony.Xmpp;
using j64.Harmony.WebApi.ViewModels.Config;

namespace j64.Harmony.WebApi.Controllers
{
    public class MyResponse<T>
    {
        public string FromHost { get; set; }
        public string Route { get; set; }
        public T Response { get; set; }
    }

    [Route("api/[controller]")]
    public class HarmonyHubController : Controller
    {
        private HarmonyHubConfiguration hubConfig = null;
        private Hub myHub = null;

        private static int previousLevel { get; set; } = 50;

        public HarmonyHubController(HarmonyHubConfiguration hubConfig, Hub hub)
        {
            this.hubConfig = hubConfig;
            myHub = hub;
        }

        // GET: api/HarmonyController
        [HttpGet]
        public IActionResult GetHarmonyConfig()
        {
            List<j64Device> l = new List<j64Device>();

            l.Add(new j64Device() { Name = hubConfig.SoundDeviceName, DeviceValue = null, DeviceType = j64DeviceType.Volume });
            l.Add(new j64Device() { Name = hubConfig.ChannelSurfDeviceName, DeviceValue = null, DeviceType = j64DeviceType.Surfing });
            l.Add(new j64Device() { Name = hubConfig.VcrPauseDeviceName, DeviceValue = "pause", DeviceType = j64DeviceType.VCR});

            foreach (var fc in hubConfig.FavoriteChannels)
                l.Add(new j64Device() { Name = fc.Name, DeviceValue = fc.Channel, DeviceType = j64DeviceType.Channel });
            l.Add(new j64Device() { Name = hubConfig.LastChannelDeviceName, DeviceValue = "previous", DeviceType = j64DeviceType.Channel });

            return new ObjectResult(new MyResponse<List<j64Device>>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = l
            });
        }

        [HttpGet("ToggleMute")]
        public IActionResult ToggleMute()
        {
            myHub.SendCommand(hubConfig.VolumeDevice, "mute", "press");
            return new ObjectResult("mute toggled");
        }

        [HttpGet("SetVolume/{level}")]
        public IActionResult SetVolume(int level)
        {
            myHub.SetVolume(level, previousLevel, hubConfig.VolumeDevice);
            return new ObjectResult("volume level set");
        }

        [HttpGet("SetChannel/{channel}")]
        public IActionResult SetChannel(string channel)
        {
            myHub.ChangeChannel(channel, hubConfig.ChannelDevice, hubConfig.ChanneKeyPauseInterval);
            return new ObjectResult("channel has been set");
        }

        [HttpGet("Transport/{command}")]
        public IActionResult Transport(string command)
        {
            myHub.SendCommand(hubConfig.ChannelDevice, command, "press");
            return new ObjectResult("transport command has been set");
        }

        [HttpGet("ChannelSurf/{startStop}")]
        public IActionResult StartChannelSurf(string startStop)
        {
            if (startStop.ToLower() == "start")
                myHub.StartChannelSurf(hubConfig.ChannelDevice);
            else
                myHub.StopChannelSurf();

            return new ObjectResult($"Surfing {startStop}");
        }
    }
}
