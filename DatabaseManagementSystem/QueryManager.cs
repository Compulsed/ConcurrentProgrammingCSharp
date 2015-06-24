using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DatabaseManagementSystem
{
    public static class RequestBuilder
    {
        public static Request BuildRequest(string inputString)
        {
            string[] commandDataSeperate = inputString.Split(',');
            string command;
            string data;

            if (commandDataSeperate.Length < 2)
                return null;

            command = commandDataSeperate[0];
            data = commandDataSeperate[1];

            if (String.IsNullOrWhiteSpace(command) || String.IsNullOrWhiteSpace(command))
                return null;

            Request returnRequest = new Request();
            ResultSet resultSet = new ResultSet();

            QueryDetails queryDetails = null;
            Row[] rowsToOperateOn = null;


            switch (command)
            {
                // 1-10
                case "s":
                {
                    string[] startEndSplit = data.Split('-');
                    UInt64 startRange = 0;
                    UInt64 endRange = 0;

                    if ((startEndSplit.Length < 2) || String.IsNullOrWhiteSpace(startEndSplit[0]) || String.IsNullOrWhiteSpace(startEndSplit[1]))
                        return null;

                    try
                    {
                        startRange = UInt64.Parse(startEndSplit[0]);
                        endRange = UInt64.Parse(startEndSplit[1]);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    if (startRange > endRange)
                        return null;

                    rowsToOperateOn = new Row[(endRange - startRange) + 1];

                    for (UInt64 i = 0; i < (UInt64)rowsToOperateOn.Length; ++i)
                    {
						rowsToOperateOn[i] = new Row(i + startRange);
                    }

                    queryDetails = new QueryDetails(RequestType.Read, rowsToOperateOn);
                    break;
                }
                
                // 10
                case "r":
                {
                    UInt64 amountOfRandomData;
                    try
                    {
                        amountOfRandomData = UInt64.Parse(data);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    rowsToOperateOn = new Row[amountOfRandomData];
                    queryDetails = new QueryDetails(RequestType.Random, rowsToOperateOn);
                    break;
                }
                
                // 42:data
                case "u":
                {
                    string[] rowIdUpdateSplit = data.Split(':');
                    UInt64 rowId;
                    string updatedData = "";

                    if ((rowIdUpdateSplit.Length < 2) || String.IsNullOrWhiteSpace(rowIdUpdateSplit[0]))
                        return null;

                    updatedData = rowIdUpdateSplit[1];

                    // Get the rowID
                    try
                    {
                        rowId = UInt64.Parse(rowIdUpdateSplit[0]);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    
                    rowsToOperateOn = new Row[]
                    {
                        new Row(rowId, updatedData),
                    };

                    queryDetails = new QueryDetails(RequestType.Update, rowsToOperateOn);
                    break;
                }
                
                // 42
                case "d":
                {
                    UInt64 recordToRemove;
                    try
                    {
                        recordToRemove = UInt64.Parse(data);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    rowsToOperateOn = new Row[]
                    {
                        new Row(recordToRemove),
                    };

                    queryDetails = new QueryDetails(RequestType.Delete, rowsToOperateOn);
                    break;
                }
                
                // data
                case "c":
                {
                    rowsToOperateOn = new Row[]
                    {
                        new Row(0, data), 
                    };
                    queryDetails = new QueryDetails(RequestType.Write, rowsToOperateOn);
                    break;
                }

                default:
                {
                    return null;
                }
            }

            returnRequest.SetQuery(queryDetails);
            returnRequest.SetResultSet(resultSet);

            return returnRequest;
        }


    }

	public class QueryManager : ChannelActiveObject<Connection>
	{
		ConnectionManager _incomingConnections;

		TableManager _myTable;


		public QueryManager(string fileName = "database.db", bool newDatabase = true)
		{
			_myTable = new TableManager (fileName, newDatabase);

			_incomingConnections = new ConnectionManager (base._inputChannel);
            _incomingConnections.Start();
        }

	    public void UserProcess()
	    {
	        while (true)
	        {
                Console.Write("Enter a Request \n~> ");
	            Request request = null;

	            while (request == null)
	            {
	                request = RequestBuilder.BuildRequest(Console.ReadLine());
	            }

				Console.WriteLine("QM: {0}", request);

	            _myTable.Accept(request);

	            ResultSet MyRows = request.GetResultSet();

                foreach (Row row in MyRows.Rows) // Automatically blocks
	            {
					Console.WriteLine("Got {0}", row);
	            }
	        }
	    }


	    protected override void Process(Connection connectionMessage)
	    {
	        ConnectionWithMessage aMessageCon = (ConnectionWithMessage) connectionMessage;

			Console.WriteLine("Request: {0}", aMessageCon.message);

            Request request = RequestBuilder.BuildRequest(aMessageCon.message);

	        StringBuilder sb = new StringBuilder();

	        if (request != null) { 
	            _myTable.Accept(request);

	            ResultSet MyRows = request.GetResultSet();

                sb.AppendLine("---------------------------------------------");
                foreach (Row row in MyRows.Rows) // Automatically blocks
	            {
					sb.AppendLine(String.Format("{0}", row));
	            }
                sb.AppendLine("---------------------------------------------");
            }
            else
	        {
	            sb.AppendLine("Invalid input!");
	        }

            sb.Append("~> ");

            connectionMessage.writer.Write (sb.ToString());
	        connectionMessage.writer.Flush();
	    }

	}
}

