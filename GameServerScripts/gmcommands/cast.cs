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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&cast",
		(uint) ePrivLevel.GM,
		"cast a spell",
		"/cast <spellid>")]
	public class CastCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				client.Out.SendMessage("Usage: /cast <spellid>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			ushort spellID = 0;
			try
			{
				spellID = Convert.ToUInt16(args[1]);
				GameObject obj = client.Player.TargetObject;
				GameLiving target = null;
				if (obj == null)
					target = client.Player;
				else if (obj is GameLiving)
					target = (GameLiving) obj;
				foreach(GamePlayer plr in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					plr.Out.SendSpellEffectAnimation(client.Player, target, spellID, 0, false, 1);
			}
			catch
			{
				client.Out.SendMessage("Usage: /cast <spellid>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			return 1;
		}
	}
}