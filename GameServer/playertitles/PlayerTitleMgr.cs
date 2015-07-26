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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace DOL.GS.PlayerTitles
{
	/// <summary>
	/// Handles loading of player titles.
	/// </summary>
	public static class PlayerTitleMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all player titles.
		/// </summary>
		private static readonly HashSet<IPlayerTitle> m_titles = new HashSet<IPlayerTitle>();
		
		/// <summary>
		/// Holds special "empty" title instance.
		/// </summary>
		public static readonly ClearTitle ClearTitle = new ClearTitle();
		
		/// <summary>
		/// Initializes/loads all known player titles.
		/// </summary>
		/// <returns>true if successful</returns>
		public static bool Init()
		{
			m_titles.Clear();
			foreach (Type t in ScriptMgr.GetDerivedClasses(typeof (IPlayerTitle)))
			{
				if (t == ClearTitle.GetType()) continue;
				
				IPlayerTitle title;
				try
				{
					title = (IPlayerTitle) Activator.CreateInstance(t);
				}
				catch (Exception e)
				{
					log.ErrorFormat("Error loading player title '{0}': {1}", t.FullName, e);
					continue;
				}
				
				m_titles.Add(title);
				log.DebugFormat("Loaded player title: {0}", title.GetType().FullName);
			}
			
			log.InfoFormat("Loaded {0} player titles", m_titles.Count);
			
			return true;
		}
		
		/// <summary>
		/// Gets all titles that are suitable for player.
		/// </summary>
		/// <param name="player">The player for title checks.</param>
		/// <returns>All title suitable for given player or an empty list if none.</returns>
		public static ICollection GetPlayerTitles(GamePlayer player)
		{
			var titles = new HashSet<IPlayerTitle>();
			
			titles.Add(ClearTitle);
			
			return titles.Concat(m_titles.Where(t => t.IsSuitable(player))).ToArray();
		}
		
		/// <summary>
		/// Gets the title by its type name.
		/// </summary>
		/// <param name="s">The type name to search for.</param>
		/// <returns>Found title or null.</returns>
		public static IPlayerTitle GetTitleByTypeName(string s)
		{
			if (string.IsNullOrEmpty(s))
				return null;
			
			return m_titles.FirstOrDefault(t => t.GetType().FullName == s);
		}
		
		/// <summary>
		/// Registers a title.
		/// </summary>
		/// <param name="title">The title to register.</param>
		/// <returns>true if successful.</returns>
		public static bool RegisterTitle(IPlayerTitle title)
		{
			if (title == null)
				return false;
			
			Type t = title.GetType();
			
			if (m_titles.Any(ttl => ttl.GetType() == t))
				return false;
			
			m_titles.Add(title);
			return true;
		}
	}
}
