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

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&blink",
		ePrivLevel.GM,
		"Makes the specified UI Part of your target or yourself blinking.",
		"/blink <id>: type /blink for a list of possible IDs")]
	public class BlinkCommandHandler : ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			GamePlayer player = client.Player;
            bool sendBlinkPanel = false;

			// If an argument is given (an Int value is expected)
			if (args.Length > 1)
			{
				// The value that is given to us
				byte value;
				// Try to parse the Int value to byte and put the result in "value"
                if (byte.TryParse(args[1].ToLower(), out value))
                {
					// Try to find the value in ePanel Enumerator
                    if (Enum.IsDefined(typeof(ePanel), value))
                    {
						// Give the user some information
                        client.Out.SendMessage("Start blinking UI part: " + Enum.GetName(typeof(ePanel), value), eChatType.CT_System, eChatLoc.CL_SystemWindow);

						// If we have a target, send the blink panel to him or make our own UI blink otherwise
                        if (player.TargetObject != null && player.TargetObject is GamePlayer && (player.TargetObject as GamePlayer).Client.IsPlaying)
                            (player.TargetObject as GamePlayer).Out.SendBlinkPanel(value);
                        else
                            player.Out.SendBlinkPanel(value);

						// Send blink panel successfull
                        sendBlinkPanel = true;
                    }
                }
			}

			// If an error occured, say the user what to do
			if (sendBlinkPanel == false)
			{
				Usage(client);
			}
		}


		/// <summary>
		/// Tell the user how to use this command
		/// </summary>
		private void Usage(GameClient client)
		{
			// Create a new string and add some Info to it
			String visualEffectList = "";

			visualEffectList += "You must specify a value!\nID: Name\n";

			int count = 0;

			// For each Item in ePanel, write the current ID and the Name to our string
			foreach (string panelID in Enum.GetNames(typeof(ePanel)))
			{
				visualEffectList += count + ": " + panelID + "\n";
				count++;
			}

			// Give the user some usefull output
			client.Out.SendMessage(visualEffectList, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}