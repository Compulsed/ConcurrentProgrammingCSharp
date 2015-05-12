using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

namespace DSalter.Submissions
{
	public class _1_Timeouts
	{

		public class SemaphoreTest
		{
			public static Semaphore noTokens = new Semaphore(0);
			public static Semaphore soonTokens = new Semaphore (0);

			public static void TestUnlimitedWait()
			{

				new Thread (() => {


					Console.WriteLine("Attempting to Acquire");
					noTokens.TryAcquire(-1);
					Console.WriteLine("Acquired!");


				}).Start ();

			}


			public static void TestWithATimeOut(int timeout)
			{
				new Thread (() => {

					while(true){
						Console.WriteLine("Attempting to Acquire with a timeout");

						if(soonTokens.TryAcquire(timeout)){
							Console.WriteLine("++ Acquired the Semaphore!");
						} 
						else {
							Console.WriteLine("-- Timed out, moving on!");

							Console.WriteLine("?? Releasing Semaphore for next time!");
							soonTokens.Release(1);
						}
					}


				}).Start ();
			}
		}


		public class ChannelTest
		{
			static Channel<string> testChannel = new Channel<string> ();


			// Tests the funcionality of Poll & Put
			public static void TestOne()
			{

				// Consumers - POLL
				for (int i = 0; i < 2; ++i) {

					int threadId = i;
					string tempString = "";

					new Thread (() => {

						while(true){
							if(testChannel.Poll(out tempString, 1000)){
								Console.WriteLine(threadId + ": Got {0} off of the queue", tempString);
							} 
							else {
								Console.WriteLine(threadId +  ": Gave up waiting on the queue!");
							}
						}
					}).Start ();
				}

				// Producer - OFFER
				new Thread (() => {
					for (int i = 0; i < 1000; ++i){
						testChannel.Offer ("" + i);
						Thread.Sleep(3000);
					}
				}).Start ();
			}

			// Tests functionality of Off
			public static void TestTwo()
			{

			}
		}


		public class BoundedChannelTest
		{
			static BoundedChannel<string> testChannel = new BoundedChannel<string> (3);


			public static void TestOne()
			{

				// Producer - 
				for (int i = 0; i < 2; ++i) {

					int threadId = i;

					new Thread (() => {

						int productionID = 0;

						while(true){
							if(testChannel.Offer("[" + threadId + ":" + productionID + "]", 1000)){
								Console.WriteLine("[Thread: " + threadId + "] >> " + productionID + " >> Q");
								productionID++;
							} 
							else {
								Console.WriteLine("[Thread: " + threadId + "] XX " + productionID + " XX Q");
							}
						}
					}).Start ();

				}

				// Consumer
				new Thread (() => {

					string t = "";

					while(true){
						Thread.Sleep(5000);

						// testChannel.Poll(out t);
					 	t = testChannel.Take();

						Console.WriteLine("Consumer took: " +  t + " off of the queue");
					}
				}).Start ();
			}

		}


		public static void Main()
		{
			Console.WriteLine("Inside timeout!");

			// SemaphoreTest.TestWithATimeOut (3000);
			// ChannelTest.TestOne();
			BoundedChannelTest.TestOne();

			// TestUnlimitedWait ();

		}

	}
}

