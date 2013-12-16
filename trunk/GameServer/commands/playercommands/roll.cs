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
/***[ roll.cs ]****
* reqired DOL:	1.5.0
* author|date:	SEpHirOTH |	2004/02/25
* modificatio:  SmallHorse (just a little cleanup)
* modificatio:  noret (made it look like on live servers)
* description: enables the usage of "/roll" command
*		"/roll <n>" rolling with <n> dice
*		results will be sent to all players in emote range
******************/

using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&roll",
		ePrivLevel.Player,
		"simulates a dice roll.",
		"/roll [#] to throw with a specified number of dice")]
	public class RollCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		// declaring some msg's
		private const int RESULT_RANGE = 512; // emote range
		private const int MAX_DICE = 100;
		private const int ONE_DIE_MAX_VALUE = 6; // :)
		private const string MESSAGE_HELP = "You must select the number of dice to roll!";
		private const string MESSAGE_RESULT_SELF = "You roll {0} dice and come up with: {1}"; // dice, thrown
		private const string MESSAGE_RESULT_OTHER = "{0} rolls {1} dice and comes up with: {2}"; // client.Player.Name, dice, thrown
		private readonly string MESSAGE_WRONG_NUMBER = "You must number of dice between 1 and " + MAX_DICE + "!";

		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "roll", 500))
			{
				DisplayMessage(client, "Slow down!");
				return;
			}

			// no args - display usage
			if (args.Length < 2)
			{
				SystemMessage(client, MESSAGE_HELP);
				return;
			}


			int dice; // number of dice to roll

			// trying to convert number
			try
			{
				dice = System.Convert.ToInt32(args[1]);
			}
			catch (OverflowException)
			{
				SystemMessage(client, MESSAGE_WRONG_NUMBER);
				return;
			}
			catch (Exception)
			{
				SystemMessage(client, MESSAGE_HELP);
				return;
			}

			if (dice < 1 || dice > MAX_DICE)
			{
				SystemMessage(client, MESSAGE_WRONG_NUMBER);
				return;
			}

			// throw result
			int thrown = Util.Random(dice, dice * ONE_DIE_MAX_VALUE);

			// building roll result msg
			string selfMessage = String.Format(MESSAGE_RESULT_SELF, dice, thrown);
			string otherMessage = String.Format(MESSAGE_RESULT_OTHER, client.Player.Name, dice, thrown);

			// sending msg to player
			EmoteMessage(client, selfMessage);

			// sending result & playername to all players in range
			foreach (GamePlayer player in client.Player.GetPlayersInRadius(RESULT_RANGE))
			{
				if (client.Player != player) // client gets unique message
					EmoteMessage(player, otherMessage); // sending msg to other players
			}
		}

		// these are to make code look better
		private void SystemMessage(GameClient client, string str)
		{
			client.Out.SendMessage(str, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void EmoteMessage(GamePlayer player, string str)
		{
			EmoteMessage(player.Client, str);
		}

		private void EmoteMessage(GameClient client, string str)
		{
			client.Out.SendMessage(str, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
		}
	}
}