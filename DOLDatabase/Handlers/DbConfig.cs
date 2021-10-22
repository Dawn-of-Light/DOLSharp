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
using System.Linq;

namespace DOL.Database
{
    public class DbConfig
    {
		private Dictionary<string,(string, string)> nonDefaultOptions = new Dictionary<string,(string, string)>();
		private Dictionary<string, (string, string)> defaultOptions = new Dictionary<string, (string, string)>();
		private Dictionary<string, (string, string)> options => nonDefaultOptions.Union(defaultOptions).ToDictionary(pair => pair.Key, pair => pair.Value);

		public string ConnectionString => string.Join(";", options.Select(kv => $"{kv.Value.Item1}={kv.Value.Item2}"));

		public DbConfig(string connectionString)
        {
			ApplyConnectionString(connectionString);
        }

		public void ApplyConnectionString(string connectionString)
		{
			var userOptions = connectionString.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
				.Select(o => new KeyValuePair<string, string>(o.Split('=')[0], o.Split('=')[1]));

			foreach(var userOption in userOptions)
            {
				SetOption(userOption.Key, userOption.Value);
            }
		}

		public string GetValueOf(string optionName)
		{
			if (options.TryGetValue(Digest(optionName), out var optionValue))
			{
				return optionValue.Item2;
			}
			else
			{
				return "";
			}
		}

		public void SetOption(string key, string value)
        {
			if(defaultOptions.ContainsKey(Digest(key)))
            {
				defaultOptions[Digest(key)] = (defaultOptions[Digest(key)].Item1, value);
            }
			else if (nonDefaultOptions.ContainsKey(Digest(key)))
			{
				nonDefaultOptions[Digest(key)] = (nonDefaultOptions[Digest(key)].Item1, value);
			}
			else
			{
				nonDefaultOptions.Add(Digest(key), (key, value));
			}
		}

		public void AddDefaultOption(string key, string value)
        {
			if (nonDefaultOptions.ContainsKey(Digest(key)))
			{
				value = nonDefaultOptions[Digest(key)].Item2;
				nonDefaultOptions.Remove(Digest(key));
			}
			defaultOptions.Add(Digest(key), (key, value));
		}

		private string Digest(string input)
        {
			return input.ToLower().Replace(" ","");
        }
    }
}
