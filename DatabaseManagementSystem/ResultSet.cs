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

	public enum ActionType
	{
		READ = 0,
		CREATE,
		UPDATE,
		DELETE,
	}


	public class RequestRow
	{
		// On read(id, rowObj?), row is return
		// On delete(id, rowObj?), check _hasSucceeded status

		// On update(oldObject, newObject) the new row is returned
		// On update(id, newObject)

		// On create(rowObj), check _hasSucceeded or row returned?
		private Row _row;

		private Latch _rowLatch;
		private bool _hasSucceeded;
		private ActionType _dbAction;

		public Row row { get { return _row; } }

		public RequestRow(ActionType aAction)
		{

		}
	}

	public class ResultSet
	{
		List<RequestRow> _rowObjects;

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

