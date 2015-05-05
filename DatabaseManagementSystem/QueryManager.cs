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

	/* public class SelectRequest : Request
	{
		
	}

	public class CreateRequest : Request 
	{

	}

	public class DeleteRequest : Request 
	{

	}

	public class UpdateRequest : Request
	{

	}*/ 

	// Just creates a bunch of nulled rows, will be filled in and then committed to the DB
	/* public class UpdateRequest : Request
	{
		private UInt64 randomRowsToMake;

		public UpdateRequest(UInt64 randomRowsToMake) 
		{
			this.randomRowsToMake = randomRowsToMake;
			base.resultSet._rowObjectsToBeCompleted = new List<Row> ((int)randomRowsToMake);
		}

		public UInt64 RandomRowsToMake {
			get {
				return randomRowsToMake;
			}
		}
	} */

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


	public static class RequestFactory 
	{

		private static RandomRequest createRandomRequest(string queryString)
		{
			Console.WriteLine ("Generating {0} rows of random data!", queryString);

			return new RandomRequest (Convert.ToUInt64 (queryString));
		}

		/* private static UpdateRequest createUpdateRequest(string queryString)
		{
			Console.WriteLine ("Generating {0} rows of update data!", queryString);

			return new UpdateRequest (Convert.ToUInt64 (queryString));
		} */


			

		public static Request messageToRequest(string message)
		{
			Request temp = null;

			List<string> words = message.Split (',').ToList();


			string queryType = words [0];
			words.RemoveAt (0);

			switch (queryType) {
			case "s":
				break;
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

