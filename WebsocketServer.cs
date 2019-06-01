/* All rights reserved, Copyrights Ashish Patil, 2010. http://ashishware.com/
 * Permission granted to modify/use this code for non commercial purpose so long as the
 * original notice is retained. This code is for educational purpose ONLY. 
 * DISCLAIMER: The code is provided as is, without warranties of any kind. The author
 * is not responsible for any damage (of any kind) this code may cause. The author is also not
 * responsible for any misuse of the code. Use and run this code AT YOUR OWN RISK.
 * 
 */

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace ServerNameVars
{
	class WebsocketServer
	{
		static WebSockClientManager m;
		public WebsocketServer(Main plugin, ref string name, ushort port, int maxconn)
		{
			TcpListener t = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
			m = new WebSockClientManager(maxconn, port, plugin);
			t.Start();
			new Thread(new ThreadStart(() => new bcastname(plugin, m))).Start();
			while (true)
			{
				TcpClient c = t.AcceptTcpClient();
				//c.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
				WebSockClient w = new WebSockClient(c);
				m.AddClient(w);
			}
		}
	}
	class bcastname
	{
		public bcastname(Main plugin, WebSockClientManager webSock)
		{
			while (true)
			{
				Thread.Sleep(1000);
				webSock.WriteData("test");
			}
		}
	}
	class WebSockClient
	{
		public int ManagingThreadId { get; set; }
//		public string WebSocketOrigin { get; set; }
//		public string WebSocketLocationURL { get; set; }
		public bool IsSubscribed { get; set; }
		private byte[] _buffer = new byte[255];
		private byte[] _writeBuffer;
		private TcpClient _tcpClient;
		public WebSockClientStatus WebSocketConnectionStatus { get; set; }

		public byte[] WriteBuffer
		{
			set { _writeBuffer = value; }
			get { return _writeBuffer; }
		}

		public TcpClient TcpClientInstance
		{ get { return _tcpClient; } }

		public WebSockClient(TcpClient t)
		{
			_tcpClient = t;
		}

		public enum WebSockClientStatus
		{
			CONNECTING = 0,
			HANDSHAKEDONE = 3,
			DISCONNECTED = 6,
		}
//		public WebSockClient(string webSockOrigin, string webSockLocationURL)
//		{
//			this.WebSocketLocationURL = webSockLocationURL;
//			this.WebSocketOrigin = webSockOrigin;
//		}

	}
	class WebSockClientManager
	{
		private List<WebSockClient> _clientList;
		private delegate void ClientHandler(WebSockClient c);
		private static int _port, _maxConnection;
		private static string _origin, _location;
		private static Main _plugin;

		private WebSockClientManager()
		{

		}

		public WebSockClientManager(int maxConnection, int port, Main plugin)
		{
			_maxConnection = maxConnection;
			_port = port;
			_clientList = new List<WebSockClient>();
			_plugin = plugin;
		}

		public void AddClient(WebSockClient c)
		{
			if (_clientList.Count >= _maxConnection)
			{ // check if any connection is available
				List<int> closedClients = new List<int>();
				for (int i = 0; i < _clientList.Count; i++)
				{
					if (_clientList[i].TcpClientInstance.Connected == false)
					{
						_clientList[i].TcpClientInstance.Close();

						closedClients.Add(i);
					}
				}

				foreach (int e in closedClients)
				{
					_clientList.RemoveAt(e);
				}
			}

			if (_clientList.Count < _maxConnection)
			{
				_clientList.Add(c);

				Thread clientThread = new Thread(delegate ()
				{
					this.HandleClient(c); ;
				});
				c.ManagingThreadId = clientThread.ManagedThreadId;
				clientThread.Start();
				_plugin.Info("New Thread Started:" + c.ManagingThreadId.ToString());
			}
			else
			{
				//sorry
				c.TcpClientInstance.Close();
			}

			_plugin.Info("Thread count :" + Process.GetCurrentProcess().Threads.Count.ToString());

		}

		private void HandleClient(WebSockClient c)
		{
			try
			{
				int b;
				c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.CONNECTING;
				using (NetworkStream n = c.TcpClientInstance.GetStream())
				using (StreamWriter streamWriter = new StreamWriter(n))
				{
					c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.CONNECTING;

					while (c.TcpClientInstance.Connected)
					{
						Thread.Sleep(1);
						//Read and Validate client Handshake.
//						if (c.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.CONNECTING)
//						{
//						}
						//Send Sever Handshake to client.
						if (c.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.CONNECTING)
						{
							Byte[] bytes = new Byte[512];
							while (n.DataAvailable){
								n.Read(bytes, 0, bytes.Length);
							}
							//translate bytes of request to string
							String data = Encoding.UTF8.GetString(bytes);
							string handshake =
							"HTTP/1.1 101 Switching Protocols" + "\r\n"
							+ "Connection: Upgrade" + "\r\n"
							+ "Upgrade: websocket" + "\r\n"
							+ "Sec-WebSocket-Accept: " + Convert.ToBase64String(
								System.Security.Cryptography.SHA1.Create().ComputeHash(
									Encoding.UTF8.GetBytes(
										new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
									)
								)
							) + "\r\n"
							+ "\r\n";
							streamWriter.Write(handshake);
							streamWriter.Flush();
							c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.HANDSHAKEDONE;
						}

						// Read data from client. Do whatever is required.
//						while (n.DataAvailable && c.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.HANDSHAKEDONE)
//						{
//							if (n.ReadByte() == 'S')
//								if (n.ReadByte() == 'T')
//								{
//									b = n.ReadByte();
//									if (b == 'P')
//									{
//										c.IsSubscribed = false;
//										_plugin.Info("Client:" + c.ManagingThreadId.ToString() + " unsubscribed");
//									}
//									else if (b == 'R')
//									{
//										c.IsSubscribed = true;
//										_plugin.Info("Client:" + c.ManagingThreadId.ToString() + " subscribed");
//									}
//								}
//								else
//									_plugin.Info("Client " + c.ManagingThreadId.ToString() + "says :" + n.ReadByte().ToString());
//						}

						// If Writebuffer is full, write stuff to client
						if (c.WriteBuffer != null && c.WriteBuffer.Length > 0)
						{
							n.WriteByte(0x00);
							n.Write(c.WriteBuffer, 0, c.WriteBuffer.Length);
							n.WriteByte(0xff);
							c.WriteBuffer = null;
						}

					}
				}
			}
			catch (Exception e)
			{
				if (c.TcpClientInstance.Connected == true)
				{ _plugin.Info(e.StackTrace); }
				return;
			}
			finally
			{
				c.TcpClientInstance.Close();
				_plugin.Info("Client:" + c.ManagingThreadId.ToString() + " closed");
				c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.DISCONNECTED;
			}

		}

		public void WriteData(string data)
		{

			foreach (WebSockClient wc in _clientList)
			{
				try
				{
					if (wc.TcpClientInstance.Connected && wc.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.HANDSHAKEDONE)
					{
						wc.WriteBuffer = UTF8Encoding.UTF8.GetBytes(data);
					}

				}
				catch
				{
					_plugin.Info("Writing to client failed.Closing client.");
					wc.TcpClientInstance.Close();
				}
			}

		}
	}
}