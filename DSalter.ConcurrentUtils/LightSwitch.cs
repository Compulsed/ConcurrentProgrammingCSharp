using System;

using DSalter.ConcurrentUtils;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// Light switch allows for multiple readers or read a particular resource
	/// 	the same Semaphore passed in can be used outside to signify writing
	/// 
	/// The idea is that the first person to enter the room will turn on the 
	/// 	light switch (Semaphore) and the last person out will turn it off.
	///
	/// Author: Dale Salter 9724397
	/// </summary>
	public class LightSwitch 
	{
		/// <summary>
		/// The total number of readers active on the resource
		/// </summary>
		private UInt64 readers = 0;

		/// <summary>
		/// The Semaphore that is passed in
		/// </summary>
		private readonly Semaphore _switchable;

		/// <summary>
		/// The Semaphore that is passed in can also be used to signify writing
		/// </summary>
		/// <param name="switchable">Switchable.</param>
		public LightSwitch (Semaphore switchable)
		{
			_switchable = switchable;
		}

		/// <summary>
		/// Used to signify a thread is reading the resource, keeps track of readers
		/// 	Locks the Semaphore if it is the first person
		/// </summary>
		public void Acquire(){
			lock (this) {
				++readers;
				if (readers == 1)
					_switchable.Acquire ();
			}

		}

		/// <summary>
		/// Used to signify that a thread is no longer reading the resource,
		/// 	the last thread to stop reading will release the Semaphore
		/// </summary>
		public void Release()
		{
			lock (this) {
				--readers;
				if (readers == 0)
					_switchable.Release ();
			}
		}


	}
}

