using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;


// Connection Manager
// Message Echoer

// Create socket to accept new connections
// Socket.Select on the active Object, until either new connection or connection with data
// For every Socket.Select
// 	- accept new connections if the socket is a the server socket...? You must add these to the managed connections within connection manager
// 	- Read the line of text passed to the socket, and then pass both the text and socket to an output channel

// Test by a Channel Active Object, that echos back and to the servers terminal

namespace DSalter.Submissions
{
	public class _6_ConnectionManager
	{


		public static void ExampleUse()
		{

			ArrayList sockList = new ArrayList(2);
			ArrayList copyList = new ArrayList(2);
			Socket main = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);

			IPEndPoint iep = new IPEndPoint(IPAddress.Any, 9050);
			byte[] data = new byte[1024];
			string stringData;
			int recv;


			main.Bind(iep);
			main.Listen(2);

			Console.WriteLine("Waiting for 2 clients...");
			Socket client1 = main.Accept();
			IPEndPoint iep1 = (IPEndPoint)client1.RemoteEndPoint;
			client1.Send(Encoding.ASCII.GetBytes("Welcome to the server"));
			Console.WriteLine("Connected to {0}", iep1.ToString());
			sockList.Add(client1);

			Console.WriteLine("Waiting for 1 more client...");
			Socket client2 = main.Accept();
			IPEndPoint iep2 = (IPEndPoint)client2.RemoteEndPoint;
			Console.WriteLine("Connected to {0}", iep2.ToString());    
			client2.Send(Encoding.ASCII.GetBytes("Welcome to the server"));
			sockList.Add(client2);
			main.Close();

			while(true)
			{
				copyList = new ArrayList(sockList);
				Console.WriteLine("Monitoring {0} sockets...", copyList.Count);

				Console.WriteLine ("Before Select");
				Socket.Select(copyList, null, null, 10000000);
				Console.WriteLine ("After Select");

				foreach(Socket client in copyList)
				{

					data = new byte[1024];
					recv = client.Receive(data);
					stringData = Encoding.ASCII.GetString(data, 0, recv);
					Console.WriteLine("Received: {0}", stringData);
					if (recv == 0)
					{
						iep = (IPEndPoint)client.RemoteEndPoint;
						Console.WriteLine("Client {0} disconnected.", iep.ToString());
						client.Close();
						sockList.Remove(client);
						if (sockList.Count == 0)
						{
							Console.WriteLine("Last client disconnected, bye");
							return;
						}
					}
					else
						client.Send(data, recv, SocketFlags.None);

				}
			}
		}



		public static void Main()
		{
			ConnectionManager A = new ConnectionManager (8000);

			A.Start ();

			Console.WriteLine ("Running the ConnectionManager!");

			Console.ReadLine ();

			return;
		}
	}
}

