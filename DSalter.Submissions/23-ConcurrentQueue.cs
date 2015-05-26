using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

namespace DSalter.Submissions
{
	public class _3_ConcurrentQueue
	{
		static LinkedQueue<string> lq = new LinkedQueue<string>();

		public static void RandomPrinter ()
		{
			Random a = new Random ();

			for (int i = 0; i < 1000; ++i) {
				switch (a.Next (2)) {
				case 0:
					lq.Enqueue ("" + i);
					break;
				case 1:
					lq.Dequeue();
					break;
				}
			}
		}

		public static void Main(string[] args)
		{

			Thread runner = new Thread (() => {
				RandomPrinter ();
			});

			Thread runner1 = new Thread (() => {
				RandomPrinter ();
			});
				
	
			runner.Start ();
			runner1.Start ();

			Console.WriteLine ("printing!");
		}

	}
}

