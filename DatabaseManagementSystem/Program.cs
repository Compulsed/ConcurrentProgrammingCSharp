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
		}


		public static void TM()
		{

		}

		public static void Main (string[] args){
			FileManager FM = FileManager.Instance;

			FM.CreateRandomRows (100);


			return;
		}
	}
}
