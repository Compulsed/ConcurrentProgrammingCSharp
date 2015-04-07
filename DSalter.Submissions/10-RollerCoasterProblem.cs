using System;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

/*
 * Suppose there are n passenger threads and a car thread. 
 * The passengers repeatedly wait to take rides in the car, which can hold C passengers, 
 * where C < n. The car can go around the tracks only when it is full.
 * 
 * 
 * - Passengers should invoke board and unboard.
 * - The car should invoke load, run and unload.
 * 
 * - Passengers cannot board until the car has invoked load
 * - The car cannot depart until C passengers have boarded.
 * - Passengers cannot unboard until the car has invoked unload.
 * 
 */ 
namespace DSalter.Submissions
{
	// 	|->	Car.load -> 
	// 	|	Passenger.board -> 
	//  |	(Once C passenger) Car.Run -> 
	//	|	Car.unload -> 
	//	|	Passenger.unboard ->
	//	|----------------------<
	public class _0_RollerCoasterProblem
	{
		public static UInt64 capacity = 5;

		public static UInt64 boarders = 0;
		public static UInt64 unboarders = 0;

		public static Mutex mutex = new Mutex(); // Protects passegners which counts the number counts the number of passengers
		// that have invoked boardCar
		public static Mutex mutex2 = new Mutex();

		public static Semaphore boardQueue = new Semaphore (0);	// Wait on this before boarding
		public static Semaphore unboardQueue = new Semaphore(0);	// Wait on this before unboarding

		public static Semaphore allAboard = new Semaphore(0); // Car is full
		public static Semaphore allAshore = new Semaphore(0);


		// board and unboard
		public class Passenger : ActiveObject
		{
			public Car boardingCar;

			public Passenger(){}

			// Attempt to Enter the car
			public void BoardCar()
			{
				Console.WriteLine ("Boarding car");
			}

			// Attempt to Exit the car
			public void UnboardCar()
			{
				Console.WriteLine ("Unboarding car");
			}

			protected override void Run ()
			{
				Console.WriteLine ("Inside Passenger");

				while (true) {
					boardQueue.Acquire ();
					BoardCar ();

					mutex.Acquire ();
					{
						++boarders;
						if (boarders == capacity) {
							allAboard.Release ();
							boarders = 0;
						}
					}
					mutex.Release ();

					unboardQueue.Acquire ();
					UnboardCar ();

					mutex2.Acquire ();
					{
						++unboarders;
						if (unboarders == capacity) {
							allAshore.Release ();
							unboarders = 0;
						}
					}
					mutex2.Release ();

				}
			}
		}

		// load, run, unload
		public class Car : ActiveObject
		{
			public Car(){}


			// Invoke to allow people to enter car
			public void Load()
			{
				Console.WriteLine ("Loading");
			}

			// Like the run method
			public void Depart()
			{
				Console.WriteLine ("DEPARTING!!!");
			}

			// Ivoke to allow people to exit the car
			public void Unload()
			{
				Console.WriteLine ("Unloading");
			}



			protected override void Run()
			{
				while (true) {
					Load ();
					boardQueue.Release (capacity);	// Allows capacity out of car 
					allAboard.Acquire ();

					Depart ();

					Unload ();
					allAboard.Release (capacity);	// Allows capacity out of car 
					allAshore.Acquire ();
				}
			}
		}

		public static void Main()
		{
			Console.WriteLine ("Hello From RollerCoasterProblem");

			Car carOne = new Car ();
			Passenger[] people = new Passenger[]{
				new Passenger(),
				new Passenger(),
				new Passenger(),
				new Passenger(),
				new Passenger()
			};

			carOne.Start ();


			foreach (Passenger person in people) {
				person.Start ();
			}

			return;
		}
	}
}

