using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	// Philosophers are threads, there will be 5 of these
	// Chopsticks are mutexes, there will be 5 of these
	// One will be left handed, the others will be right
	// This will be done with an Array of Philophers, left will be current position on 
	// 	the chopsticks array and, right will be + 1 of the Philosopher position

	// Left hand		Self		Right hand
	// Chopstick0 	Philospher0 	Chopstick1
	// Chopstick1	Philospher1 	ChopStick2
	// Chopstick2 	Philospher2		ChopStick3
	// Chopstick3 	Philospher3		ChopStick4
	// Chopstick4 	Philospher4		Chopstick0 <-- this is where it cycles back


	/// Author: Dale Salter 9724397
	public static class PhilosophersProfiler
	{
		public static readonly UInt32 timeEating = 0;
		public static readonly UInt32 timeThinking = 0;
		public static readonly bool debug = false;
		public static readonly UInt32 runtimeBeforeStats = 10000;
	}

	public class Philosophers
	{
		private Philosopher[] _phils;
		private Mutex[] _chopsticks;

		public Philosophers(){}

		public void Setup(){
			_phils = new Philosopher[5];
			_chopsticks = new Mutex[5];
		
			for (int i = 0; i < 5; ++i) 
				_chopsticks [i] = new Mutex ();
			

			_phils [0] = new Philosopher ("Philosopher #" + 0, true, _chopsticks [0], _chopsticks [1]);
			_phils [1] = new Philosopher ("Philosopher #" + 1, true, _chopsticks [1], _chopsticks [2]);
			_phils [2] = new Philosopher ("Philosopher #" + 2, true, _chopsticks [2], _chopsticks [3]);
			_phils [3] = new Philosopher ("Philosopher #" + 3, true, _chopsticks [3], _chopsticks [4]);
			_phils [4] = new Philosopher ("Philosopher #" + 4, false, _chopsticks [4], _chopsticks [0]);
		}

		public void PrintStats()
		{
			Console.WriteLine ("--- After {0} milliseconds, times eaten for each Philosopher ---", PhilosophersProfiler.runtimeBeforeStats);
			foreach(Philosopher phil in _phils)
				Console.WriteLine(phil);
			Console.WriteLine ("");
		}

		public void StartAll()
		{
			foreach (Philosopher phil in _phils)
				phil.Start ();
		}

		public void StopAll()
		{
			foreach (Philosopher phil in _phils)
				phil.Stop ();
		}
	}

	public class Philosopher : ActiveObject
	{
		private Mutex _leftChopstick;
		private Mutex _rightChopstick;
		private bool _startLeft;

		public UInt64 timesEating = 0;

		public Philosopher(string philosopherName, 
			bool startLeft,
			Mutex leftChopstick,
			Mutex rightChopstick
		) : base(philosopherName)
		{
			_leftChopstick = leftChopstick;
			_rightChopstick = rightChopstick;
			_startLeft = startLeft;
		}

		public override string ToString()
		{
			return base.activeThread.Name + " has been able to eat " + timesEating;
		}

		// The logic
		protected override void Run ()
		{
			if(PhilosophersProfiler.debug)
				Console.WriteLine ("Inside run()!");

			while (true) {

				// Some thinking
				Thread.Sleep((int)PhilosophersProfiler.timeThinking);

				if(PhilosophersProfiler.debug)
					Console.WriteLine (base.activeThread.Name + " Has finished thinking and wants to eat!");

				// Attempting to start eating
				if (_startLeft) {
					_leftChopstick.Acquire ();
					_rightChopstick.Acquire ();
				} else {
					_rightChopstick.Acquire ();
					_leftChopstick.Acquire ();
				}

				// Eating
				if(PhilosophersProfiler.debug)
					Console.WriteLine(base.activeThread.Name + " Is now eating food!");

				++timesEating;

				Thread.Sleep((int)PhilosophersProfiler.timeEating);

				_rightChopstick.Release ();
				_leftChopstick.Release ();
			}
		}
	}


	public class __DiningPhilosophers
	{

		
		// Entry Point
		public static void Main(){
			Console.WriteLine ("Running the DiningPhilosophers");

			Philosophers tableOne = new Philosophers ();
			tableOne.Setup ();
			tableOne.StartAll ();

			while (true) {
				Thread.Sleep ((int)PhilosophersProfiler.runtimeBeforeStats);
				tableOne.PrintStats ();
			}

			return;
		}
	}
}

