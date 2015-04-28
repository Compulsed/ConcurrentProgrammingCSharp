using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
// using System.Text.Encoding.Unicode;

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

		// 2 bytes * length
		Char[] _charString;
		const UInt64 _charLength = 10; // Includes the null terminator 


		public Row ()
		{
			_rowId = 0;
			_charString = new Char[_charLength];
			_charString [0] = Char.MinValue;
		}

		public Row (UInt64 RowId, string toCharString)
		{
			_rowId = RowId;

			// Leaves enough room for null terminator
			_charString = (Truncate (toCharString, (int)(_charLength - 1)) + Char.MinValue).ToCharArray (); 
		}
			

		public override string ToString() 
		{
			return new String (_charString);
		}

		public UInt64 rowId
		{
			get { return _rowId; }
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write (_rowId);
			bw.Write (_charString);
		}

		public void Read(BinaryReader rw)
		{
			_rowId = rw.ReadUInt64 ();
			_charString = rw.ReadChars((int)_charLength);
		}

		public UInt64 ByteSize ()
		{
			// RowId + Max length of char * number of chars
			return sizeof(UInt64) + sizeof(char) * _charLength;
		}

		public static string Truncate(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
		}
			
	}
		
}

