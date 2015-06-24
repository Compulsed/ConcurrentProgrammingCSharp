using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

    public class FileManager : ChannelActiveObject<Request>
	{
		private static FileManager _instance = null;
        private const UInt64 INCREASE_DATABASE_BY = 100;
        private const double DATABASE_EXPANDER = 2;
        private Random rand = new Random(Guid.NewGuid().GetHashCode());


        // Given Row ID, contains the actual row
        private Dictionary<UInt64, WrRow> _rowCache = null;

        // Given Row ID, contains location within the file
        private Dictionary<UInt64, UInt64> _rowLocationInFile = null;

        // The row offsets for empty files
        private SortedSet<UInt64> _freeFilePositions = null; 
       
        private UInt64 _idOfNextRow = 0;                // The row id of the next row to be added   $Persistent
        private UInt64 _currentDatabaseCapacity = 0;    // The size amount of space the current database has, can be resized
		private UInt64 _noActiveRows = 0;               // The amount of active rows in the file

        private string _fileName = "database.db";

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
        public void InitializeDatabase(Dictionary<UInt64, WrRow> rowCache, string fileName, bool newDatabase = true)
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
                StreamReader sr = new StreamReader(FileName + ".cfg");
                ConfigurationReader(sr);
                sr.Close();

                _databaseFile = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _binaryReader = new BinaryReader(_databaseFile, System.Text.Encoding.Unicode);
                _binaryWriter = new BinaryWriter(_databaseFile, System.Text.Encoding.Unicode);

                Console.WriteLine("Loaded Database: {0}", _fileName);
            }
        }

        //---------------------------------------------------------------------
        //          : Processors
        //---------------------------------------------------------------------
        private void HandleRandom(Request randomRequest)
        {
            while (randomRequest.NumberOfRowsLeft() != 0)
            {
                randomRequest.RemoveRow();

                randomRequest.AddRow(CreateRandomRow());
            }

            randomRequest.Unlock(OperationStatus.Completed);
        }

        private void HandleCreate(Request aCreateRequest)
        {
            Row rowToCreate = aCreateRequest.RemoveRow();

            rowToCreate.RowId = GetNewId();

            InsertRow(rowToCreate);
            aCreateRequest.AddRow(rowToCreate);

            aCreateRequest.Unlock(OperationStatus.Completed);
        }

        // BUG: Does not remove the rows to operate on, hence doubling data on cache hit
        private void HandleSelect(Request aSelectRequest)
        { 
            List<Row> rowsToOperateOn = aSelectRequest.GetOperationRows();
            UInt64 numberOfRowsToCheck = (UInt64)rowsToOperateOn.Count;

            HashSet<Row> rowsToRemove = new HashSet<Row>();

            for (UInt64 i = 0; i < numberOfRowsToCheck; ++i)
            {
                Row tempRow = GetRowByIndex(rowsToOperateOn[(int)i].RowId);

                if (tempRow != null)
                {
                    rowsToRemove.Add(tempRow);
                    aSelectRequest.AddRow(tempRow);
                }
               
            }

            foreach (Row row in rowsToRemove)
                rowsToOperateOn.Remove(row);


            aSelectRequest.Unlock(aSelectRequest.GetOperationRows().Count == 0
                ? OperationStatus.Completed
                : OperationStatus.Partial);
        }

        private void HandleUpdate(Request aUpdateRequest)
        {
            Row newRow = aUpdateRequest.GetOperationRows()[0];


            if (UpdateRow(newRow))
            {
                aUpdateRequest.AddRow(newRow);
                aUpdateRequest.Unlock(OperationStatus.Completed);
            }
            else
            {
                aUpdateRequest.Unlock(OperationStatus.Failed);
            }
        }

        private void HandleDelete(Request aDeleteRequest)
        {
            Row rowToDelete = aDeleteRequest.RemoveRow();

            rowToDelete = DeleteRowByIndex(rowToDelete.RowId);

            if (rowToDelete != null)
            {
                aDeleteRequest.AddRow(rowToDelete);

                aDeleteRequest.Unlock(OperationStatus.Completed);
            }
            else
            {
                aDeleteRequest.Unlock(OperationStatus.Failed);
            }
        }


        protected override void Process(Request aRequest)
        {
			Console.WriteLine("FM: {0}", aRequest);

            switch (aRequest.RequestType)
            {
                case RequestType.Random:
                {
                    HandleRandom(aRequest);
                    break;
                }

                case RequestType.Write:
                {
                    HandleCreate(aRequest);
                    break;
                }

                case RequestType.Read:
                {
                    HandleSelect(aRequest);
                    break;
                }

                case RequestType.Update:
                {
                    HandleUpdate(aRequest);
                    break;
                }

                case RequestType.Delete:
                {
                    HandleDelete(aRequest);
                    break;
                }

                default:
                {
					Console.WriteLine("FM - Unable to process {0} at the moment", aRequest);
                    break;
                }
                
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Statistics());
            Console.ResetColor();
        }

        //---------------------------------------------------------------------
        //          : CRUD OPERATIONS
        //---------------------------------------------------------------------
        public Row GetRowByIndex(UInt64 rowIndex)
        {
            if (_rowCache.ContainsKey(rowIndex))
            {
                return _rowCache[rowIndex].GetRow();
            }
            else
            {
                return null;
            }
        }

        public Row DeleteRowByIndex(UInt64 rowIndex)
        {
            Row deletedRow = null;

            if (_rowCache.ContainsKey(rowIndex))
            {
                deletedRow = _rowCache[rowIndex].GetRow();

                _freeFilePositions.Add(_rowLocationInFile[rowIndex]);
                _rowCache[rowIndex].GetRow().Delete(_binaryWriter, _rowLocationInFile);
                _rowCache.Remove(rowIndex);

                --_noActiveRows;
            }

            return deletedRow;
        }


        public bool UpdateRow(Row aRow)
        {
            // Get the ID of the aRow
            UInt64 id = aRow.RowId;

            if (!_rowCache.ContainsKey(id))
                return false;

            // Update the cache pointer
            _rowCache[id] = new WrRow(aRow);

            // Change the value in the file
            _binaryWriter.BaseStream.Seek((long)(Row.ByteSize() * _rowLocationInFile[id]), SeekOrigin.Begin);
            aRow.Write(_binaryWriter);

            return true;
        }

        public void InsertRow(Row rowToInsert)
        {
            UInt64 locationToInsertAt;

            try
            {
                locationToInsertAt = _freeFilePositions.First();
            }
            catch (Exception)
            {
                // Make more space
                // Console.WriteLine("Making more space!");

                long oldCapacity = (long)_currentDatabaseCapacity;
                long newFileCapacity = (long)((_currentDatabaseCapacity + INCREASE_DATABASE_BY) * DATABASE_EXPANDER);

                _binaryWriter.BaseStream.SetLength((long)Row.ByteSize() * newFileCapacity);

                long changeInCapacity = newFileCapacity - oldCapacity;
                for (long i = 0; i < changeInCapacity; ++i)
                {
                    _freeFilePositions.Add((UInt64)(oldCapacity + i));
                }
                _currentDatabaseCapacity = (UInt64)newFileCapacity;

                locationToInsertAt = _freeFilePositions.First();
            }

            // This row location is now no longer free
            _freeFilePositions.Remove(locationToInsertAt);

            // Add the new row to the cache
            _rowCache.Add(rowToInsert.RowId, new WrRow(rowToInsert));

            // Add the new row id and this location in the file it is at
            _rowLocationInFile.Add(rowToInsert.RowId, locationToInsertAt);

            // Seek to that location and then write it to it
            _binaryWriter.BaseStream.Seek((long)(Row.ByteSize() * locationToInsertAt), SeekOrigin.Begin);
            rowToInsert.Write(_binaryWriter);

            ++_noActiveRows;
        }

        //---------------------------------------------------------------------
        //          : THINGS THAT USE CRUD OPERATIONS
        //---------------------------------------------------------------------
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
                Row rowToRemove = _rowCache[rowIndex].GetRow();
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

        public Row CreateRandomRow()
        {
            UInt64 thisRowId = GetNewId();

            Row newRow = new Row(thisRowId, "Random Row with this row Id -> " + thisRowId);

            InsertRow(newRow);

            return newRow;
        }


        public void UpdateRandomRow(UInt64 amountToRandom)
        {
            for (UInt64 i = 0; i < amountToRandom; ++i)
            {
                Row toBeUpdated = _rowCache.ElementAt(rand.Next(0, _rowCache.Count)).Value.GetRow();
                Row newRow = new Row(toBeUpdated.RowId, DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm tt"));
                UpdateRow(newRow);
            }
        }


        //---------------------------------------------------------------------
        //          : Helpers
        //---------------------------------------------------------------------
        public UInt64 GetNewId()
        {
            return _idOfNextRow++;
        }

        //---------------------------------------------------------------------
        //          : Building Indexes
        //---------------------------------------------------------------------
        public void Rebuild()
        {
            // We CAN rebuild indexes
            // _idOfNextRow = Configuration.IdOfNextRow;

            // Tuple<Dictionary<ulong, ulong>, SortedSet<ulong>> rowLocationsInFile = BuildRowLocationsFromFile();
            // _rowLocationInFile = rowLocationsInFile.Item1;
            // _freeFilePositions = rowLocationsInFile.Item2;

            // _rowCache = BuildRowCacheFromFile(_rowCache);
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

        public Dictionary<UInt64, WrRow> BuildRowCacheFromFile(Dictionary<UInt64, WrRow> rowCache)
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
                        rowCache[tempRow.RowId] = new WrRow(tempRow);
                    }
                    else
                    {
                        rowCache.Add(tempRow.RowId, new WrRow(tempRow)); // TODO: Do not add in the row, maybe save the row cache on exit
                    }
                }
            }

            return rowCache;
        }


        // MAY CAUSE BUG: What if Key values are not returned in the right order...? or if the order changes...
        public Dictionary<UInt64, WrRow> BuildRowCacheFromRowOffset(Dictionary<UInt64, WrRow> rowCache)
        {
            _binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            long currentOffset = 0;

            foreach (KeyValuePair<UInt64, UInt64> entry in _rowLocationInFile)
            {
                long seekDelta = (long)entry.Value - currentOffset;

                _binaryReader.BaseStream.Seek(seekDelta, SeekOrigin.Current);

                Row tempRow = new Row();
                tempRow.Read(_binaryReader);

                if (tempRow.IsActive())
                {
                    if (rowCache.ContainsKey(tempRow.RowId))
                    {
                        rowCache[tempRow.RowId] = new WrRow(tempRow);
                    }
                    else
                    {
                        rowCache.Add(tempRow.RowId, new WrRow(tempRow)); // TODO: Do not add in the row, maybe save the row cache on exit
                    }
                }

                currentOffset = (long)entry.Value;
            }

            return rowCache;
        }


        public void FillCache(bool fromFile = false)
        {
            if (fromFile)
                _rowCache = BuildRowCacheFromFile(_rowCache);
            else
                _rowCache = BuildRowCacheFromRowOffset(_rowCache);
        }

        public void EmptyCache()
        {
            foreach (var key in _rowCache.Keys.ToList())
                _rowCache[key].Empty();
        }

        //---------------------------------------------------------------------
        //          : Save / Load Indexes
        //---------------------------------------------------------------------
        public void Save()
        {
            StreamWriter sw = new StreamWriter(FileName + ".cfg");
            ConfigurationWriter(sw);
            sw.Close();
        }

        public void ConfigurationWriter(StreamWriter sw)
        {
            sw.WriteLine(_idOfNextRow);
            sw.WriteLine(_noActiveRows);
            sw.WriteLine(_currentDatabaseCapacity);

            StringBuilder sb = new StringBuilder();

            // RowId:Location In File, 
            foreach (KeyValuePair<UInt64, UInt64> entry in _rowLocationInFile)
				sb.Append(String.Format("{0}:{1},", entry.Key, entry.Value));

            if (sb.Length > 0) // Removes the trailing comma
                sb.Length--;

            sw.WriteLine(sb.ToString());
            sb.Clear();


            // Print Free spaces in file
            foreach (UInt64 entry in _freeFilePositions)
				sb.Append(String.Format("{0},", entry));
            if (sb.Length > 0) // Removes the trailing comma
                sb.Length--;

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

                _rowCache.Add(
                    UInt64.Parse(idFileLocationPair[0]),  
                    new WrRow(UInt64.Parse(idFileLocationPair[0]))
                );

                _rowLocationInFile.Add(UInt64.Parse(idFileLocationPair[0]), UInt64.Parse(idFileLocationPair[1]));
            }

            string freeLocationString = sr.ReadLine();
            string[] freeLocation = freeLocationString.Split(',');

            foreach (string location in freeLocation)
            {
                _freeFilePositions.Add(UInt64.Parse(location));
            }
        }

        //---------------------------------------------------------------------
        //          : Console Commands
        //---------------------------------------------------------------------
        public void PrintKeyValue(bool fetch = true)
		{
			Console.WriteLine("\n-------------------------");
			Console.WriteLine("----- Row Cache ----------");
			Console.WriteLine("--------------------------");
            foreach (KeyValuePair<UInt64, WrRow> entry in _rowCache)
            {
                Row cachedRow;

                if (!fetch)
                {
                    cachedRow = entry.Value.CacheValue();
                }
                else
                {
                    cachedRow = entry.Value.GetRow(); // The RowCache must be filled with WrRows
                }

                if (cachedRow != null)
                {
                    Console.WriteLine("(Row ID: {0}, Row {1})", entry.Key, cachedRow);
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
                    Console.WriteLine(tempRow.FileString());
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
				Console.Write("{0} ", freeOffset);
            Console.WriteLine();

            Console.WriteLine("-----------------------");
        }


        public string Statistics()
        {
            StringBuilder sb = new StringBuilder();

			sb.AppendLine (String.Format("-- Statistics --"));
			sb.AppendLine (String.Format("Name of file:\t{0}", _fileName));
			sb.AppendLine (String.Format ("Active rows:\t{0}", _noActiveRows));
			sb.AppendLine (String.Format("Id of Next Row:\t{0}", _idOfNextRow));
			sb.AppendLine (String.Format ("Database cap:\t{0} rows", _currentDatabaseCapacity));
			sb.AppendLine (String.Format ("Accesses: {0}, Fetches: {1}, Hits: {2}.", WrRow.acceses, WrRow.fetchCount, WrRow.hits));

            if (WrRow.acceses != 0)
				sb.AppendLine(String.Format("{0}% Hit Rate", (WrRow.hits/WrRow.acceses)*100));
            else
                sb.AppendLine("No row accesses yet!");

            return sb.ToString();
        }


        //---------------------------------------------------------------------
        //          : Getters and Setters
        //---------------------------------------------------------------------
        public string FileName
        {
            get { return _fileName; }
        }

        public Dictionary<UInt64, UInt64> RowLocationInFile
        {
            get { return _rowLocationInFile; }
        }

        public BinaryReader BW
        {
            get
            {
                return _binaryReader;
            }
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

    public static class RPEL
    {
        public static void FMREPL()
        {
            Dictionary<UInt64, WrRow> rowCache = Table._rowCache;

            while (true)
            {
                Console.WriteLine("save, random <no>, statistics, save, kv <y/n>, file, rl, delete <no.>, cache <f/e>, urandom <no.>");
                Console.WriteLine("get <no.>");
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
                    case "get":
                        {
                            try
                            {
                                Row aRow = FileManager.Instance.GetRowByIndex(UInt64.Parse(parsedInput[1]));
								Console.WriteLine("Got: {0}", aRow);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Not a valid numnber");
                            }
                            break;
                        }

                    case "del":
                        {
                            try
                            {
                                Row deleted = FileManager.Instance.DeleteRowByIndex(UInt64.Parse(parsedInput[1]));
                                if (deleted == null)
                                {
                                    Console.WriteLine("Deleted the row");
                                }
                                else
                                {
                                    Console.WriteLine("Unable to delete the row");
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Not a valid numnber");
                            }
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
                                FileManager.Instance.CreateRandomRow();
                            break;
                        }

                    case "statistics":
                        {
                            Console.WriteLine(FileManager.Instance.Statistics());
                            break;
                        }

                    case "save":
                        {
                            FileManager.Instance.Save();

                            Environment.Exit(0);
                            break;
                        }

                    case "kv":
                        {
                            // F does not fetch from file
                            if (parsedInput.Length > 1 && parsedInput[1] == "n")
                                FileManager.Instance.PrintKeyValue(false);
                            else
                                FileManager.Instance.PrintKeyValue(true);
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
                            if (parsedInput[1] == "f")
                                // From the file
                                FileManager.Instance.FillCache(true);
                            else if (parsedInput[1] == "s")
                                // From the File Offsets
                                FileManager.Instance.FillCache(false);
                            else
                                FileManager.Instance.EmptyCache();
                            break;
                        }

                    case "urandom":
                        {
                            FileManager.Instance.UpdateRandomRow(UInt64.Parse(parsedInput[1]));
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
                                FileManager.Instance.DeleteRandomRows();
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
        }
    }

}