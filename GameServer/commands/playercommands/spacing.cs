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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&spacing",
		ePrivLevel.Player,
		"Change the spacing of your pets!", "/spacing {normal, big, huge}")]
	public class SpacingHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "spacing"))
				return;

			GamePlayer player = client.Player;

			//No one else needs to use this spell
			if (player.CharacterClass.ID != (int)eCharacterClass.Bonedancer)
			{
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.BonedancerOnly"));
				return;
			}

			//Help display
			if (args.Length == 1)
			{
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.Info1"));
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.Info2"));
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.Info3"));
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.Info4"));
				return;
			}

			//Check to see if the BD has a commander and minions
			if (player.ControlledBrain == null)
			{
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.NoCommander"));
				return;
			}
			bool haveminion = false;
			foreach (AI.Brain.IControlledBrain icb in player.ControlledBrain.Body.ControlledNpcList)
			{
				if (icb != null)
					haveminion = true;
			}
			if (!haveminion)
			{
				DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.NoMinions"));
				return;
			}

			switch (args[1].ToLower())
			{
				//Triangle Formation
				case "normal":
					player.ControlledBrain.Body.FormationSpacing = 1;
					break;
				//Line formation
				case "big":
					player.ControlledBrain.Body.FormationSpacing = 2;
					break;
				//Protect formation
				case "huge":
					player.ControlledBrain.Body.FormationSpacing = 3;
					break;
				default:
					DisplayMessage(player, LanguageMgr.GetTranslation(client, "Scripts.Players.Spacing.Unrecognized", args[1]));
					break;
			}
		}
	}
}