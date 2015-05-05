using System;
using DSalter.ConcurrentUtils;

using System.Collections.Generic;

namespace DatabaseManagementSystem
{
	/// <summary>
	/// Result set.
	/// 
	/// Is a future object
	/// 
	/// Must contain
	/// - Set (list) of Row objects - those read from the file, or to be updated
	/// - A result status (success or failure)
	/// - A latch opened when the value is ready (within the get accessors for rows and the status)
	/// 
	/// </summary>
	/// 


	public class ResultSet
	{
		public List<Row> _rowObjectsCompleted;
		public List<Row> _rowObjectsToBeCompleted;

		public bool _hasSucceeded;

		public bool _hasCompleted = false;
		public Latch _completedLatch;


		public ResultSet (List<Row> uncompletedRows)
		{
			_completedLatch = new Latch ();

			_rowObjectsToBeCompleted = uncompletedRows;
		}
			

		public List<Row> getRows()
		{
			_completedLatch.Acquire ();

			return _rowObjectsCompleted;
		}

		public bool hasCompletionStatus()
		{
			_completedLatch.Acquire ();

			return _hasSucceeded;
		}


	}





}

