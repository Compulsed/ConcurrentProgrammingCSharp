using System;
using System.Threading;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// A Semaphore has a various amount of tokens (ability to access a resource or pool of resources)
	/// 	this is defined at the instantiation of the object. You then take the tokens while you use the 
	/// 	resource, once there is no more tokens left a thread has to wait until one is released. After the
	/// 	thread has finished with the resource it releases it and then alerts other waiting threads so that
	/// 	they are now able to take the resource. If there is contention still on the token, one thread will
	/// 	get the token and the others will have to wait for another opportunity. 
	/// 	
	/// Author: Dale Salter 9724397
	/// </summary>
	public class Semaphore
	{
		protected UInt64 _count;
		protected Object objectLock = new Object();

		/// <summary>
		/// Creates the Semaphore, returns an instance with 0 tokens
		/// </summary>
		public Semaphore () : this (0) {}


		/// <summary>
		/// Creates the Semaphore, returns an instance with the number of specified tokens
		/// </summary>
		/// <param name="initialTokens">Initial tokens.</param>
		public Semaphore(UInt64 initialTokens)
		{
			_count = initialTokens;
		}



		/// <summary>
		/// Releases the specified tokens back to the Semaphore.
		/// </summary>
		/// <param name="tokensToRelease">Tokens that will be released, this number cannot be less than 1</param>
		public virtual void Release(UInt64 tokensToRelease = 1)
		{
			lock (objectLock) {
				_count += tokensToRelease;
				Monitor.PulseAll (objectLock);
			}
		}

		/// <summary>
		/// Waits for tokens to be available, then takes a token
		/// </summary>
		public virtual void Acquire()
		{
			lock (objectLock) {

				// While there is no tokens
				while (_count == 0) {
					Monitor.Wait (objectLock);
				}
					
				_count -= 1; 
			}
		}



	}
}

