using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// BrinaryReader, FileStream

namespace DatabaseManagementSystem
{


	class MainClass
	{

		public static void RPEL()
		{
			TableManager a = new TableManager ("database.db");

			Request rr = null;

			string inputText = "";
			while (true) {
				inputText = Console.ReadLine ();

				if (inputText.Length == 0) {
					Console.WriteLine ("Closing!");
					return;
				}

				rr = RequestFactory.messageToRequest (inputText);
				if(rr != null)
					a.Execute ((dynamic)rr);

			}


			return;
		}


		public static void Main (string[] args){
			TableManager a = new TableManager ("database.db");

			Request rr = RequestFactory.messageToRequest ("r,2");
			a.Execute((dynamic)rr);

			a.Print ();

			return;
		}
	}
}
