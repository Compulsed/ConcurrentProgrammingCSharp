using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
// using System.Text.Encoding.Unicode;
using System.Text;

namespace DatabaseManagementSystem
{

    public enum RowStatus { Free = 0, Active, Deleted }

	/// <summary>
	/// Row.
	/// </summary>
	public class Row
	{
	    private RowStatus _rowStatus = RowStatus.Active;

		private UInt64 _rowId;
		private Byte[] _byteString;

        const UInt64 _byteLength = 1024; // (10 / 2) - 1 = Max string stored


		public Row (UInt64 rowId = 0, string toCharString = "")
		{
			_rowId = rowId;
			_byteString = Encoding.Unicode.GetBytes (toCharString.PadRight ((int)_byteLength, '\0'));
		}

        // -- Helper functions
        public bool IsDelete()
        {
            return RowStatus.Deleted == _rowStatus;
        }

	    public bool IsActive()
	    {
	        return RowStatus.Active == _rowStatus;
	    }

	    public bool IsFree()
	    {
	        return RowStatus.Free == _rowStatus;
	    }

        public void SetStatus(RowStatus newStatus)
	    {
            _rowStatus = newStatus;
	    }

        // -- Getters & Setters
		public UInt64 RowId
		{
			get { return _rowId; }
            set { _rowId = value; }
		}

	    public string Value
	    {
	        get { return Encoding.Unicode.GetString(_byteString).TrimEnd('\0');  }
	    }

        // -- Writer and Readers
		public void Write(BinaryWriter bw)
		{
            bw.Write ((UInt64)_rowStatus);
			bw.Write (_rowId);
			bw.Write (_byteString, 0, (int)_byteLength);
		}

		public void Read(BinaryReader rw)
		{
            _rowStatus = (RowStatus)rw.ReadUInt64();
			_rowId = rw.ReadUInt64 ();
			_byteString = rw.ReadBytes((int)_byteLength);
		}

        // -- Static Functions
        public static UInt64 ByteSize()
        {
            // RowId + Max length of char * number of chars
            return sizeof(UInt64) + sizeof(UInt64) + sizeof(byte) * _byteLength;
        }


	    public void Delete(BinaryWriter bw, Dictionary<UInt64, UInt64> rowLocationInFile)
	    {
            SetStatus(RowStatus.Deleted);
            bw.BaseStream.Seek((long)(ByteSize() * rowLocationInFile[_rowId]), SeekOrigin.Begin);

            Write(bw); // Write that it is deleted to file

            rowLocationInFile.Remove(_rowId);
	    }

	    public override string ToString()
	    {
            return "[Row Id: " + _rowId + ", Row Value: " + Encoding.Unicode.GetString(_byteString).TrimEnd('\0') + "]";
        }

        // -- Overrides
        public string FileString()
        {
            switch (_rowStatus)
            {
                case RowStatus.Active:
                {
                    return "[Row Id: " + _rowId + ", Row Value: " + Encoding.Unicode.GetString(_byteString).TrimEnd('\0') + "]";
                }

                case RowStatus.Deleted:
                {
                    return "Deleted Row";
                }

                case RowStatus.Free:
                {
                    return "Free space";
                }

                default:
                {
                    return "Invalid state";
                }
            }
        }


    }
    
    public class WrRow
    {
        private WeakReference<Row> _row;
        private UInt64 _rowId;

        public static UInt64 fetchCount = 0;
        public static UInt64 hits = 0;
        public static UInt64 acceses = 0;

        public WrRow(Row aRow)
        {
            _rowId = aRow.RowId;
            _row = new WeakReference<Row>(aRow);
        }

        public WrRow(UInt64 rowId)
        {
            _rowId = rowId;
            _row = new WeakReference<Row>(null);
        }

        //TODO: This feels really hacky
        public Row GetRow()
        {
            Row myRow = null;

            ++acceses;
            if (!_row.TryGetTarget(out myRow))
            {
                ++fetchCount;

                // The row much be fetched from file as it is not in the cache
                myRow = new Row();

                UInt64 fileOffset = FileManager.Instance.RowLocationInFile[_rowId];
                FileManager.Instance.BW.BaseStream.Seek((long)(Row.ByteSize() * fileOffset), SeekOrigin.Begin); // Todo: Esp this

                myRow.Read(FileManager.Instance.BW);
            }
            else
            {
                ++hits;
            }

            return myRow;
        }

        public Row CacheValue()
        {
            Row returnRow;
            _row.TryGetTarget(out returnRow);
            return returnRow;
        }

        public void Empty()
        {
            _row = null;
        }
    }
}

