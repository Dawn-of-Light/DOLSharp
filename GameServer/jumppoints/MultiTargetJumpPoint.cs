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
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.JumpPoints
{
	/// <summary>
	/// This jump point teleport the player to different location using its realm
	/// </summary>
	public class MultiTargetJumpPoint : AbstractJumpPoint
	{
		#region Declaration

		/// <summary>
		/// The list of all location this jump point to telport to
		/// </summary>
		private Iesi.Collections.ISet m_targetLocations;

		/// <summary>
		/// Gets or sets the list of all location this jump point to telport to
		/// </summary>
		public Iesi.Collections.ISet TargetLocations
		{
			get
			{
				if(m_targetLocations == null) m_targetLocations = new Iesi.Collections.HybridSet();
				return m_targetLocations;
			}
			set	{ m_targetLocations = value; }
		}

		#endregion

		#region Function

		/// <summary>
		/// Decides whether player can jump to the target point.
		/// All messages with reasons must be sent here.
		/// Can change destination too.
		/// </summary>
		/// <param name="player">the player who want to jump</param>
		/// <returns>the JumpPointTargetLocation if allowed, null if not</returns>
		public override JumpPointTargetLocation IsAllowedToJump(GamePlayer player)
		{
			foreach(JumpPointMultiTargetLocation loc in TargetLocations)
			{
				if(loc.Realm == (eRealm)player.Realm)
				{
					Region reg = WorldMgr.GetRegion((ushort)loc.Region);
					if(reg == null)
					{
						player.Out.SendMessage("Destination region (" + reg.Description + ") is actually not in use.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return null;
					}
					if (reg.Expansion >= player.Client.ClientType)
					{
						player.Out.SendMessage("Destination region (" + reg.Description + ") is not supported by your client type.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return null;
					}
					return loc;
				}
			}

			return null;
		}
		
		#endregion
	}
}
