using System;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	public class _3_ConcurrentQueue
	{

		public static DoubleLinkedList<string> dll = new DoubleLinkedList<string>();

		public static void Main(string[] args)
		{
			Console.WriteLine ("--");

			dll.Add ("1");
			dll.Add ("2");
			dll.Add ("3");
			dll.Add ("4");
			dll.Add ("5");

			Console.WriteLine (dll);


			foreach (Node<string> str in dll)
				Console.WriteLine (str);


		}



	}
}

