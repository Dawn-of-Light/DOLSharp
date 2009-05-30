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
using System.Collections.Generic;

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
		public void OnCommand(GameClient client, string[] args)
		{
			ePrivLevel privilegeLevel = (ePrivLevel)client.Account.PrivLevel;
			bool isCommand = true;

			if (args.Length > 1)
			{
				try
				{
					privilegeLevel = (ePrivLevel)Convert.ToUInt32(args[1]);
				}
				catch (Exception)
				{
					isCommand = false;
				}
			}

			if (isCommand)
			{
                String[] commandList = GetCommandList(privilegeLevel);
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Cmdhelp.PlvlCommands", privilegeLevel.ToString()));

                foreach (String command in commandList)
					DisplayMessage(client, command);
			}
			else
			{
				string command = args[1];

				if (command[0] != '&')
					command = "&" + command;

				ScriptMgr.GameCommand gameCommand = ScriptMgr.GetCommand(command);

				if (gameCommand == null)
                    DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Cmdhelp.NoCommand", command));
                else
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Cmdhelp.Usage", command));

					foreach (String usage in gameCommand.Usage)
						DisplayMessage(client, usage);
				}
			}
		}

        private static IDictionary<ePrivLevel, String[]> m_commandLists = new Dictionary<ePrivLevel, String[]>();
        private static object m_syncObject = new object();

        private String[] GetCommandList(ePrivLevel privilegeLevel)
        {
            lock (m_syncObject)
            {
                if (!m_commandLists.Keys.Contains(privilegeLevel))
                {
                    String[] commandList = ScriptMgr.GetCommandList(privilegeLevel, true);
                    Array.Sort(commandList);
                    m_commandLists.Add(privilegeLevel, commandList);
                }

                return m_commandLists[privilegeLevel];
            }
        }
    }
}