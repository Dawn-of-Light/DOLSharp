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
using System.Data;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using MySql.Data.Types;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Represents a parameter to a <see cref="MySqlCommand"/>, and optionally, its mapping to <see cref="DataSet"/> columns. This class cannot be inherited.
	/// </summary>
	[TypeConverter(typeof(MySqlParameter.MySqlParameterConverter))]
	public sealed class MySqlParameter : MarshalByRefObject, IDataParameter, IDbDataParameter, ICloneable
	{
		private object				paramValue;
		private MySqlValue			valueObject;
		private ParameterDirection	direction = ParameterDirection.Input;
		private bool				isNullable  = false;
		private string				paramName;
		private string				sourceColumn;
		private DataRowVersion		sourceVersion = DataRowVersion.Current;
		private int					size;
		private byte				precision;
		private bool				isUnsigned;
		private byte				scale;
		private MySqlDbType			mySqlDbType;
		private DbType				dbType;
		private bool				inferType;

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the MySqlParameter class.
		/// </summary>
		public MySqlParameter()
		{
			inferType = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name and a value of the new MySqlParameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="value">An <see cref="Object"/> that is the value of the <see cref="MySqlParameter"/>. </param>
		public MySqlParameter(string parameterName, object value) : this ()
		{
			ParameterName = parameterName;
			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		public MySqlParameter( string parameterName, MySqlDbType dbType) : this (parameterName, null)
		{
			SetMySqlDbType( dbType );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the <see cref="MySqlDbType"/>, and the size.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		public MySqlParameter( string parameterName, MySqlDbType dbType, int size ) : this ( parameterName, dbType )
		{
			this.size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the <see cref="MySqlDbType"/>, the size, and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		/// <param name="sourceColumn">The name of the source column. </param>
		public MySqlParameter( string parameterName, MySqlDbType dbType, int size, string sourceColumn ) : 
			this ( parameterName, dbType )
		{
			this.size = size;
			this.direction = ParameterDirection.Input;
			this.sourceColumn = sourceColumn;
			this.sourceVersion = DataRowVersion.Current;
		}

		internal MySqlParameter(string name, MySqlDbType type, ParameterDirection dir, string col, 
			DataRowVersion ver, object val) : this (name, type)
		{
			if (direction != ParameterDirection.Input)
				throw new ArgumentException("Only input parameters are supported by MySql");
			direction = dir;
			sourceColumn = col;
			sourceVersion = ver;
			Value = val;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the type of the parameter, the size of the parameter, a <see cref="ParameterDirection"/>, the precision of the parameter, the scale of the parameter, the source column, a <see cref="DataRowVersion"/> to use, and the value of the parameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		/// <param name="direction">One of the <see cref="ParameterDirection"/> values. </param>
		/// <param name="isNullable">true if the value of the field can be null, otherwise false. </param>
		/// <param name="precision">The total number of digits to the left and right of the decimal point to which <see cref="MySqlParameter.Value"/> is resolved.</param>
		/// <param name="scale">The total number of decimal places to which <see cref="MySqlParameter.Value"/> is resolved. </param>
		/// <param name="sourceColumn">The name of the source column. </param>
		/// <param name="sourceVersion">One of the <see cref="DataRowVersion"/> values. </param>
		/// <param name="value">An <see cref="Object"/> that is the value of the <see cref="MySqlParameter"/>. </param>
		/// <exception cref="ArgumentException"/>
		public MySqlParameter( string parameterName, MySqlDbType dbType, int size, ParameterDirection direction,
			bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion,
			object value) : this (parameterName, dbType, size, sourceColumn)
		{
			if (direction != ParameterDirection.Input)
				throw new ArgumentException("Only input parameters are supported by MySql");

			this.direction = direction;
			this.sourceVersion = sourceVersion;
			Value = value;
		}
		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the <see cref="DbType"/> of the parameter.
		/// </summary>
		public DbType DbType 
		{
			get { return dbType; }
			set 
			{ 
				SetDbType( value ); 
				inferType = false;
			}
		}

		/// <summary></summary>
		public bool IsUnsigned 
		{
			get { return isUnsigned; }
			set 
			{ 
				if (isUnsigned != value) 
					valueObject = null; 
				isUnsigned = value; 
				SetMySqlDbType( mySqlDbType ); 
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
		/// As of MySql version 4.1 and earlier, input-only is the only valid choice.
		/// </summary>
		[Category("Data")]
		public ParameterDirection Direction 
		{
			get { return direction; }
			set { direction = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter accepts null values.
		/// </summary>
		[Browsable(false)]
		public Boolean IsNullable 
		{
			get { return isNullable; }
			set { isNullable = value; }
		}

		/// <summary>
		/// Gets or sets the MySqlDbType of the parameter.
		/// </summary>
		[Category("Data")]
		public MySqlDbType MySqlDbType 
		{
			get { return mySqlDbType; }
			set 
			{ 
				SetMySqlDbType( value ); 
				inferType = false;
			}
		}

		/// <summary>
		/// Gets or sets the name of the MySqlParameter.
		/// </summary>
		[Category("Misc")]
		public String ParameterName 
		{
			get { return paramName; }
			set { paramName = value; }
		}

		/// <summary>
		/// Gets or sets the maximum number of digits used to represent the <see cref="Value"/> property.
		/// </summary>
		[Category("Data")]
		public byte Precision 
		{
			get { return precision; }
			set { precision = value; }
		}

		/// <summary>
		/// Gets or sets the number of decimal places to which <see cref="Value"/> is resolved.
		/// </summary>
		[Category("Data")]
		public byte Scale 
		{
			get { return scale; }
			set { scale = value; }
		}

		/// <summary>
		/// Gets or sets the maximum size, in bytes, of the data within the column.
		/// </summary>
		[Category("Data")]
		public int Size 
		{
			get { return size; }
			set { size = value; }
		}

		/// <summary>
		/// Gets or sets the name of the source column that is mapped to the <see cref="DataSet"/> and used for loading or returning the <see cref="Value"/>.
		/// </summary>
		[Category("Data")]
		public String SourceColumn 
		{
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="DataRowVersion"/> to use when loading <see cref="Value"/>.
		/// </summary>
		[Category("Data")]
		public DataRowVersion SourceVersion 
		{
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		/// <summary>
		/// Gets or sets the value of the parameter.
		/// </summary>
		[TypeConverter(typeof(StringConverter))]
		[Category("Data")]
		public object Value 
		{
			get	{ return paramValue; }
			set	
			{ 
				paramValue = value; 
				if (value is Byte[])
					size = (value as Byte[]).Length;
				else if (value is String)
					size = (value as string).Length;
				if (inferType)
					SetTypeFromValue();
			}
		}

		#endregion

		/// <summary>
		/// Overridden. Gets a string containing the <see cref="ParameterName"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString() 
		{
			return paramName;
		}

		internal MySqlValue GetValueObject() 
		{
			if (valueObject == null)
			{
				valueObject = MySqlValue.GetMySqlValue( mySqlDbType, IsUnsigned, true );

				MySqlDecimal dec = (valueObject as MySqlDecimal);
				if (dec != null) 
				{
					dec.Precision = precision;
					dec.Scale = scale;
				}
			}
			return valueObject;
		}

		internal void Serialize( PacketWriter writer, bool binary ) 
		{
			GetValueObject();

			if (!binary && (paramValue == null || paramValue == DBNull.Value))
				writer.WriteStringNoNull("NULL");
			else
				valueObject.Serialize( writer, binary, paramValue, size );
		}

		private void SetMySqlDbType( MySqlDbType mySqlDbType ) 
		{
			if (this.mySqlDbType != mySqlDbType)
				valueObject = null;
			this.mySqlDbType = mySqlDbType;
			switch (mySqlDbType) 
			{
				case MySqlDbType.Decimal: dbType = DbType.Decimal; break;
				case MySqlDbType.Byte: dbType = isUnsigned ? DbType.Byte : DbType.SByte; break;
				case MySqlDbType.Int16: dbType = isUnsigned ? DbType.UInt16 : DbType.Int16; break;
				case MySqlDbType.Int24: dbType = isUnsigned ? DbType.UInt32 : DbType.Int32; break;
				case MySqlDbType.Int32: dbType = isUnsigned ? DbType.UInt32 : DbType.Int32; break;
				case MySqlDbType.Int64: dbType = isUnsigned ? DbType.UInt64 : DbType.Int64; break;
				case MySqlDbType.Bit : dbType = DbType.UInt64; break;
				case MySqlDbType.Float: dbType = DbType.Single; break;
				case MySqlDbType.Double: dbType = DbType.Double; break;

				case MySqlDbType.Timestamp:
				case MySqlDbType.Datetime: dbType = DbType.DateTime; break;

				case MySqlDbType.Date:
				case MySqlDbType.Newdate:
				case MySqlDbType.Year: dbType = DbType.Date; break;
				
				case MySqlDbType.Time: dbType = DbType.Time; break;
				
				case MySqlDbType.Enum:
				case MySqlDbType.Set:
				case MySqlDbType.VarChar: dbType = DbType.String;  break;

				case MySqlDbType.TinyBlob:
				case MySqlDbType.MediumBlob:
				case MySqlDbType.LongBlob:
				case MySqlDbType.Blob: dbType = DbType.Object; break;

				case MySqlDbType.String: dbType = DbType.StringFixedLength; break;
			}
		}


		private void SetDbType( DbType dbType ) 
		{
			this.dbType = dbType;
			switch (dbType) 
			{
				case DbType.Guid:
				case DbType.AnsiString:
				case DbType.String: mySqlDbType = MySqlDbType.VarChar; break;

				case DbType.AnsiStringFixedLength:
				case DbType.StringFixedLength: mySqlDbType = MySqlDbType.String; break;

				case DbType.Boolean:
				case DbType.Byte: 
				case DbType.SByte:
					mySqlDbType = MySqlDbType.Byte; 
					isUnsigned = dbType == DbType.Byte;
					break;

				case DbType.Date: mySqlDbType = MySqlDbType.Date; break;
				case DbType.DateTime: mySqlDbType = MySqlDbType.Datetime; break;

				case DbType.Time: mySqlDbType = MySqlDbType.Time; break;
				case DbType.Single: mySqlDbType = MySqlDbType.Float; break;
				case DbType.Double: mySqlDbType = MySqlDbType.Double; break;

				case DbType.Int16: 
				case DbType.UInt16:
					mySqlDbType = MySqlDbType.Int16; 
					isUnsigned = dbType == DbType.UInt16;
					break;

				case DbType.Int32: 
				case DbType.UInt32:
					mySqlDbType = MySqlDbType.Int32; 
					isUnsigned = dbType == DbType.UInt32;
					break;

				case DbType.Int64: 
				case DbType.UInt64:
					mySqlDbType = MySqlDbType.Int64; 
					isUnsigned = dbType == DbType.UInt64;
					break;

				case DbType.Decimal:
				case DbType.Currency: mySqlDbType = MySqlDbType.Decimal; break;

				case DbType.Object:
				case DbType.VarNumeric: 
				case DbType.Binary: 
				default: 
					mySqlDbType = MySqlDbType.Blob; break;
			}
		}


		private void SetTypeFromValue() 
		{
			if (paramValue == null) return;

			if (paramValue is Guid) SetDbType( DbType.String );
			else if (paramValue is TimeSpan) SetDbType( DbType.Time );
			else if (paramValue is bool) SetDbType( DbType.Byte );
			else 
			{

				TypeCode tc = Type.GetTypeCode( paramValue.GetType() );
				switch (tc) 
				{
					case TypeCode.SByte: SetDbType( DbType.SByte ); break;
					case TypeCode.Byte: SetDbType( DbType.Byte ); break;
					case TypeCode.Int16: SetDbType( DbType.Int16 ); break;
					case TypeCode.UInt16: SetDbType( DbType.UInt16 ); break;
					case TypeCode.Int32: SetDbType( DbType.Int32 ); break;
					case TypeCode.UInt32: SetDbType( DbType.UInt32 ); break;
					case TypeCode.Int64: SetDbType( DbType.Int64 ); break;
					case TypeCode.UInt64: SetDbType( DbType.UInt64 ); break;
					case TypeCode.DateTime: SetDbType( DbType.DateTime ); break;
					case TypeCode.String: SetDbType( DbType.String ); break;
					case TypeCode.Single: SetDbType( DbType.Single ); break;
					case TypeCode.Double: SetDbType( DbType.Double ); break;
					case TypeCode.Decimal: SetDbType( DbType.Decimal); break;
					case TypeCode.Object: 
					default: SetDbType( DbType.Object ); break;
				}
			}
		}


		#region ICloneable
		object System.ICloneable.Clone() 
		{
			MySqlParameter clone = new MySqlParameter( paramName, mySqlDbType, direction,
				sourceColumn, sourceVersion, paramValue );
			return clone;
		}
		#endregion

		internal class MySqlParameterConverter : TypeConverter
		{
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					return true;
				}

				// Always call the base to see if it can perform the conversion.
				return base.CanConvertTo(context, destinationType);
			}

			public override object ConvertTo(ITypeDescriptorContext context, 
				System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					ConstructorInfo ci = typeof(MySqlParameter).GetConstructor(
						new Type[]{typeof(string), typeof(MySqlDbType), typeof(int), typeof(ParameterDirection),
									  typeof(bool), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion),
									  typeof(object)});
					MySqlParameter p = (MySqlParameter) value;
					return new InstanceDescriptor(ci,new object[]{ 
																	 p.ParameterName, p.DbType, p.Size, p.Direction, p.IsNullable, p.Precision,
																	 p.Scale, p.SourceColumn, p.SourceVersion, p.Value});
				}

				// Always call base, even if you can't convert.
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}
