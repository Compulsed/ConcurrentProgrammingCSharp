using System;
using System.IO;
using System.Collections.Generic;

using DSalter.ConcurrentUtils;

namespace DatabaseManagementSystem
{
	/// <summary>
	/// File manager.
	/// 
	/// Must handle
	/// - Reading and writing to the file
	/// 
	/// When getting request to fill the ResultSet 
	/// 	must set status and open latch
	/// 
	/// Must check that the Row is not already in the Tables cachce
	/// 	if it is add it to the ResultSet, if not add the row to the
	/// 	cache and then add it to the ResultSet.
	/// 
	/// Always handles Updates and deletes
	/// 
	/// Rows will be marked as deleted will have their "deleted" flag updated in the file
	/// 	There should no longer be considered to exist, but iwll remain in the file until
	/// 	overriden
	/// 
	/// Deleted rows should be in a queue, as the latest one to be deleted will be overriden
	/// 
	/// If there is no more space it should allocate more space by increasing the file size by
	/// 120% then adding the record
	/// 
	/// Compression can be added at a later stage to remove the deleted files if need be
	/// 
	/// File should be binary and fixed length and offsets should be used.
	/// 
	/// Weak references for the Rows within the File manager cache so that the 
	/// 	cached objects can be freed if memory is scarce. 
	/// 
	/// </summary>
	public class FileManager : ChannelActiveObject<Request>
	{

		// Binary, each row takes a fixed no bytes
		// Strings are to be fixed size
		// File tableFile;

		// Weak references for the Rows
		// Must have access to the cache
		Dictionary<UInt64, Row> _rowCache;

		// Loccation of first free space? // #2

		// - Where a new row can be inserted if a deleted row is not replaced

		// Next row that will be used when a new record is made
		UInt64 _idOfNextRow; // #4

		// Total rows the table has (not the same as _idOfNextRow? active rows?)
		UInt64 _numberOfRows; // #1


		FileStream _databaseFile;
		BinaryWriter _binaryWriter;
		BinaryReader _binaryReader;


		// Key: Row ID, Value: Offset on file // #3
		Dictionary<UInt64, UInt64> _rowLocationInFile;


		string _fileName;

		public FileManager (Dictionary<UInt64, Row> rowCache, string fileName = "database.db")
		{
			_numberOfRows = 0;
			_idOfNextRow = 1;
			_rowLocationInFile = new Dictionary<UInt64, UInt64> ();


			_rowCache = rowCache;
			_fileName = fileName;
			_databaseFile = new FileStream (_fileName, FileMode.OpenOrCreate, 
				FileAccess.ReadWrite, FileShare.None);

			_binaryReader = new BinaryReader (_databaseFile, System.Text.Encoding.Unicode);
			_binaryWriter = new BinaryWriter (_databaseFile, System.Text.Encoding.Unicode);

			Console.WriteLine ("Creating file: " + fileName);
		}




		void ProcessRequest(RandomRequest aRandomRequest)
		{
			Console.WriteLine ("FileManager -> Processing aRandomRequest");


			for(UInt64 i = 0; i < aRandomRequest.randomRowsToMake; ++i){
				UInt64 thisRowId = _idOfNextRow++;
				++_numberOfRows;

				Row rowToBeAdded = new Row (thisRowId, "R -> " + thisRowId);

				Console.WriteLine (rowToBeAdded);

				_rowCache.Add (thisRowId, rowToBeAdded);

				rowToBeAdded.Write(_binaryWriter);

				aRandomRequest.resultSet._rowObjectsCompleted.Add (rowToBeAdded);
				aRandomRequest.resultSet._completedLatch.Release ();
			}

			PrintKeyValue ();
			PrintFile ();
		}


		void ProcessRequest(SelectRequest aSelectRequest)
		{
			Console.WriteLine ("FileManager -> Processing aSelectRequest");
		}

		protected override void Process (Request passedData)
		{

			ProcessRequest ((dynamic)passedData);

			// If Reading, check that it is not already in the cache
			// if it is, add to the ResultSet, if not fetch from file
			// set latch

			// If Updating, read the rows and update the values in the file

			// If deleted the deleted flag in the file ill be set
			// These are no longer to be considered existent, but will remain 
			// in the file to be overriden

			// If creating, locate first deleted row, and insert the new row at that location
			// Otherwise add the new row to the end of the row
			// Allocate 120% of its current size when it becomes full

			// Add a compress method later on

			// throw new NotImplementedException ();
		}

		public void PrintKeyValue()
		{
			Console.WriteLine("\n-----------------------");
			Console.WriteLine("----- Row Cache -------");
			Console.WriteLine("-----------------------");
			foreach (KeyValuePair<UInt64, Row> entry in _rowCache)
				Console.WriteLine ("(Key: {0}, Value {1})", entry.Key, entry.Value);
			Console.WriteLine("-----------------------");
		}

		public void PrintFile()
		{
			Console.WriteLine("\n-----------------------");
			Console.WriteLine("----- File Contents ---");
			Console.WriteLine("-----------------------");

			_binaryReader.BaseStream.Seek (0, SeekOrigin.Begin);
			Row tempRow = new Row ();

			for (UInt64 i = 0; i < _numberOfRows; ++i) {
				tempRow.Read (_binaryReader);

				Console.WriteLine("(Position: {0}, Row: {1})", i, tempRow);
			}

			Console.WriteLine("-----------------------");
		}
	}
}

