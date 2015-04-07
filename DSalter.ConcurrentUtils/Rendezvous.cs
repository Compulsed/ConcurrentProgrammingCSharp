using System;
using System.Threading;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// Allows for synchronization by blocking threads that reach a certain point until
	/// 	a given number of threads is hit. Then releases all the threads again. 
	/// 
	/// Author: Dale Salter 9724397
	/// </summary>
	public class Rendezvous
	{
		DSalter.ConcurrentUtils.Semaphore barrierPermission = new DSalter.ConcurrentUtils.Semaphore(0);
		DSalter.ConcurrentUtils.Semaphore turnstile = new DSalter.ConcurrentUtils.Semaphore(0);

		UInt64 threadsArrived;				// Number of threads waiting to move forward
		readonly UInt64 threadsToContinue;	// Number of required threads for them to move forward

		/// <summary>
		/// A thread will operate this inside their execution, it will block the thread
		/// until the amount of threads that the rendezvous requires to be waiting before moving forward
		/// </summary>
		public void Arrive()
		{
			lock (this) {
				++threadsArrived;

				// Checks to see if the required thread count is met, if so the barrier is broken
				if (threadsArrived == threadsToContinue) {
					barrierPermission.Release (threadsToContinue); 
				}
			}

			barrierPermission.Acquire (); // Blocks all of the threads threads here

			// Critical Section

			lock (this) {
				--threadsArrived;

				if (threadsArrived == 0) {
					turnstile.Release (threadsToContinue);
				}
			}
				

			turnstile.Acquire ();
		}

		/// <summary>
		/// Rendezvous synchronizes a specified number of threads, the default is 2 this can be
		/// extended to any another number. It synchronizes the group by blocking all threads that reach
		/// Arrive() until the specified number of threads hit arrive.
		/// </summary>
		/// names that went through the barrier</param>
		public Rendezvous () : this (2) {}

		/// <summary>
		/// Same as Rendezvous() but allows for a specific number of threads to wait 
		/// </summary>
		/// <param name="threadsToContiue">Threads at rendezvous point required before progressing</param>
		/// names that went through the barrier.</param>
		public Rendezvous (UInt64 threadsToContiue)
		{
			this.threadsToContinue = threadsToContiue;
		}
		


	}
}

