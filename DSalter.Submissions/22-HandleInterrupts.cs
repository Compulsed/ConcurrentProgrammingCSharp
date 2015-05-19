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
					Console.WriteLine("Running Thread!");

					try {
						testSema.Acquire();
					} 
					catch (Exception){}

					Console.WriteLine("HERE!");
				});

				One.Start ();

				Thread.Sleep (1000);
				One.Interrupt ();

			}

		}

		class ChannelForceReleaser
		{

			static Channel<string> testChannel = new Channel<string> ();

			public static void TestOne()
			{

				Thread One = new Thread (() => {
					Console.WriteLine("Running Thread!");

					string s = "";
					try {
						testChannel.Offer(s);
					} catch (Exception) {}

					Console.WriteLine("HERE!");
				});

				One.Start ();

				Thread.Sleep (1000);
				One.Interrupt ();

			}


		}

		public static void Main()
		{
			// SemaphoreForceRelease.TestOne ();
			ChannelForceReleaser.TestOne ();


			return;
		}

	}
}

