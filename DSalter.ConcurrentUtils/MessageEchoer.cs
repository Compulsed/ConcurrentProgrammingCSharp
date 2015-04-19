using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using System.Text;

namespace DSalter.ConcurrentUtils
{
	public class MessageEchoer : ChannelActiveObject<Connection>
	{
		ConnectionManager _a;


		public MessageEchoer()
		{
			_a = new ConnectionManager (base._inputChannel);
			_a.Start ();
		}

		protected override void Process (Connection con)
		{
			ConnectionWithMessage aMessageCon = (ConnectionWithMessage)con;


			con.writer.WriteLine (aMessageCon.message);

			Console.WriteLine (aMessageCon.message);
		}
	}
}

