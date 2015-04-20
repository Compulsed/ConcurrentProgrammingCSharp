using System;
using System.IO;

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
	public class FileManager<RowType> : ChannelActiveObject<Request>
	{

		// Binary, each row takes a fixed no bytes
		// Strings are to be fixed size
		// File tableFile;

		// Must have access to the cache

		// Weak references for the Rows



		public FileManager ()
		{


		}



		protected override void Process (Request passedData)
		{

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

			throw new NotImplementedException ();
		}
	}
}

