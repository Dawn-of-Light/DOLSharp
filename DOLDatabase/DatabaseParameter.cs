/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;

namespace DOL.Database
{
	/// <summary>
	/// Parameter for Prepared Queries
	/// </summary>
	public sealed class QueryParameter : Tuple<string, object, Type>
	{
		/// <summary>
		/// Parameter Name
		/// </summary>
		public string Name { get { return Item1; } }
		
		/// <summary>
		/// Parameter Value
		/// </summary>
		public object Value { get { return Item2; } }
		
		/// <summary>
		/// Parameter Value
		/// </summary>
		public Type ValueType { get { return Item3; } }
		
		/// <summary>
		/// Create an instance of <see cref="QueryParameter"/>
		/// </summary>
		/// <param name="Name">Parameter Name</param>
		/// <param name="Value">Parameter Value</param>
		public QueryParameter(string Name, object Value)
			: base(Name, Value, null)
		{
		}
		
		/// <summary>
		/// Create a Typed instance of <see cref="QueryParameter"/>
		/// </summary>
		/// <param name="Name">Parameter Name</param>
		/// <param name="Value">Parameter Value</param>
		/// <param name="Type">Parameter Type</param>
		public QueryParameter(string Name, object Value, Type Type)
			: base(Name, Value, Type)
		{
		}
		
		/// <summary>
		/// Create a default instance of <see cref="QueryParameter"/>
		/// </summary>
		public QueryParameter()
			: base(null, null, null)
		{
		}
	}
}
