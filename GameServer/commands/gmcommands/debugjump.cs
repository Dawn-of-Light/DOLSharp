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
using DOL.GS.Geometry;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[Cmd(
		"]jump",
		ePrivLevel.GM,
		"GMCommands.DebugJump.Description",
		"GMCommands.DebugJump.Usage")]
	public class OnDebugJump : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length != 6)
			{
				DisplaySyntax(client);
				return;
			}

			ushort zoneID = 0;
			if (!ushort.TryParse(args[1], out zoneID))
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.DebugJump.InvalidZoneID", args[1]));
				return;
			}

			var zone = WorldMgr.GetZone(zoneID);
			if (zone == null)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.DebugJump.UnknownZoneID", args[1]));
				return;
			}

            if (!CheckExpansion(client, zone.ZoneRegion.ID))
                return;

            var jumpPositionInZone = Position.Create(
                x: Convert.ToInt32(args[2]),
                y: Convert.ToInt32(args[3]),
                z: Convert.ToInt32(args[4]),
                heading: Convert.ToUInt16(args[4]),
                regionID: zone.ZoneRegion.ID);
            var jumpPosition = jumpPositionInZone + zone.Offset;
			client.Player.MoveTo(jumpPosition);
		}

		public bool CheckExpansion(GameClient client, ushort RegionID)
		{
			Region reg = WorldMgr.GetRegion(RegionID);
			if (reg == null)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.DebugJump.UnknownRegion", RegionID.ToString()));
				return false;
			}
			else if (reg.Expansion > (int)client.ClientType)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.DebugJump.RegionNotSuppByClient", reg.Description));
				return false;
			}
			return true;
		}
	}
}