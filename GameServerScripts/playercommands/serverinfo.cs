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
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&serverinfo", //command to handle
		(uint) ePrivLevel.Player, //minimum privelege level
		"Shows information about the server", //command description
		"/serverinfo")] //usage
		public class ServerInfoCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			client.Out.SendMessage(GameServer.Instance.Configuration.ServerName, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			AssemblyName an = Assembly.GetAssembly(typeof (GameServer)).GetName();
			client.Out.SendMessage("version: " + an.Version, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("type: " + GameServer.Instance.Configuration.ServerType + " (" + GameServer.ServerRules.RulesDescription() + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("playing: " + WorldMgr.GetAllPlayingClients().Count, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			long sec = client.Player.Region.Time/1000;
			long min = sec/60;
			long hours = min/60;
			long days = hours/24;
			client.Out.SendMessage(string.Format("uptime: {0}d {1}h {2}m {3:00}s", days, hours%24, min%60, sec%60), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}
	}
}