using System;

namespace DSalter.Assignment
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
}

