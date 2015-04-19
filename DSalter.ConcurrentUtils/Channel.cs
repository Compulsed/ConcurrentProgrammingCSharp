using System;
using System.Collections.Generic;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// This channel is a thread safe queue that allows the clients to queue and
	/// 	dequeue generic data to it. 
	/// 
	/// Author: Dale Salter (9724397)
	/// </summary>
	public class Channel<T>
	{
		protected Object queueLock = new object ();
		Semaphore resource = new Semaphore(0);

		protected Queue<T> channelQueue = new Queue<T>();

		/// <summary>
		/// Creates a new thread safe channel on which you can queue and deque.
		/// </summary>
		public Channel (){}

		/// <summary>
		/// Used to safely enqueue data to the channel.
		/// </summary>
		/// <param name="data">The data to enqueue into the channel.</param>
		public virtual void Put(T data)
		{
			lock (queueLock) {
				// Has to be inside the lock because our count is provided by the semaphore
				channelQueue.Enqueue (data);
			}
			resource.Release ();
		}

		/// <summary>
		/// Used to safely dequeue data off of the channel.
		/// </summary>
		public virtual T Take()
		{
			resource.Acquire ();
			lock (queueLock) {
				return channelQueue.Dequeue ();
			}
		}

	}
}

