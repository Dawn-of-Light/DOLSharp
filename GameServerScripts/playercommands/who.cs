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
/*
TODO: listStart

/who  Can be modified with
[playername],
[class],
[#] level,
[location],
[##] [##] level range -
please note that /who CSR will not show hidden CSRs


1.69
- The /who command has been altered to show a [CG] and a [BG] next to players'
 names who are the leaders of a public chat group and battlegroup respectively.

- The /who command now allows multiple different search filters at once.
 For example, typing /who 40 50 Wizard Emain Dragonhearts would list all of the
 level 40 through 50 Wizards currently in Emain Macha with a guild that matches
 the "Dragonhearts" filter.

*/

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&who",
		(uint) ePrivLevel.Player,
		"Shows who is online",
		//help:
		//"/who  Can be modified with [playername], [class], [#] level, [location], [##] [##] level range",
		"/WHO ALL lists all players online",
		// "/WHO CSR lists all Customer Service Representatives currently online",
		// "/WHO DEV lists all Development Team Members currently online",
		// "/WHO QTA lists all Quest Team Assistants currently online",
		"/WHO <name> lists players with names that start with <name>",
		"/WHO <guild name> lists players with names that start with <guild name>",
		"/WHO <class> lists players with of class <class>",
		"/WHO <location> lists players in the <location> area",
		"/WHO <level> lists players of level <level>",
		"/WHO <level> <level> lists players in level range"
		)]
	public class WhoCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int MAX_LIST_SIZE = 26;
		private const string MESSAGE_LIST_TRUNCATED = "(Too many matches ({0}).  List truncated.)";
		private const string MESSAGE_NO_MATCHES = "No Matches.";
		private const string MESSAGE_NO_ARGS = "Type /WHO HELP for variations on the WHO command.";
		private const string MESSAGE_PLAYERS_ONLINE = "{0} player{1} currently online.";

		public int OnCommand(GameClient client, string[] args)
		{
			int listStart = 1;
			ArrayList filters = null;
			ArrayList clientsList = new ArrayList();
			ArrayList resultMessages = new ArrayList();

			// get list of clients depending on server type
			foreach (GameClient serverClient in WorldMgr.GetAllPlayingClients())
			{
				GamePlayer addPlayer = serverClient.Player;
                if (addPlayer == null) continue;
				if (serverClient.Account.PrivLevel > (int)ePrivLevel.Player && serverClient.Player.IsAnonymous == false)
				{
					clientsList.Add(addPlayer.Client);
					continue;
				}
				if (addPlayer.Client != client // allways add self
					&& client.Account.PrivLevel==(int)ePrivLevel.Player
					&& (addPlayer.IsAnonymous
					|| !GameServer.ServerRules.IsSameRealm(addPlayer, client.Player, true)))
					continue;
				clientsList.Add(addPlayer.Client);
			}

			// no params
			if (args.Length == 1)
			{
				int playing = clientsList.Count;

				// including anon?
				client.Out.SendMessage(string.Format(MESSAGE_PLAYERS_ONLINE, playing, playing > 1 ? "s" : ""), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(MESSAGE_NO_ARGS, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}


			// any params passed?
			switch (args[1].ToLower())
			{
				case "all": // display all players, no filter
					filters = null;
					break;

				case "gm":
				case "admin":
					filters = new ArrayList(1);
					filters.Add(new GMFilter());
					break;

				default:
					filters = new ArrayList();
					AddFilters(filters, args, 1);
					break;
			}


			int resultCount = 0;
			foreach (GameClient clients in clientsList)
			{
				if (ApplyFilter(filters, clients.Player))
				{
					resultCount++;
					if (resultMessages.Count < MAX_LIST_SIZE && resultCount >= listStart)
					{
						resultMessages.Add(resultCount + ") " + FormatLine(clients.Player, client.Account.PrivLevel));
					}
				}
			}

			foreach (string str in resultMessages)
			{
				client.Out.SendMessage(str, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			if (resultCount == 0)
			{
				client.Out.SendMessage(MESSAGE_NO_MATCHES, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else if (resultCount > MAX_LIST_SIZE)
			{
				client.Out.SendMessage(string.Format(MESSAGE_LIST_TRUNCATED, resultCount), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			filters = null;

			return 1;
		}


		// make /who line using GamePlayer
		private string FormatLine(GamePlayer player, uint PrivLevel)
		{
			/*
			 * /setwho class | trade
			 * Sets how the player wishes to be displayed on a /who inquery.
			 * Class displays the character's class and level.
			 * Trade displays the tradeskill type and level of the character.
			 * and it is saved after char logs out
			 */

			if (player == null)
			{
				if (log.IsErrorEnabled)
					log.Error("null player in who command");
				return "???";
			}

			StringBuilder result = new StringBuilder(player.Name, 100);
			if (player.GuildName != "")
			{
				result.Append(" <");
				result.Append(player.GuildName);
				result.Append(">");
			}

			// simle format for PvP
			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP && PrivLevel == 1)
				return result.ToString();

			result.Append(" the Level ");
			result.Append(player.Level);
			if (player.CharacterClass != null)
			{
				result.Append(" ");
				result.Append(player.CharacterClass.Name);
			}
			else
			{
				if (log.IsErrorEnabled)
					log.Error("no character class spec in who commandhandler for player " + player.Name);
			}
			if (player.CurrentZone != null)
			{
				result.Append(" in ");
				result.Append(player.CurrentZone.Description);
			}
			else
			{
				if (log.IsErrorEnabled)
					log.Error("no currentzone in who commandhandler for player " + player.Name);
			}
			ChatGroup mychatgroup = (ChatGroup) player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
			if (mychatgroup != null && ((bool) mychatgroup.Members[player]) == true)
			{
				result.Append(" [CG]");
			}
			if (player.IsAnonymous)
			{
				result.Append(" <ANON>");
			}
			if (player.TempProperties.getProperty(GamePlayer.AFK_MESSAGE, null) != null)
			{
				result.Append(" <AFK>");
			}
			if(player.Client.Account.PrivLevel == (int)ePrivLevel.GM)
			{
				result.Append(" <GM>");
			}
			if(player.Client.Account.PrivLevel == (int)ePrivLevel.Admin)
			{
				result.Append(" <Admin>");
			}

			return result.ToString();
		}


		private void AddFilters(ArrayList filters, string[] args, int skip)
		{
			for (int i = skip; i < args.Length; i++)
			{
				if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
					filters.Add(new StringFilter(args[i]));
				else
				{
					try
					{
						int currentNum = (int) System.Convert.ToUInt32(args[i]);
						int nextNum = -1;
						try
						{
							nextNum = (int) System.Convert.ToUInt32(args[i + 1]);
						}
						catch
						{
						}

						if (nextNum != -1)
						{
							filters.Add(new LevelRangeFilter(currentNum, nextNum));
							i++;
						}
						else
						{
							filters.Add(new LevelFilter(currentNum));
						}
					}
					catch
					{
						filters.Add(new StringFilter(args[i]));
					}
				}
			}
		}


		private bool ApplyFilter(ArrayList filters, GamePlayer player)
		{
			if (filters == null)
				return true;
			foreach (IWhoFilter filter in filters)
			{
				if (!filter.ApplyFilter(player))
					return false;
			}
			return true;
		}


		//Filters

		private class StringFilter : IWhoFilter
		{
			private string m_filterString;

			public StringFilter(string str)
			{
				m_filterString = str.ToLower();
			}

			public bool ApplyFilter(GamePlayer player)
			{
				if (player.Name.ToLower().StartsWith(m_filterString))
					return true;
				if (player.GuildName.ToLower().StartsWith(m_filterString))
					return true;
				if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
					return false;
				if (player.CharacterClass.Name.ToLower().StartsWith(m_filterString))
					return true;
				if (player.CurrentZone != null && player.CurrentZone.Description.ToLower().StartsWith(m_filterString))
					return true;
				return false;
			}
		}

		private class LevelRangeFilter : IWhoFilter
		{
			private int m_minLevel;
			private int m_maxLevel;

			public LevelRangeFilter(int minLevel, int maxLevel)
			{
				m_minLevel = Math.Min(minLevel, maxLevel);
				m_maxLevel = Math.Max(minLevel, maxLevel);
			}

			public bool ApplyFilter(GamePlayer player)
			{
				if (player.Level >= m_minLevel && player.Level <= m_maxLevel)
					return true;
				return false;
			}
		}

		private class LevelFilter : IWhoFilter
		{
			private int m_level;

			public LevelFilter(int level)
			{
				m_level = level;
			}

			public bool ApplyFilter(GamePlayer player)
			{
				if (player.Level != m_level)
					return false;
				return true;
			}
		}

		private class GMFilter : IWhoFilter
		{
			public bool ApplyFilter(GamePlayer player)
			{
				if(!player.IsAnonymous && player.Client.Account.PrivLevel > (int)ePrivLevel.Player)
					return true;
				return false;
			}
			public GMFilter() {}
		}

		private interface IWhoFilter
		{
			bool ApplyFilter(GamePlayer player);
		}
	}
}