using System;
using System.Collections.Generic;

namespace DatabaseManagementSystem
{
	public class TableManager 
	{
		private Table _mainTable;  


		public TableManager (string fileName = "database.db", bool newDatabase = true)
		{
			_mainTable = new Table (fileName, newDatabase);
		}

		public void Accept(Request aRequest)
		{
            Console.WriteLine($"TM: {aRequest}");
		    _mainTable.Accept(aRequest);
		}
    }
}

