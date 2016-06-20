using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace j64.Harmony.Xmpp
{
    public class Hub
    {
        private HarmonyAuthToken harmonyAuthToken = null;
        private string hubAddress = null;
        private int hubPort = 0;
        private TcpClient _client = null;
        private Thread _surfThread = null;
        private bool _surfThreadShouldRun = true;
        private int PollInterval = 5;
        private Thread _pingThread;
        private Object thisLock = new Object();

        public HubConfig hubConfig { get; set; }

        /// <summary>
        /// Logs in to the Logitech Harmony web service to get a UserAuthToken.
        /// </summary>
        /// <param name="username">myharmony.com username</param>
        /// <param name="password">myharmony.com password</param>
        /// <returns>Logitech UserAuthToken</returns>
        public HarmonyAuthToken GetUserAuthToken(string username, string password)
        {
            try
            {
                if (harmonyAuthToken == null)
                {
                    const string logitechAuthUrl = "https://svcs.myharmony.com/CompositeSecurityServices/Security.svc/json/GetUserAuthToken";

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(logitechAuthUrl);
                    httpWebRequest.ContentType = "text/json";
                    httpWebRequest.Method = "POST";

                    Task<Stream> request = httpWebRequest.GetRequestStreamAsync();
                    request.Wait(10000);
                    using (StreamWriter sw = new StreamWriter(request.Result))
                    {
                        sw.Write(JsonConvert.SerializeObject(new { email = username, password = password }, Formatting.Indented));
                        sw.Flush();
                    }

                    Task<WebResponse> response = httpWebRequest.GetResponseAsync();
                    response.Wait(10000);

                    var responseStream = response.Result.GetResponseStream();
                    if (responseStream == null)
                        return null;

                    using (var streamReader = new StreamReader(responseStream))
                    {
                        var result = streamReader.ReadToEnd();
                        harmonyAuthToken = JsonConvert.DeserializeObject<GetUserAuthTokenResultRootObject>(result).GetUserAuthTokenResult;
                    }
                }
            }
            catch (Exception)
            {
            }

            return harmonyAuthToken;
        }

        public bool StartNewConnection(string userName, string password, string address, int port)
        {
            // Reset the connection if it is already open
            ResetConnection();
            hubConfig = null;

            // We can't connect if these have not been set
            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(address) || port == 0)
                return false;

            // Get an auth token if we dont have one
            var harmonyAuthToken = GetUserAuthToken(userName, password);
            if (harmonyAuthToken == null)
                return false;

            OpenConnection(address, port);

            // Start pinging the harmony to keep the connection alive
            if (_pingThread == null)
            {
                _pingThread = new Thread(new ThreadStart(PingHarmony));
                _pingThread.Name = "Ping_Harmony";
                _pingThread.Start();
            }

            // Get the config info
            GetConfig();

            return true;
        }

        public void OpenConnection(string address, int port)
        {
            this.hubAddress = address;
            this.hubPort = port;

            // Connect to the hub
            EnsureConnection();

            var stream = $"<stream:stream to='{address}' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0' xml:lang='en'>";
            var response = SendReceiveSocketData(stream, 2);

            var base64Token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(harmonyAuthToken.UserAuthToken));
            var auth = $"<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='PLAIN'>{base64Token}</auth>";
            response = SendReceiveSocketData(auth, 1);

            stream = $"<stream:stream to='{address}' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0' xml:lang='en'>";
            response = SendReceiveSocketData(stream, 1);

            //var bind = $"<iq id='j64Harmony_1' type='set' to='192.168.1.118'><bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><resource>j64Harmony</resource></bind></iq>";
            var bind = $"<iq id='j64Harmony_1' type='set' to='{address}'><bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><resource>j64Harmony</resource></bind></iq>";
            response = SendReceiveSocketData(bind, 1);

            var session = $"<iq id='j64Harmony_2' type='set' to='{address}'><session xmlns='urn:ietf:params:xml:ns:xmpp-session'/></iq>";
            response = SendReceiveSocketData(session, 1);
        }

        public void GetConfig()
        {
            var iqCmd = $"<iq type='get' from='1' to='guest' id='j64Harmony_4'><oa xmlns='connect.logitech.com' mime='vnd.logitech.harmony/vnd.logitech.harmony.engine?config'/></iq>";
            var response = SendReceiveSocketData(iqCmd, 2);

            // Turn the config into a strongly typed document
            string config = response.DocumentElement.FirstChild?.NextSibling?.InnerText;
            if (config == null)
            {
                config = File.ReadAllText("myHubConfig.json");
                if (config == null)
                    throw new Exception("Could not get config from the harmony hub.  Consider power cycling the hub.");
            }

            hubConfig = JsonConvert.DeserializeObject<HubConfig>(config);

            // Save the config to a file for debugging purposes
            File.WriteAllText("myHubConfig.json", JsonConvert.SerializeObject(hubConfig, Formatting.Indented));
        }

        public void SendCommand(string deviceName, string command, string status)
        {
            // Find the id for this device
            string deviceId = "";
            foreach (var d in hubConfig.device)
            {
                if (d.label.ToLower() == deviceName.ToLower())
                    deviceId = d.id.ToString();
            }

            var iq = $"<iq type='get' from='1' to='guest' id='j64Harmony_4'><oa xmlns='connect.logitech.com' mime='vnd.logitech.harmony/vnd.logitech.harmony.engine?holdAction'>action={{'type'::'IRCommand','deviceId'::'{deviceId}','command'::'{command}'}}:status={status}</oa></iq>";

            int counter = 0;
            int maxTries = 5;
            bool found = false;
            while (!found && counter < maxTries)
            {
                counter++;
                var response = SendReceiveSocketData(iq, 1, true);
                if (response?.DocumentElement?.FirstChild?.Name == "iq")
                    found = true;
                else
                    Thread.Sleep(250);
            }

            if (counter == maxTries)
                counter = 0;
        }

        public void StartChannelSurf(string channelDevice)
        {
            if (_surfThread != null && _surfThread.IsAlive)
                return;

            _surfThreadShouldRun = true;
            _surfThread = new Thread(() => SurfEm(channelDevice));
            _surfThread.Name = "Channel_Surf";
            _surfThread.Start();
        }

        public void StopChannelSurf()
        {
            _surfThreadShouldRun = false;
            if (_surfThread.IsAlive)
                _surfThread.Join();
        }

        private void SurfEm(string channelDevice)
        { 
            // Channel Surfing!  Yahoo!
            for (int i = 0; i < 30 && _surfThreadShouldRun == true; i++)
            {
                SendCommand(channelDevice, "channelup", "press");
                SendCommand(channelDevice, "channelup", "release");
                Thread.Sleep(3000);
            }
        }

        private XmlDocument SendReceiveSocketData(string cmdData, int numNodes, bool directReadSocket = false, int retryCount = 0)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("responseNodes"));

            lock (thisLock)
            {
                try
                {
                    EnsureConnection();

                    byte[] data = System.Text.Encoding.ASCII.GetBytes(cmdData);

                    // Send the data
                    var serverStream = _client.GetStream();
                    serverStream.Write(data, 0, data.Length);
                    serverStream.Flush();

                    // Build the reply
                    serverStream.ReadTimeout = 5000;

                    Stream stream;
                    if (directReadSocket)
                    {
                        var message = new byte[4096];
                        var bytesRead = serverStream.Read(message, 0, message.Length);
                        stream = new MemoryStream(message, 0, bytesRead);
                    }
                    else
                    {
                        stream = serverStream;
                    }

                    var settings = new XmlReaderSettings()
                    {
                        IgnoreProcessingInstructions = true,
                        IgnoreComments = true,
                        IgnoreWhitespace = true,
                        ConformanceLevel = ConformanceLevel.Fragment
                    };

                    XmlReader reader = XmlReader.Create(stream, settings);
                    while (doc.DocumentElement.ChildNodes.Count < numNodes && reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "stream:stream")
                                doc.DocumentElement.AppendChild(ParseElement(reader, doc.DocumentElement, false));
                            else
                                doc.DocumentElement.AppendChild(ParseElement(reader, doc.DocumentElement, true));
                        }
                    }
                }
                catch (Exception)
                {
                    ResetConnection();
                    OpenConnection(hubAddress, hubPort);
                    if (retryCount == 0)
                        doc = SendReceiveSocketData(cmdData, numNodes, directReadSocket, retryCount + 1);
                }
            }

            return doc;
        }

        private XmlElement ParseElement(XmlReader reader, XmlElement parent, bool hasClosingElement)
        {
            XmlElement ele = parent.OwnerDocument.CreateElement(reader.LocalName);
            if (reader.IsEmptyElement)
                return ele;

            GetAttributes(reader, ele);

            if (hasClosingElement)
            {
                // Get the next element
                reader.Read();

                // If we are starting a new element, then this is the child
                if (reader.NodeType == XmlNodeType.Element)
                {
                    ele.AppendChild(ParseElement(reader, ele, true));
                    if (reader.HasValue)
                        ele.InnerText = reader.Value;
                } 
                else if (reader.NodeType == XmlNodeType.EndElement)
                    return ele;
            }

            return ele;
        }

        private void GetAttributes(XmlReader reader, XmlElement ele)
        {
            bool moreAttributes = reader.MoveToFirstAttribute();
            while (moreAttributes == true)
            {
                ele.SetAttribute(reader.Name, reader.Value);
                moreAttributes = reader.MoveToNextAttribute();
            }
        }

        private void ResetConnection()
        {
            lock (thisLock)
            {
                if (_client != null)
                {
                    if (_client.Connected)
                        _client.GetStream().Dispose();

                    _client.Dispose();
                    _client = null;
                }
            }
        }

        private void EnsureConnection()
        {
            do
            {
                lock (thisLock)
                {
                    if (_client != null && _client.Connected)
                    {
                        // Already connected
                        return;
                    }

                    // Clean up anything that is outstanding
                    if (_client != null)
                    {
                        //_client.Client.Shutdown(SocketShutdown.Both);
                        _client.Dispose();
                    }

                    _client = new TcpClient();
                    Task ca = _client.ConnectAsync(hubAddress, hubPort);
                    if (ca.Wait(15000) == false)
                    {
                        // Could not connect within 15 seconds
                        return;
                    }

                    // Make sure it connected properly
                    if (_client.Connected)
                        return;

                    // No good, it will retry shortly
                    _client.Dispose();
                    _client = null;
                }
            } while (_client == null);
        }

        /// <summary>
        /// Listen for data coming from the TPI
        /// </summary>
        private void PingHarmony()
        {
            // Start the first poll 10 seconds after start up
            Thread.Sleep(10 * 1000);

            //PollInterval
            while (true)
            {
                EnsureConnection();

                //try
                //{
                //    var iq = $"<iq type='get' from='1' to='guest' id='j64Harmony_4'><oa xmlns='connect.logitech.com' mime='vnd.logitech.harmony/vnd.logitech.ping></oa></iq>";
                //    var response = SendReceiveSocketData(iq, 1, true);
                //}
                //catch (Exception)
                //{
                //}

                Thread.Sleep(PollInterval * 1000);
            }
        }

        public void ChangeChannel(string channel, string channelDevice, int pauseBetweenKeypressInterval)
        {
            if (channel.ToLower() == "previous")
            {
                SendCommand(channelDevice, "channelPrev", "press");
            }
            else if (channel.ToLower() == "up")
            {
                SendCommand(channelDevice, "channelup", "press");
                SendCommand(channelDevice, "channelup", "release");
            }
            else if (channel.ToLower() == "down")
            {
                SendCommand(channelDevice, "channeldown", "press");
                SendCommand(channelDevice, "channeldown", "release");
            }
            else
            {
                int pause = 0;
                foreach (char c in channel)
                {
                    System.Threading.Thread.Sleep(pause);
                    SendCommand(channelDevice, c.ToString(), "press");
                    pause = pauseBetweenKeypressInterval;
                }
                SendCommand(channelDevice, "numberenter", "press");
            }
        }

        public void SetVolume(int level, int previousLevel, string volumeDevice)
        {
            string cmd;
            if (level == 0)
                cmd = "volumedown";
            else if (level == 100)
                cmd = "volumeup";
            else if (level > previousLevel)
                cmd = "volumeup";
            else if (level < previousLevel)
                cmd = "volumedown";
            else if (level < 50)
                cmd = "volumedown";
            else
                cmd = "volumeup";

            SendCommand(volumeDevice, cmd, "press");
            System.Threading.Thread.Sleep(100);
            SendCommand(volumeDevice, cmd, "press");
        }
    }

    public class HarmonyAuthToken
    {
        public int AccountId { get; set; }
        public string UserAuthToken { get; set; }
    }

    public class GetUserAuthTokenResultRootObject
    {
        public HarmonyAuthToken GetUserAuthTokenResult { get; set; }
    }
}
