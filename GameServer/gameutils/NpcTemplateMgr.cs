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
using System.Collections;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Manages NPC templates data
	/// </summary>
	public sealed class NpcTemplateMgr
	{
		public enum eBodyType : int
		{
			None = 0,
			Animal = 1,
			Demon = 2,
			Dragon = 3,
			Elemental = 4,
			Giant = 5,
			Humanoid = 6,
			Insect = 7,
			Magical = 8,
			Reptile = 9,
			Plant = 10,
			Undead = 11,
			_Last = 11,
		}

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all NPC templates
		/// </summary>
		private static readonly Hashtable m_mobTemplates = new Hashtable(1024);
		private static readonly Hashtable m_mobTemplatesByName = new Hashtable(1024);

		public Hashtable MobTemplates
		{
			get { return m_mobTemplatesByName; }
		}

		/// <summary>
		/// Initializes NPC templates manager
		/// </summary>
		/// <returns>success</returns>
		public static bool Init()
		{
			try
			{
				lock (m_mobTemplates.SyncRoot)
				{
					m_mobTemplates.Clear();
					var objs = GameServer.Database.SelectAllObjects<DBNpcTemplate>();
					foreach (DBNpcTemplate dbTemplate in objs)
					{
						AddTemplate(new NpcTemplate(dbTemplate));
					}

					return true;
				}
			}
			catch (Exception e)
			{
				log.Error(e);
				return false;
			}
		}

		/// <summary>
		/// Reload templates from the database, being careful not to wipe out script loaded templates
		/// </summary>
		/// <returns></returns>
		public static bool Reload()
		{
			try
			{
				lock (m_mobTemplates.SyncRoot)
				{
					var objs = GameServer.Database.SelectAllObjects<DBNpcTemplate>();
					foreach (DBNpcTemplate dbTemplate in objs)
					{
						AddTemplate(new NpcTemplate(dbTemplate));
					}
					return true;
				}
			}
			catch (Exception e)
			{
				log.Error(e);
				return false;
			}
		}

		/// <summary>
		/// Adds the mob template to collection
		/// </summary>
		/// <param name="template">New mob template</param>
		public static void AddTemplate(INpcTemplate template)
		{
			lock (m_mobTemplates.SyncRoot)
			{
				object entry = m_mobTemplates[template.TemplateId];

				if (entry == null)
				{
					m_mobTemplates[template.TemplateId] = template;
				}
				else if (entry is ArrayList)
				{
					ArrayList array = (ArrayList)entry;
					array.Add(template);
				}
				else
				{
					ArrayList arr = new ArrayList(2);
					arr.Add(entry);
					arr.Add(template);
					m_mobTemplates[template.TemplateId] = arr;
				}
			}
		}

		/// <summary>
		/// Gets mob template by ID, returns random if multiple templates with same ID
		/// </summary>
		/// <param name="templateId">The mob template ID</param>
		/// <returns>The mob template or null if nothing is found</returns>
		public static NpcTemplate GetTemplate(int templateId)
		{
			if (templateId == -1)
				return null;
			lock (m_mobTemplates.SyncRoot)
			{
				object entry = m_mobTemplates[templateId];
				if (entry is ArrayList)
				{
					ArrayList array = (ArrayList)entry;
					return (NpcTemplate)array[Util.Random(array.Count - 1)];
				}
				else if (entry == null)
				{
					//if (log.IsWarnEnabled)
						//log.Warn("No npctemplate with ID " + templateId + " found.");
					return null;
				}
				return (NpcTemplate)entry;
			}
		}
	}
}