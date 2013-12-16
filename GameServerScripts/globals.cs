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
using System.Collections;
using System.IO;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// The hand flag for an item
	/// </summary>
	public enum eHandFlag : int
	{
		/// <summary>
		/// Standard or Right hand
		/// </summary>
		Right = 0,
		/// <summary>
		/// Two handed
		/// </summary>
		Two = 1,
		/// <summary>
		/// Left handed
		/// </summary>
		Left = 2,
	}

	/// <summary>
	///This class is used to read commaseperated files!
	///Can be used to parse userdefined tables by index (first value in row).
	///(eg. from Excel or *hint* the tables from the gamedata.mpk file)
	///Values are cached, so it only needs to read the file once! 
	/// </summary>
	public class CSVFileTableReader
	{
		private Hashtable table = null;

		/// <summary>
		/// Creates an instance of the CSVFileTableReader from a csvfile
		/// </summary>
		/// <param name="csvFile"></param>
		public CSVFileTableReader(string csvFile)
		{
			StreamReader reader = null;
			try
			{
				table = new Hashtable();
				reader = File.OpenText(csvFile);
				while (reader.Peek() != -1)
				{
					//read a line from the stream
					string line = reader.ReadLine();
					int firstsep = line.IndexOf(",");
					if (firstsep != -1)
					{
						string key = line.Substring(0, firstsep);
						line.Remove(0, firstsep + 1);
						string[] values = line.Split(',');
						table.Add(key, values);
					}
				}
				reader.Close();
			}
			catch
			{
				table = null;
			}
		}

		/// <summary>
		/// Is the table ready
		/// </summary>
		public bool IsReady
		{
			get { return table != null; }
		}

		/// <summary>
		/// Find a CSV Entry by a firstvalue
		/// </summary>
		/// <param name="firstvalue"></param>
		/// <returns></returns>
		public string[] FindCSVEntry(string firstvalue)
		{
			if (table == null)
				return null;
			return (string[]) table[firstvalue];
		}
	}
}