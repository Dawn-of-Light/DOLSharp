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
/***[ random.cs ]****
* reqired DOL:	1.5.0
* author|date:	SEpHirOTH |	2004/02/25
* modificatio:  SmallHorse (just a little cleanup)
* modificatio:  noret (made it look like on live servers)
* description: enables the usage of "/random" command
*		"/random <n>" get a random number between 1 and <n>
*		results will be send to all players in emote range
******************/

using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&random",
		ePrivLevel.Player,
		"prints out a random number between 1 and the number specified.",
		"/random [#] to get a random number between 1 and the number you specified.")]
	public class RandomCommandHandler : ICommandHandler
	{
		// declaring some msg's
		private const int RESULT_RANGE = 512; // emote range
		private const string MESSAGE_HELP = "You must select a maximum number for your random selection!";
		private const string MESSAGE_RESULT_SELF = "You pick a random number between 1 and {0}: {1}"; // thrownMax, thrown
		private const string MESSAGE_RESULT_OTHER = "{0} picks a random number between 1 and {1}: {2}"; // client.Player.Name, thrownMax, thrown
		private const string MESSAGE_LOW_NUMBER = "You must select a maximum number greater than 1!";


		public int OnCommand(GameClient client, string[] args)
		{
			// no args - display usage
			if (args.Length < 2)
			{
				SystemMessage(client, MESSAGE_HELP);
				return 0;
			}

			int thrownMax;

			// trying to convert number
			try
			{
				thrownMax = System.Convert.ToInt32(args[1]);
			}
			catch (OverflowException)
			{
				thrownMax = int.MaxValue - 1; // max+1 is used in GameObject.Random(int,int)
			}
			catch (Exception)
			{
				SystemMessage(client, MESSAGE_HELP);
				return 0;
			}

			if (thrownMax < 2)
			{
				SystemMessage(client, MESSAGE_LOW_NUMBER);
				return 0;
			}

			// throw result
			int thrown = Util.Random(1, thrownMax);

			// building result messages
			string selfMessage = String.Format(MESSAGE_RESULT_SELF, thrownMax, thrown);
			string otherMessage = String.Format(MESSAGE_RESULT_OTHER, client.Player.Name, thrownMax, thrown);

			// sending msg to player
			EmoteMessage(client, selfMessage);

			// sending result & playername to all players in range
			foreach (GamePlayer player in client.Player.GetPlayersInRadius(RESULT_RANGE))
				if (client.Player != player) // client gets unique message
					EmoteMessage(player, otherMessage); // sending msg to other players

			return 1;
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