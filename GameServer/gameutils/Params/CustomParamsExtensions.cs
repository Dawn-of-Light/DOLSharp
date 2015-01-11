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
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DOL.GS
{
	/// <summary>
	/// Allow to handle Custom Params Valuable Interface Member as typed generic imported from string collections.
	/// </summary>
	public static class CustomParamsExtensions
	{
		/// <summary>
		/// Parse Params String to Extract the Value identified by Key.
		/// Expected Format : {"key":["value"#, "value", ...#]#, "key2":[...], ...#}
		/// From Dictionary<stringKey, List<stringValue>> (List<stringValue>.First() Casted to T)
		/// </summary>
		/// <param name="key">Param Key</param>
		/// <returns>Param Value</returns>
		public static T GetParamValue<T>(this ICustomParamsValuable obj, string key)
		{
			if (obj.CustomParamsDictionary == null || obj.CustomParamsDictionary.Count == 0 || Util.IsEmpty(key))
				return default(T);
			
			// Is key existing ?
			if (obj.CustomParamsDictionary.ContainsKey(key) && obj.CustomParamsDictionary[key].Count > 0)
			{
				try
				{
					return (T)Convert.ChangeType(obj.CustomParamsDictionary[key].First(), typeof(T));
				}
				catch
				{
					return default(T);
				}
			}
			
			return default(T);
		}

		/// <summary>
		/// Parse Params String to Extract the Values identified by Key.
		/// Expected Format : {"key":["value"#, "value", ...#]#, "key2":[...], ...#}
		/// From Dictionary<stringKey, List<stringValue>> (List<stringValue> Casted to List<T>)
		/// </summary>
		/// <param name="key">Param Key</param>
		/// <returns>List of Values object</returns>
		public static IList<T> GetParamValues<T>(this ICustomParamsValuable obj, string key)
		{
			if (obj.CustomParamsDictionary == null || obj.CustomParamsDictionary.Count == 0 || Util.IsEmpty(key))
				return new List<T>();

			var list = new List<T>();
			
			// Is key existing ?
			if (obj.CustomParamsDictionary.ContainsKey(key) && obj.CustomParamsDictionary[key].Count > 0)
			{
				foreach(string val in obj.CustomParamsDictionary[key])
				{
					T content;
					try
					{
						content = (T)Convert.ChangeType(val, typeof(T));
						list.Add(content);
					}
					catch
					{
					}
				}
			}
			
			return list;
		}
		
		/// <summary>
		/// Parse Params String to Extract the Values identified by Key.
		/// Expected Format : {"key":["value"#, "value", ...#]#, "key2":[...], ...#}
		/// From Dictionary<stringKey, List<stringValue>> (List<stringValue> Casted to List<T>)
		/// </summary>
		/// <param name="key">Param Key</param>
		/// <returns>List of Values object</returns>
		public static IDictionary<K, V> GetParamValues<K, V>(this ICustomParamsValuable obj, string key)
		{
			if (obj.CustomParamsDictionary == null || obj.CustomParamsDictionary.Count == 0 || Util.IsEmpty(key))
				return new Dictionary<K, V>();
			
			Regex regex = new Regex(string.Format(@"{0}\[(.+?)\]", key));
			var dict = new Dictionary<K, V>();
			// Browse all Dictionary
			foreach (var pair in obj.CustomParamsDictionary)
			{
				Match match = regex.Match(pair.Key);
				// this could be an entry
				if (match.Success)
				{
					K keyResult;
					V valueResult;
					try
					{
						keyResult = (K)Convert.ChangeType(match.Groups[1].Value, typeof(K));
						valueResult = (V)Convert.ChangeType(pair.Value.FirstOrDefault(), typeof(V));
						dict.Add(keyResult, valueResult);
					}
					catch
					{
					}
					
				}
			}
			
			return dict;
		}

	}
}
