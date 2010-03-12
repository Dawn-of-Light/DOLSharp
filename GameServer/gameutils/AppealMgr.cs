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
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Events;

namespace DOL.GS.Appeal
{
	public static class AppealMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static int m_CallbackFrequency = 5 * 60 * 1000; // How often appeal stat updates are sent out.
		private static volatile Timer m_timer = null;
		public enum eSeverity
		{
			Low = 1,
			Medium = 2,
			High = 3,
			Critical = 4
		}
		public static List<GamePlayer> StaffList = new List<GamePlayer>();
		public static int TotalAppeals;

		#region Initialisation/Unloading
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.DISABLE_APPEALSYSTEM)
			{

				//Register and load the DB.
				GameServer.Database.RegisterDataObject(typeof(DBAppeal));
				GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnter));
				GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
				GameEventMgr.AddHandler(GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerQuit));
				m_timer = new Timer(new TimerCallback(RunTask), m_timer, 0, m_CallbackFrequency);
			}
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.DISABLE_APPEALSYSTEM)
			{
				GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnter));
				GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
				GameEventMgr.RemoveHandler(GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerQuit));
				RunTask(null);
			}
		}
		#endregion

		#region Methods

		private static void RunTask(object state)
		{
			NotifyStaff();
			m_timer.Change(m_CallbackFrequency, Timeout.Infinite);
			return;
		}

		public static void NotifyStaff()
		{
			//here we keep the staff up to date on the status of the appeal queue, if there are open Appeals.
			List<DBAppeal> Appeals = new List<DBAppeal>();
			Appeals = GetAllAppeals();
			if (Appeals.Count < 1 || Appeals == null) { return; }
			int low = 0;
			int med = 0;
			int high = 0;
			int crit = 0;
			foreach (DBAppeal a in Appeals)
			{
				if (a == null) { return; }
				if (a.Severity < 1) { return; }
				switch (a.Severity)
				{
					case (int)AppealMgr.eSeverity.Low:
						low++;
						break;
					case (int)AppealMgr.eSeverity.Medium:
						med++;
						break;
					case (int)AppealMgr.eSeverity.High:
						high++;
						break;
					case (int)AppealMgr.eSeverity.Critical:
						crit++;
						break;
				}
			}
			//There are some Appeals to handle, let's send out an update to staff.
			if (Appeals.Count >= 2)
			{
				MessageToAllStaff("There are " + Appeals.Count + " Appeals in the queue.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				MessageToAllStaff("There are " + Appeals.Count + " Appeals in the queue.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}
			if (Appeals.Count == 1)
			{
				MessageToAllStaff("There is " + Appeals.Count + " appeal in the queue.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				MessageToAllStaff("There is " + Appeals.Count + " appeal in the queue.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}

			MessageToAllStaff("Crit:" + crit + ", High:" + high + ", Med:" + med + ", Low:" + low + ".  [use /gmappeal]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			MessageToAllStaff("Crit:" + crit + ", High:" + high + ", Med:" + med + ", Low:" + low + ".  [use /gmappeal]", eChatType.CT_Say, eChatLoc.CL_ChatWindow);

			if (crit >= 1)
			{
				MessageToAllStaff("Critical Appeals may need urgent attention!", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
				log.Warn("There is a critical appeal which may need urgent attention!");
			}

		}

		public static void MessageToAllStaff(string msg)
		{
			if (msg == null) { return; }

			foreach (GamePlayer staffplayer in StaffList)
			{
				MessageToClient(staffplayer.Client, msg);
			}
			return;
		}

		public static void MessageToClient(GameClient client, string msg)
		{
			if (msg == null) { return; }
			client.Player.Out.SendMessage("[Appeals]: " + msg, eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			return;
		}

		public static void MessageToAllStaff(string msg, eChatType chattype, eChatLoc chatloc)
		{
			if (msg == null) { return; }

			foreach (GamePlayer staffplayer in StaffList)
			{
				staffplayer.Out.SendMessage("[Appeals]: " + msg, chattype, chatloc);
			}
			return;
		}

		public static DBAppeal GetAppealByPlayerName(string name)
		{
			DBAppeal appeal = GameServer.Database.SelectObject<DBAppeal>("`Name` = '" + GameServer.Database.Escape(name) + "'");
			return appeal;
		}

		public static DBAppeal GetAppealByAccountName(string name)
		{
			DBAppeal appeal = GameServer.Database.SelectObject<DBAppeal>("`Account` = '" + GameServer.Database.Escape(name) + "'");
			return appeal;
		}

		/// <summary>
		/// Gets a combined list of Appeals for every player that is online.
		/// </summary>
		/// <returns></returns>
		public static List<DBAppeal> GetAllAppeals()
		{
			List<DBAppeal> rlist = new List<DBAppeal>();
			ArrayList clientlist = WorldMgr.GetAllPlayingClients();
			foreach (GameClient c in clientlist)
			{
				DBAppeal ap = GetAppealByPlayerName(c.Player.Name);
				if (ap != null)
				{
					rlist.Add(ap);
				}
			}
			TotalAppeals = rlist.Count;
			return rlist;
		}
		/// <summary>
		/// Gets a combined list of Appeals including player Appeals who are offline.
		/// </summary>
		/// <returns></returns>
		public static List<DBAppeal> GetAllAppealsOffline()
		{
			return(List<DBAppeal>)GameServer.Database.SelectAllObjects<DBAppeal>();
		}
		/// <summary>
		/// Creates a New Appeal
		/// </summary>
		/// <param name="Name"></param>The name of the Player who filed the appeal.
		/// <param name="Severity"></param>The Severity of the appeal (low, medium, high, critical)
		/// <param name="Status"></param>The status of the appeal (Open or InProgress)
		/// <param name="Text"></param>The text content of the appeal
		public static void CreateAppeal(GamePlayer Player, int Severity, string Status, string Text)
		{
			if (Player.IsMuted)
			{
				Player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(Player.Client, "Scripts.Players.Appeal.YouAreMuted"), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
				return;
			}
			bool HasPendingAppeal = Player.TempProperties.getProperty<bool>("HasPendingAppeal");
			if (HasPendingAppeal)
			{
				Player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(Player.Client, "Scripts.Players.Appeal.AlreadyActiveAppeal"), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
				return;
			}
			string eText = GameServer.Database.Escape(Text); //prevent SQL injection
			string TimeStamp = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString();
			DBAppeal appeal = new DBAppeal(Player.Name, Player.Client.Account.Name, Severity, Status, TimeStamp, eText);
			GameServer.Database.AddObject(appeal);
			Player.TempProperties.setProperty("HasPendingAppeal", true);
			Player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(Player.Client, "Scripts.Players.Appeal.AppealSubmitted"), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			Player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(Player.Client, "Scripts.Players.Appeal.IfYouLogOut"), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			Player.Out.SendPlaySound(eSoundType.Craft, 0x04);
			NotifyStaff();
			return;
		}

		/// <summary>
		/// Sets and saves the new status of the appeal
		/// </summary>
		/// <param name="name"></param>The name of the staff member making this change.
		/// <param name="appeal"></param>The appeal to change the status of.
		/// <param name="status">the new status (Open, Being Helped)</param>
		public static void ChangeStatus(string staffname, GamePlayer target, DBAppeal appeal, string status)
		{
			appeal.Status = status;
			appeal.Dirty = true;
			GameServer.Database.SaveObject(appeal);
			MessageToAllStaff("Staffmember " + staffname + " has changed the status of " + target.Name + "'s appeal to " + status + ".");
			target.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(target.Client, "Scripts.Players.Appeal.StaffChangedStatus", staffname, status), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			return;
		}

		/// <summary>
		/// Removes an appeal from the queue and deletes it from the db.
		/// </summary>
		/// <param name="name"></param>The name of the staff member making this change.
		/// <param name="appeal"></param>The appeal to remove.
		/// <param name="Player"></param>The Player whose appeal we are closing.
		public static void CloseAppeal(string staffname, GamePlayer Player, DBAppeal appeal)
		{
			MessageToAllStaff("[Appeals]: " + "Staffmember " + staffname + " has just closed " + Player.Name + "'s appeal.");
			Player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(Player.Client, "Scripts.Players.Appeal.StaffClosedYourAppeal", staffname), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			Player.Out.SendPlaySound(eSoundType.Craft, 0x02);
			GameServer.Database.DeleteObject(appeal);
			Player.TempProperties.setProperty("HasPendingAppeal", false);
			return;
		}
		/// <summary>
		/// Removes an appeal from an offline player and deletes it from the db.
		/// </summary>
		/// <param name="name"></param>The name of the staff member making this change.
		/// <param name="appeal"></param>The appeal to remove.
		public static void CloseAppeal(string staffname, DBAppeal appeal)
		{
			MessageToAllStaff("[Appeals]: " + "Staffmember " + staffname + " has just closed " + appeal.Name + "'s (offline) appeal.");
			GameServer.Database.DeleteObject(appeal);
			return;
		}

		public static void CancelAppeal(GamePlayer Player, DBAppeal appeal)
		{
			MessageToAllStaff("[Appeals]: " + Player.Name + " has canceled their appeal.");
			Player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(Player.Client, "Scripts.Players.Appeal.CanceledYourAppeal"), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			Player.Out.SendPlaySound(eSoundType.Craft, 0x02);
			GameServer.Database.DeleteObject(appeal);
			Player.TempProperties.setProperty("HasPendingAppeal", false);
			return;
		}

		#endregion

		#region Player enter

		public static void PlayerEnter(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null) { return; }
			if (player.Client.Account.PrivLevel > (uint)ePrivLevel.Player)
			{

				StaffList.Add(player);
				
				List<DBAppeal> Appeals = GetAllAppeals();
				if (Appeals.Count > 0 && Appeals != null)
				{
					player.Out.SendMessage("[Appeals]: " + "There are " + Appeals.Count + " Appeals in the queue!  Use /gmappeal to work the Appeals queue.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
				}
			}

			//Check if there is an existing appeal belonging to this player.
			DBAppeal appeal = GetAppealByAccountName(player.Client.Account.Name);

			if (appeal == null)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Appeal.LoginMessage"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (appeal.Name != player.Name)
			{
				//players account has an appeal but it dosn't belong to this player, let's change it.
				appeal.Name = player.Name;
				appeal.Dirty = true;
				GameServer.Database.SaveObject(appeal);
			}
			player.Out.SendMessage("[Appeals]: " + LanguageMgr.GetTranslation(player.Client, "Scripts.Players.Appeal.YouHavePendingAppeal"), eChatType.CT_Important, eChatLoc.CL_ChatWindow);
			player.TempProperties.setProperty("HasPendingAppeal", true);
			NotifyStaff();
		}

		#endregion

		#region Player quit
		public static void PlayerQuit(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;
			if (player.Client.Account.PrivLevel > (uint)ePrivLevel.Player)
			{
				StaffList.Remove(player);
			}
		}
		#endregion Player quit

	}
}