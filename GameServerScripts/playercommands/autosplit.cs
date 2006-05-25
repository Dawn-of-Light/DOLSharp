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

namespace DOL.GS.Scripts
{
	[CmdAttribute("&autosplit", 
		 (uint) ePrivLevel.Player,
		 "Choose how the loot and money are split between members of group",
		 "/autosplit on/off (Leader only: Toggles both coins and loot for entire group)",
		 "/autosplit coins (Leader only: When turned off, will send coins to the person who picked it up, instead of splitting it evenly across other members)",
		 "/autosplit loot (Leader only: When turned off, will send loot to the person who picked it up, instead of splitting it evenly across other members)",
		 "/autosplit self (Any group member: Choose not to receive autosplit loot items. You will still receive autosplit coins.)")]
	public class AutosplitCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{

			// If they are not in a group, then this command should not work at all
			if (client.Player.PlayerGroup == null)
			{
				client.Out.SendMessage("You must be in a group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			if (args.Length < 2)
			{
				return DisplaySyntax(client);
			}

			string command = args[1].ToLower();

			// /autosplit for leaders -- Make sue it is the group leader using this command, if it is, execute it.
			if (command == "on" || command == "off" || command == "coins" || command == "loot")
			{
				if (client.Player != client.Player.PlayerGroup.Leader)
				{
					client.Out.SendMessage("You must be the leader of the group to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				switch (command)
				{
					case "on":
						client.Player.PlayerGroup.AutosplitLoot = true;
						client.Player.PlayerGroup.AutosplitCoins  = true;
						client.Player.PlayerGroup.SendMessageToGroupMembers("The leader switched on the autosplit", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;

					case "off":
						client.Player.PlayerGroup.AutosplitLoot = false;
						client.Player.PlayerGroup.AutosplitCoins = false;
						client.Player.PlayerGroup.SendMessageToGroupMembers("The leader switched off the autosplit", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;

					case "coins":
						client.Player.PlayerGroup.AutosplitCoins = !client.Player.PlayerGroup.AutosplitCoins;
						client.Player.PlayerGroup.SendMessageToGroupMembers("The leader switched " + (client.Player.PlayerGroup.AutosplitCoins ? "on" : "off") + " the autosplit coin", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;

					case "loot":
						client.Player.PlayerGroup.AutosplitLoot = !client.Player.PlayerGroup.AutosplitLoot;
						client.Player.PlayerGroup.SendMessageToGroupMembers("The leader switched " + (client.Player.PlayerGroup.AutosplitLoot ? "on" : "off") + " the autosplit loot", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
				}
				return 1;
			}

			// /autosplit for Members including leader -- 
			if (command == "self")
			{
				client.Player.AutoSplitLoot = !client.Player.AutoSplitLoot;
				client.Player.PlayerGroup.SendMessageToGroupMembers(client.Player.Name + " switched " + (client.Player.AutoSplitLoot ? "on" : "off") + " their autosplit loot", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			//if nothing matched, then they tried to invent thier own commands -- show syntax
			return DisplaySyntax(client);
		}
	}
}