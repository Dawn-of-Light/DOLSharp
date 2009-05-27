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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute("&cmdhelp", //command to handle
		ePrivLevel.Player, //minimum privelege level
		"Displays available commands", //command description
		//usage
		"'/cmdhelp' displays a list of all the commands and their descriptions",
		"'/cmdhelp <plvl>' displays a list of all commands that require at least plvl",
		"'/cmdhelp <cmd>' displays the usage for cmd")]
	public class CmdHelpCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static string[] cmds; 
		public void OnCommand(GameClient client, string[] args)
		{
			ePrivLevel plvl = (ePrivLevel)client.Account.PrivLevel;
			bool iscmd = true;

			if (args.Length > 1)
			{
				try
				{
					plvl = (ePrivLevel)Convert.ToUInt32(args[1]);
				}
				catch (Exception)
				{
					iscmd = false;
				}
			}

			if (iscmd)
			{
				if (cmds == null)
				{
					cmds = ScriptMgr.GetCommandList(plvl, true);
					Array.Sort(cmds);
				}
				
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Cmdhelp.PlvlCommands", plvl.ToString()));

				foreach (string s in cmds)
				{
					DisplayMessage(client, s);
				}
			}
			else
			{
				string arg = args[1];

				if (arg[0] != '&')
				{
					arg = "&" + arg;
				}

				ScriptMgr.GameCommand cmd = ScriptMgr.GetCommand(arg);

				if (cmd != null)
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Cmdhelp.Usage", arg));

					foreach (string s in cmd.m_usage)
					{
						DisplayMessage(client, s);
					}
				}
				else
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Cmdhelp.NoCommand", arg));
				}
			}
		}
	}
}