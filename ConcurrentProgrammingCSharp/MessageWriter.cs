using System;
using System.Threading;

using DSalter.ConcurrentUtils;

namespace DSalter.Assignment
{

	public class MessageWriter : ActiveObject 
	{
		private readonly string toPrint;

		/// <summary>
		/// Is an active object, which stores the thread name and a message
		/// 	and just prints a loop of that message
		/// </summary>
		/// <param name="toPrint">A message that will keep looping over and printing</param>
		/// <param name="threadName">A name of that act object's thread</param>
		public MessageWriter(string toPrint, string threadName) : base(threadName)
		{
			this.toPrint = toPrint;
		}

		protected override void Run ()
		{
			while (true) {
				Console.WriteLine (toPrint);
				Thread.Sleep (1000);
			}
		}

	}

}

