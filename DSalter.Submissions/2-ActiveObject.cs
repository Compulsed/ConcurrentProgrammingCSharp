using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread; 
	
namespace DSalter.Submissions
{
	public class Account
	{
		private decimal _balance; // 128 bits, 64 bit access each time

		public Account(decimal initialBalance)
		{
			_balance = initialBalance;
		}

		/// <summary>
		/// Protected withdraw method, will lock the Account object to guarantee safety
		/// </summary>
		/// <param name="withdrawAmount">Withdraws any amount of money from the account, can go into the negitives.</param>
		public void Withdraw(decimal withdrawAmount)
		{	
			lock(this)
				_balance -= withdrawAmount;
		}

		/// <summary>
		/// Protected deposit method, will lock the Account object to guarantee safety
		/// </summary>
		/// <param name="depositAmount">Deposits any amount of money to the account</param>
		public void Deposit(decimal depositAmount)
		{	
			lock(this)
				_balance += depositAmount;
		}

		public decimal Balance
		{
			get { lock(this) return _balance; }
		}
	}

	public class MessageWriter : ActiveObject 
	{
		private readonly string toPrint;

		/// <summary>
		/// Is an active object, which stores the thread name and a message
		/// 	and just prints a loop of that message
		/// </summary>
		/// <param name="toPrint">A message that will keep looping over and printing</param>
		/// <param name="threadName">A name of that act object's thread</param>
		public MessageWriter(string toPrint, string threadName) : base(threadName)
		{
			this.toPrint = toPrint;
		}

		protected override void Run ()
		{
			while (true) {
				Console.WriteLine (toPrint);
				Thread.Sleep (1000);
			}
		}

	}


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
