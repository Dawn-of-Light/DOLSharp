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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&addbind",
		(uint) ePrivLevel.GM,
		"adds a bindpoint to the game",
		"/addbind [radius=750]")]
	public class AddBindCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			ushort bindRadius = 750;
			if (args.Length >= 2)
			{
				try
				{
					bindRadius = UInt16.Parse(args[1]);
				}
				catch
				{
				}
			}
			BindPoint bp = new BindPoint();
			bp.X = client.Player.X;
			bp.Y = client.Player.Y;
			bp.Z = client.Player.Z;
			bp.Region = client.Player.CurrentRegionID;
			bp.Radius = bindRadius;
			GameServer.Database.AddNewObject(bp);
			client.Player.CurrentRegion.AddArea(new Area.BindArea("bind point", bp));
			client.Out.SendMessage("Bindpoint added: X=" + bp.X + " Y=" + bp.Y + " Z=" + bp.Z + " Radius=" + bp.Radius + " Region=" + bp.Region, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}
	}
}