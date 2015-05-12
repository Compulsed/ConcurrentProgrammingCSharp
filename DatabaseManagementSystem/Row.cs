using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
// using System.Text.Encoding.Unicode;
using System.Text;

namespace DatabaseManagementSystem
{
	/// <summary>
	/// Row.
	/// 
	/// Must contain only fixed length data, and needs to be serializabl
	/// 
	/// </summary>
	public class Row 
	{
		UInt64 _rowId;

		Byte[] _byteString;
		static UInt64 _byteLength = 1000; // (10 / 2) - 1 = Max string stored


		public Row (UInt64 RowId = 0, string toCharString = "")
		{
			_rowId = RowId;
			_byteString = Encoding.Unicode.GetBytes (toCharString.PadRight ((int)_byteLength, '\0'));
		}
			
		public override string ToString() 
		{
			return  "[Row Id: "  +_rowId + ", Row Value: " + Encoding.Unicode.GetString(_byteString) + "]";
		}

		public UInt64 rowId
		{
			get { return _rowId; }
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write (_rowId);
			bw.Write (_byteString, 0, (int)_byteLength);
		}

		public void Read(BinaryReader rw)
		{
			_rowId = rw.ReadUInt64 ();
			_byteString = rw.ReadBytes((int)_byteLength);
		}

		public static UInt64 ByteSize ()
		{
			// RowId + Max length of char * number of chars
			return sizeof(UInt64) + sizeof(byte) * _byteLength;
		}

			
	}
		
}

