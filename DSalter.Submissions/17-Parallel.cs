using System;
using System.Linq;

using System.Collections.Generic;

using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

namespace DSalter.Submissions
{
	public class _7_Parallel
	{
		public class ParallelStatistics
		{
			UInt64[] nums;
			UInt64 workers;
			UInt64[] threadStoreSum;
			UInt64[] threadStoreMin;
			UInt64[] threadStoreMax;


			Barrier sumBarrier;
			Barrier minBarrier;
			Barrier maxBarrier;

			Semaphore calculationsCompleted;

			public ParallelStatistics(UInt64[] nums, UInt64 workers = 4)
			{
				this.nums = nums;
				this.workers = workers;
				this.threadStoreSum = new UInt64[workers];
				this.threadStoreMin = new UInt64[workers];
				this.threadStoreMax = new UInt64[workers];

				sumBarrier = new Barrier(workers);
				minBarrier = new Barrier(workers);
				maxBarrier = new Barrier(workers);

				calculationsCompleted = new Semaphore(workers);
			}

			public void Generate()
			{
				// Console.WriteLine("Presum: {0}", nums.Sum());

				UInt64 size = (UInt64)nums.LongLength / workers;
				for (UInt64 i = 0; i < workers; ++i){
					if (i != (workers - 1))
						SumNumbers (i * size, (i + 1) * size, i);
					else
						SumNumbers (i * size, (i + 1) * size + (UInt64)nums.Length % workers, i);
				}

			}

			public void SumNumbers(UInt64 start, UInt64 end, UInt64 threadNo)
			{
				new Thread(() => {
					Console.WriteLine("Working [{0}, {1}]", start, end);

					calculationsCompleted.Acquire();

					// Sum
					{

						UInt64 total = 0;
						for(UInt64 i = start; i < end; ++i){
							total += (UInt64)nums[i]; 
						}
						threadStoreSum[threadNo] = total; 

						Console.WriteLine("Thread sum:\t {0}", total);

						if(sumBarrier.Arrive()){
							UInt64 agregateTotal = 0;
							for(UInt64 i = 0; i < workers; ++i)
								agregateTotal += threadStoreSum[i];

							Console.WriteLine("Agregate:\t {0}", agregateTotal);
							Console.WriteLine("Average:\t {0}", agregateTotal / (UInt64)workers);
							Console.WriteLine("--------------------------------------");


							calculationsCompleted.Release(workers);
						}
					}
					calculationsCompleted.Acquire();

					// Min
					{


						UInt64 min = nums[0];
						for(UInt64 i = start; i < end; ++i){
							if(nums[i] < min) 
								min = nums[i];
						}
						threadStoreMin[threadNo] = min;
						Console.WriteLine("Thread min:\t {0}", min);

						if(minBarrier.Arrive()){
							UInt64 smallest = threadStoreMin[0];
							for(UInt64 i = 0; i < workers; ++i)
								if(threadStoreMin[i] < smallest)
									smallest = threadStoreMin[i];

							Console.WriteLine("Smallest:\t {0}", smallest);
							Console.WriteLine("--------------------------------------");

							calculationsCompleted.Release(workers);
						}
					}
					calculationsCompleted.Acquire();


					// Max
					{
						UInt64 max = nums[0];
						for(UInt64 i = start; i < end; ++i){
							if(nums[i] > max) 
								max = nums[i];
						}
						threadStoreMax[threadNo] = max;
						Console.WriteLine("Thread max:\t {0}", max);

						if(maxBarrier.Arrive()){
							UInt64 largest = threadStoreMax[0];
							for(UInt64 i = 0; i < workers; ++i)
								if(threadStoreMax[i] < largest)
									largest = threadStoreMax[i];

							Console.WriteLine("Largest:\t {0}", largest);
							Console.WriteLine("--------------------------------------");

							calculationsCompleted.Release(workers);
						}
					}

				}).Start();
			}

			public void CreateNums()
			{
				Random generator = new Random();

				for(UInt64 i = 0; i < (UInt64)nums.Length; ++i)

					nums[i] = (UInt64)generator.Next (1, int.MaxValue -1);
			}

			public void PrintNums()
			{
				foreach (int num in nums)
					Console.Write (num + " ");
			}

		}



		public static void Main()
		{

			UInt64[] nums = new UInt64[100000000];


			ParallelStatistics one = new ParallelStatistics (nums, 4);
			one.CreateNums ();
			one.Generate ();



			return;
		}

	}
}

