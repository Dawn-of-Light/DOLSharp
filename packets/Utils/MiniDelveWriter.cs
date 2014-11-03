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
using System.Collections.Generic;
using System.Text;

namespace DOL.GS.PacketHandler
{
	/// <summary>
	/// MiniDelveWriter is used to build v1.110+ hovering tool tip
	/// format is : (Subject (Key "Value")(Key2 "Value2")[...](Expires "Timestamp"))
	/// Sent to Client when hovering an icon that need some attached tool tip.
	/// </summary>
	public class MiniDelveWriter
	{
		/// <summary>
		/// Max Length of resulting Delve String
		/// </summary>
		public const ushort MAX_DELVE_STR_LENGTH = 2048;
		
		/// <summary>
		/// Subject's Name
		/// </summary>
		private string m_name;
		
		/// <summary>
		/// Subject's Name
		/// </summary>
		public string Name {
			get { return m_name; }
		}
		
		/// <summary>
		/// Key / Value Collection
		/// </summary>
		private Dictionary<string, string> m_values;
		
		/// <summary>
		/// Key / Value Collection
		/// </summary>
		public Dictionary<string, string> Values {
			get { return m_values; }
		}
		
		/// <summary>
		/// Times of Cache Expires
		/// </summary>
		private ulong m_expires;
		
		/// <summary>
		/// Times of Cache Expires, if 0 not sent to client.
		/// </summary>
		public ulong Expires {
			get { return m_expires; }
		}
		
		/// <summary>
		/// Build a MiniDelveWriter with Implicit Expires.
		/// </summary>
		/// <param name="name"></param>
		public MiniDelveWriter(string name)
			: this(name, 0)
		{
		}
		
		/// <summary>
		/// Build a MiniDelveWriter with Expires Explicitely Set
		/// </summary>
		/// <param name="name"></param>
		/// <param name="expires"></param>
		public MiniDelveWriter(string name, ulong expires)
		{
			m_name = name;
			m_expires = expires;
			m_values = new Dictionary<string, string>();
		}

		/// <summary>
		/// Add a Key / Value pair
		/// </summary>
		/// <param name="name"></param>
		/// <param name="val"></param>
		public void AddKeyValuePair(string name, object val)
		{
			AddKeyValuePair(name, val.ToString());
		}
		
		/// <summary>
		/// Add a Key / Value pair
		/// </summary>
		/// <param name="name"></param>
		/// <param name="val"></param>
		public void AddKeyValuePair(string name, string val)
		{
			m_values[name] = val;
		}

		/// <summary>
		/// Add a Key / Value pair
		/// </summary>
		/// <param name="name"></param>
		/// <param name="val"></param>
		public void AppendKeyValuePair(string name, string val, string sep = ", ")
		{
			if (m_values.ContainsKey(name))
				m_values[name] += sep + val;
			else
				m_values[name] = val;
		}
		
		/// <summary>
		/// Build the Formatted String object and return it as a String.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder res = new StringBuilder();
			
			res.AppendFormat("({0} ", Name);
			
			foreach(KeyValuePair<string, string> kv in Values)
			{
				KeyValuePair<string, string> pair = kv;
				
				if ((res.Length + pair.Key.Length + pair.Value.Length + 7) > MAX_DELVE_STR_LENGTH)
					break;
				
				res.AppendFormat("({0} \"{1}\")", pair.Key, pair.Value);
			}
			
			if (Expires > 0 && (res.Length + 26) < MAX_DELVE_STR_LENGTH)
			{
				res.AppendFormat("(Expires \"{0}\")", Expires);
			}
			
			res.Append(")");
			
			return res.ToString();
		}

	}
}
