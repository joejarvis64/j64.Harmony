using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using j64.Harmony.Xmpp;

namespace j64.Harmony.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var harmony = new j64.Harmony.Xmpp.Hub();

            string email = "joe.jarvis@att.net";
            string password = "W1bigSGbCIwu";
            string hubAddress = "192.168.1.118";
            int hubPort = 5222;

            // Get an auth token from the harmony "cloud"
            var harmonyAuthToken = harmony.GetUserAuthToken(email, password);
            if (harmonyAuthToken == null)
                throw new Exception("Could not get token from Logitech server.");

            // Open a connection with the local hub
            harmony.OpenConnection(hubAddress, hubPort);

            // Refresh the configuration info
            harmony.GetConfig();

            // Start sending commands
            harmony.SendCommand("Samsung Amp", "mute", "press");

            /*
            harmony.SendCommand("volumedown", "33734344", "press");

            harmony.SendCommand("volumeup", "33734344", "press");

            harmony.SendCommand("pause", "33733733", "press");
            harmony.SendCommand("play", "33733733", "press");

            // Channel Surfing!
            for (int i = 0; i < 30; i++)
            {
                harmony.SendCommand("channelup", "33733733", "press");
                harmony.SendCommand("channelup", "33733733", "release");
                System.Threading.Thread.Sleep(3000);
            }
            */
        }
    }
}
