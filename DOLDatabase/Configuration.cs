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
	/// <summary>
	/// Handles configuration of <see cref="Database.DatabaseMgr"/>
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the list of DAO types to register.
		/// </summary>
		private readonly List<RegisterEntry> m_daosToRegister = new List<RegisterEntry>();

		/// <summary>
		/// Holds the state object type.
		/// </summary>
		private Type m_state;

		/// <summary>
		/// Holds the state params.
		/// </summary>
		private readonly Dictionary<string, string> m_stateParams = new Dictionary<string, string>();

		/// <summary>
		/// Holds the DAO manager type.
		/// </summary>
		private Type m_databaseMgr;
		
		/// <summary>
		/// Holds database params.
		/// </summary>
		private readonly Dictionary<string, string> m_databaseMgrParams = new Dictionary<string, string>();

		/// <summary>
		/// Gets or sets the state object type.
		/// Must implement <see cref="IDisposable"/> interface.
		/// </summary>
		/// <value>The state object type.</value>
		public Type State
		{
			get { return m_state; }
			set { m_state = value; }
		}

		/// <summary>
		/// Gets the state params.
		/// </summary>
		/// <value>The state params.</value>
		public IDictionary<string, string> StateParams
		{
			get { return m_stateParams; }
		}

		/// <summary>
		/// Gets or sets the DAO manager type.
		/// </summary>
		/// <value>The DAO manager type.</value>
		public Type DatabaseMgr
		{
			get { return m_databaseMgr; }
			set { m_databaseMgr = value; }
		}

		/// <summary>
		/// Gets the database's params.
		/// </summary>
		/// <value>The database's params.</value>
		public Dictionary<string, string> DatabaseMgrParams
		{
			get { return m_databaseMgrParams; }
		}

		/// <summary>
		/// Reads configuration from an XML file.
		/// </summary>
		/// <param name="xmlFileName">The XML config file name.</param>
		/// <returns>Configuration.</returns>
		/// <seealso cref="AddXml(XmlDocument)"/>
		public Configuration AddXmlFile(string xmlFileName)
		{
			return AddXml(new XmlTextReader(xmlFileName));
		}

		/// <summary>
		/// Reads configuration from XML text string.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns>Configuration.</returns>
		/// <seealso cref="AddXml(XmlDocument)"/>
		public Configuration AddXml(string xml)
		{
			return AddXml(new XmlTextReader(new StringReader(xml)));
		}

		/// <summary>
		/// Reads configuration from an <see cref="XmlTextReader"/>.
		/// </summary>
		/// <param name="xmlReader">The XML config to read from.</param>
		/// <returns>Configuration.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="xmlReader"/> is null.</exception>
		/// <seealso cref="AddXml(XmlDocument)"/>
		public Configuration AddXml(XmlReader xmlReader)
		{
			if (xmlReader == null)
			{
				throw new ArgumentNullException("xmlReader");
			}
			
			XmlDocument doc = new XmlDocument();
			using (xmlReader)
			{
				doc.Load(xmlReader);
			}

			AddXml(doc);
			return this;
		}

		/// <summary>
		/// Reads configuration from an <see cref="XmlDocument"/>. 
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <returns></returns>
		/// <remarks>
		/// <para>Root element name must be "DatabaseConfiguration".</para>
		/// <para>XML config example:
		/// <code>
		/// &lt;DatabaseMgrConfiguration&lt;
		///   &lt;IDatabaseMgr type="DOL.Database.NHibernate.NHDatabaseMgr, DOLDatabaseNHibernate" /&gt;
		///   &lt;State type="DOL.Database.NHibernate.NHState, DOLDatabase.NHibernate" config="NHibernateConfig.xml"/&gt;
		///   &lt;Register interface="DOL.Database.DataAccessInterfaces.IAccountDao, DOLDatabase" dao="DOL.Database.NHibernate.AccountDao, DOLDatabase.NHibernate" /&gt;
		/// &lt;/DatabaseMgrConfiguration&gt;
		/// </code>
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="doc"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if "DataAccessMgrConfiguration" element is not found.</exception>
		public Configuration AddXml(XmlDocument doc)
		{
			if (doc == null)
			{
				throw new ArgumentNullException("doc");
			}
			
			XmlElement root = doc["DatabaseMgrConfiguration"];
			if (root == null)
			{
				throw new ArgumentException("Root element \"DatabaseMgrConfiguration\" is not found", "doc");
			}

			XmlNode node;

			node = root["IDatabaseMgr"];
			if (node != null)
			{
				m_databaseMgr = GetTypeFromAttribute(node, "type");
				AddAttributes(node, m_databaseMgrParams);
			}
			
			node = root["State"];
			if (node != null)
			{
				m_state = GetTypeFromAttribute(node, "type");
				AddAttributes(node, m_stateParams);
			}

			RegisterEntry entry = new RegisterEntry();
			foreach (XmlNode reg in root.GetElementsByTagName("Register"))
			{
				entry.InterfaceType = GetTypeFromAttribute(reg, "interface");
				entry.DaoType       = GetTypeFromAttribute(reg, "dao");
				entry.DaoParams     = AddAttributes(reg, new Dictionary<string, string>());
				
				m_daosToRegister.Add(entry);
			}
			
			return this;
		}

		/// <summary>
		/// Adds the attributes to the <paramref name="dictionary"/>.
		/// </summary>
		/// <param name="node">The node to read attributes from.</param>
		/// <param name="dictionary">The dictionary.</param>
		/// <returns><paramref name="dictionary"/></returns>
		private T AddAttributes<T>(XmlNode node, T dictionary) where T : IDictionary<string, string>
		{
			foreach (XmlAttribute attribute in node.Attributes)
			{
				dictionary.Add(attribute.Name, attribute.Value);
			}
			return dictionary;
		}

		/// <summary>
		/// Gets the Type from XML node attribute.
		/// </summary>
		/// <param name="node">The XML reader.</param>
		/// <param name="attributeName">The attribute name.</param>
		/// <returns>A type.</returns>
		private Type GetTypeFromAttribute(XmlNode node, string attributeName)
		{
			string typeName = node.Attributes[attributeName].Value;
			if (typeName == null)
			{
				throw new ArgumentException("Attribute '" + attributeName + "' is not found on node " + node.Name);
			}
			return Type.GetType(typeName, true);
		}

		/// <summary>
		/// Adds DAO interface and DAO type to configuration.
		/// </summary>
		/// <param name="daoInterface">The DAO interface type.</param>
		/// <param name="dao">The DAO type.</param>
		/// <returns>Configuration.</returns>
		public Configuration AddDao(Type daoInterface, Type dao, IDictionary<string, string> param)
		{
			RegisterEntry entry = new RegisterEntry();
			entry.InterfaceType = daoInterface;
			entry.DaoType       = dao;
			entry.DaoParams     = param;
			
			m_daosToRegister.Add(entry);
			
			return this;
		}

		/// <summary>
		/// Builds a database manager.
		/// </summary>
		/// <remarks>
		/// <para>
		/// First, creates an instance of <see cref="State"/> type with <see cref="StateParams"/>;
		/// then creates an instance of <see cref="DatabaseMgr"/> type with State object passed to constructor;
		/// then creates an instance of all DAO types with State object passed to constructor;
		/// and registers them.
		/// </para>
		/// <para>
		/// The State object must implement <see cref="IDisposable"/> interface.
		/// </para>
		/// </remarks>
		/// <returns>Created database manager instance.</returns>
		public IDatabaseMgr BuildDatabaseMgr()
		{
			IDisposable state = null;
			if (m_state != null)
			{
				if (!typeof(IDisposable).IsAssignableFrom(m_state))
				{
					throw new Exception("The state type " + m_state.FullName + " must implement IDisposable interface");
				}
				
				state = (IDisposable) Activator.CreateInstance(m_state, m_stateParams);
			}

			IDatabaseMgr mgr = (IDatabaseMgr) Activator.CreateInstance(m_databaseMgr, m_databaseMgrParams, state);

			foreach (RegisterEntry entry in m_daosToRegister)
			{
				mgr.Register(entry.InterfaceType, entry.DaoType, entry.DaoParams);
			}
			
			return mgr;
		}
		
		/// <summary>
		/// Holds the information needed to register DAOs.
		/// </summary>
		public struct RegisterEntry
		{
			/// <summary>
			/// Holds the DAO interface type.
			/// </summary>
			public Type InterfaceType;
			/// <summary>
			/// Holds the DAO type.
			/// </summary>
			public Type DaoType;
			/// <summary>
			/// Holds the DAO params.
			/// </summary>
			public IDictionary<string, string> DaoParams;
			
#warning add "operator==" and equals
		}
	}
}
