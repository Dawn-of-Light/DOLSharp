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
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerTitles;
using DOL.GS.Commands;

namespace DOL.GS.Commands
{
	[Cmd(
		 "&settitle",
		 ePrivLevel.Player,
		 "Sets the current player title",
		 "/settitle <index> - to change current title using index in the list")]
	public class SetTitleCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "settitle"))
				return;

			int index = -1;
			if (args.Length > 1)
			{
				try { index = int.Parse(args[1]); }
				catch { }

				IPlayerTitle current = client.Player.CurrentTitle;
				if (current != null && current.IsForced(client.Player))
					client.Out.SendMessage("You cannot change the current title.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				else
				{
					IList titles = client.Player.Titles;
					if (index < 0 || index >= titles.Count)
						client.Player.CurrentTitle = PlayerTitleMgr.ClearTitle;
					else
						client.Player.CurrentTitle = (IPlayerTitle)titles[index];
				}
			}
			else
			{
				client.Out.SendPlayerTitles();
			}
		}
	}
}
