using System;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// Latch will block all threads that attempt to acquire it until
	/// 	the latched is released. When it is released all waiting threads
	/// 	and future threads will pass through
	/// 
	/// Author: Dale Salter 9724397
	/// </summary>
	public class Latch : Semaphore
	{
		/// <summary>
		/// Initally sets the Latch is it is in blocking state
		/// </summary>
		public Latch () : base(0){}

		/// <summary>
		/// Blocks any threads that run this method until a Release() has been 
		/// 	used. Once Release has been run, all the waiting threads will pass
		/// 	and so will the future ones.
		/// </summary>
		public override void Acquire()
		{
			base.Acquire (); // Will take a token
			base.Release (); // Then immediately replace it
		}
	}
}