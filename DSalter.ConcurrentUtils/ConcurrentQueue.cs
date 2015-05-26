using System;
using System.Collections.Generic;
using System.Collections;

namespace DSalter.ConcurrentUtils
{

	public class Node<T>
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
		
	public class LinkedQueue<T> 
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

	}

			


}

