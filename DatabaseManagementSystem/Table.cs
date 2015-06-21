using System;
using System.Collections.Generic;
using System.Linq;
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
	/// -
	/// </summary>
	public class Table
	{
		private FileManager _fileManager = FileManager.Instance;
	    private Channel<Request> _fileManagerChannel = null;


		// Key: Row ID, Value: Row is cache
		public static Dictionary<UInt64, WrRow> _rowCache = new Dictionary<UInt64, WrRow>(); 

		public Table (string fileName = "database.db", bool newDatabase = true)
		{
		    FileManager.Instance.InitializeDatabase(_rowCache, fileName, newDatabase);
            _fileManagerChannel = _fileManager._inputChannel;
            _fileManager.Start ();
        }  

        // BUG: Does not remove the rows to operate on, hence doubling data on cache hit
	    private bool HandleSelect(Request aSelectRequest)
	    {
	        List<Row> rowsToOperateOn = aSelectRequest.GetOperationRows();
	        UInt64 numberOfRowsToCheck = (UInt64)rowsToOperateOn.Count;

            HashSet<Row> rowsToRemove = new HashSet<Row>();


            for (UInt64 i = 0; i < numberOfRowsToCheck; ++i)
            { 
                if ( _rowCache.ContainsKey(rowsToOperateOn[(int)i].RowId ))
	            {
	                Row tempRow = _rowCache[rowsToOperateOn[(int) i].RowId].CacheValue();

	                if (tempRow != null)
	                {
                        Console.WriteLine("Cache HIT! {0}", tempRow);

                        rowsToRemove.Add(tempRow);
                        aSelectRequest.AddRow(tempRow);
                    }
	            }
	        }

	        foreach (Row row in rowsToRemove) // PROBLEM
	            rowsToOperateOn.Remove(row);

            // They were all in the cache
	        return aSelectRequest.GetOperationRows().Count == 0;
	    }

        public void Accept(Request aRequest)
	    {
            Console.WriteLine($"TB: {aRequest}");

            if (aRequest.RequestType == RequestType.Read)
            {
                // They were all in the cache
                if (HandleSelect(aRequest))
                {
                    aRequest.Unlock(OperationStatus.Completed);
                    return;
                }
            }

            _fileManagerChannel.Put(aRequest);
        }
	}
}

