using System.Net.WebSockets;

namespace MonitorMultiplePeersRTC.WebSocketRTC
{
    public class WebSocket_Dict
    {

        public string ClientKey { get; set; }
        public string ClientAccessFrom { get; set; }
        public WebSocket WebSocket { get; set; }

    }
}
