using System;
using System.Linq;


namespace DSalter.Submissions
{
	public class _7_Parallel
	{
		public class ParallelStatistics
		{
			int[] nums;
			UInt64 workers;
			UInt64[] threadStore;




			public ParallelStatistics(int[] nums, UInt64 workers = 4)
			{
				this.nums = nums;
				this.workers = workers;
				this.threadStore = new UInt64[workers];
			}

			public void Generate()
			{
				Console.WriteLine(nums.Sum());
			}

			public void CreateNums()
			{
				Random generator = new Random();

				for(int i = 0; i < nums.Length; ++i)
					nums[i] = generator.Next (1, 10000);
			}

			public void PrintNums()
			{
				foreach (int num in nums)
					Console.Write (num + " ");
			}

		}


		static int[] nums = new int[1000];

		public static void Main()
		{

			ParallelStatistics one = new ParallelStatistics (nums);
			one.CreateNums ();
			one.Generate ();



			return;
		}

	}
}

