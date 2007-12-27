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
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[Cmd("]jump", ePrivLevel.GM, "Teleports yourself to the specified location",
			"']jump <zoneID> <locX> <locY> <locZ> <heading>' (Autoused for *jump in debug mode)")]
	public class OnDebugJump : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 6)
			{
				ushort zoneID = 0;
				ushort.TryParse(args[1], out zoneID);
				if (zoneID == 0)
				{
					DisplayMessage(client, "Invalid zoneID: " + args[1]);
					return;
				}
				Zone z = WorldMgr.GetZone(zoneID);
				if (z == null)
				{
					DisplayMessage(client, "Unknown zone ID: " + args[1]);
					return;
				}
				ushort RegionID = z.ZoneRegion.ID;
				int X = z.XOffset + Convert.ToInt32(args[2]);
				int Y = z.YOffset + Convert.ToInt32(args[3]);
				int Z = Convert.ToInt32(args[4]);
				ushort Heading = Convert.ToUInt16(args[5]);
				if (!CheckExpansion(client, RegionID)) return;
				client.Player.MoveTo(RegionID, X, Y, Z, Heading);
			}
			else
			{
				DisplaySyntax(client);
			}
		}

		public bool CheckExpansion(GameClient client, ushort RegionID)
		{
			Region reg = WorldMgr.GetRegion(RegionID);
			if (reg == null)
			{
				DisplayMessage(client, "Unknown region (" + RegionID.ToString() + ").");
				return false;
			}
			else if (reg.Expansion >= (int)client.ClientType)
			{
				DisplayMessage(client, "Region (" + reg.Description + ") is not supported by your client.");
				return false;
			}
			return true;
		}
	}
}
