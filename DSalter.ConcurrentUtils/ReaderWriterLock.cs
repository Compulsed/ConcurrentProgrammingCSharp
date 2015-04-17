using System;

using Monitor = System.Threading.Monitor;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// 
	/// 1 Solves the starvation for the writers
	/// 
	///  
	/// Starvation for the readers now exists, must queue the readers and writers up
	/// 
	/// 
	/// </summary>
	public class ReaderWriterLock
	{
		private Semaphore writePermission;
		private LightSwitch LS;

		// Used to block an LS that is already running, stops the writer form straving when readers just keep getting added to the LS
		private Mutex readerTS;

		private Semaphore writerTS;

		private UInt64 readersWaiting;

		private Object lockObject;


		public ReaderWriterLock ()
		{
			writePermission = new Mutex();
			LS = new LightSwitch(writePermission);

			readerTS = new Mutex ();
			writerTS = new FIFOSemaphore (1);

			readersWaiting = 0;

			lockObject = new object ();
		}
			
		public void AcquireReader()
		{

			// ---------------------- #1 TS -------------------------------------------------------
			// Should be signed if there there is something waiting and should be released when the last
			// 	thread has finished with the LS
			readerTS.Acquire();
			readerTS.Release();

			lock (lockObject) {
				++readersWaiting;	// Used to figure out when the last reader has finished with the LS
			}
			// -------------------------------------------------------------------------------------


			LS.Acquire();					// << ADDS READER TO LS

			lock (lockObject) {
				--readersWaiting;
				Monitor.PulseAll(lockObject);
			}
		}

		public void ReleaseReader()
		{
			LS.Release();					// >> REMOVES READER FROM LS
		}

		public void AcquireWriter()
		{
			// ---------------------- #1 Locker ----------------------------------------------------
			// Should stop threads being added to the TS
			readerTS.Acquire();
			// -------------------------------------------------------------------------------------

			writerTS.Acquire ();

			writePermission.Acquire (); // << ADDS WRITER TO WRITE QUEUE

			// ---------------------- #1 Releaser --------------------------------------------------
			// Allows threads to be added back to the TS
			readerTS.Release ();
			// -------------------------------------------------------------------------------------
		}

		public void ReleaseWriter()
		{
			writePermission.Release(); // >> REMOVES WRITER FROM WRITE QUEUE

			lock (lockObject) {
				if (readersWaiting > 0)
					Monitor.Wait (lockObject);
			}

			writerTS.Release ();
		}



		/// <summary>
		/// To be used with a using block for safe discarding (exception or return)
		/// 	or the reader
		/// </summary>
		public class ReaderReleaser : IDisposable
		{
			private readonly ReaderWriterLock toRelease;

			bool disposed = false;

			public ReaderReleaser(ReaderWriterLock parent)
			{
				toRelease = parent;
			}

			~ReaderReleaser()
			{
				Dispose(false);
			}

			// true, comes from a Dispose method
			// false, comes from finalizer
			protected virtual void Dispose(bool disposing)
			{
				if (disposed)
					return;

				if (disposing) {
					toRelease.ReleaseReader ();
				}

				disposed = true;
			}

			public void Dispose()
			{
				Dispose (true);
				GC.SuppressFinalize (true);
			}
		}

		public IDisposable AcquireReaderLock()
		{
			AcquireReader ();

			return new ReaderReleaser (this);
		}

		/// <summary>
		/// To be used with a using block for safe discarding (exception or return)
		/// 	or the writer
		/// </summary>
		public class WriterReleaser : IDisposable
		{
			private readonly ReaderWriterLock toRelease;

			bool disposed = false;

			public WriterReleaser(ReaderWriterLock parent)
			{
				toRelease = parent;
			}

			~WriterReleaser()
			{
				Dispose(false);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposed)
					return;

				if (disposing) {
					toRelease.ReleaseWriter ();
				}

				disposed = true;
			}

			public void Dispose()
			{
				Dispose (true);
				GC.SuppressFinalize (true);
			}
		}

		public IDisposable AcquireWriterLock()
		{
			AcquireWriter ();

			return new WriterReleaser (this);
		}


	}
}

