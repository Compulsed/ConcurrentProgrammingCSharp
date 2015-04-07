using System;
using System.Threading;
using DSalter.Assignment;

namespace DSalter.Submissions
{
	/// Author: Dale Salter 9724397
	class Accounts
	{
		static Account myAccount = new Account(new decimal(100));

		static void depositOneMillion()
		{
			int million = (int)Math.Pow (10, 6);

			for (int i = 0; i < million; ++i)
					myAccount.Deposit(1);

			Console.WriteLine ("You account balance is now ${0}, after depositing", myAccount.Balance);
		}

		static void withdrawOneMillion()
		{
			int million = (int)Math.Pow (10, 6);

			for (int i = 0; i < million; ++i)
				myAccount.Withdraw(1);

			Console.WriteLine ("You account balance is now ${0}, after withdrawing", myAccount.Balance);
		}

		public static void Main (string[] args)
		{
			Thread depositThread = new Thread (depositOneMillion);
			Thread withdrawThread = new Thread (withdrawOneMillion);

			depositThread.Start ();
			withdrawThread.Start ();

			Console.ReadLine ();
		}
	}
}

