using System;
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

        const UInt64 _byteLength = 16; // (10 / 2) - 1 = Max string stored


		public Row (UInt64 RowId = 0, string toCharString = "")
		{
			_rowId = RowId;
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
		public static UInt64 ByteSize ()
		{
			// RowId + Max length of char * number of chars
			return sizeof(UInt64) + sizeof(UInt64) + sizeof(byte) * _byteLength;
		}


        // -- Overrides
        public override string ToString()
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
		
}

