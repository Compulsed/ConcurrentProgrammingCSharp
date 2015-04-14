using System;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;


namespace DSalter.Submissions
{
	/// <summary>
	/// Two kinds of threads, oxygen and hydrogen
	/// 	We have to create a barrier to assemble these threads, and they must wait until the correct combination is ready
	/// 	As each thread passes the barrier it should invoke bond
	/// 
	/// 	You must guarantee that all threads from one molecule invoke bond before any of the threads from the next molecule do
	/// 
	///		* If an oxygen thread arrives at the barrier when no hydrogen threads are present, it has to wait for two hydrogen threads
	/// 	* If a hydrogen thread arrives at the barrier when no other thread are present, it has to wait for an oxygen thread and another 
	/// 		hydrogren thread
	/// 
	/// 	We just need to release 3 threads, 1 oxygen and 2 hydrogen	
	/// </summary>
	public class _0_BuldingH20
	{

		static Mutex _combinationPermission = new Mutex();
		static Barrier _combinationBarrier = new Barrier(3);

		static UInt64 _oxygenCount = 0;
		static UInt64 _hydrogenCount = 0;

		static Semaphore _oxygenQueue = new Semaphore(0);
		static Semaphore _hydrogenQueue = new Semaphore(0);


		public class Oxygen : ActiveObject 
		{
			public Oxygen(UInt64 oxygenNo) : base (Convert.ToString(oxygenNo))
			{
				
			}

			protected override void Run ()
			{
				Console.WriteLine ("Inside Oxygen Run()");

				while (true) {
					_combinationPermission.Acquire ();

					// Do not need to lock, because there is enough hydrogens to move forward
					if (_hydrogenCount >= 2) {

						// Hydrogen run methods can continue
						_hydrogenCount -= 2;
						_hydrogenQueue.Release (2); // (--> 2x Hydrogen) can move forward to the barrier
					} 

					// There is not enough hydrogen atoms so much lock on queue
					else {
						++_oxygenCount;

						_combinationPermission.Release ();	// (--> Other threads) May now check if there is enough atoms
						_oxygenQueue.Acquire (); 			// Wait on the queue;
					}


					// 1 Thread will be running this at the same time
					Console.WriteLine("#" + this.ToString() + "\t Oxygen now waiting at the barrier");
					if (_combinationBarrier.Arrive ()) {
						Console.WriteLine ("\n");
					}

					_combinationBarrier = new Barrier (3);
					// 1 Thread will be running this at the same time



					_combinationPermission.Release ();	// One of the threads will need to release it, as there is only one oxygen, it should
				}
			}
		}

		public class Hydrogen : ActiveObject
		{
			public Hydrogen(UInt64 hydrogenNo) : base (Convert.ToString(hydrogenNo))
			{

			}

			protected override void Run ()
			{
				Console.WriteLine ("Inside Hydrogen Run()");

				while (true) {
					_combinationPermission.Acquire ();

					// Do not need to lock, because there is enough hydrogen and oxygen atoms now
					if ((_hydrogenCount >= 1) && (_oxygenCount >= 1)) {

						--_hydrogenCount;
						--_oxygenCount;

						_hydrogenQueue.Release (1); // (--> 1x Hydrogen) can move forward to the barrier
						_oxygenQueue.Release (1); 	// (--> 1x Oxygen) can move forward to the barrier
					} 

					// There is not enough hydrogen or oxygen atoms so lock 
					else {
						++_hydrogenCount;

						_combinationPermission.Release ();	// (--> Other threads) May now check if there is enough atoms
						_hydrogenQueue.Acquire (); 			// Wait on the queue;
					}


					// 2 Threads will be running this at the same time
					Console.WriteLine ("#" + this.ToString() + "\tHydrogen now waiting at the barrier");
					if (_combinationBarrier.Arrive ()) {
						Console.WriteLine ("\n");
					}


					Thread.Sleep (5000);
				}
			}
		}

		public static void Main()
		{

			for (UInt64 i = 0; i < 2; ++i) {
				new Oxygen (i).Start ();
			}

			for (UInt64 i = 0; i < 4; ++i) {
				new Hydrogen (i).Start ();
				Thread.Sleep (1000);
			}

			return;
		}

	}
}

