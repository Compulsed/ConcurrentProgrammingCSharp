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
	/// This is the cache?
	/// 
	/// Handles the FileManager
	/// 
	/// </summary>
	public class Table<RowType>
	{
		// Total rows the table has (not the same as _idOfNextRow? active rows?)
		UInt64 _numberOfRows; // #1

		// Next row that will be used when a new record is made
		UInt64 _idOfNextRow; // #4

		// Loccation of first free space? // #2
		// - Where a new row can be inserted if a deleted row is not replaced
		// Where is this populated or handles

		// Key: Row ID, Value: Offset on file // #3
		Dictionary<UInt64, UInt64> _rowLocationInFile;

		// Key: Row ID, Value: Row is cache
		Dictionary<UInt64, Row> _rowCache; // #5


		public Table ()
		{
			_numberOfRows = 0;
			_idOfNextRow = 0;
			_rowLocationInFile = new Dictionary<UInt64, UInt64> ();
			_rowCache = new Dictionary<UInt64, Row> ();


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

		void Update(Request aUpdateRequest)
		{
			// Always pass to the FileManager
		}
	}
}

