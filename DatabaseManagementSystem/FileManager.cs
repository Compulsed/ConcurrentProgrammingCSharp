using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DSalter.ConcurrentUtils;

namespace DatabaseManagementSystem
{
    /// <summary>
    /// File manager.
    /// 
    /// Must handle
    /// - Reading and writing to the file
    /// 
    /// When getting request to fill the ResultSet 
    /// 	must set status and open latch
    /// 
    /// Must check that the Row is not already in the Tables cachce
    /// 	if it is add it to the ResultSet, if not add the row to the
    /// 	cache and then add it to the ResultSet.
    /// 
    /// Always handles Updates and deletes
    /// 
    /// Rows will be marked as deleted will have their "deleted" flag updated in the file
    /// 	There should no longer be considered to exist, but iwll remain in the file until
    /// 	overriden
    /// 
    /// Deleted rows should be in a queue, as the latest one to be deleted will be overriden
    /// 
    /// If there is no more space it should allocate more space by increasing the file size by
    /// 120% then adding the record
    /// 
    /// Compression can be added at a later stage to remove the deleted files if need be
    /// 
    /// File should be binary and fixed length and offsets should be used.
    /// 
    /// Weak references for the Rows within the File manager cache so that the 
    /// 	cached objects can be freed if memory is scarce. 
    /// 
    ///         // Loccation of first free space? // #2
    /// </summary>

    // TODO: Probably a smarter way of doing configuration
    public static class Configuration
    {
        // Format: Last Row ID
        public static UInt64 IdOfNextRow = 0; // TODO: This is public and I am lazy

        public static void SaveFileManager(FileManager aFileManager)
        {}

        public static void LoadFileManager(Dictionary<UInt64, Row> rowCache, string fileName)
        {
            FileManager.Instance.InitializeDatabase(rowCache, fileName, false);
        }

        public static FileManager EmptyFileManager(Dictionary<UInt64, Row> rowCache, string fileName)
        {
            FileManager.Instance.InitializeDatabase(rowCache, fileName, true);

            return FileManager.Instance;
        }
    }

    public class FileManager : ChannelActiveObject<Request>
	{
		private static FileManager _instance = null;
        private const UInt64 INCREASE_DATABASE_BY = 100;

        // Given Row ID, contains the actual row
		private Dictionary<UInt64, Row> _rowCache = null;

        // Given Row ID, contains location within the file
        private Dictionary<UInt64, UInt64> _rowLocationInFile = null;

        // The row offsets for empty files
        private SortedSet<UInt64> _freeFilePositions = null; 
       
        private UInt64 _idOfNextRow = 0;                // The row id of the next row to be added   $Persistent
        private UInt64 _currentDatabaseCapacity = 0;    // The size amount of space the current database has, can be resized
		private UInt64 _noActiveRows = 0;               // The amount of active rows in the file

        private string _fileName = "DefaultDatabase.db";

        private FileStream _databaseFile = null;
		private BinaryWriter _binaryWriter = null;
		private BinaryReader _binaryReader = null;


        public FileManager (){}

        public void _SetUpNewDatabase()
        {
            _noActiveRows = 0;
            _idOfNextRow = 0;
            _currentDatabaseCapacity = 0;

            _rowCache.Clear(); // Must clear it due to the RowCache actually being passed in, not created within this object

            // Close any old file handlers
            if (_binaryReader != null)
                _binaryReader.Close();

            if (_binaryWriter != null)
                _binaryWriter.Close();

            if (_databaseFile != null)
                _databaseFile.Close();

            // Should delete the old files
            if (File.Exists(_fileName))
                File.Delete(_fileName);
            

            if (File.Exists(_fileName + ".cfg"))
                File.Delete(_fileName + ".cfg");
            
        }

        // For configuration data to be valid, must be called from the Configuration.LoadFileManager TODO: This is yuk
        public void InitializeDatabase(Dictionary<UInt64, Row> rowCache, string fileName, bool newDatabase = true)
        {
            _fileName = fileName;

            // Must be repopulated if loading in from a save
            _rowCache = rowCache;
            _freeFilePositions = new SortedSet<UInt64>();
            _rowLocationInFile = new Dictionary<ulong, ulong>();

            if (newDatabase || !File.Exists(_fileName) || !File.Exists(_fileName + ".cfg"))
            {
                _SetUpNewDatabase();

                // Create all the required files and streams
                _databaseFile = new FileStream(_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                _binaryReader = new BinaryReader(_databaseFile, System.Text.Encoding.Unicode);
                _binaryWriter = new BinaryWriter(_databaseFile, System.Text.Encoding.Unicode);

                Console.WriteLine("Created Database: {0}", _fileName);
            }
            else
            {
                _databaseFile = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _binaryReader = new BinaryReader(_databaseFile, System.Text.Encoding.Unicode);
                _binaryWriter = new BinaryWriter(_databaseFile, System.Text.Encoding.Unicode);

                // We CAN rebuild indexes
                // _idOfNextRow = Configuration.IdOfNextRow;

                // Tuple<Dictionary<ulong, ulong>, SortedSet<ulong>> rowLocationsInFile = BuildRowLocationsFromFile();
                // _rowLocationInFile = rowLocationsInFile.Item1;
                // _freeFilePositions = rowLocationsInFile.Item2;

                // _rowCache = BuildRowCacheFromFile(_rowCache);

                Console.WriteLine("Loaded Database: {0}", _fileName);
            }
        }

        public void FillCache()
        {
            _rowCache = BuildRowCacheFromFile(_rowCache);
        }

        public void EmptryCache()
        {
            foreach (var key in _rowCache.Keys.ToList())
                _rowCache[key] = null;
        }

        public void InsertRowInEmptyPlace(Row rowToInsert)
        {
            UInt64 locationToInsertAt;

            try
            {
                locationToInsertAt = _freeFilePositions.First();
            }
            catch (Exception)
            {
                // Make more space
                Console.WriteLine("Making more space!");

                _binaryWriter.BaseStream.SetLength((long)INCREASE_DATABASE_BY * (long)Row.ByteSize() + _binaryWriter.BaseStream.Length);

                for (UInt64 i = 0; i < INCREASE_DATABASE_BY; ++i)
                {
                    _freeFilePositions.Add(_currentDatabaseCapacity + i);
                }
                _currentDatabaseCapacity += INCREASE_DATABASE_BY;

                locationToInsertAt = _freeFilePositions.First();
            }

            // This row location is now no longer free
            _freeFilePositions.Remove(locationToInsertAt);

            // Add the new row to the cache
            _rowCache.Add(rowToInsert.RowId, rowToInsert);

            // Add the new row id and this location in the file it is at
            _rowLocationInFile.Add(rowToInsert.RowId, locationToInsertAt);

            // Seek to that location and then write it to it
            _binaryWriter.BaseStream.Seek((long)(Row.ByteSize() * locationToInsertAt), SeekOrigin.Begin);
            rowToInsert.Write(_binaryWriter);

            ++_noActiveRows;
        }



        public void CreateRandomRows()
        {
                UInt64 thisRowId = _idOfNextRow++; // TODO: Probably should not be here, but inside function

                InsertRowInEmptyPlace(new Row(thisRowId, "R -> " + thisRowId));            
        }


        public Tuple<Dictionary<ulong, ulong>, SortedSet<UInt64>> BuildRowLocationsFromFile()
        {
            Dictionary<ulong, ulong> builtRowLocations = new Dictionary<ulong, ulong>();
            SortedSet<UInt64> freeRowLocations = new SortedSet<ulong>();

            _binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            Row tempRow = new Row();
            UInt64 rowPositionInFile = 0;

            while (_binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length)
            {
                tempRow.Read(_binaryReader);

                if (tempRow.IsActive())
                {
                    builtRowLocations.Add(tempRow.RowId, rowPositionInFile);
                }
                else
                {
                    freeRowLocations.Add(rowPositionInFile);
                }

                ++rowPositionInFile;
            }

            return new Tuple<Dictionary<ulong, ulong>, SortedSet<ulong>>(builtRowLocations, freeRowLocations);
        }

        public Dictionary<UInt64, Row> BuildRowCacheFromFile(Dictionary<UInt64, Row> rowCache)
        {
            _binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            while (_binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length)
            {
                Row tempRow = new Row();
                tempRow.Read(_binaryReader);

                if (tempRow.IsActive())
                {
                    if (rowCache.ContainsKey(tempRow.RowId))
                    {
                        rowCache[tempRow.RowId] = tempRow;
                    }
                    else
                    {
                        rowCache.Add(tempRow.RowId, tempRow); // TODO: Do not add in the row, maybe save the row cache on exit
                    }
                }
            }

            return rowCache;
        }

        public void DeleteRandomRows()
        {
            UInt64 rowIndex = 0;
            bool found = false;

            foreach (KeyValuePair<UInt64, UInt64> entry in _rowLocationInFile)
            { 
                Console.WriteLine("Row Index: {0}, Position In File {1})", entry.Key, entry.Value);
                rowIndex = entry.Key;

                found = true;

                break;
            }

            if (found)
            {
                Row rowToRemove = _rowCache[rowIndex];
                _rowCache.Remove(rowIndex); // Remove it from the cache
                
                rowToRemove.SetStatus(RowStatus.Deleted);

                // Write the deleted record back to file
                _binaryWriter.BaseStream.Seek((long)(Row.ByteSize() * _rowLocationInFile[rowIndex]), SeekOrigin.Begin);

                rowToRemove.Write(_binaryWriter);

                // Add the deleted space for later additions
                _freeFilePositions.Add(_rowLocationInFile[rowIndex]);

                // Remove from valid locations
                _rowLocationInFile.Remove(rowIndex);

                --_noActiveRows;
            }
            else
            {
                Console.WriteLine("No more rows left to delete!");
            }
        }


		void ProcessRequest(Request aRandomRequest){}


		void ProcessRequest(SelectRequest aSelectRequest)
		{
			Console.WriteLine ("FileManager -> Processing aSelectRequest");
		}

		protected override void Process (Request passedData)
		{

			// ProcessRequest ((dynamic)passedData);

			// If Reading, check that it is not already in the cache
			// if it is, add to the ResultSet, if not fetch from file
			// set latch

			// If Updating, read the rows and update the values in the file

			// If deleted the deleted flag in the file ill be set
			// These are no longer to be considered existent, but will remain 
			// in the file to be overriden

			// If creating, locate first deleted row, and insert the new row at that location
			// Otherwise add the new row to the end of the row
			// Allocate 120% of its current size when it becomes full

			// Add a compress method later on

			// throw new NotImplementedException ();
		}

        // Must Save... Row Id -> Location
        //              Locations Free
        //              Id of the next row
        //              Active rows would also be cool
        public void ConfigurationWriter(StreamWriter sw)
        {
            sw.WriteLine(_idOfNextRow);
            sw.WriteLine(_noActiveRows);
            sw.WriteLine(_currentDatabaseCapacity);

            StringBuilder sb = new StringBuilder();

            // RowId:Location In File, 
            foreach (KeyValuePair<UInt64, UInt64> entry in _rowLocationInFile)
                sb.Append($"{entry.Key}:{entry.Value},");
            sb.Length--; // Removes the tailing comma

            sw.WriteLine(sb.ToString());
            sb.Clear();


            // Print Free spaces in file
            foreach (UInt64 entry in _freeFilePositions)
                sb.Append($"{entry},");
            sb.Length--; // Removes the trailing comma

            sw.WriteLine(sb.ToString());
            sb.Clear();
        }

        public void ConfigurationReader(StreamReader sr)
        {
            _idOfNextRow = (UInt64.Parse(sr.ReadLine()));
            _noActiveRows = (UInt64.Parse(sr.ReadLine()));
            _currentDatabaseCapacity = (UInt64.Parse(sr.ReadLine()));

            // RowId:Location In File
            string rowIds = sr.ReadLine();
            string[] sRowId = rowIds.Split(',');

            foreach (string id in sRowId)
            {
                string[] idFileLocationPair = id.Split(':');

                _rowCache.Add(UInt64.Parse(idFileLocationPair[0]), null);
                _rowLocationInFile.Add(UInt64.Parse(idFileLocationPair[0]), UInt64.Parse(idFileLocationPair[1]));
            }

            string freeLocationString = sr.ReadLine();
            string[] freeLocation = freeLocationString.Split(',');

            foreach (string location in freeLocation)
            {
                _freeFilePositions.Add(UInt64.Parse(location));
            }
        }

        public void PrintKeyValue()
		{
			Console.WriteLine("\n-------------------------");
			Console.WriteLine("----- Row Cache ----------");
			Console.WriteLine("--------------------------");
            foreach (KeyValuePair<UInt64, Row> entry in _rowCache)
            {
                if (entry.Value != null)
                {
                    Console.WriteLine("(Row ID: {0}, Row {1})", entry.Key, entry.Value);
                }
                else
                {
                    Console.WriteLine("(Row ID: {0}, NOT IN CACHE)", entry.Key);
                }
            }
            Console.WriteLine("-----------------------");
		}

		public void PrintFileContents()
		{
			Console.WriteLine("\n--------------------------");
			Console.WriteLine("----- File Contents -------");
			Console.WriteLine("---------------------------");

			_binaryReader.BaseStream.Seek (0, SeekOrigin.Begin);
			Row tempRow = new Row ();

		    UInt64 i = 0;
            while (_binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length)
            { 
                tempRow.Read (_binaryReader);

                if (tempRow.IsActive())
                {
                    Console.WriteLine("Offset In File: {0}, Row: {1})", i, tempRow);
                }
                else
                {
                    Console.WriteLine(tempRow);
                }

                ++i;
            }

			Console.WriteLine("-----------------------");
		}

        public void PrintRowLocationsInFile()
        {
            Console.WriteLine("\n----------------------------------");
            Console.WriteLine("----- Row locations in file -------");
            Console.WriteLine("-----------------------------------");
            foreach (KeyValuePair<UInt64, UInt64> entry in _rowLocationInFile)
                Console.WriteLine("Row Index: {0}, Position In File {1})", entry.Key, entry.Value);

            Console.WriteLine("Free Spaces in file: ");
            foreach(UInt64 freeOffset in _freeFilePositions)
                Console.Write($"{freeOffset} ");
            Console.WriteLine();

            Console.WriteLine("-----------------------");
        }


        public string Statistics()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-- Statistics --");
            sb.AppendLine($"Name of file:\t{_fileName}");
            sb.AppendLine($"Active rows:\t{_noActiveRows}");
            sb.AppendLine($"Id of Next Row:\t{_idOfNextRow}");
            sb.AppendLine($"Database cap:\t{_currentDatabaseCapacity} rows");

            return sb.ToString();
        }

        public UInt64 IdOfNextRow
        {
            get { return _idOfNextRow; }
            set { _idOfNextRow = value; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public static FileManager Instance
		{
			get 
			{
				if (_instance == null)
					_instance = new FileManager ();

				return _instance;
			}
		}
	}
}

