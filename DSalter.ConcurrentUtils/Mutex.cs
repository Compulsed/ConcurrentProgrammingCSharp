using System;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// A Mutex is a Semaphore that can only have 1 or 0 tokens, this allows for blocking the 
	/// 	thread if there is no tokens, and once the token is released a thread attempts to 
	/// 	acquire it.
	/// 
	/// Author: Dale Salter 9724397
	/// </summary>
	public class Mutex : Semaphore 
	{

		/// <summary>
		/// Creates a Semaphore with only 1 token by default
		/// </summary>
		public Mutex () : base(1){}


		/// <summary>
		/// Releases 1 token to the Semaphore
		/// </summary>
		public override void Release(UInt64 n = 1)
		{
			if (n > 1)
				throw new ArgumentException ("n can only be 1", "n");

			lock (this) {
				// If there already is a token, you should not be able to release another one
				if (base._count == 1)
					throw new Exception ("Cannot not release on a mutex that has not been acquired");

				base.Release (); // Adds a token
			}
		}

// 		THIS LINE IS BREAKTHING THINGS
//		public override void Release(UInt64 n)
//		{
//			if (n > 1)
//				throw new ArgumentException ("n can only be 1", "n");
//
//			base.Release();
//		}


		/// <summary>
		/// Releases the mutex used by the <see cref="DSalter.ConcurrentUtils.Mutex"/> object.
		/// </summary>
		public void Dispose()
		{
			base.Release ();
		} 


	}
}

