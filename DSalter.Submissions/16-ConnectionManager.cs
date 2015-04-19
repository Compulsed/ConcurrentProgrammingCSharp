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

		public static void Main()
		{
			MessageEchoer One = new MessageEchoer ();
			One.Start ();

			Console.WriteLine ("Running the ConnectionManager!");

			Console.ReadLine ();

			return;
		}
	}
}

