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
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The GravestoneMgr manadge all player gravetones visibles in the world
	/// </summary>
	public class GravestoneMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This holds all gravestone of the game
		/// Player unique db id -> GameGraveStone
		/// </summary>
		private static readonly Hashtable m_graveStones = new Hashtable();

		/// <summary>
		/// Add a new gravestone to the GravestoneMgr
		/// </summary>
		public static bool AddGravestone(GameGravestone grave)
		{
			if(grave == null) return false;
			lock(m_graveStones.SyncRoot)
			{
				if(m_graveStones.Contains(grave.InternalID))
				{
					if (log.IsErrorEnabled)
						log.Error("GravestoneMgr.AddGravestone -> The player with the db id (" + grave.InternalID + ") already have a gravestone.\n\n" + Environment.StackTrace);
					return false;
				}

				m_graveStones.Add(grave.InternalID, grave);
				return true;
			}
		}

		/// <summary>
		/// Remove a gravestone from the GravestoneMgr
		/// </summary>
		public static bool RemoveGravestone(GameGravestone grave)
		{
			if(grave == null) return false;
			lock(m_graveStones.SyncRoot)
			{
				if(!m_graveStones.Contains(grave.InternalID))
				{
					if (log.IsErrorEnabled)
						log.Error("GravestoneMgr.RemoveGravestone -> The player with the db id (" + grave.InternalID + ") does not have a gravestone to remove.\n\n" + Environment.StackTrace);
					return false;
				}

				m_graveStones.Remove(grave.InternalID);
				return true;
			}
		}

		/// <summary>
		/// Get the player gravestone or null if no grave
		/// </summary>
		public static GameGravestone GetPlayerGravestone(GamePlayer player)
		{
			if(player == null) return null;
			lock(m_graveStones.SyncRoot)
			{
				return (m_graveStones[player.InternalID] as GameGravestone);
			}
		}
	}
}
