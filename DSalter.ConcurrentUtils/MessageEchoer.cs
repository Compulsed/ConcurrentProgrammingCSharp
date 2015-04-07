using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using System.Text;

namespace DSalter.ConcurrentUtils
{
	public class MessageEchoer : ChannelActiveObject<Tuple<Socket, string>>
	{
		public MessageEchoer(){}

		protected override void Process (Tuple<Socket, string> passedData)
		{
			Socket socket = passedData.Item1;
			string message = "Echo back -> " + passedData.Item2;

			byte[] messageBytes = new byte[message.Length * sizeof(char)];
			System.Buffer.BlockCopy (message.ToCharArray (), 0, messageBytes, 0, messageBytes.Length);

			socket.Send (messageBytes, messageBytes.Length, SocketFlags.None);

			Console.WriteLine (message + "\n");
		}
	}
}

