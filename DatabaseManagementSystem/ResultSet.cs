using System;
using DSalter.ConcurrentUtils;

using System.Collections.Generic;
using System.Linq;

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

    public enum RequestType
    {
        Read,   // Input: Empty rows with Ids           Output: Filled rows with those ids 
        Write,  // Input: Row without                   Output: Row with an id
        Update, // Input: Row with old id, new data     Output: That row
        Delete, // Input: The row with the id           Output: The deleted row
        Random  // Input: Row arrays pointing to null   Output: Random rows
    }

    public enum OperationStatus
    {
        Completed,      // Everything worked fine
        Failed,         // Fatal error, noting was committed
        Inprogress,     // The starting status of the request
        Partial         // Part of the query completed correctly
    }

    // Contains the Result set and the Query details
    public class Request
    {
        private QueryDetails _queryDetails;
        private ResultSet _resultSet;

        public Request()
        {
            
        }

        // -- Setup
        public void SetQuery(QueryDetails queryDetails)
        {
            _queryDetails = queryDetails;
        }

        public void SetResultSet(ResultSet resultSet)
        {
            _resultSet = resultSet;
        }

        // -- Internal
        public ResultSet GetResultSet()
        {
            return _resultSet;
        }

        public QueryDetails GetQueryDetails()
        {
            return _queryDetails;
        }

        public RequestType RequestType
        {
            get { return _queryDetails.RequestType; }
        }

        public void AddRow(Row aRow)
        {
            _resultSet._AddRow(aRow);
        }

        public Row RemoveRow()
        {
            return _queryDetails._RemoveRow();
        }

        public UInt64 NumberOfRowsLeft()
        {
            return _queryDetails._NumberOfRowsLeft;
        }

        public void Unlock(OperationStatus anOpStatus)
        {
            _resultSet._Completed(anOpStatus);
        }

        public List<Row> GetOperationRows()
        {
            return _queryDetails._GetOperationRows();
        } 

        // -- For special
        public override string ToString()
        {
			return String.Format("Is a {0} and has {1} to complete", _queryDetails.RequestType.ToString(), _queryDetails._NumberOfRowsLeft);
        }
    }

    // Contains list of row objects, status and a latch
    public class ResultSet
    {
        private OperationStatus _status = OperationStatus.Inprogress;
        private Latch _completed = new Latch();
        private List<Row> _completedRows = new List<Row>();

        public ResultSet()
        {

        }

        // -- Client use
        public List<Row> Rows
        {
            get
            {
                _completed.Acquire();
                return _completedRows;
            }
        }

        // -- Internal use
        public void _AddRow(Row aRow)
        {
            _completedRows.Add(aRow);
        }

        public void _Completed(OperationStatus anOperationStatus)
        {
            _status = anOperationStatus;
            _completed.Release();
        }

    }

    // Contains, The command, all the information for the construction f the result set
    // Query has been completed when there are no more rowsToOperateOn
    public class QueryDetails
    {
        private RequestType _requestType;
        private List<Row> _rowsToOperateOn;

        public QueryDetails(RequestType requestType, Row[] rowsToOperateOn)
        {
            _requestType = requestType;
            _rowsToOperateOn = rowsToOperateOn.ToList();
        }

        public List<Row> _GetOperationRows()
        {
            return _rowsToOperateOn;
        }

        public RequestType RequestType
        {
            get
            {
                return _requestType;
            }
        }

        public Row _RemoveRow()
        {
            Row temp = _rowsToOperateOn.First();
            _rowsToOperateOn.RemoveAt(0);
            return temp;
        }

        public UInt64 _NumberOfRowsLeft
        {
            get { return (UInt64)_rowsToOperateOn.Count; }
        }
    }


}

