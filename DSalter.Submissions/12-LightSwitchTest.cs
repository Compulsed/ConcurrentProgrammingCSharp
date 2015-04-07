using System;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

namespace DSalter.Submissions
{
	public class _2_LightSwitchTest
	{
		public static Mutex writePermission = new Mutex();
		public static LightSwitch LS = new LightSwitch(writePermission);


		public class ReaderThread : ActiveObject
		{
			public ReaderThread(string name) : base(name){}

			protected override void Run ()
			{
				while (true) {
					LS.Acquire ();

					Console.WriteLine(base.ToString() + " is reading");
					Thread.Sleep (1000);

					LS.Release ();

					Console.WriteLine (base.ToString() + " has finished reading!");
					Thread.Sleep (10000);
				}
			}
		}

		public class WriterThread : ActiveObject
		{
			public WriterThread(string name) : base(name){}

			protected override void Run()
			{
				while (true) {
					writePermission.Acquire ();

					Console.WriteLine ("<< " + base.ToString() + " is writing");
					Thread.Sleep (10000);

					writePermission.Release ();
					Console.WriteLine (">>" + base.ToString() + " has finished writing");
					Thread.Sleep (10000);
				}
			}
		}


		public static void TestOne()
		{
			ReaderThread[] readers = new ReaderThread[5];
			WriterThread[] writers = new WriterThread[2];

			for (int i = 0; i < readers.Length; ++i) {
				readers [i] = new ReaderThread ("#" + i);
				readers [i].Start ();
			}

			for (int i = 0; i < writers.Length; ++i) {
				writers [i] = new WriterThread ("#" + i);
				writers [i].Start ();
			}
		}

		public static void Main()
		{

			TestOne ();

			return;
		}
	}
}

