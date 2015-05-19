using System;
using DSalter.ConcurrentUtils;
using Thread = System.Threading.Thread;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using System.Text;
using System.Linq;

namespace DatabaseManagementSystem
{

	public abstract class Request 
	{
		public ResultSet resultSet;

		public Request(UInt64 randomRowsToMake)
		{
			resultSet = new ResultSet (new List<Row> ((int)randomRowsToMake));
		}

		public ResultSet ResulltSet {
			get {
				return resultSet;
			}
		}
	}


	/*public class CreateRequest : Request 
	{

	}

	public class DeleteRequest : Request 
	{

	}

	public class UpdateRequest : Request
	{

	}*/ 

	// Just creates a bunch of nulled rows, will be filled in and then committed to the DB
	public class RandomRequest : Request
	{
		public UInt64 randomRowsToMake;

		public RandomRequest(UInt64 randomRowsToMake) : base(randomRowsToMake)
		{
			this.randomRowsToMake = randomRowsToMake;
			base.resultSet._rowObjectsCompleted = new List<Row> ();
		}

		public UInt64 RandomRowsToMake {
			get {
				return randomRowsToMake;
			}
		}
	}

	public class SelectRequest : Request
	{
		public UInt64 startId;
		public UInt64 endId;

		public SelectRequest(UInt64 start, UInt64 end) : base(end - start)
		{
			this.startId = start;
			this.endId = end;

			base.ResulltSet._rowObjectsCompleted = new List<Row> ();
		}
	}


	public static class RequestFactory 
	{

		private static RandomRequest createRandomRequest(string queryString)
		{
			Console.WriteLine ("Generating {0} rows of random data!", queryString);

			return new RandomRequest (Convert.ToUInt64 (queryString));
		}

		private static SelectRequest createSelectRequest(string start, string end)
		{
			Console.WriteLine ("Selecting rows between [{0} - {1}]", start, end);

			return new SelectRequest (Convert.ToUInt64 (start), Convert.ToUInt64(end));
		} 


			

		public static Request messageToRequest(string message)
		{
			Request temp = null;

			List<string> words = message.Split (',').ToList();


			string queryType = words [0];
			words.RemoveAt (0);

			switch (queryType) {
			case "s":
				return createSelectRequest (words [0], words [1]);
			case "c":
				break;
			case "u":
				return null; //createUpdateRequest (words[0]);
			case "d":
				break;
			case "r":
				return createRandomRequest (words[0]);
			}

			return temp;
		}
	}

	public class QueryManager : ChannelActiveObject<Connection>
	{
		ConnectionManager _incomingConnections;

		TableManager _myTable;


		public QueryManager()
		{
			_myTable = new TableManager ();


			_incomingConnections = new ConnectionManager (base._inputChannel);
			_incomingConnections.Start ();
		}




		protected override void Process (Connection connectionMessage)
		{
			ConnectionWithMessage aMessageCon = (ConnectionWithMessage)connectionMessage;


			// _myTable.execute()

			connectionMessage.writer.WriteLine (aMessageCon.message);

			Console.WriteLine (aMessageCon.message);
		}

	}
}

