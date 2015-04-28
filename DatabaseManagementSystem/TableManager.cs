using System;
using System.Collections.Generic;

namespace DatabaseManagementSystem
{


	public class Request
	{
		// QueryDetails (MultipleActions?)
		// - Read: ranges
		// - Update: Create & Delete
		// - Create & Delete: Set of rows to be changed, and how each row should changed
		RequestRow rows; // Or with RequiredChanges?

		public Request()
		{

		}


	}

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
		FileManager mainFileManager;

		public TableManager ()
		{
			mainTable = new Table ();

			// mainFileManager = new FileManager();
		}

		void Create(Request aCreateRequest)
		{
		
		}

		// Includes: Create and Delete
		void Update(Request aUpdateRequest)
		{

		}

		void Delete(Request aDeleteRequest)
		{

		}

		// Include a range
		// Will load or fetch rows from the Table cache and populate the ResultSet
		void Read(Request aReadRequest)
		{

		}

	}
}

