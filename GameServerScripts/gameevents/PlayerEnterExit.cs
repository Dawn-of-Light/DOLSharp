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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Spams everyone online with player enter/exit messages
	/// </summary>
	public class PlayerEnterExit
	{
		/// <summary>
		/// Event handler fired when server is started
		/// </summary>
		[GameServerStartedEvent]
		public static void OnServerStart(DOLEvent e, object sender, EventArgs arguments)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
			GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
		}

		/// <summary>
		/// Event handler fired when server is stopped
		/// </summary>
		[GameServerStoppedEvent]
		public static void OnServerStop(DOLEvent e, object sender, EventArgs arguments)
		{
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
			GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
		}

		/// <summary>
		/// Event handler fired when players enters the game
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private static void PlayerEntered(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;
			if (player.IsAnonymous) return;

			string message = LanguageMgr.GetTranslation(player.Client, "Scripts.Events.PlayerEnterExit.Entered", player.Name);
			if (player.Client.Account.PrivLevel > 1)
				message = LanguageMgr.GetTranslation(player.Client, "Scripts.Events.PlayerEnterExit.Staff", message);
			else
			{
				string realm = "";
				if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
				{
					realm = "[";
					switch (player.Realm)
					{
						case 1: realm = realm + "Albion"; break;
						case 2: realm = realm + "Midgard"; break;
						case 3: realm = realm + "Hibernia"; break;
					}
					realm = realm + "] ";
				}
				message = realm + message;
			}
			foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
			{
				if (player.Client != pclient)
					pclient.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Event handler fired when player leaves the game
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private static void PlayerQuit(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;
			if (player.IsAnonymous) return;

			string message = LanguageMgr.GetTranslation(player.Client, "Scripts.Events.PlayerEnterExit.Left", player.Name);
			if (player.Client.Account.PrivLevel > 1)
				message = LanguageMgr.GetTranslation(player.Client, "Scripts.Events.PlayerEnterExit.Staff", message);
			else
			{
				string realm = "";
				if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
				{
					realm = "[";
					switch (player.Realm)
					{
						case 1: realm = realm + "Albion"; break;
						case 2: realm = realm + "Midgard"; break;
						case 3: realm = realm + "Hibernia"; break;
					}
					realm = realm + "] ";
				}
				message = realm + message;
				foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
				{
					if (player.Client != pclient)
						pclient.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}
	}
}