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
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace DOL.Database
{
	public class DaoFactoryConfiguration
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// list of config file
		/// </summary>
		private readonly List<XmlTextReader> m_xmlconfigurationFiles = new List<XmlTextReader>();

		/// <summary>
		/// add an xml configuration file for dao factory
		/// </summary>
		/// <param name="xmlFileName"></param>
		public void AddDocument(string xmlFileName)
		{
			XmlTextReader reader;
			try
			{
				reader = new XmlTextReader(xmlFileName);
				AddXmlReader(reader);
			}
			catch(Exception e)
			{
				log.Error("Can not add this configuration file!",e);
				return;
			}
		}

		/// <summary>
		/// add an xml reader for dao factory
		/// </summary>
		/// <param name="xmlreader"></param>
		public void AddXmlReader(XmlTextReader xmlreader)
		{
			m_xmlconfigurationFiles.Add(xmlreader);
		}
		
		/// <summary>
		/// add xml configuration for dao factory
		/// </summary>
		/// <param name="xml"></param>
		public void AddXml(string xml)
		{
			XmlTextReader reader;
			try
			{
				AddXmlReader(new XmlTextReader(new StringReader(xml)));
			}
			catch (Exception e)
			{
				log.Error("Can not add this configuration file!", e);
				return;
			}
		}
		public List<XmlTextReader> Config
		{
			get 
			{
				return m_xmlconfigurationFiles;
			}
		}
		/// <summary>
		/// build he dao factory
		/// </summary>
		public IDaoFactory BuildDaoFactory()
		{
			return new DaoFactory(this);
		}
	}
}
