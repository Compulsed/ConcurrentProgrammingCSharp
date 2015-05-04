using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace DSalter.ConcurrentUtils
{

	public class Connection
	{
		private StreamReader _reader;
		private StreamWriter _writer;
		private Socket _client;
		private NetworkStream _ns;

		public Connection(Socket client)
		{
			_ns = new NetworkStream (client);

			_reader = new StreamReader (_ns);
			_writer = new StreamWriter (_ns);
			_client = client;
		}

		public Connection(Connection conn)
		{
			_reader = conn.reader;
			_writer = conn.writer;
			_client = conn.client;
			_ns = conn._ns;
		}


		public StreamReader reader
		{
			get { return _reader; } 
		}

		public StreamWriter writer
		{
			get { return _writer; }
		}

		public Socket client
		{
			get { return _client; }
		}

		public NetworkStream ns
		{
			get { return _ns; }
		}
	}

	public class ConnectionWithMessage : Connection 
	{
		private string _message;

		public ConnectionWithMessage(Socket client) : base(client)
		{
			_message = base.reader.ReadLine ();
		}

		public ConnectionWithMessage(Connection passedConn) : base(passedConn)
		{
			_message = base.reader.ReadLine ();
		}

		public string message
		{
			get { return _message; }
		}
	 } 

	public class ConnectionManager : ActiveObject
	{
		Socket _mainSocket;
		IPEndPoint _iep;
		Channel<Connection> _outputChannel;

		List<Socket> _socketList = new List<Socket>();

		public ConnectionManager (Channel<Connection> outputChannel, UInt16 mainPort = 8000, int maxConnectionQueue = 30)
		{
			_mainSocket = new Socket (AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);

			_iep = new IPEndPoint (IPAddress.Any, mainPort);

			_outputChannel = outputChannel;

			_mainSocket.Bind (_iep);
			_mainSocket.Listen (maxConnectionQueue);
			_socketList.Add (_mainSocket);

		}

		protected override void Run ()
		{
			Console.WriteLine ("Now accepting connections");

			List<Socket> socketListCopy;
			while (true) {
				socketListCopy = new List<Socket> (_socketList);


				Socket.Select (socketListCopy, null, null, -1);

				// if the returned socket is the one listening for connections
				// We now need to process the new connection, and then remove it from further processing
				if (socketListCopy [0] == _mainSocket) {
					Socket client = socketListCopy [0].Accept ();
					_socketList.Add (client);
					socketListCopy.Remove (_mainSocket);

					Console.WriteLine ("Established a new connection with: {0}", ((IPEndPoint)client.RemoteEndPoint).ToString ());
				}


				foreach (Socket client in socketListCopy) {

					// If there in no data on the socket, but something has changed on the socket state, it has been disconnected
					if (client.Available == 0) {
						Console.WriteLine ("Client {0} disconnected.", ((IPEndPoint)client.RemoteEndPoint).ToString ());
						client.Close ();
						_socketList.Remove (client);
					}
					else {
						// How can I deatch this?
						// Must do the reading on this thread or bugs will happen

						_outputChannel.Put (new ConnectionWithMessage (client));
					}
				}


			}
		}
	}
}

