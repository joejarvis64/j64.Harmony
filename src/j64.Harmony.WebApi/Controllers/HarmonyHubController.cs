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
