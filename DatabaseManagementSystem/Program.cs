using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public static void Main (string[] args)
		{
            Console.Write("Database Name: ");
		    string databaseName = Console.ReadLine();

            Console.Write("Load or Create Empty? (load/empty): ");
		    string option = Console.ReadLine();

		    bool bOption = option != "load";
            

            QueryManager qm = new QueryManager(databaseName, bOption);
            qm.Start();

            RPEL.FMREPL();

			return;
		}
	}
}
