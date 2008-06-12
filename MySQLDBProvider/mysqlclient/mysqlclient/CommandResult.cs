// Copyright (C) 2004-2005 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using MySql.Data.Types;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CommandResult.
	/// </summary>
	internal class CommandResult
	{
		private Driver				driver;

		private long				affectedRows;
		private ulong				fieldCount;
		private long				lastInsertId;

		private bool				readSchema;
		private bool				readRows;
		private bool				isBinary;
		private MySqlField[]		fields;
		private MySqlValue[]		values;
		private bool				dataRowOpen;
		private bool				usingSequentialAccess;
		private int					seqColumn;

		public CommandResult( Driver d, bool isBinary )
		{
			driver = d;
			this.isBinary = isBinary;
			affectedRows = -1;
			ReadNextResult(true);
		}

/*		public CommandResult( Driver d )
		{
			driver = d;
		}
*/
		#region Properties

		public MySqlValue this[int index] 
		{
			get { return values[index]; }
			set { values[index] = value; }
		}

		public MySqlField[] Fields 
		{
			get { return fields; }
		}

		public bool IsResultSet 
		{
			get { return fieldCount > 0; }
		}

/*		public long LastInsertId 
		{
			get { return lastInsertId; }
			set { lastInsertId = value; }
		}
*/
		//TODO: remove comment
/*		public ulong FieldCount 
		{
//			get { return fieldCount; }
//			set { fieldCount = value; }
		}
*/
		public long AffectedRows
		{
			get { return affectedRows; }
//			set { affectedRows = value; }
		}

		#endregion

		public MySqlValue ReadColumnValue(int index)
		{
			if (! usingSequentialAccess || seqColumn == index) 
				return this[index];

			if (index < seqColumn)
				throw new MySqlException("Invalid attempt to read a prior column using SequentialAccess");

			while ( (seqColumn+1) < index )
			{
				driver.SkipField(values[seqColumn+1]);
				seqColumn++;
			}

			values[index] = driver.ReadFieldValue( index, fields[index], values[index] );
			seqColumn = index;
			return values[index];
		}

		public bool ReadNextResult(bool isFirst) 
		{
			long rows = 0;

			while ( (driver.ServerStatus & (ServerStatusFlags.MoreResults | ServerStatusFlags.AnotherQuery )) != 0 ||
				    isFirst)
			{
				fieldCount = (ulong)driver.ReadResult( ref rows, ref lastInsertId );
				if (rows != -1)
				{
					if (affectedRows == -1) affectedRows = 0;
					affectedRows += rows;
				}
				if (isFirst) isFirst = false;

				if (IsResultSet) 
				{
					readSchema = false;
					readRows = false;
					return true;
				}
			} 

			// if our batch resulted in warnings, then report them now
			if (driver.HasWarnings)
				driver.ReportWarnings();

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Load()
		{
			try 
			{
				driver.ReadFieldMetadata( (int)fieldCount, ref fields );
				readSchema = true;

				values = new MySqlValue[ fields.Length ];
				for (int i=0; i < fields.Length; i++) 
					values[i] = fields[i].GetValueObject();

				if (! driver.OpenDataRow(fields.Length, isBinary)) 
				{
					readRows = true;
					return false;
				}
				dataRowOpen = true;

				return true;
			}
			catch (Exception) 
			{
				readSchema = true;
				readRows = true;
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool ReadDataRow(bool loadFields)
		{
			if ( readRows ) return false;

			seqColumn = -1;
			if (! dataRowOpen) 
			{
				if (! driver.OpenDataRow(fields.Length, isBinary) ) 
				{
					readRows = true;
					return false;
				}
			}
			dataRowOpen = false;
			usingSequentialAccess = ! loadFields;

			if (! loadFields) 
				return true;

			for (int x=0; x < fields.Length; x++)
				values[x] = driver.ReadFieldValue( x, fields[x], values[x] );

			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		public void Consume()
		{
			// if we are not a resultset, then we are done
			if (! IsResultSet) return;

			if (! readSchema)
			{
				driver.ReadFieldMetadata( (int)fieldCount, ref fields );
				readSchema = true;
			}

			if (! readRows) 
			{
				while (driver.OpenDataRow( 0, false )) {}  
				readRows = true;
			}
		}
	}
}
