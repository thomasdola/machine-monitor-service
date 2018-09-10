using Phoenix;

namespace MacMon.Services.WebSocket
{
    public class WebsocketSharpFactory : IWebsocketFactory
    {
        public IWebsocket Build(WebsocketConfiguration config)
        {
            var socket = new WebSocketSharp.WebSocket(config.uri.AbsoluteUri);
            return new WebsocketSharpAdapter(socket, config);
        }
    }
}