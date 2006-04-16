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
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.JumpPoints
{
	/// <summary>
	/// This jump point type allow all player to go to its destination
	/// </summary>
	public class ClassicJumpPoint : AbstractJumpPoint
	{	
		#region Declaration
		
		/// <summary>
		/// The target destination of this jump point
		/// </summary>
		protected JumpPointTargetLocation m_targetLocation;	

		/// <summary>
		/// Returns the client ID of this jump point
		/// </summary>
		public JumpPointTargetLocation TargetLocation
		{
			get { return m_targetLocation; }
			set { m_targetLocation = value; }
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
			Region reg = WorldMgr.GetRegion((ushort)m_targetLocation.Region);
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

			return m_targetLocation;
		}
		
		#endregion
	}
}
