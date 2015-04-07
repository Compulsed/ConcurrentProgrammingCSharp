using System;
using System.Threading;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// Upon inheriting this abstract class, it allows you to add the required 
	/// functionality to create an active object. This comes from overloading
	/// the Run() virtual method. 
	/// 
	/// Author: Dale Salter 9724397
	/// </summary>
	public abstract class ActiveObject 
	{
		protected readonly Thread activeThread;

		/// <summary>
		/// The method where you define the logic of the active object
		/// </summary>
		protected abstract void Run();

		public ActiveObject() : this("No name given to active object thread"){}

		/// <summary>
		/// Takes a name that can be given to the execution thread
		/// </summary>
		/// <param name="threadName">A name that you want to give the thread</param>
		public ActiveObject(string threadName)  
		{
			activeThread = new Thread (Run);
			this.activeThread.Name = threadName;
		}

		/// <summary>
		/// Starts the thread attached to the active object
		/// </summary>
		public void Start()
		{
			activeThread.Start ();
		}

		/// <summary>
		/// Stops the thread attached to the active object
		/// </summary>
		public void Stop()
		{
			activeThread.Interrupt ();
		}

		public override string ToString()
		{
			return this.activeThread.Name;
		}
	}
}

