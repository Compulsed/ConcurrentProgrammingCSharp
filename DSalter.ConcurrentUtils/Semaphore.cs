using System;
using System.Threading;
using System.Diagnostics;

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

		protected UInt64 threadsWaiting = 0;


		/// <summary>
		/// Use when used withing a using block, for example
		/// 	using(aSemaphore.lock()){}
		/// 	allows for exception safety / and eartly returns
		/// </summary>
		public IDisposable Lock()
		{
			return new SemaphoreReleaser (this);
		}

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
				if(_count > 0)
					Monitor.Pulse (objectLock);
			}
		}

		/// <summary>
		/// Stops the Release process being interuppted by the ThreadInterruptException
		/// 	This is needed for other utilities
		/// </summary>
		/// <param name="tokensToRelease">Tokens to release.</param>
		public virtual void ForceRelease(UInt64 tokensToRelease = 1){
			Boolean interrupted = false;

			while(true){
				try {
					Release(tokensToRelease);
					if (interrupted){
						Thread.CurrentThread.Interrupt();
					}
					return;
				}
				catch (ThreadInterruptedException) {
					interrupted = true;
				}
			}

		}


		/// <summary>
		/// Waits for tokens to be available, then takes a token. The thread will block until there is a token.
		/// </summary>
		public virtual void Acquire()
		{
			TryAcquire (-1);
		}

		/// <summary>
		/// Tries the acquire, if given a certain amount of time a acquire could not happen
		/// it will just release and return false
		/// 
		/// This operation gracefully handles thread interrupts
		/// </summary>
		/// <returns><c>true</c>, if acquire was successfuk, <c>false</c> otherwise.</returns>
		/// <param name="timeout">Timeout time waiting in milliseconds</param>
		public virtual bool TryAcquire(int timeout)
		{
			int waitTime = timeout;

			lock (objectLock) {

				threadsWaiting++;

				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start ();

				while (_count == 0) {
					try {
						// Interrupt may occur here
						if(!Monitor.Wait(objectLock, waitTime)){
							return false;
						}

						if ( _count == 0 && timeout != -1)
						{
							waitTime = timeout - (int)stopWatch.ElapsedMilliseconds;
							// counter does not matter if -1 has been given
							if ( waitTime < 0 ){
								return false;
							}
						}



					}
					catch (ThreadInterruptedException){
						lock (objectLock)
						{
							// One less thread waiting, to avoid concurrency problems (special cases)
							// 	we should pulse other threads on this Semaphore 
							// -> A token is released while we are catching an exception
							if ((threadsWaiting - 1) > 0) {
								Monitor.Pulse (objectLock);
							}
							--threadsWaiting;
						}
						throw;
					}
				}
					
				threadsWaiting--;
				_count--;

				// There may be other threads waiting, if there are
				// and there is still additional tokens, a single pulse should be done
				if (_count > 0 && threadsWaiting > 0) {
					Monitor.Pulse (objectLock);
				}

				return true;
			}
		}

		private class SemaphoreReleaser : IDisposable
		{
			private readonly Semaphore toRelease;

			public SemaphoreReleaser(Semaphore parent)
			{
				toRelease = parent;
			}

			~SemaphoreReleaser()
			{
				Dispose(false);
			}

			public void Dispose(bool disposed)
			{
				toRelease.Release ();
				if (disposed)
					GC.SuppressFinalize (this);
			}

			public void Dispose()
			{
				Dispose (true);
			}
		}


	}
}

