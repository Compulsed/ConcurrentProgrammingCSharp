using System;
using DSalter.Assignment;

namespace DSalter.Submissions
{
	/// Author: Dale Salter 9724397
	class ActiveObject
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Main Class > Main");

			MessageWriter one = new MessageWriter ("Hello World! - one ", "one - HelloWorld");
			one.Start();

			MessageWriter two = new MessageWriter ("\tHello World! - two", "two - HelloWorld");
			two.Start();


			Console.ReadLine ();
		}
	}
}
