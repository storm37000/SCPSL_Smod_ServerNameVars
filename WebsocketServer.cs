using WebSocketSharp;
using WebSocketSharp.Server;

namespace ServerNameVars
{
	class WebsocketServer
	{
		private WebSocketServer wssv;
		public sockserv sockserv;

		public WebsocketServer(EventHandler eventHandler, ushort port, string servicename)
		{
			wssv = new WebSocketServer(port);
			wssv.AddWebSocketService<sockserv>("/" + servicename, () => new sockserv(eventHandler,this));
			wssv.Start();
		}
		public void sendmsg(string msg)
		{
			sockserv.sendmsg(msg);
		}
	}
	class sockserv : WebSocketBehavior
	{
		private EventHandler EventHandler;

		public sockserv(EventHandler eventHandler, WebsocketServer websocketServer)
		{
			this.EventHandler = eventHandler;
			websocketServer.sockserv = this;
		}
		public void sendmsg(string msg)
		{
			SendAsync(msg,null);
		}
		protected override void OnOpen()
		{
			base.OnOpen();
			SendAsync(EventHandler.ServerName,null);
		}
	}
}