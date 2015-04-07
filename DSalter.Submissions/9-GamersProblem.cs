using System;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;

namespace DSalter.Submissions
{
	/// <summary>
	/// Logic:
	/// 	Mediators.
	/// 		Will attempt to take any networkSem, snackSem, creditSem that is released from the Agents (Operating system)
	///
	/// 		On the first attempt to aquire one of the two resources given (networkSem, snackSem, creditsSem), 
	/// 			it will assign the true value to to corresponding is{Network,Snacks,Credits} and keep it from being reclaimed.
	/// 		On the second go, it will...
	/// 			- Reassign the true that was done in the first step to a false (resetting)
	/// 			- With logic it will know which gamer to tell to start gaming (it must be the other one)
	/// 
	/// 	Agents.
	/// 		Will Randomly release 2 resources (snackSem, creditSem, networkSem) based on when the scheduler decides to
	/// 			resume any of the 3 blocked Agents (this will be based on luck, and if the other set of resouces have been used)
	/// 
	/// 	Gamers
	/// 		Will get a signal from the Mediators when to start playing games, after it is finished it can allow the agents to resume
	/// 			by signaling one of them to unblock (Random)
	/// 
	/// Author: Dale Salter 9724397
	/// </summary>
	public class Gamers
	{
		private Semaphore resourceGeneration = new Semaphore(1);

		private bool isNetwork = false, isSnacks = false, isCredits = false;
		private Semaphore networkSem = new Semaphore (0);
		private Semaphore snacksSem = new Semaphore (0);
		private Semaphore creditsSem = new Semaphore (0);


		private Semaphore allowNetworkGamer = new Semaphore (0);
		private Semaphore allowSnacksGamer = new Semaphore (0);
		private Semaphore allowCreditsGamer = new Semaphore (0);
	
		public void MediatorNetwork()
		{
			while (true) {
				// If the Agent has released a network resourse
				networkSem.Acquire ();

				lock (this) {
					if (isSnacks) {
						isSnacks = false;
						allowCreditsGamer.Release ();
					} else if (isCredits) {
						isCredits = false;
						allowSnacksGamer.Release ();
					} else {
						isNetwork = true;
					}
				}
			}
		}

		public void MediatorSnacks()
		{
			while (true) {
				snacksSem.Acquire ();

				lock (this) {
					if (isNetwork) {
						isNetwork = false;
						allowCreditsGamer.Release ();
					} else if (isCredits) {
						isCredits = false;
						allowNetworkGamer.Release ();
					} else {
						isSnacks = true;
					}
				}
			}
		}

		public void MediatorCredits()
		{
			while (true) {
				creditsSem.Acquire ();

				lock (this) {
					if (isSnacks) {
						isSnacks = false;
						allowNetworkGamer.Release ();
					} else if (isNetwork) {
						isNetwork = false;
						allowSnacksGamer.Release ();
					} else {
						isCredits = true;
					}
				}
			}
		}

		public void Game (string gamer)
		{
			Console.WriteLine ("{0}, has started playing games!", gamer);
			// Thread.Sleep (1000);
			Console.WriteLine ("{0}, has stopped playing games, and wants to play again!", gamer);
		}

		public void Prepare (string gamer)
		{
			Console.WriteLine ("{0}, has prepared!", gamer);
			// Thread.Sleep (10000);
			Console.WriteLine ("{0}, has finished preparing, and now allows other people to have a go at preparing!", gamer);
		}

		public void NetworkGamer()
		{
			while (true) {
				allowNetworkGamer.Acquire ();
				Prepare ("NetworkGamer");
				resourceGeneration.Release ();
				Game ("NetworkGamer");
			}
		}

		public void SnacksGamer()
		{
			while (true) {
				allowSnacksGamer.Acquire ();
				Prepare ("SnacksGamer");
				resourceGeneration.Release ();
				Game ("SnacksGamer");
			}
		}

		public void CreditsGamer()
		{
			while (true) {
				allowCreditsGamer.Acquire ();
				Prepare ("CreditsGamer");
				resourceGeneration.Release ();
				Game ("CreditsGamer");
			}
		}


		public void NetworksCreditsAgent()
		{
			while (true) {
				resourceGeneration.Acquire ();
				networkSem.Release ();
				creditsSem.Release ();
			}
		}

		public void NetworkSnacksAgent()
		{
			while (true) {
				resourceGeneration.Acquire ();
				networkSem.Release ();
				snacksSem.Release ();
			}			
		}

		public void CreditsSnacksAgent()
		{
			while (true) {
				resourceGeneration.Acquire ();
				creditsSem.Release ();
				snacksSem.Release ();
			}	
		}

		public void StartUp()
		{
			Console.WriteLine ("Starting Mediators");
			new Thread (MediatorNetwork).Start ();
			new Thread (MediatorSnacks).Start ();
			new Thread (MediatorCredits).Start ();

			Console.WriteLine ("Starting Gamers");
			new Thread (NetworkGamer).Start ();
			new Thread (SnacksGamer).Start ();
			new Thread (CreditsGamer).Start ();

			Console.WriteLine ("Starting Agents");
			new Thread (CreditsSnacksAgent).Start ();
			new Thread (NetworksCreditsAgent).Start ();
			new Thread (NetworkSnacksAgent).Start ();

			Console.WriteLine ("Finished Starting All Threads");
		}	

	}



	public class __GamerProblem
	{
		public static void Main()
		{
			Console.WriteLine ("Inside Main()");

			Gamers setOne = new Gamers ();
			setOne.StartUp ();
		}	
	}
}

