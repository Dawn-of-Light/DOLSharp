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
	/// This jump point type allow only player of one realm to go to its destination
	/// </summary>
	public class RealmCheckJumpPoint : ClassicJumpPoint
	{
		#region Declaration
		
		/// <summary>
		/// The realm allowed to use this jump point
		/// </summary>
		protected eRealm m_allowedRealm;		

		/// <summary>
		/// Return the realm allowed to use this jump point
		/// </summary>
		public eRealm AllowedRealm
		{
			get { return m_allowedRealm; }
			set { m_allowedRealm = value; }
		}

		#endregion

		#region Function

		/// <summary>
		/// Decides whether player can jump to the target point.
		/// All messages with reasons must be sent here.
		/// Can change destination too.
		/// </summary>
		/// <param name="player">the player who want to jump</param>
		/// <returns>true if allowed</returns>
		public override JumpPointTargetLocation IsAllowedToJump(GamePlayer player)
		{
			if(base.IsAllowedToJump(player) == null) return null;

			if(m_allowedRealm != (eRealm)player.Realm)
			{
				player.Out.SendMessage("Only players from "+m_allowedRealm+" are allowed to use this jump point !",eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return null;
			}

			return m_targetLocation;
		}
		
		#endregion
	}
}