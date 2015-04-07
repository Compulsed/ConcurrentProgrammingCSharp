using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	/// Author: Dale Salter 9724397
	public class __LatchTest
	{

		static Latch latchOne = new Latch();

		public static void OtherThread()
		{
			Console.WriteLine ("Is waiting for latch to be released!");
			latchOne.Acquire ();
			// Shows that all the waiting threads will be released and that they are blocked with Acquire
			Console.WriteLine ("Latch has been released!");
		}

		public static void TestOne()
		{
			Console.WriteLine ("Threads will soon be started");
			for (int i = 0; i < 10; ++i)
				new Thread (OtherThread).Start ();


			Console.WriteLine ("Press any key to release the latch!");
			Console.ReadLine ();

			latchOne.Release ();

			// Shows that it does not block once the latch has been released
			new Thread (OtherThread).Start ();
		}

		public static void Main(){
			TestOne ();

			return;
		}


	}
}

