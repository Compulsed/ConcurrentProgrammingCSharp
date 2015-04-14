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

		public static Mutex entryTurnstile = new Mutex(); // Protects passegners which counts the number counts the number of passengers
		// that have invoked boardCar
		public static Mutex exitTurnstil = new Mutex();

		public static Semaphore boardQueue = new Semaphore (0);		// Wait on this before boarding
		public static Semaphore unboardQueue = new Semaphore(0);	// Wait on this before unboarding

		public static Semaphore allAboard = new Semaphore(0); // Car is full
		public static Semaphore allAshore = new Semaphore(0);


		// board and unboard
		public class Passenger : ActiveObject
		{
			public Passenger(UInt64 personNo) : base(Convert.ToString(personNo)){}

			// Attempt to Enter the car
			public void BoardCar()
			{
				Console.WriteLine (this.ToString() + " - Boarding car");
			}

			// Attempt to Exit the car
			public void UnboardCar()
			{
				Console.WriteLine (this.ToString() + " - Unboarding car");
			}

			protected override void Run ()
			{
				Console.WriteLine ("Inside Passenger " + this.ToString());

				while (true) {
					
					boardQueue.Acquire ();				// (<-- Car) Signals we can start to enter the ride

					BoardCar (); // #2

					using (entryTurnstile.Lock ()) {
						++boarders;
						if (boarders == capacity) {
							allAboard.Release ();		// (--> Car) Signal we are ready to leave
							boarders = 0;
						}
					}

					unboardQueue.Acquire (); 			// (<-- Car) Signals ride is over


					UnboardCar ();	// #4

					using(exitTurnstil.Lock()){
						++unboarders;
						if (unboarders == capacity) {
							allAshore.Release ();		// (--> Car) Signal everyone is off
							unboarders = 0;
						}
					}

			
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
				Console.WriteLine ("Car has started Loading");
			}

			// Like the run method
			public void Depart()
			{
				Console.WriteLine ("DEPARTING!!!");
				Thread.Sleep (3000);
				Console.WriteLine ("FINISHED, we have ARRIVED!!!");
			}

			// Ivoke to allow people to exit the car
			public void Unload()
			{
				Console.WriteLine ("Car has started Unloading");
			}



			protected override void Run()
			{
				while (true) {
					Load (); // #1
					Thread.Sleep (1000);

					boardQueue.Release (capacity);	// (--> Passenger) to get on the car
					Thread.Sleep(1000);

					allAboard.Acquire ();			// (<-- Passenger) everyone is ready!
					Thread.Sleep (1000);

					Depart (); // #3
					Thread.Sleep (1000);

					Unload (); // #4
					Thread.Sleep(1000);


					unboardQueue.Release (5); 		// (--> Passenger) to get off the car


					allAshore.Acquire (); 		 	// (<-- Passenger) everyone is off the car
					Thread.Sleep(1000);
				}
			}
		}

		public static void Main()
		{
			Console.WriteLine ("Hello From RollerCoasterProblem");

			Car carOne = new Car ();
			Passenger[] people = new Passenger[]{
				new Passenger(1),
				new Passenger(2),
				new Passenger(3),
				new Passenger(4),
				new Passenger(5)
			};

			carOne.Start ();


			foreach (Passenger person in people) {
				person.Start ();
			}

			return;
		}
	}
}

