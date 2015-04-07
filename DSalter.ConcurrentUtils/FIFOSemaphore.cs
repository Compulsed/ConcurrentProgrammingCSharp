using System;
using System.Collections.Generic;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// A FIFO Semaphore differs from a normal semaphore because it offers a queuing
	/// 	mechanism that will ensure that the first thread to acquire it will be the
	/// 	first semaphore that is released.
	///
	/// Author: Dale Salter 9724397
	/// </summary>
	public class FIFOSemaphore : Semaphore
	{
		Queue<Semaphore> threadsWaitingQueue = new Queue<Semaphore>();
		private Object queueLock = new Object();

		/// <summary>
		/// Instantiates the class with a given number of tokens, the default amount is 0
		/// </summary>
		/// <param name="initialTokens">Initial tokens.</param>
		public FIFOSemaphore (UInt64 initialTokens = 0) : base(initialTokens){}

		/// <summary>
		/// Use like standard Acquire, if there are not enough tokens
		/// 	the thread will be put on a queue and pulled off fairly,
		/// 	this avoids starvation
		/// </summary>
		public override void Acquire ()
		{
			Semaphore threadSemaphore;

			// As there is interaction with the queue
			lock (queueLock) {
				// As there is interaction with the object count and avoids calling base.Release()
				lock (base.objectLock) {
					// If queue is empty and there are enough tokens it by passes queueing
					if ((base._count > 0) && (threadsWaitingQueue.Count == 0)) {
						base._count--;
						return; // Thread now may continue
					}
					// We must queue the thread by locking on a Semaphore
					else {
						threadSemaphore = new Semaphore (0);
						threadsWaitingQueue.Enqueue (threadSemaphore);
					}
				}
			}

			// Must release all locks so other threads can queue at the same time
			// TODO: Can the item be put off the queue before it is Acquired?
			//			May not matter because it is a local reference
			threadSemaphore.Acquire ();
		}

		/// <summary>
		///	Releases tokens back to the Semaphore and will dequeue any waiting threads
		/// </summary>
		/// <param name="releaseTokens">Number of tokens to release</param>
		public override void Release (UInt64 releaseTokens = 1)
		{
			// As there is another interaction with the queue
			lock (queueLock) {
				UInt64 queueSize = (UInt64)threadsWaitingQueue.Count;

				while (queueSize > 0 && releaseTokens > 0) {
				 	threadsWaitingQueue.Dequeue ().Release ();

					--queueSize;
					--releaseTokens;
				}

				base.Release (releaseTokens);
			}
		}
	}
}
