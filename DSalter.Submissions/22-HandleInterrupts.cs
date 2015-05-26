using System;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

namespace DSalter.Submissions
{
	public class _2_HandleInterrupts
	{

		class SemaphoreForceRelease
		{

			static Semaphore testSema = new Semaphore();

			public static void TestOne()
			{


				Thread One = new Thread (() => {
					try {
						testSema.Acquire();
					} 
					catch (Exception e){
						Console.WriteLine("testSema.Acquire() interuptCalled -> {0}", e.GetType());
					}
				});

				One.Start ();

				Console.WriteLine ("Press any key to interrupt the Semaphore");
				Console.ReadLine ();

				One.Interrupt ();
			}

		}

		class ChannelForceReleaser
		{

			static Channel<string> testChannel = new Channel<string> ();

			public static void TestOne()
			{

				Thread One = new Thread (() => {
					string s = "";
					try {
						testChannel.Offer(s);
					} catch (Exception e) {
						Console.WriteLine("testChannel.Take() interuptCalled -> {0}", e.GetType());
					}
				});
				One.Start ();

				Console.WriteLine ("Press any key to interrupt the Channel");
				Console.ReadLine ();

				One.Interrupt ();

			}


		}

		public static void Main()
		{
			SemaphoreForceRelease.TestOne ();
			ChannelForceReleaser.TestOne ();

			return;
		}

	}
}

