using System;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;
/*
 * This tets proves that
 * 1. All thread block while the number that have arrived is less than the number
 * 		required by the barrier
 * 
 * 2. When the nth thread arrives, all threads proceed.
 * 
 * 3. The barrier can be reused without ut being possible for a thread to "arrive" twice
 * 		for the one group
 * 
 */

namespace DSalter.Submissions
{
	public class _1_BarrierTest
	{
		public const UInt64 barrierThreshold = 5;
		public static Barrier barrierOne = new Barrier(5);

		public class TestThread : ActiveObject
		{
			public TestThread(string threadName) : base(threadName) {}

			protected override void Run()
			{
				while (true) {
					Thread.Sleep (2000);

					if (barrierOne.Arrive ()) {
						Console.WriteLine ("{0} - is the last thread to the barrier!", base.ToString ());
					} else {
						Console.WriteLine ("{0} - did not come last!", base.ToString ());
					}
				}
			}
		}

		/*
		 * 1. By the user deciding on how many threads to start, the barrier will not break
		 * 		until that number is shit
		 * 
		 * 2. Same as above, all threads will process when the barrier limit has been reached
		 * 
		 * 3. This is hard to prove, but with the added turnstile one should be able to prove
		 * 		that with stepping through the code this is valid
		 * 
		 */
		public static void TestOneTwoThree()
		{
			const UInt64 noThreads = 100;

			TestThread[] threads = new TestThread[noThreads];

			Console.WriteLine ("Press any key to create a thread:");
			Console.WriteLine ("Barrier threshold has been set to: {0}", barrierThreshold);

			for (int i = 0; i < threads.Length; ++i) {
				Console.ReadLine ();
				threads [i] = new TestThread ("#" + i);
				threads [i].Start ();
				Console.WriteLine ("Thread {0} has been created and started!", i);
			}
		}

		public static void Main()
		{
			TestOneTwoThree ();


			return;
		}
	}
}

