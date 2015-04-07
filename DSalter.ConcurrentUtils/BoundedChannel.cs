using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;


namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// A Bounded channel limits the amount of information that can be put on the channel
	/// 	the channel is implemented using a queue system.
	/// 	Threads will block putting data on the channel if it is full.
	/// 	Threads will also block if there is no data to take off the channel.
	/// </summary>
	public class BoundedChannel<T> : Channel<T>
	{
		// Defines the maxium number of items in the queue
		Semaphore maxResouces;

		/// <summary>
		/// Defines the maxium amount that can be put on the channel at a given time
		/// </summary>
		/// <param name="capacity">Capacity.</param>
		public BoundedChannel (UInt64 capacity) : base()
		{
			maxResouces = new Semaphore (capacity);
		}

		/// <summary>
		/// Put the specified data on the channel, full block if channel capacity has been reached
		/// </summary>
		/// <param name="data">The data that you want to put on the channel</param>
		public override void Put(T data)
		{
			maxResouces.Acquire();
			base.Put(data);
		}

		/// <summary>
		/// Takes the data off of the channel
		/// </summary>
		public override T Take()
		{
			T temp = base.Take();
			maxResouces.Release();

			return temp;
		}

	}
}

