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
			UInt64[] resultStore;

			Barrier syncBarrier;
			Semaphore calculationsCompleted;

			public ParallelStatistics(UInt64[] nums, UInt64 workers = 4)
			{
				this.nums = nums;
				this.workers = workers;
				this.resultStore = new UInt64[workers];

				syncBarrier = new Barrier(workers);
					
				calculationsCompleted = new Semaphore(0);
			}

			/// <summary>
			/// Prints the statistics out to the console for various 
			/// 	information related to the large arrary
			/// </summary>
			public void GenerateStatistics()
			{
				UInt64 size = (UInt64)nums.LongLength / workers;
				UInt64 startRange = 0;
				UInt64 endRange = size + (UInt64)nums.Length % workers;

				for (UInt64 i = 0; i < workers; ++i){
					CalculateNumbers (startRange, endRange, i);
					startRange = endRange;
					endRange += size;
				}
			}

			public void CalculateNumbers(UInt64 start, UInt64 end, UInt64 threadNo)
			{
				new Thread(() => {
					Console.WriteLine("Working [{0}, {1}]", start, end);

					// Sum
					{
						UInt64 total = 0;
						for(UInt64 i = start; i < end; ++i){
							total += (UInt64)nums[i]; 
						}
						resultStore[threadNo] = total; 

						Console.WriteLine("Thread sum:\t {0}", total);

						if(syncBarrier.Arrive()){
							UInt64 agregateTotal = 0;
							for(UInt64 i = 0; i < workers; ++i)
								agregateTotal += resultStore[i];

							Console.WriteLine("Aggregate:\t {0}", agregateTotal);
							Console.WriteLine("Average:\t {0}", agregateTotal / (UInt64)workers);
							Console.WriteLine("--------------------------------------");

							calculationsCompleted.Release(workers);
						}
						calculationsCompleted.Acquire();
					}

					// Min
					{
						UInt64 min = nums[0];
						for(UInt64 i = start; i < end; ++i){
							if(nums[i] < min) 
								min = nums[i];
						}
						resultStore[threadNo] = min;
						Console.WriteLine("Thread min:\t {0}", min);


						if(syncBarrier.Arrive()){
							UInt64 smallest = resultStore[0];
							for(UInt64 i = 0; i < workers; ++i)
								if(resultStore[i] < smallest)
									smallest = resultStore[i];

							Console.WriteLine("Smallest:\t {0}", smallest);
							Console.WriteLine("--------------------------------------");

							calculationsCompleted.Release(workers);
						}
						calculationsCompleted.Acquire();
					}



					// Max
					{
						UInt64 max = nums[0];
						for(UInt64 i = start; i < end; ++i){
							if(nums[i] > max) 
								max = nums[i];
						}
						resultStore[threadNo] = max;
						Console.WriteLine("Thread max:\t {0}", max);

						if(syncBarrier.Arrive()){
							UInt64 largest = resultStore[0];
							for(UInt64 i = 0; i < workers; ++i)
								if(resultStore[i] > largest)
									largest = resultStore[i];

							Console.WriteLine("Largest:\t {0}", largest);
							Console.WriteLine("--------------------------------------");

							calculationsCompleted.Release(workers);
						}
						calculationsCompleted.Acquire();
					}

				}).Start();
			}

			/// <summary>
			/// Generates random numbers on a single thread
			/// </summary>
			public void SingleRandomNumbers()
			{
				Random generator = new Random();

				for(UInt64 i = 0; i < (UInt64)nums.Length; ++i)
					nums[i] = (UInt64)generator.Next (1, int.MaxValue -1);
			}

			/// <summary>
			/// Generates random numbers on multiple threads, this
			/// 	may not work as Random() is not concurrent
			/// </summary>
			public void ParallelRandomNumbers()
			{
				List<Thread> workerThreads = new List<Thread> ();

				UInt64 size = (UInt64)nums.LongLength / workers;
				UInt64 startRange = 0;
				UInt64 endRange = size + (UInt64)nums.Length % workers;

				for (UInt64 i = 0; i < workers; ++i){
					// Does each thread get a local copy of these?
					UInt64 start = startRange;
					UInt64 end = endRange;

					Thread thread = new Thread (() => {
						// Will this work correctly? May reuse same numbers (it does)
						Random generator = new Random ();

						while (start < end) {
							nums [start] = (UInt64)generator.Next (0, int.MaxValue - 1);
							++start;
						}
					});
					workerThreads.Add (thread);
					thread.Start ();

					startRange = endRange;
					endRange += size;
				}

				// Blocks the main thread so GenerateStatistics cannot be used until 
				// all threads have completed
				foreach (Thread thread in workerThreads)
					thread.Join ();
			}

			/// <summary>
			/// Prints all of the randomly generated numbers
			/// </summary>
			public void PrintNums()
			{
				foreach (int num in nums)
					Console.Write (num + " ");
			}

		}



		public static void Main()
		{

			UInt64[] nums = new UInt64[(UInt64)Math.Pow(2, 27)];


			ParallelStatistics one = new ParallelStatistics (nums, 8);

			// one.ParallelRandomNumbers ();
			one.SingleRandomNumbers ();

			one.GenerateStatistics ();

			return;
		}

	}
}

