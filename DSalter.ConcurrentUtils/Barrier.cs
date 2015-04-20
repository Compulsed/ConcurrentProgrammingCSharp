using System;
using DSalter.ConcurrentUtils;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// Allows for synchronization by blocking threads that reach a certain point until
	/// 	a given number of threads is hit. Then releases all the threads again. 
	///		The last thread will return true
	/// 
	///	Problems this barrier attempts to avoid
	/// 	- Allowing one thread to go through twice, this is fixed with the turnstile 
	///
	/// Author: Dale Salter 9724397
	/// </summary>
	public class Barrier
	{
		Semaphore barrierExitPermission = new Semaphore(0);
		Mutex turnstile = new Mutex();		// Used to satisfy test #3

		UInt64 threadsArrived;				// Number of threads waiting to move forward
		readonly UInt64 threadsToContinue;	// Number of required threads for them to move forward

		/// <summary>
		/// A thread will operate this inside their execution, it will block the thread
		/// until the amount of threads that the rendezvous requires to be waiting before moving forward
		///
		/// The last thread to reach the barrier will return true,
		/// 	all other threads will return false
		/// </summary>
		public bool Arrive()
		{
			turnstile.Acquire ();
			lock (this) {
				++threadsArrived;

				// Checks to see if the required thread count is met, if so the barrier is broken
				if (threadsArrived == threadsToContinue) {
					--threadsArrived;

					barrierExitPermission.Release (threadsToContinue - 1); 
					return true; // The first thread returns with a true status
				}
			}
			turnstile.Release();

			barrierExitPermission.Acquire (); // Blocks all of the threads threads here

			lock (this) {
				--threadsArrived;

				if (threadsArrived == 0) {
					turnstile.Release ();
				}
			}

			return false; // All other threads return will a false status
		}

		/// <summary>
		/// Barrier synchronizes a specified number of threads, the default is 2 this can be
		/// extended to any another number. It synchronizes the group by blocking all threads that reach
		/// Arrive() until the specified number of threads hit arrive.
		/// </summary>
		public Barrier() : this (2) {}

		/// <summary>
		/// Same as Barrier() but allows for a specific number of threads to wait 
		/// </summary>
		/// <param name="threadsToContiue">Threads at rendezvous point required before progressing</param>
		/// names that went through the barrier.</param>
		public Barrier (UInt64 threadsToContiue)
		{
			this.threadsToContinue = threadsToContiue;
		}



	}
}

