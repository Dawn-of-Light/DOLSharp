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
		 "&titlegm",
		 ePrivLevel.GM,
		 "Changes target player's titles",
		 "/titlegm <add> <class type> - add a title to the target player",
		 "/titlegm <remove> <class type> - remove a title from the target player",
		 "/titlegm <set> <class type> - sets current title of the target player",
		 "/titlegm <list> - lists all target player's titles")]
	public class TitleGmCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GamePlayer target = client.Player.TargetObject as GamePlayer;
			if (target == null)
			{
				client.Out.SendMessage("You must target a player to change his titles!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			if (args.Length > 2)
			{
				IPlayerTitle title = PlayerTitleMgr.GetTitleByTypeName(args[2]);
				if (title == null)
				{
					client.Out.SendMessage("Title '" + args[2] + "' not found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				
				switch (args[1].ToLower())
				{
					case "add":
						{
							target.AddTitle(title);
							return 1;
						}
						
					case "remove":
						{
							target.RemoveTitle(title);
							return 1;
						}
						
					case "set":
						{
							target.CurrentTitle = title;
							return 1;
						}
				}
			}
			else if (args.Length > 1)
			{
				switch (args[1].ToLower())
				{
					case "list":
						{
							ArrayList list = new ArrayList();
							foreach (IPlayerTitle title in target.Titles)
							{
								list.Add("- " + title.GetDescription(target));
								list.Add(" (" + title.GetType().FullName + ")");
							}
							list.Add(" ");
							list.Add("Current:");
							list.Add("- " + target.CurrentTitle.GetDescription(target));
							list.Add(" (" + target.CurrentTitle.GetType().FullName + ")");
							client.Out.SendCustomTextWindow(target.Name + "'s titles", list);
							return 1;
						}
				}
			}
			
			DisplaySyntax(client);
			
			return 1;
		}
	}
}
