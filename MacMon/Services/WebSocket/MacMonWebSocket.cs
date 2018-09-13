using System;
using System.Collections.Generic;
using MacMon.Models;
using Phoenix;

namespace MacMon.Services.WebSocket
{
    
    public interface IMacMonWebSocket{}
    
    public class MacMonWebSocket : IMacMonWebSocket
    {
        public const string NetworkStatusChanged = "NETWORK_STATUS_CHANGED";
        public const string UserActivityChanged = "USER_ACTIVITY_CHANGED";
        public const string ApplicationStatusChanged = "APPLICATION_STATUS_CHANGED";
        public const string ServiceStatusChanged = "SERVICE_STATUS_CHANGED";
        public const string LocationStatusChanged = "LOCATION_STATUS_CHANGED";

        private readonly Socket _socket;
        private readonly string _host;

        public MacMonWebSocket(string server, JWT jwt)
        {
            _host = server;
            var socketFactory = new WebsocketSharpFactory();
            _socket = new Socket(socketFactory);
            try
            {
                Connect(jwt);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot connect to socket because -> {0}", e.Message);
            }
        }

        public static Socket InitSocket(JWT jwt, string server)
        {
            var macMonWebSocket = new MacMonWebSocket(server, jwt);
            return macMonWebSocket._socket;
        }

        private void Connect(JWT jwt)
        {
            var parameters = new Dictionary<string, string> {{"token", jwt.Token}};
            _socket.Connect($"ws://{_host}/socket", parameters);
        }
    }
}