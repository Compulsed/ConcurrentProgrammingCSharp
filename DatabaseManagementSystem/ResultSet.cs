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
	public class ResultSet
	{

		List<Row> _rowObjects;
		Latch _completedLatch;
		bool _hasSucceeded;

		public ResultSet ()
		{
			_rowObjects = new List<Row> ();
			_completedLatch = new Latch ();
		}


		public List<Row> getRows()
		{
			_completedLatch.Acquire ();

			return _rowObjects;
		}

		public bool hasCompleted()
		{
			_completedLatch.Acquire ();

			return _hasSucceeded;
		}
	}
}

