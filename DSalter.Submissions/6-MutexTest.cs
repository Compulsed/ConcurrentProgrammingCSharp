using System;
using Thread = System.Threading.Thread;
using DSalter.ConcurrentUtils;


namespace DSalter.Submissions
{
	/// Author: Dale Salter 9724397
	public class __MutexTest
	{
		static Mutex writeMtx = new Mutex();

		public static void OtherThread()
		{
			writeMtx.Acquire ();
			{
				Console.WriteLine ("Inside OtherThread");
				Thread.Sleep (1000);

				Console.WriteLine ("1 - Other");
				Thread.Sleep (1000);

				Console.WriteLine ("2 - Other");
				Thread.Sleep (1000);

				Console.WriteLine ("3 - Other");
				Thread.Sleep (1000);
			}
			Console.WriteLine ("Attempting to release OtherThread writeMtx");
			writeMtx.Release();	
		}

		/// <summary>
		/// Test one, proves the utility correct by showing that the other thread cannot write to the 
		/// 	output while while the first thread is executing. 
		/// </summary>
		public static void Main()
		{
			new Thread (OtherThread).Start ();


			Console.WriteLine ("Inside MutexTest");


			writeMtx.Acquire ();
			{
				Console.WriteLine ("Inside Main");
				Thread.Sleep (1000);

				Console.WriteLine ("1 - Main");
				Thread.Sleep (1000);

				Console.WriteLine ("2 - Main");
				Thread.Sleep (1000);

				Console.WriteLine ("3 - Main");
				Thread.Sleep (1000);
			}
			Console.WriteLine ("Attempting to release Main writeMtx");
			writeMtx.Release();


			return;
		}


	}
}

