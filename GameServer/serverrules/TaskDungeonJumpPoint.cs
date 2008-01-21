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

using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;

namespace DOL.GS.ServerRules
{
	/// <summary>
	/// Handles task dungeon jump points
	/// </summary>
	public class TaskDungeonJumpPoint : IJumpPointHandler
	{
		/// <summary>
		/// Decides whether player can jump to the target point.
		/// All messages with reasons must be sent here.
		/// Can change destination too.
		/// </summary>
		/// <param name="targetPoint">The jump destination</param>
		/// <param name="player">The jumping player</param>
		/// <returns>True if allowed</returns>
		public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
		{
			TaskDungeonMission mission = player.Mission as TaskDungeonMission;

			//make sure we have a mission
			if (mission == null)
			{
				player.Out.SendMessage("You need to have a proper mission before entering this area!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			//get the proper location
			ZonePoint zp = (ZonePoint)GameServer.Database.SelectObject(typeof(ZonePoint), "`Region` = '" + mission.TaskRegion.ID + "'");
			if (zp != null)
			{
				targetPoint.Region = zp.Region;
				targetPoint.X = zp.X;
				targetPoint.Y = zp.Y;
				targetPoint.Z = zp.Z;
				targetPoint.Heading = zp.Heading;
			}
			else
			{
				player.Out.SendMessage("Jump Point for the task dungeon instance region " + mission.TaskRegion.ID + " cannot be found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}
	}
}
