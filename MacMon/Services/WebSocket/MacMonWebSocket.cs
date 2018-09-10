using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MacMon.Models;

namespace MacMon.Services.WebSocket
{
    
    public interface IMacMonWebSocket{}
    
    public class MacMonWebSocket : IMacMonWebSocket
    {
        public static readonly string NETWORK_STATUS_CHANGED = "NETWORK_STATUS_CHANGED";
        public static readonly string USER_ACTIVITY_CHANGED = "USER_ACTIVITY_CHANGED";
        public static readonly string APPLICATION_STATUS_CHANGED = "APPLICATION_STATUS_CHANGED";
        public static readonly string SERVICE_STATUS_CHANGED = "SERVICE_STATUS_CHANGED";
        
        private readonly Phoenix.Socket _socket;
        private const string Host = "localhost:4000";
        
        public MacMonWebSocket(JWT jwt)
        {
            var socketFactory = new WebsocketSharpFactory();
            _socket = new Phoenix.Socket(socketFactory);
            Connect(jwt);
        }

        public static Phoenix.Socket InitSocket(JWT jwt)
        {
            var macMonWebSocket = new MacMonWebSocket(jwt);
            return macMonWebSocket._socket;
        }

        private void Connect(JWT jwt)
        {
            var parameters = new Dictionary<string, string> {{"token", jwt.Token}};
            _socket.Connect($"ws://{Host}/socket", parameters);
        }
    }
}