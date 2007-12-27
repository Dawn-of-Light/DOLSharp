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
using DOL.GS;
using DOL.Database;
using DOL.GS.Commands;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&level", //command to handle
	ePrivLevel.Player, //minimum privelege level
	"Allows you to level 20 instantly if you have a level 50", "/level")] //usage
	public class LevelCommandHandler : AbstractCommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (ServerProperties.Properties.SLASH_LEVEL_TARGET <= 1)
			{
				DisplayMessage(client, "/level is disabled on this server currently!");
				return 0;
			}
			if (!ServerProperties.Properties.ALLOW_CATA_SLASH_LEVEL)
			{
				switch ((eCharacterClass)client.Player.CharacterClass.ID)
				{
					case eCharacterClass.Heretic:
					case eCharacterClass.Valkyrie:
					case eCharacterClass.Warlock:
					case eCharacterClass.Vampiir:
					case eCharacterClass.Bainshee:
					case eCharacterClass.Mauler_Alb:
					case eCharacterClass.Mauler_Hib:
					case eCharacterClass.Mauler_Mid:
						{
							client.Player.Out.SendMessage("Your class cannot use /level command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;
						}
				}
			}

			if (client.Player.TargetObject is GameTrainer)
			{
				if (client.Player.CanUseSlashLevel)
				{
					//if there is a level 50.. calculate the xp needed to get to
					//level x from the current level and give it to the player

					// only do this if the players level is  < target level
					if (client.Player.Experience < GamePlayer.XPLevel[ServerProperties.Properties.SLASH_LEVEL_TARGET - 1])
					{
						long newXP;
						//calculate xp to level 20 from current level..
						newXP = GamePlayer.XPLevel[19] - client.Player.Experience;
						if (newXP < 0) newXP = 0;
						client.Player.GainExperience(newXP);
						client.Player.PlayerCharacter.UsedLevelCommand = true;
						client.Player.Out.SendMessage("You have been rewarded enough Experience to reach level " + ServerProperties.Properties.SLASH_LEVEL_TARGET + ", right click on your trainer to gain levels!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Player.SaveIntoDatabase();
						return 1;
					}
					client.Player.Out.SendMessage("/level only allows you to level to " + ServerProperties.Properties.SLASH_LEVEL_TARGET, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				client.Player.Out.SendMessage("You don't have a level " + ServerProperties.Properties.SLASH_LEVEL_REQUIREMENT + " on your account!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			client.Player.Out.SendMessage("You need to be at your trainer to use this command", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 0;
		}
	}
}
