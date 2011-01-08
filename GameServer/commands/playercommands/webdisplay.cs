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
using DOL.GS;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&webdisplay",
		ePrivLevel.Player,
		"Set informations displayed on the herald",
		"/webdisplay <position|template|equipment|craft> [on|off]")]
	public class WebDisplayCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplayInformations(client);
				return;
			}
			
			if (args[1].ToLower() == "position")
				WdChange(GlobalConstants.eWebDisplay.position, client.Player, args.Length==3?args[2].ToLower():null);
			if (args[1].ToLower() == "equipment")
				WdChange(GlobalConstants.eWebDisplay.equipment, client.Player, args.Length==3?args[2].ToLower():null);
			if (args[1].ToLower() == "template")
				WdChange(GlobalConstants.eWebDisplay.template, client.Player, args.Length==3?args[2].ToLower():null);
			if (args[1].ToLower() == "craft")
				WdChange(GlobalConstants.eWebDisplay.craft, client.Player, args.Length==3?args[2].ToLower():null);
			
			DisplayInformations(client);
		}

		// Set the eWebDisplay status
		private void WdChange(GlobalConstants.eWebDisplay category, GamePlayer player, string state)
		{
			if (string.IsNullOrEmpty(state))
				player.DBCharacter.NotDisplayedInHerald ^= (byte)category;
			else
			{
				if (state == "off")
					player.DBCharacter.NotDisplayedInHerald |= (byte)category;
				
				if (state == "on")
					player.DBCharacter.NotDisplayedInHerald &= (byte)~category;
			}
			
			Log.Debug("Player " + player.Name + ": WD = " + player.DBCharacter.NotDisplayedInHerald);
		}

		// Display the informations
		private void DisplayInformations(GameClient client)
		{
			byte webDisplay = client.Player.DBCharacter.NotDisplayedInHerald;
			byte webDisplayFlag;

			string state = "/webdisplay <position|template|equipment|craft> [on|off]\n";
			
			webDisplayFlag = (byte)GlobalConstants.eWebDisplay.equipment;
			if ((webDisplay & webDisplayFlag) == webDisplayFlag)
				state += "Your equipment is not displayed.\n";
			else
				state += "Your equipment is displayed.\n";
			
			webDisplayFlag = (byte)GlobalConstants.eWebDisplay.position;
			if ((webDisplay & webDisplayFlag) == webDisplayFlag)
				state += "Your position is not displayed.\n";
			else
				state += "Your position is displayed.\n";
			
			webDisplayFlag = (byte)GlobalConstants.eWebDisplay.template;
			if ((webDisplay & webDisplayFlag) == webDisplayFlag)
				state += "Your template is not displayed.\n";
			else
				state += "Your template is displayed.\n";
	
			webDisplayFlag = (byte)GlobalConstants.eWebDisplay.craft;
			if ((webDisplay & webDisplayFlag) == webDisplayFlag)
				state += "Your crafting skill is not displayed.\n";
			else
				state += "Your crafting skill is displayed.\n";		
			
			DisplayMessage(client, state);
		}
	}
}