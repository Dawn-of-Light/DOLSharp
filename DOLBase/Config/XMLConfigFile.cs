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
using System.Text;
using System.Xml;

namespace DOL.Config
{
	/// <summary>
	/// Reads and parses an XML file
	/// </summary>
	public class XMLConfigFile : ConfigElement
	{
		/// <summary>
		/// Constructs a new XML config file
		/// </summary>
		public XMLConfigFile() : base(null)
		{
		}

		/// <summary>
		/// Constructs a new XML config file element
		/// </summary>
		/// <param name="parent">The parent of the XML config file element</param>
		protected XMLConfigFile(ConfigElement parent): base(parent)
		{
		}

		/// <summary>
		/// Test wether the given string contains invalid xml characters 
		/// </summary>
		/// <param name="name">The name to test</param>
		/// <returns>true if invalid characters are contained, false if the element is ok</returns>
		protected bool IsBadXMLElementName(string name)
		{
			if(name==null)
				return false;

			if(name.IndexOf(@"\")!=-1)
				return true;

			if(name.IndexOf(@"/")!=-1)
				return true;

			if(name.IndexOf(@"<")!=-1)
				return true;

			if(name.IndexOf(@">")!=-1)
				return true;
			
			return false;
		}

		/// <summary>
		/// Saves a single config element in an xml stream
		/// </summary>
		/// <param name="writer">the xml text writer</param>
		/// <param name="name">the name for this element</param>
		/// <param name="element">the element to save</param>
		protected void SaveElement(XmlTextWriter writer, string name, ConfigElement element)
		{
			bool badName = IsBadXMLElementName(name);

			if(element.HasChildren)
			{
				if(name == null)
					name = "root";
				
				if(badName)
				{
					writer.WriteStartElement("param");
					writer.WriteAttributeString("name",name);
				}
				else
				{
					writer.WriteStartElement(name);
				}
				
				foreach(DictionaryEntry entry in element.Children)
				{
					SaveElement(writer, (string) entry.Key, (ConfigElement) entry.Value);
				}
				writer.WriteEndElement();
			}
			else
			{
				if(name != null)
				{
					if(badName)
					{
						writer.WriteStartElement("param");
						writer.WriteAttributeString("name",name);
						writer.WriteString(element.GetString());
						writer.WriteEndElement();
					}
					else
					{
						writer.WriteElementString(name, element.GetString());
					}
				}
			}
		}

		/// <summary>
		/// Saves this config file
		/// </summary>
		/// <param name="configFile">The filename</param>
		public void Save(FileInfo configFile)
		{
			if(configFile.Exists)
				configFile.Delete();

			XmlTextWriter writer = new XmlTextWriter(configFile.FullName, Encoding.UTF8);
			// Indent the XML document for readability
			writer.Formatting = System.Xml.Formatting.Indented;	
			
			writer.WriteStartDocument();
			SaveElement(writer, null, this);
			writer.WriteEndDocument();
			writer.Close();
		}

		/// <summary>
		/// Loads the xml configuration from a file
		/// </summary>
		/// <param name="configFile">The config file</param>
		/// <returns>The parsed config</returns>
		public static XMLConfigFile ParseXMLFile(FileInfo configFile)
		{
			XMLConfigFile root = new XMLConfigFile(null);
			if(!configFile.Exists)
				return root;
			
			ConfigElement current = root;
			XmlTextReader reader = new XmlTextReader(configFile.OpenRead());
			
			while(reader.Read())
			{
				if(reader.NodeType == XmlNodeType.Element)
				{
					if(reader.Name=="root")
						continue;

					if(reader.Name=="param")
					{
						string name = reader.GetAttribute("name");
						if(name != null && name != "root")
						{
							ConfigElement newElement = new ConfigElement(current);
							current[name] = newElement;
							current = newElement;													
						}
					}
					else
					{
						ConfigElement newElement = new ConfigElement(current);
						current[reader.Name] = newElement;
						current = newElement;						
					}
				}
				else if(reader.NodeType == XmlNodeType.Text)
				{
					current.Set(reader.Value);
				}
				else if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.Name!="root")
						current = current.Parent;
				}
			}
			reader.Close();
			return root;
		}
	}
}
