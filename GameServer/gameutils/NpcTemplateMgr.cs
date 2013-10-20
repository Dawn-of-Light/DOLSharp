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
using System.Collections.Generic;
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
		private static readonly Dictionary<int, List<INpcTemplate>> m_mobTemplates = new Dictionary<int, List<INpcTemplate>>();

		/// <summary>
		/// Initializes NPC templates manager
		/// </summary>
		/// <returns>success</returns>
		public static bool Init()
		{
			try
			{
				lock (((ICollection)m_mobTemplates).SyncRoot)
				{
					m_mobTemplates.Clear();
					IList<DBNpcTemplate> objs = GameServer.Database.SelectAllObjects<DBNpcTemplate>();
					foreach (DBNpcTemplate dbTemplate in objs)
					{
						try
						{
							AddTemplate(new NpcTemplate(dbTemplate));
						}
						catch (Exception ex)
						{
							log.Error("Error loading template " + dbTemplate.Name, ex);
						}
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
				lock (((ICollection)m_mobTemplates).SyncRoot)
				{
					IList<DBNpcTemplate> objs = GameServer.Database.SelectAllObjects<DBNpcTemplate>();

					// remove all the db templates
					foreach (DBNpcTemplate dbTemplate in objs)
					{
						RemoveTemplate(new NpcTemplate(dbTemplate));
					}

					// add them back in
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
		/// Removes a template
		/// </summary>
		/// <param name="template">mob template</param>
		public static void RemoveTemplate(INpcTemplate template)
		{
			lock (((ICollection)m_mobTemplates).SyncRoot)
			{
				if (m_mobTemplates.ContainsKey(template.TemplateId) && m_mobTemplates[template.TemplateId] != null && m_mobTemplates[template.TemplateId].Contains(template))
				{
					if(m_mobTemplates[template.TemplateId].Count == 1)
						m_mobTemplates[template.TemplateId].Clear();
					else
						m_mobTemplates[template.TemplateId].Remove(template);
				}
			}
		}

		/// <summary>
		/// Adds the mob template to collection
		/// </summary>
		/// <param name="template">New mob template</param>
		public static void AddTemplate(INpcTemplate template)
		{
			lock (((ICollection)m_mobTemplates).SyncRoot)
			{

				if (!m_mobTemplates.ContainsKey(template.TemplateId) || m_mobTemplates[template.TemplateId] == null)
				{
					m_mobTemplates[template.TemplateId] = new List<INpcTemplate>();
					m_mobTemplates[template.TemplateId].Add(template);
				}
				else
				{
					m_mobTemplates[template.TemplateId].Add(template);
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
			
			lock (((ICollection)m_mobTemplates).SyncRoot)
			{
				if(!m_mobTemplates.ContainsKey(templateId) || m_mobTemplates[templateId] == null || m_mobTemplates[templateId].Count == 0)
				   return null;	
				
				return (NpcTemplate)m_mobTemplates[templateId][Util.Random(m_mobTemplates[templateId].Count - 1)];
			}
		}
	}
}