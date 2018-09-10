using System;
using Phoenix;
using WebSocketSharp;

namespace MacMon.Services.WebSocket
{
    public sealed class WebsocketSharpAdapter : IWebsocket
    {
        private readonly WebSocketSharp.WebSocket ws;
        private readonly WebsocketConfiguration config;
        
        
        public WebsocketSharpAdapter(WebSocketSharp.WebSocket ws, WebsocketConfiguration config) {
			
            this.ws = ws;
            this.config = config;

            ws.OnOpen += OnWebsocketOpen;
            ws.OnClose += OnWebsocketClose;
            ws.OnError += OnWebsocketError;
            ws.OnMessage += OnWebsocketMessage;
        }
        
        
        #region IWebsocket methods
        
        public void Connect() { ws.Connect(); }
        public void Send(string message) { ws.Send(message); }
        public void Close(ushort? code = null, string message = null) { ws.Close(); }
        
        #endregion
        
        
        
        #region websocketsharp callbacks

        public void OnWebsocketOpen(object sender, EventArgs args) {
            config.onOpenCallback(this);
        }

        public void OnWebsocketClose(object sender, CloseEventArgs args) {
            config.onCloseCallback(this, args.Code, args.Reason);
        }

        public void OnWebsocketError(object sender, ErrorEventArgs args) {
            config.onErrorCallback(this, args.Message);
        }

        public void OnWebsocketMessage(object sender, MessageEventArgs args) {
            config.onMessageCallback(this, args.Data);
        }

        #endregion
    }
}