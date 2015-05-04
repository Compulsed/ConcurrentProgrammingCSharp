using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// BrinaryReader, FileStream

namespace DatabaseManagementSystem
{
	class MainClass
	{


		public static void Main (string[] args){

			TableManager a = new TableManager ("database.db");

			Request rr = RequestFactory.messageToRequest ("r,42");
			// Request ur = RequestFactory.messageToRequest ("u,42");

			a.Execute ((dynamic)rr);
			// a.Execute ((dynamic)ur);

			return;
		}
	}
}
