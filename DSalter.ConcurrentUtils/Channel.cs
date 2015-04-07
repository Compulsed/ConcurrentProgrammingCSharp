using System;
using System.Collections.Generic;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// Channel.
	/// </summary>
	public class Channel<T>
	{
		protected Object queueLock = new object ();
		Semaphore resource = new Semaphore(0);

		protected Queue<T> channelQueue = new Queue<T>();

		public Channel (){}

		public virtual void Put(T data)
		{
			lock (queueLock) {
				// Has to be inside the lock because our count is provided by the semaphore
				resource.Release (1);
				channelQueue.Enqueue (data);
			}
		}

		public virtual T Take()
		{
			resource.Acquire ();
			lock (queueLock) {
				return channelQueue.Dequeue ();
			}
		}

	}
}

