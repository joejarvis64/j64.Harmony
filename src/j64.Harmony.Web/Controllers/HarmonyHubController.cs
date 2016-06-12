using Microsoft.AspNetCore.Mvc;
using j64.Harmony.Xmpp;
using j64.Harmony.Web.Models;

namespace j64.Harmony.Web.Controllers
{
    [Route("api/[controller]")]
    public class HarmonyHubController : Controller
    {
        private j64HarmonyGateway j64Config = null;
        private Hub myHub = null;

        private static int previousLevel { get; set; } = 50;

        public HarmonyHubController(j64HarmonyGateway hubConfig, Hub hub)
        {
            this.j64Config = hubConfig;
            myHub = hub;
        }

        [HttpGet("ToggleMute")]
        public IActionResult ToggleMute()
        {
            myHub.SendCommand(j64Config.VolumeDevice, "mute", "press");
            return new ObjectResult("mute toggled");
        }

        [HttpGet("SetVolume/{level}")]
        public IActionResult SetVolume(int level)
        {
            myHub.SetVolume(level, previousLevel, j64Config.VolumeDevice);
            return new ObjectResult("volume level set");
        }

        [HttpGet("SetChannel/{channel}")]
        public IActionResult SetChannel(string channel)
        {
            myHub.ChangeChannel(channel, j64Config.ChannelDevice, j64Config.ChanneKeyPauseInterval);
            return new ObjectResult("channel has been set");
        }

        [HttpGet("Transport/{command}")]
        public IActionResult Transport(string command)
        {
            myHub.SendCommand(j64Config.ChannelDevice, command, "press");
            return new ObjectResult("transport command has been set");
        }

        [HttpGet("ChannelSurf/{startStop}")]
        public IActionResult StartChannelSurf(string startStop)
        {
            if (startStop.ToLower() == "start")
                myHub.StartChannelSurf(j64Config.ChannelDevice);
            else
                myHub.StopChannelSurf();

            return new ObjectResult($"Surfing {startStop}");
        }
    }
}
