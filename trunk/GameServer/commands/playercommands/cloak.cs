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


namespace DOL.GS.Commands
{
	[CmdAttribute("&cloak", //command to handle
		ePrivLevel.Player, //minimum privelege level
	   "Show / hide your cloak.", //command description
	   "Usage: /cloak [on|off].", //usage
	   "Example: \"/cloak off\" to hide your cloak")] 
	public class CloakCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/* version 1.98 :
		 * [19:37:34] Usage: /cloak [on|off].
		 * [19:37:34] Example: "/cloak off" to hide your cloak
		 * [19:37:36] Your cloak will no longer be hidden from view.
		 * [19:37:39] Your cloak will now be hidden from view.
		 * */
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "cloak"))
				return;

			if (args.Length != 2)
			{
				DisplaySyntax(client);
				return;
			}
			string onOff = args[1].ToLower();
			if (onOff == "on")
			{
				if(client.Player.IsCloakInvisible)
				{
					client.Player.IsCloakInvisible = false;
					client.Out.SendMessage("Your cloak will no longer be hidden from view.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
					return;
				}
				else
				{
					client.Out.SendMessage("Your cloak is already visible.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
					return;
				}
			}
			
			if (onOff == "off")
			{
				if (client.Player.IsCloakInvisible)
				{
					client.Out.SendMessage("Your cloak is already invisible.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
					return;
				}
				else
				{
					client.Player.IsCloakInvisible = true;
					client.Out.SendMessage("Your cloak will now be hidden from view.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
					return;
				}
			}
			DisplaySyntax(client);
		}
	}
}
