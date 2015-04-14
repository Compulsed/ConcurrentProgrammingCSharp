using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using System.Text;


namespace DSalter.ConcurrentUtils
{
	public class ConnectionManager : ActiveObject
	{
		Socket _mainSocket;
		IPEndPoint _iep;

		List<Socket> _socketList = new List<Socket>();

		byte[] _data;
		string _stringData;
		int _recv;

		MessageEchoer _echoer = new MessageEchoer ();

		public ConnectionManager (UInt16 mainPort = 8000, int maxConnectionQueue = 30)
		{
			_mainSocket = new Socket (AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);

			_iep = new IPEndPoint (IPAddress.Any, mainPort);

			_mainSocket.Bind (_iep);
			_mainSocket.Listen (maxConnectionQueue);
			_socketList.Add (_mainSocket);

			_echoer.Start ();
		}


		protected override void Run ()
		{
			Console.WriteLine ("Now accepting connections");

			List<Socket> socketListCopy;
			while (true) {

				socketListCopy = new List<Socket> (_socketList);


				Socket.Select (socketListCopy, null, null, -1);

				// if the returned socket is the one listening for connections
				// We now need to process the new connection, and then remove it
				if (socketListCopy [0] == _mainSocket) {
					Socket client = socketListCopy [0].Accept ();
					_socketList.Add (client);
					socketListCopy.Remove (_mainSocket);

					Console.WriteLine ("Established a new connection with: {0}", ((IPEndPoint)client.RemoteEndPoint).ToString ());
				}


				foreach (Socket client in socketListCopy) {
					_data = new byte[1024];
					_recv = client.Receive (_data);
					_stringData = Encoding.ASCII.GetString (_data, 0, _recv);

					Console.WriteLine ("Received: {0}From: {1}", _stringData, ((IPEndPoint)client.RemoteEndPoint).ToString ());

					if (_recv == 0) {
						Console.WriteLine ("Client {0} disconnected.", ((IPEndPoint)client.RemoteEndPoint).ToString ());
						client.Close ();
						_socketList.Remove (client);
					}
					else {
						// TODO: Figure out how to decouple this echoer
						_echoer._inputChannel.Put (new Tuple<Socket, String>(client, _stringData));
					}
				}


			}
		}
	}
}

