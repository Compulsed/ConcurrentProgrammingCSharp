using System;
using System.Collections.Generic;

using DSalter.ConcurrentUtils;

namespace DatabaseManagementSystem
{
	/// <summary>
	/// Table.
	/// 
	/// Must contain:
	/// - Number of rows
	/// - The location of the first free space - where a new row can be inserted 
	/// 	if a deleted row is not replaced
	/// - An index that maps the Row id numbers to a row location in the file (the 
	/// 	offset from the start of the file to seek when reading/writing the row)
	/// - The id of the next Row created
	/// - A Cache of the Rows that have been read from the file
	/// 
	/// If the table cannot load all of the rows that it needs,
	/// 	it should pass a request to the File Manager (via its channel)
	/// 	to read the missing rows from the file
	///	The file manager sets the result status and opens the latch 
	///
	/// 
	/// Handles the FileManager
	/// 
	/// </summary>
	public class Table
	{
		// Stores the actual representation of the file
		FileManager fileManager;
		Channel<Request> fileManagerChannel;

		// Key: Row ID, Value: Row is cache
		Dictionary<UInt64, Row> _rowCache; 

		public Table (string fileName = "tablename.db")
		{
			_rowCache = new Dictionary<UInt64, Row> ();

			//fileManager = new FileManager (_rowCache, fileName);
			//fileManagerChannel = fileManager._inputChannel;
			//fileManager.Start ();
		}

		public void Print()
		{
			// Console.WriteLine (_rowCache);
			//
			//	foreach (Row row in _rowCache) {
			//	
			//	}

			Console.WriteLine("-----------------------");
			foreach (KeyValuePair<UInt64, Row> entry in _rowCache)
				Console.WriteLine ("{0} : {1}", entry.Key, entry.Value);
			Console.WriteLine("-----------------------");
		}

		// Should interface with FileManager to get records that
		// are not within this cache
		// If can get from cache, set the status and latch
		// if not let the FileManager deal with it
		void Read(Request aReadReqest)
		{
			// pass request to channel if not cached
			// if cached handle and set latch and status
		}

		// void Update(Request aUpdateRequest)
		// {
			// Always pass to the FileManager
		//}

		public void Execute (Request aRequest)
		{
			// Console.WriteLine ("Table -> Sending aRandomRequest");
//			fileManagerChannel.Put (aRequest);
		}

		public void Execute(SelectRequest aSelectRequest)
		{

//			for (UInt64 i = aSelectRequest.startId; i <= aSelectRequest.endId; ++i) {
//				Console.WriteLine (_rowCache [i]);
//			}

			// Console.WriteLine ("Table -> Sending aSelectRequest");
			// fileManagerChannel.Put (aSelectRequest);
		}

	}
}

