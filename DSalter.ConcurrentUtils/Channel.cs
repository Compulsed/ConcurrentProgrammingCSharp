using System;
using System.Collections.Generic;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;
using System.Threading;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// This channel is a thread safe queue that allows the clients to queue and
	/// 	dequeue generic data to it. 
	///
	/// 	Threads will block putting data on the channel if it is full
	/// 	Threads will also block if there is no data to take off the channel
	/// 
	/// 	With the additional option to include polling when taking information off 
	/// 		off the channel
	/// 
	/// 	This Utility includes threadsafe methods, so that you can exit the utility early
	/// 		if it is blocking
	/// 
	/// Author: Dale Salter (9724397)
	/// </summary>
	/// 
	public class Channel<T>
	{
		protected Object queueLock = new object ();

		// How many objects are in the queue we use this rather than the queue count
		private Semaphore itemsOnQueue = new Semaphore(0);

		protected Queue<T> channelQueue = new Queue<T>();

		/// <summary>
		/// Creates a new thread safe channel on which you can queue and deque.
		/// </summary>
		public Channel (){}

		/// <summary>
		/// Used to safely enqueue data to the channel, continues
		/// 	trying to put the data on the channel even if it is interrupted
		/// 
		/// This method gracefully handles Thread Interrupts
		/// </summary>
		/// <param name="data">The data to enqueue into the channel.</param>
		public virtual void Put(T data)
		{
			Boolean successful = false;
			Boolean hasInterrupted = false;

			while (!successful) {
				try {
					successful = Offer(data);
				} catch (ThreadInterruptedException){
					hasInterrupted = true;
				}
			}
				
			if (hasInterrupted) {
				Thread.CurrentThread.Interrupt ();
			}

		}

		/// <summary>
		/// Continually tries to take data from the channel
		/// </summary>
		public virtual T Take()
		{
			T data = default(T);


			Boolean successful = false;
			Boolean hasInterrupted = false;

			while (!successful) {
				try {
					successful = Poll (out data);
				} 
				catch (ThreadInterruptedException){
					hasInterrupted = true;
				}
			}
			if (hasInterrupted) {
				Thread.CurrentThread.Interrupt ();
			}

			return data;
		}


		/// <summary>
		/// Offer up the specified data to the queue, it may be interrupted
		/// 	
		/// A standard channel should always accept the offer
		/// </summary>
		/// <param name="data">Data to put on the queue</param>
		/// <param name="timeout">The timeout is intended for subclasses, do not change</param>
		public virtual bool Offer(T data, int timeout = -1)
		{
			lock (queueLock) {
				channelQueue.Enqueue (data);
			}
			itemsOnQueue.ForceRelease ();

			return true;
		}

	
		/// <summary>
		/// Keeps attempting to take data off of the channel
		/// 	if specified amount of time is reached it returns a failure
		/// 	if data is put on the channel it returns a success
		/// </summary>
		/// <param name="data">Data to put on the channel</param>
		/// <param name="timeout">Time in ms to wait before considering it a failure</param>
		public virtual bool Poll(out T data, int timeout = -1)
		{
			data = default(T); // Returns default value of type T

			// Checks to see if there is data on the channel, gives up after timeout ms
			if (itemsOnQueue.TryAcquire (timeout)) {
				try {
					lock (queueLock){
						data = channelQueue.Dequeue();
					}
				}
				catch (ThreadInterruptedException){
					// We must release the token taken that we TryAcquired
					itemsOnQueue.ForceRelease();
					throw;
				}
				return true;
				
			} 
			else {
				return false;
			}

		}
	}
}