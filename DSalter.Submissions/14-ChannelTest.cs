using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	public class _4_ChannelTest
	{
		public class Consumer : ActiveObject
		{
			private Channel<string> stringChannel;
			private UInt64 timeout;

			public Consumer(string name, Channel<string> stringChannel, UInt64 timeout = 1000) : base(name)
			{
				this.stringChannel = stringChannel;
				this.timeout = timeout;
			}

			protected override void Run()
			{
				while (true) {
					Thread.Sleep ((int)timeout);
					Console.WriteLine (" >> " + base.ToString () + ": has consumed - " + stringChannel.Take ());
				}
			}
		}

		public class Producer : ActiveObject
		{
			private Channel<string> stringChannel;
			private UInt64 timeout;

			public Producer(string name, Channel<string> stringChannel, UInt64 timeout = 1000) : base(name){
				this.stringChannel = stringChannel;
				this.timeout = timeout;
			}

			protected override void Run()
			{
				UInt64 dataNumber = 0;

				while (true) {
					Thread.Sleep ((int)timeout);
					dataNumber++;
					stringChannel.Put ("Data" + dataNumber.ToString ());
					Console.WriteLine (" << " + base.ToString () + ": has produced Data" + dataNumber.ToString ());
				}

			}
		}

		public static void TestOne()
		{
			Channel<String> stringChannel = new Channel<string> ();

			new Consumer ("Consumer 1", stringChannel).Start ();
			new Consumer ("Consumer 2", stringChannel).Start ();
			new Consumer ("Consumer 3", stringChannel).Start ();
			new Consumer ("Consumer 4", stringChannel).Start ();
			new Consumer ("Consumer 5", stringChannel).Start ();

			new Producer ("Producer 1", stringChannel).Start ();
			new Producer ("Producer 2", stringChannel).Start ();
			new Producer ("Producer 3", stringChannel).Start ();
			new Producer ("Producer 4", stringChannel).Start ();
			new Producer ("Producer 5", stringChannel).Start ();
		}

		public static void TestTwo()
		{
			BoundedChannel<String> stringChannel = new BoundedChannel<string> (1);

			new Consumer ("Consumer 1", stringChannel).Start ();
			new Consumer ("Consumer 2", stringChannel).Start ();
			new Consumer ("Consumer 3", stringChannel).Start ();
			new Consumer ("Consumer 4", stringChannel).Start ();
			new Consumer ("Consumer 5", stringChannel).Start ();

			new Producer ("Producer 1", stringChannel).Start ();
		}

		public static void Main()
		{
			// TestOne ();
			TestTwo	();

			return;
		}
	}
}

