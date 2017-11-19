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
using DOL.Database;
using DOL.GS.PacketHandler;

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
			Teleport destination =  WorldMgr.GetTeleportLocation(player.Realm, $"{GetType()}:{Description}");
			
			if (destination != null)
			{
			    OnTeleport(player, destination);
			}
            else
			{
			    player.Out.SendMessage("This destination is not available : " + $"{GetType()}:{Description}" + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
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
				GameLocation currentLocation = new GameLocation("TeleportStart", player.CurrentRegionID, player.X, player.Y, player.Z);
				player.MoveTo((ushort)destination.RegionID, destination.X, destination.Y, destination.Z, (ushort)destination.Heading);
				GameServer.ServerRules.OnPlayerTeleport(player, currentLocation, destination);
			}
		}
		
	}

	/// <summary>
	/// Description of TeleportArea.
	/// Used to teleport players when someone enters, Withtout Z-checks
	/// </summary>	
	public class TeleportPillarArea : TeleportArea
	{
		
		public override bool IsContaining(int x, int y, int z)
		{
			return base.IsContaining(x, y, z, false);
		}
		
		public override bool IsContaining(IPoint3D spot)
		{
			return base.IsContaining(spot, false);
		}
		
		public override bool IsContaining(int x, int y, int z, bool checkZ)
		{
			return base.IsContaining(x, y, z, false);
		}
		
		public override bool IsContaining(IPoint3D p, bool checkZ)
		{
			return base.IsContaining(p, false);
		}
		
	}
}
