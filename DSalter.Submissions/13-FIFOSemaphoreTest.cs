using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	/// <summary>
	/// Tests show that.
	///  
	/// Threads cannot acquire until a token arrives (TestOneTwo)
	/// 
	/// When multiple threads are waiting to acquire, and one token is released, only one of the
	/// 	waiting threads proceeds (TestOneTwo)
	/// 
	/// When multiple threads are waiting to acquire, and the multiple tokens are released the number
	/// 	of threads that process equal the number of tokens released (TestThree)
	/// 
	/// When a token is released to a Semaphore with no waiting threads, the next thread to Acquire
	/// 	will process without waiting (TestFour)
	/// 
	/// 
	/// Author: Dale Salter (9724397)
	/// </summary>
	public class __FIFOSemaphoreTest
	{
		/* -----------------------------
		 *  	Test One, Two
		 * ----------------------------
		 */

		static FIFOSemaphore writePermission = new FIFOSemaphore(1);


		static void Write()
		{
			Console.WriteLine ("Starting Write()");

			while (true) {
				Thread.Sleep (1000);

				writePermission.Acquire ();
				{
					Console.Write (Thread.CurrentThread.Name + ": ");
					for (int i = 0; i < 10; ++i)
						Console.Write (i + " ");
					Console.WriteLine ("");
				}
				writePermission.Release ();
			}
		}

		// Logic - Only one of the three threads can write at a time
		static void TestOneTwo()
		{
			Thread one = new Thread (Write);
			Thread two = new Thread (Write);
			Thread three = new Thread (Write);

			one.Name = "Thread One";
			two.Name = "\t Thread Two";
			three.Name = "\t\t Thread Three";

			one.Start ();
			two.Start ();
			three.Start ();
		}


		/* -----------------------------
		 *  	Test Three
		 * ----------------------------
		 */
		static object LockObject = new object();
		static FIFOSemaphore barrierPermission = new FIFOSemaphore(0);
		static int count = 0;

		static void AttemptToComplete()
		{

			lock (LockObject) {
				++count;

				if (count == 5)
					barrierPermission.Release (5);
			}

			Console.WriteLine (Thread.CurrentThread.Name + " is waiting for the last thread");
			barrierPermission.Acquire ();

			Console.WriteLine (Thread.CurrentThread.Name + " has finished!");
		}

		// Test 3 - Logic, 5 tokens get released at the same time, 5 should complete, 1 should not
		static void TestThree()
		{
			Thread[] threads = { 
				new Thread (AttemptToComplete),
				new Thread (AttemptToComplete), 
				new Thread (AttemptToComplete), 
				new Thread (AttemptToComplete), 
				new Thread (AttemptToComplete), 
				new Thread (AttemptToComplete) // 6 Threads to show that it should be working
			};

			foreach (Thread thread in threads)
				thread.Start ();
		}

		/* -----------------------------
		 *  	Test Four
		 * ----------------------------
		 */
		static FIFOSemaphore testPermission = new FIFOSemaphore(1);

		static void Runner()
		{
			Thread.Sleep (1000); // Potentially gives enough time for Runner() to have completed

			testPermission.Acquire ();
			{
				Console.WriteLine ("Inside of Runner()");
			}
			testPermission.Release ();

		}

		static void Releaser()
		{
			testPermission.Acquire ();
			{
				Console.WriteLine ("Inside of Releaser()");
			}
			testPermission.Release ();
		}

		// Test 4 - Logic it should avoid the while loop inside of the Semaphore class because it will have 1 Token at all times
		static void TestFour(){
			Thread releaser = new Thread (Releaser);
			Thread runner = new Thread (Runner);

			releaser.Start ();
			runner.Start ();
		}

		static FIFOSemaphore howMuchToWriteSema = new FIFOSemaphore(0);

		public static void OtherThreadFive()
		{
			howMuchToWriteSema.Acquire ();

			Console.WriteLine ("1 in total has been released");

			howMuchToWriteSema.Acquire ();
			howMuchToWriteSema.Acquire ();

			Console.WriteLine ("3 in total has been released");

			howMuchToWriteSema.Acquire ();
			howMuchToWriteSema.Acquire ();
			howMuchToWriteSema.Acquire ();
			howMuchToWriteSema.Acquire ();

			Console.WriteLine ("7 in total has been released");
		}

		// Test 5 - Using multiple resources 
		public static void TestFive()
		{
			new Thread (OtherThreadFive).Start ();

			Console.WriteLine ("Pressing enter releases Semaphore");
			while (true) {
				Console.ReadLine ();
				howMuchToWriteSema.Release (1);
			}

		}


		/* -----------------------------
		 *  	Entry
		 * ----------------------------
		 */
		public static void Main(){
			Console.WriteLine ("Inside __SemaphoreTest");

			TestOneTwo ();
			// TestTwo ();
			// TestThree ();
			// TestFive ();

			Console.ReadLine ();
		}
	}
}