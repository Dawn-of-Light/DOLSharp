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
using System.Reflection;

using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Geometry;

namespace DOL.GS
{
	/// <summary>
	/// Description of TeleportArea.
	/// Used to teleport players when someone enters, with Z-checks
	/// </summary>
	public class TeleportArea : Area.Circle
	{
		public override void OnPlayerEnter(GamePlayer player)
		{
			base.OnPlayerEnter(player);
			Teleport destination =  WorldMgr.GetTeleportLocation(player.Realm, String.Format("{0}:{1}", this.GetType(), this.Description));
			
			if (destination != null)
				OnTeleport(player, destination);
			else
				player.Out.SendMessage("This destination is not available : "+String.Format("{0}:{1}", this.GetType(), this.Description)+".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		
		/// <summary>
		/// Teleport the player to the designated coordinates. 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected void OnTeleport(GamePlayer player, Teleport destination)
		{
			if (player.InCombat == false && GameRelic.IsPlayerCarryingRelic(player) == false)
			{
				player.LeaveHouse();
				player.MoveTo(destination.GetPosition());
				GameServer.ServerRules.OnPlayerTeleport(player, destination);
			}
		}
		
	}

    public class TeleportPillarArea : TeleportArea
    {
        public override bool IsContaining(Coordinate spot, bool ignoreZ)
            => base.IsContaining(spot, ignoreZ: true);
    }
}
