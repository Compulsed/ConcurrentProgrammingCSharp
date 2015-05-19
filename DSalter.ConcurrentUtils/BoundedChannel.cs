using System;
using System.Threading;
using DSalter.ConcurrentUtils;


namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// A Bounded channel limits the amount of information that can be put on the channel
	/// 	the channel is implemented using a queue system.
	/// 	
	/// 	Threads will block putting data on the channel if it is full
	/// 	Threads will also block if there is no data to take off the channel
	/// 
	/// 	This Utility is interrupt safe, so that you can exit the utility early
	/// 		if it is blocking
	/// 
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
		/// Used to safely enqueue data to the channel, continues
		///  trying to put the data on the channel even if it is interrupted
		/// 
		///	This method may block until there is space on the channel
		/// 
		/// This method gracefully handled Thread Interrupts
		/// </summary>
		/// <param name="data">The data to enqueue into the channel.</param>
		public override void Put(T data)
		{
			Boolean successful = false;
			Boolean hasInterrupted = false;

			while (!successful) {
				try {
					maxResouces.Acquire(); // Waits until there is space
					successful = true;
				} catch (ThreadInterruptedException){
					hasInterrupted = true;
				}

				base.Put (data);

				if (hasInterrupted) {
					Thread.CurrentThread.Interrupt ();
				}

			}
		}

		/// <summary>
		/// Continually tries to take data from the channel
		/// TODO This function is BUGGED
		/// </summary>
		public override T Take()
		{
			T data;

			try {
				// data = base.Take(); // Using this causes a bug?
				base.Poll(out data);
			} 
			catch (ThreadInterruptedException){
				throw;
			}
			finally {
				maxResouces.Release ();
			}

			return data;
		}

		/// <summary>
		/// Offer up the specified data to the queue, it may be interrupted
		/// 
		/// A bounded channel may not accept the offer
		/// </summary>
		/// <param name="data">Data to put on the queue</param>
		/// <param name="timeout">Timeout before this operation should be stopped</param>
		public override bool Offer(T data, int timeout)
		{
			if (maxResouces.TryAcquire (timeout)) {
				try {
					base.Offer (data);
				} catch (ThreadInterruptedException) {
					maxResouces.ForceRelease ();
					throw;
				}
				return true;
			} else {
				return false;
			}

		}



		/// <summary>
		/// Keeps attempting to take data off of the channel
		///  if specified amount of time is reached it returns a failure
		///  if data is put on the channel it returns a success
		/// </summary>
		/// <param name="data">Data to put on the channel</param>
		/// <param name="timeout">Time in ms to wait before considering it a failure</param>
		public override bool Poll(out T data, int timeout = -1)
		{
			Boolean isSuccessful = base.Poll (out data, timeout);

			if (isSuccessful) {
				maxResouces.Release ();
			}
			return isSuccessful;
		}

	}
}

