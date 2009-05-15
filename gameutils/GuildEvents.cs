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
using System.Collections;
using System.Threading;
using DOL.Events;
using DOL.GS;
using log4net;
using DOL.GS.PacketHandler;

namespace DOL.Regiment
{
	public class SocialPlayerEvent : GamePlayerEvent
	{
		/// <summary>
		/// Constructs a new GamePlayer Event
		/// </summary>
		/// <param name="name">the event name</param>
		protected SocialPlayerEvent(string name) : base(name) { }
	}

	public class SocialEventHandler
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region guildbuffdeclaration
		/// <summary>
		/// Bonus to Realm Points
		/// </summary>
		public static readonly double BUFF_RPS = 5.0; // 5%

		/// <summary>
		/// Bonus to Bounty Points
		/// </summary>
		public static readonly double BUFF_BPS = 5.0; // 5%

		/// <summary>
		/// Time Interval to check for expired guild buffs
		/// </summary>
		public static readonly int BUFFCHECK_INTERVAL = 60 * 1000; // 1 Minute

		/// <summary>
		/// Static Timer for the Timer to check for expired guild buffs
		/// </summary>
		private static Timer m_timer;
		#endregion

		#region ScriptLoadedEvent
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.NextCraftingTierReached, new DOLEventHandler(OnNextCraftingTierReached));
			// Realm Points Check
			GameEventMgr.AddHandler(GamePlayerEvent.GainedRealmPoints, new DOLEventHandler(RealmPointsGain));
			// Bounty Points Check
			GameEventMgr.AddHandler(GamePlayerEvent.GainedBountyPoints, new DOLEventHandler(BountyPointsGain));
			// Realm Rank Check
			GameEventMgr.AddHandler(GamePlayerEvent.RRLevelUp, new DOLEventHandler(RealmRankUp));
			GameEventMgr.AddHandler(GamePlayerEvent.RLLevelUp, new DOLEventHandler(RealmRankUp));
			// Guild Buff Check
			m_timer = new Timer(new TimerCallback(StartCheck), m_timer, 0, BUFFCHECK_INTERVAL);
		}
		#endregion

		#region ScriptUnloadedEvent
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(GamePlayerEvent.NextCraftingTierReached, new DOLEventHandler(OnNextCraftingTierReached));
			// Realm Points Check
			GameEventMgr.RemoveHandler(GamePlayerEvent.GainedRealmPoints, new DOLEventHandler(RealmPointsGain));
			// Bounty Points Check
			GameEventMgr.RemoveHandler(GamePlayerEvent.GainedBountyPoints, new DOLEventHandler(BountyPointsGain));
			// Realm Rank Check/Announce
			GameEventMgr.RemoveHandler(GamePlayerEvent.RRLevelUp, new DOLEventHandler(RealmRankUp));
			GameEventMgr.RemoveHandler(GamePlayerEvent.RLLevelUp, new DOLEventHandler(RealmRankUp));
			// Guild Buff Check
			if (m_timer != null)
			{
				m_timer.Dispose();
				m_timer = null;
			}
		}
		#endregion

		public static void OnNextCraftingTierReached(DOLEvent e, object sender, EventArgs args)
		{
			NextCraftingTierReachedEventArgs cea = args as NextCraftingTierReachedEventArgs;
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;
			if (!player.IsEligibleToGiveMeritPoints)
			{
				return;
			}
			// skill  700 - 100 merit points
			// skill  800 - 200 merit points
			// skill  900 - 300 merit points
			// skill 1000 - 400 merit points
			if (cea.Points <= 1000 && cea.Points >= 700)
			{
				int meritpoints = cea.Points - 600;
				player.Guild.GainMeritPoints(meritpoints);
				player.Out.SendMessage("You have earned "+meritpoints+" merit points for your guild!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}


		}

		


		#region RealmRankUp
		public static void RealmRankUp(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null) 
				return;

			if (!player.IsEligibleToGiveMeritPoints)
			{
				return;
			}

			GainedRealmPointsEventArgs rpsArgs = args as GainedRealmPointsEventArgs;

			if (player.RealmLevel % 10 == 0)
			{
				int newRR = 0;
				newRR = ((player.RealmLevel / 10) + 1);
				if (player.Guild != null && player.RealmLevel > 45)
				{
					int a = (int)Math.Pow((3 * (newRR - 1)), 2);
					player.Guild.GainMeritPoints(a);
					player.Out.SendMessage("Your guild is awarded " + (int)Math.Pow((3 * (newRR - 1)), 2) + " Merit Points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}
			else if (player.RealmLevel > 60)
			{
				int RRHigh = ((int)Math.Floor(player.RealmLevel * 0.1) + 1);
				int RRLow = (player.RealmLevel % 10);
				if (player.Guild != null)
				{
					int a = (int)Math.Pow((3 * (RRHigh - 1)), 2);
					player.Guild.GainMeritPoints(a);
					player.Out.SendMessage("Your guild is awarded " + (int)Math.Pow((3 * (RRHigh - 1)), 2) + " Merit Points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				if (player.RealmLevel > 10)
				{
					if (player.Guild != null)
					{
						int RRHigh = ((int)Math.Floor(player.RealmLevel * 0.1) + 1);
						int a = (int)Math.Pow((3 * (RRHigh - 1)), 2);
						player.Guild.GainMeritPoints(a);
						player.Out.SendMessage("Your guild is awarded " + (int)Math.Pow((3 * (RRHigh - 1)), 2) + " Merit Points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}
			}

			if (player.Guild != null)
				player.Guild.UpdateGuildWindow();
		}
		#endregion

		#region RealmPointsGain

		public static void RealmPointsGain(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.Guild == null) return;

			GainedRealmPointsEventArgs rpsArgs = args as GainedRealmPointsEventArgs;

			if (player.Guild != null)
			{
				long OldRealmPoints = player.Guild.RealmPoints;
				long NewRealmPoints = rpsArgs.RealmPoints;

				#region Buff Bonus to RPS
				if (player.Guild.BuffType == 1)
				{
					player.RealmPoints += (long)Math.Ceiling(rpsArgs.RealmPoints * BUFF_RPS / 100);
					player.SaveIntoDatabase();
					player.Guild.RealmPoints += (long)Math.Ceiling(rpsArgs.RealmPoints * BUFF_RPS / 100);
					player.Out.SendMessage("You get an additional " + (long)Math.Ceiling(rpsArgs.RealmPoints * BUFF_RPS / 100) + " Realm Points due to your Guild's Buff!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
				#endregion

				long GuildLevel = player.Guild.GuildLevel;

				if ((OldRealmPoints < 100000000) && (player.Guild.RealmPoints > 100000000))
				{
					// Report to Newsmgr
					string message = player.Guild.Name + " [" + GlobalConstants.RealmToName((eRealm)player.Realm) + "] has reached 100,000,000 Realm Points!";
					NewsMgr.CreateNews(message, player.Realm, eNewsType.RvRGlobal, false);
				}
				player.Guild.UpdateGuildWindow();
			}
		}

		#endregion

		#region Bounty Points Gained

		public static void BountyPointsGain(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.Guild == null) return;

			GainedBountyPointsEventArgs bpsArgs = args as GainedBountyPointsEventArgs;

			#region Buff Bonus to BPS
			if (player.Guild != null)
			{
				if (player.Guild.BuffType == 2)
				{
					player.BountyPoints += (long)Math.Ceiling(bpsArgs.BountyPoints * BUFF_BPS / 100);
					player.SaveIntoDatabase();
					player.Guild.BountyPoints += (long)Math.Ceiling(bpsArgs.BountyPoints * BUFF_BPS / 100);
					player.Out.SendMessage("You get an additional " + (long)Math.Ceiling(bpsArgs.BountyPoints * BUFF_BPS / 100) + " Bounty Points due to your Guild's Buff!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}
			#endregion

			if (player.Guild != null)
				player.Guild.UpdateGuildWindow();
		}

		#endregion

		#region Guild Buff GameTimer Check

		public static void StartCheck(object timer)
		{
			Thread th = new Thread(new ThreadStart(StartCheckThread));
			th.Start();
		}

		public static void StartCheckThread()
		{
			//log.Warn("Starting BuffTimer Check...");

			foreach (Guild checkGuild in GuildMgr.ListGuild())
			{
				if (checkGuild.BuffType != 0)
				{
					TimeSpan buffDate = DateTime.Now.Subtract(checkGuild.BuffTime);

					//int TimeLeft;
					if (buffDate.Days > 0)
					{
						checkGuild.BuffType = 0;

						checkGuild.SaveIntoDatabase();

						string message1 = "[Guild Buff] Your Guild Buff has now worn off!";
						foreach (GamePlayer player in checkGuild.ListOnlineMembers())
						{
							player.Out.SendMessage(message1, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
						}
					}
				}
			}
		}

		#endregion
	}
	
}
