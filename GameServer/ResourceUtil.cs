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
using System.Reflection;
using System.IO;

namespace DOL.Config
{
	/// <summary>
	/// Helps managing embedded resources
	/// </summary>
	public class ResourceUtil
	{
		/// <summary>
		/// Searches for a specific resource and returns the stream
		/// </summary>
		/// <param name="fileName">the resource name</param>
		/// <returns>the resource stream</returns>
		public static Stream GetResourceStream(string fileName)
		{
			Assembly myAssembly = Assembly.GetAssembly(typeof(ResourceUtil));
			fileName = fileName.ToLower();
			foreach(string name in myAssembly.GetManifestResourceNames())
			{
				if(name.ToLower().EndsWith(fileName))
					return myAssembly.GetManifestResourceStream(name);
			}
			return null;
		}

		/// <summary>
		/// Extracts a given resource
		/// </summary>
		/// <param name="fileName">the resource name</param>
		public static void ExtractResource(string fileName)
		{
			ExtractResource(fileName, fileName);
		}
		
		/// <summary>
		/// Extracts a given resource
		/// </summary>
		/// <param name="resourceName">the resource name</param>
		/// <param name="fileName">the external file name</param>
		public static void ExtractResource(string resourceName, string fileName)
		{
			FileInfo finfo = new FileInfo(fileName);
			if(!finfo.Directory.Exists)
				finfo.Directory.Create();

			using(StreamReader reader = new StreamReader(GetResourceStream(resourceName)))
			{
				using(StreamWriter writer = new StreamWriter(File.Create(fileName)))
				{
					writer.Write(reader.ReadToEnd());
				}
			}
		}
	}
}
