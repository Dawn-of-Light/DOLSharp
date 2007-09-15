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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&formation",
		(uint)ePrivLevel.Player,
		"Change the formation of your pets!", "/formation <type>")]
	public class FormationHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GamePlayer player = client.Player;

			//No one else needs to use this spell
			if (player.CharacterClass.ID != (int)eCharacterClass.Bonedancer)
			{
				client.Out.SendMessage("Only Bonedancers can use this command!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			//Help display
			if (args.Length == 1)
			{
				client.Out.SendMessage("Formation commands:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/formation triangle' Place the pets in a triangle formation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/formation line' Place the pets in a line formation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/formation protect' Place the pets in a protect formation that surrounds the commander.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			//Check to see if the BD has a commander and minions
			if (player.ControlledNpc == null)
			{
				client.Out.SendMessage("You don't have a commander!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			bool haveminion = false;
			foreach (AI.Brain.IControlledBrain icb in player.ControlledNpc.Body.ControlledNpcList)
			{
				if (icb != null)
				{
					haveminion = true;
					break;
				}
			}
			if (!haveminion)
			{
				client.Out.SendMessage("You don't have any minions!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch (args[1].ToLower())
			{
				//Triangle Formation
				case "triangle":
					player.ControlledNpc.Body.Formation = GameNPC.eFormationType.Triangle;
					break;
				//Line formation
				case "line":
					player.ControlledNpc.Body.Formation = GameNPC.eFormationType.Line;
					break;
				//Protect formation
				case "protect":
					player.ControlledNpc.Body.Formation = GameNPC.eFormationType.Protect;
					break;
				default:
					client.Out.SendMessage("Unrecognized argument: " + args[1], eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;
			}

			return 1;
		}
	}
}