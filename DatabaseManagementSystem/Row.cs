using System;

namespace DatabaseManagementSystem
{


	/// <summary>
	/// Row.
	/// 
	/// Must contain only fixed length data, and needs to be serializabl
	/// 
	/// </summary>
	public abstract class Row
	{
		UInt64 _RowId;

		public Row (UInt64 RowId)
		{}

		abstract public UInt64 byteSize ();
	}

	public class StringRow : Row
	{
		UInt64 _maxStringSize;
		string _rowString;

		public StringRow(UInt64 rowId, string rowString, UInt64 fixedStringSize) : base(rowId)
		{
			_rowString = rowString;
			_maxStringSize = fixedStringSize;
		}

		override public UInt64 byteSize ()
		{
			return sizeof(UInt64); // + What ever the string is
		}
	}
}

