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
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&safety",
		 ePrivLevel.Player,
		 "Turns off PvP safety flag.",
		 "/safety off")]
	public class SafetyCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.IsPvP == false)
				return;

			if(args.Length >= 2 && args[1].ToLower() == "off")
			{
				client.Player.SafetyFlag = false;
				DisplayMessage(client, "Your safety flag is now set to OFF!  You can now attack non allied players, as well as be attacked.");
			}
			else if(client.Player.SafetyFlag)
			{
				DisplayMessage(client, "The safety flag keeps your character from participating in combat");
				DisplayMessage(client, "with non allied players in designated zones when you are below level 10.");
				DisplayMessage(client, "Type /safety off to begin participating in PvP combat in these zones, though once it is off it can NOT be turned back on!");
			}
			else
			{
				DisplayMessage(client, "Your safety flag is already off.");
			}
		}
	}
}