using System;
using System.Collections.Generic;
using DSalter.ConcurrentUtils;

using Thread = System.Threading.Thread;
using System.Threading;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// This channel is a thread safe queue that allows the clients to queue and
	/// 	dequeue generic data to it. 
	///
	/// 	Threads will block putting data on the channel if it is full
	/// 	Threads will also block if there is no data to take off the channel
	/// 
	/// 	With the additional option to include polling when taking information off 
	/// 		off the channel
	/// 
	/// 	This Utility includes threadsafe methods, so that you can exit the utility early
	/// 		if it is blocking
	/// 
	/// Author: Dale Salter (9724397)
	/// </summary>
	/// 
	public class Channel<T>
	{
		// protected Object queueLock = new object ();

		// How many objects are in the queue we use this rather than the queue count
		private Semaphore itemsOnQueue = new Semaphore(0);

		LinkedQueue<T> channelQueue = new LinkedQueue<T>();

		/// <summary>
		/// Creates a new thread safe channel on which you can queue and deque.
		/// </summary>
		public Channel (){}

		/// <summary>
		/// Used to safely enqueue data to the channel, continues
		/// 	trying to put the data on the channel even if it is interrupted
		/// 
		/// This method gracefully handles Thread Interrupts
		/// </summary>
		/// <param name="data">The data to enqueue into the channel.</param>
		/// 
		/// Just calls offer with -1, think about the interrupt
		public virtual void Put(T data)
		{
				Offer(data, -1);
		}

		/// <summary>
		/// Continually tries to take data from the channel
		/// 
		/// TODO This function is buggy? Find out why
		/// </summary>
		public virtual T Take()
		{
			T data = default(T);

			Poll (out data, -1); // Calls poll

			return data;
		}


		/// <summary>
		/// Offer up the specified data to the queue, it may be interrupted
		/// 	
		/// A standard channel should always accept the offer
		/// </summary>
		/// <param name="data">Data to put on the queue</param>
		/// <param name="timeout">The timeout is intended for subclasses, do not change</param>
		public virtual bool Offer(T data, int timeout = -1)
		{
			channelQueue.Enqueue (data);
			itemsOnQueue.ForceRelease ();

			return true;
		}

	
		/// <summary>
		/// Keeps attempting to take data off of the channel
		/// 	if specified amount of time is reached it returns a failure
		/// 	if data is put on the channel it returns a success
		/// </summary>
		/// <param name="data">Data to put on the channel</param>
		/// <param name="timeout">Time in ms to wait before considering it a failure</param>
		public virtual bool Poll(out T data, int timeout = -1)
		{
			data = default(T); // Returns default value of type T

			// Checks to see if there is data on the channel, gives up after timeout ms
			if (itemsOnQueue.TryAcquire (timeout)) {
				try {
					data = channelQueue.Dequeue();
				}
				catch (ThreadInterruptedException){
					// We must release the token taken that we TryAcquired to get here
					itemsOnQueue.ForceRelease();
					throw;
				}
				return true;
				
			} 
			else {
				return false;
			}

		}


		private class LinkedQueue<T> 
		{
			public UInt64 Count { get; private set; }

			Node<T> front = Node<T>.Instance; 
			Node<T> back = Node<T>.Instance;	  

			protected Object EnqueueLock = new object ();
			protected Object DequeueLock = new object ();

			public LinkedQueue ()
			{
				Count = 0;
			}

			public void Enqueue (T value)
			{
				lock (EnqueueLock){
					lock (back) {
						Node<T> newNode = new Node<T> (value);

						if (Count == 0) {
							front = newNode;
						} else {
							newNode.SetPrevious (Node<T>.Instance); // There is no previous person
							newNode.SetNext (back);  				// Person infront of you
							back.SetPrevious (newNode);				// Person behind you, points to you
						} 

						back = newNode; 

						++Count;
					}
				}
			}


			public T Dequeue ()
			{
				lock (DequeueLock) {
					lock (front) {
						Node<T> toDetach;

						if (Count == 0)
							throw new Exception ("Queue is empty");

						toDetach = front;  // Get the person at the front

						// Will be no more people in the queue
						if (Count == 1) {
							front = Node<T>.Instance;
						} 
						//  Will will be more people in the queue
						else {
							front = toDetach.GetPrevious (); // front will = previous
							front.SetNext (Node<T>.Instance);
						}

						--Count;

						return toDetach.GetValue ();
					}
				}
			}


			/// <summary>
			/// Node is used as a doubly linked list connection
			/// </summary>
			private class Node<T>
			{
				private static volatile Node<T> instance;

				Node<T> next = null;
				Node<T> previous = null;
				T value;

				public Node (T value) : this(Instance, value, Instance) {}

				public Node (Node<T> previous, T value, Node<T> next)
				{
					this.value = value;
					this.previous = previous;
					this.next = next;
				}

				public void SetNext (Node<T> T)
				{
					if (this == instance)
						throw new Exception ("Cannot set next of NullNode");

					next = T;
				}

				public void SetPrevious (Node<T> T)
				{
					if (this == instance)
						throw new Exception ("Cannot set previous of NullNode");

					previous = T;
				}

				public Node<T> GetNext ()
				{
					if (this == instance)
						throw new Exception ("Cannot return next of NullNode");
					return next;
				}

				public Node<T> GetPrevious ()
				{
					if (this == instance)
						throw new Exception ("Cannot return previous of NullNode");

					return previous;
				}

				public void SetValue (T value)
				{
					if (this == instance)
						throw new Exception ("Cannot set value of NullNode");

					this.value = value;
				}

				public T GetValue ()
				{
					if (this == instance)
						throw new Exception ("Cannot get the value of a NullNode");

					return this.value;
				}

				public override string ToString ()
				{
					if (this == instance)
						throw new Exception ("Cannot print value of NullNode");

					return value.ToString ();
				}

				public bool IsNullNode ()
				{
					return this == instance;
				}

				private static object syncRoot = new Object();
				private Node(){}

				public static Node<T> Instance
				{
					get 
					{
						if (instance == null) 
						{
							lock (syncRoot) 
							{
								if (instance == null) {
									instance = new Node<T>(); // This may not work
								}
							}
						}

						return instance;
					}
				}
			}



		}

	}

}