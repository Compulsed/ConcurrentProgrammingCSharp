using System;
using System.Collections.Generic;

namespace DatabaseManagementSystem
{

	/// <summary>
	/// Table manager.
	///
	/// Must contain
	/// - A cache of the table and the rows that have been loaded
	/// 
	/// 
	/// 
	/// Uses file manager to do the do the actual reading and writing
	/// 	to the file
	/// 
	/// CRUD Requests will be sent here
	/// 
	/// Concept of "Action type"
	/// 
	/// </summary>

	public class TableManager 
	{

		Table mainTable; 

		public TableManager (string fileName = "database.db")
		{
			mainTable = new Table (fileName);
		}


		public void Request(Request aRequest)
		{
			// Console.WriteLine ("Random request sent!");
			// mainTable.Execute (aRequest);
		}



		// public void Execute (UpdateRequest aUpdateRequest)
		//{
		// 	Console.WriteLine ("Update request sent!");
		// }
	}
}

