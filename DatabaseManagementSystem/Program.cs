using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Thread = System.Threading.Thread;

// BrinaryReader, FileStream
using System.Text;

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
//				if(rr != null)
//					a.Execute ((dynamic)rr);

			}


			return;
		}


		public static void TM()
		{
			TableManager a = new TableManager ("database.db");

			Request rr = RequestFactory.messageToRequest ("r,2");
//			a.Execute((dynamic)rr);

			Thread.Sleep (2000);

			Request sr = RequestFactory.messageToRequest ("s,1,2");
//			a.Execute ((dynamic)sr);
		}

		public static void Main (string[] args){


			TM ();


			return;
		}
	}
}
