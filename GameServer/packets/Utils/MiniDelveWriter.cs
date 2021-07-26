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
using System.Collections.Generic;
using System.Text;

namespace DOL.GS.PacketHandler
{
	public class MiniDelveWriter
	{
		private static ushort MAX_DELVE_STR_LENGTH = 2048;
		
		public string Name { get; }
		public Dictionary<string, string> Values { get; }
		public ulong Expires { get; }
		
		public MiniDelveWriter(string name)
		{
			Name = name;
			Expires = 0;
			Values = new Dictionary<string, string>();
		}

		public void AddKeyValuePair(string name, object val)
		{
			if (name != "cast_timer" && val.ToString() == "0") return;
			AddKeyValuePair(name, val.ToString());
		}
		
		public void AddKeyValuePair(string name, string val)
		{
			Values[name] = val;
		}

		public void AppendKeyValuePair(string name, string val, string sep = ", ")
		{
			if (Values.ContainsKey(name))
				Values[name] += sep + val;
			else
				Values[name] = val;
		}

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
