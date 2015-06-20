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

		public static void RPEL()
		{
		}


		public static void TM()
		{

		}

		public static void Main (string[] args)
		{
            // Which would be contained within the table


            Dictionary<UInt64, Row> rowCache = new Dictionary<ulong, Row>();

            while (true)
		    {
                Console.WriteLine("empty, save, load, random <no>, statistics, save, kv, file, rl, delete <no>, cache <f/e>");
                Console.Write("~~> ");

		        string inputString = Console.ReadLine();

		        if (String.IsNullOrEmpty(inputString))
		        {
                    Console.WriteLine("Invalid Input");
		            continue;
		        }

		        string[] parsedInput = inputString.Split(' ');
		        string command = parsedInput[0];


		        switch (command)
		        {
		            case "load":
		            {
                            StreamReader sr = new StreamReader("Database.db" + ".cfg");
                            Configuration.LoadFileManager(rowCache, "Database.db");

                            FileManager.Instance.ConfigurationReader(sr);

                            sr.Close();

                            break;
		            }

                    case "empty":
		            {
                            Configuration.EmptyFileManager(rowCache, "Database.db");
                            break;
		            }

                    case "random":
		            {
		                UInt64 noToInsert = 0;
                        try
		                {
		                    noToInsert = UInt64.Parse(parsedInput[1]);
		                }
		                catch (Exception)
		                {
                            Console.WriteLine("random <number of random records>");
		                    break;
		                }

                        for (UInt64 i = 0; i < noToInsert; ++i)
                            FileManager.Instance.CreateRandomRows();
                        break;
		            }

                    case "statistics":
		            {
                            Console.WriteLine(FileManager.Instance.Statistics());
                            break;
		            }

                    case "save":
		            {       
                       Configuration.SaveFileManager(FileManager.Instance);

		                StreamWriter sw = new StreamWriter("Database.db" + ".cfg");
                        FileManager.Instance.ConfigurationWriter(sw);
                        sw.Close();

                        Environment.Exit(0);
                        break;
		            }

                    case "kv":
		            {
		                    FileManager.Instance.PrintKeyValue();
		                    break;
		            }

                    case "file":
		            {
		                    FileManager.Instance.PrintFileContents();
		                    break;
		            }

                    case "rl":
		            {
		                    FileManager.Instance.PrintRowLocationsInFile();
		                    break;
		            }

                    case "cache":
		            {
                        if(parsedInput[1] == "f")
                            FileManager.Instance.FillCache();
                        else
                           FileManager.Instance.EmptryCache();
                        break;
		            }

                    case "delete":
		            {
                            UInt64 noToRemove = 0;
                            try
                            {
                                noToRemove = UInt64.Parse(parsedInput[1]);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("delete <number of records to delete records>");
                                break;
                            }

                            for (UInt64 i = 0; i < noToRemove; ++i)
                                FileManager.Instance.CreateRandomRows();
                            break;
                        }

		            default:
		            {
                            Console.WriteLine("Invalid Input");
		                    break;
		            }
		        }

                Console.WriteLine();
		    }


		    Console.ReadLine();

			return;
		}
	}
}
