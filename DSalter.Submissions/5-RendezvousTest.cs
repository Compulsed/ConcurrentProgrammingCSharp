using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	/// <summary>
	/// Tests show that.
	/// 
	/// When one thread arrives it does not continue (TestTwo)
	/// When a second thread arrives, both the first and second threat proceed (TestOne)
	/// This pattern can be repeated (TestOne/TestTwo)
	/// 
	/// Author: Dale Salter 9724397
	/// </summary>
	public class __Rendezvous
	{
		static Rendezvous meetingPoint;

		// Has multiple fast threads and one slow one, the time at which the threads can pass will be 
		// 	dependent on the slowest thread
		public static void ThreadOperation()
		{
			while (true) {
				if (Thread.CurrentThread.Name.Equals ("1"))
					Thread.Sleep (2000);

				meetingPoint.Arrive ();
				Console.WriteLine (Thread.CurrentThread.Name + " Passed meeting point");
			}
		}

		/* -----------------------------
		 *  	Test One
		 * ----------------------------
		 */
		// Proves that threads can proceed and it is in a reusable pattern
		// Slows one thread, so prove that the others are slowed along with it, this proves that the threads do not race ahead
		// Rendezvous, this is done internally by counting how many threads have passed, and a turnstile
		public static void TestOne()
		{
			Thread[] threadCollection = {
				new Thread (ThreadOperation),
				new Thread (ThreadOperation),
				new Thread (ThreadOperation)
			};

			meetingPoint = new Rendezvous (threadCollection.Length);

			// Gives the thread a name and starts each of them
			int threadNumber = 0;
			foreach (Thread thread in threadCollection) {
				thread.Name = (++threadNumber).ToString();
				thread.Start ();
			}
		}

		/* -----------------------------
		 *  	Test Two
		 * ----------------------------
		 */
		// Proves that the first thread does not continue until the second hits the Rendezvous
		// The second thread starts when a user presses a key
		public static void TestTwo()
		{
			Thread[] threadCollection = {
				new Thread (ThreadOperation),
				new Thread (ThreadOperation)
			};

			// Will purposely stop one of the threads, until the second one starts
			meetingPoint = new Rendezvous (2); 

			// Gives the thread a name and starts each of them
			int threadNumber = 0;
			threadCollection[0].Name = (++threadNumber).ToString();
			threadCollection[0].Start ();

			Console.WriteLine ("Pressing any key allows to other thread to go to the Rendezvous");
			Console.ReadLine ();
			Console.WriteLine ("Key pressed!");

			threadCollection[1].Name = (++threadNumber).ToString();
			threadCollection[1].Start ();
		}

		public static void Main()
		{
			TestOne ();
			// TestTwo();
		}
	}
}

