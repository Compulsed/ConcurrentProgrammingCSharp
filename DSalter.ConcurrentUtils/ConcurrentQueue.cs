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

	public class DoubleLinkedList<T> : IEnumerable
	{
		public UInt64 Count { get; private set; }

		Node<T> start = Node<T>.Instance; 
		Node<T> end = Node<T>.Instance;	  

		Node<T> current = Node<T>.Instance;

		public DoubleLinkedList ()
		{
			Count = 0;
		}

		public void Add (T value)
		{
			Node<T> newNode = new Node<T>(value);

			if (Count == 0) {
				start = newNode;
				end = newNode;
			} else {
				// Point new node to the currents next, changes currents next to new node
				newNode.SetNext(current.GetNext());
				current.SetNext (newNode);

				// New nodes previous is the current node
				newNode.SetPrevious(current);

				if (!newNode.GetNext ().IsNullNode ())
					newNode.GetNext ().SetPrevious (newNode);
				else
					end = newNode;
			}

			// Current is now the new node
			current = newNode;

			++Count;
		}

		// --

		public Node<T> GetCurrent ()
		{
			return current;
		}

		public void MoveNext ()
		{
			current = current.GetNext ();
		}

		public void MovePrevious ()
		{
			current = current.GetPrevious ();
		}

		// ---


		public bool HasNext ()
		{
			return current.GetNext() != Node<T>.Instance;
		}

		public bool HasPrevious ()
		{
			return current.GetPrevious() != Node<T>.Instance;
		}

		public bool NotEnd ()
		{
			return current != Node<T>.Instance;
		}
			
		public void ToStart ()
		{
			current = start;
		}

		public void ToEnd ()
		{
			current = end;
		}

		// -- 

		public override string ToString ()
		{
			return String.Format(
				"[Start: {0}, End: {1}, Current: {2}, Length: {3}]", start, end, current, Count
			);
		}

		public IEnumerator GetEnumerator()
		{
			ToStart ();

			Node<T> toPrint = current;	
			while (!current.IsNullNode ()) {
				toPrint = current;
				MoveNext ();
				yield return toPrint;
			}
		}

	}

	public class ConcurrentQueue<T> 
	{
		public UInt64 Count { get; private set; }



		public ConcurrentQueue ()
		{
			Count = 0;
		}



		public void Enqueue(T item)
		{
		}

		public T Dequeue()
		{
			return default(T);
		}


	}
}

