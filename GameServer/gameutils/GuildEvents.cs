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
			GameEventMgr.AddHandler(GamePlayerEvent.ReachedAHundredthCraftingSkill, new DOLEventHandler(OnReachedAHundredthCraftingSkill));
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
			GameEventMgr.RemoveHandler(GamePlayerEvent.ReachedAHundredthCraftingSkill, new DOLEventHandler(OnReachedAHundredthCraftingSkill));
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


		/// <summary>
		/// Holds realm points needed for special guild level
		/// </summary>
		public static readonly long[] REALMPOINTS_FOR_GUILDLEVEL =
			{
                #region guildrps
				0,	        // for level 0
				10,	        // for level 1
				250,	    // for level 2
				1250,	    // for level 3
				3500,	    // for level 4
				7500,	    // for level 5
				13750,	    // for level 6
				22750,	    // for level 7
				35000,	    // for level 8
				51000,	    // for level 9
				71250,	    // for level 10
				96250,	    // for level 11
				126500,	    // for level 12
				162500,	    // for level 13
				204750,	    // for level 14
				253750,	    // for level 15
				310000,	    // for level 16
				374000,	    // for level 17
				446250,	    // for level 18
				527250,	    // for level 19
				617500,	    // for level 20
				717500,	    // for level 21
				827750,	    // for level 22
				948750,	    // for level 23
				1081000,	// for level 24
				1225000,	// for level 25
				1381250,	// for level 26
				1550250,	// for level 27
				1732500,	// for level 28
				1928500,	// for level 29
				2138750,	// for level 30
				2363750,	// for level 31
				2604000,	// for level 32
				2860000,	// for level 33
				3132250,	// for level 34
				3421250,	// for level 35
				3727500,	// for level 36
				4051500,	// for level 37
				4393750,	// for level 38
				4754750,	// for level 39
				5135000,	// for level 40
				5535000,	// for level 41
				5955250,	// for level 42
				6396250,	// for level 43
				6858500,	// for level 44
				7342500,	// for level 45
				7848750,	// for level 46
				8377750,	// for level 47
				8930000,	// for level 48
				9506000,	// for level 49
				10106250,	// for level 50
				10731250,	// for level 51
				11381500,	// for level 52
				12057500,	// for level 53
				12759750,	// for level 54
				13488750,	// for level 55
				14245000,	// for level 56
				15029000,	// for level 57
				15841250,	// for level 58
				16682250,	// for level 59
				17552500,	// for level 60
				18452500,	// for level 61
				19382750,	// for level 62
				20343750,	// for level 63
				21336000,	// for level 64
				22360000,	// for level 65
				23416250,	// for level 66
				24505250,	// for level 67
				25627500,	// for level 68
				26783500,	// for level 69
				27973750,	// for level 70
				29198750,	// for level 71
				30459000,	// for level 72
				31755000,	// for level 73
				33087250,	// for level 74
				34456250,	// for level 75
				35862500,	// for level 76
				37306500,	// for level 77
				38788750,	// for level 78
				40309750,	// for level 79
				41870000,	// for level 80
				43470000,	// for level 81
				45110250,	// for level 82
				46791250,	// for level 83
				48513500,	// for level 84
				50277500,	// for level 85
				52083750,	// for level 86
				53932750,	// for level 87
				55825000,	// for level 88
				57761000,	// for level 89
				59741250,	// for level 90
				61766250,	// for level 91
				63836500,	// for level 92
				65952500,	// for level 93
				68114750,	// for level 94
				70323750,	// for level 95
				72580000,	// for level 96
				74884000,	// for level 97
				77236250,	// for level 98
				79637250,	// for level 99
				82087500,	// for level 100
				91117130,	// for level 101
				101140010, 	// for level 102
				112265410, 	// for level 103
				124614600,	// for level 104
				138322210, 	// for level 105
				153537650,	// for level 106
				170426800,	// for level 107
				189173740, 	// for level 108
				209982860,	// for level 109
				233080970,	// for level 110
                258719880,  // for level 111
                287179060,  // for level 112
                318768760,  // for level 113
                353833330,  // for level 114
                392754990,  // for level 115
                435958040,  // for level 116
                483913430,  // for level 117
                537143900,  // for level 118
                596229730,  // for level 119
                661815010   // for level 120
                #endregion
			};

		public static void OnReachedAHundredthCraftingSkill(DOLEvent e, object sender, EventArgs args)
		{
			ReachedAHundredthCraftingSkillEventArgs cea = args as ReachedAHundredthCraftingSkillEventArgs;
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
				GameServer.Database.SaveObject(player.Guild.theGuildDB);
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
					GameServer.Database.SaveObject(player.Guild.theGuildDB);
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
					GameServer.Database.SaveObject(player.Guild.theGuildDB);
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
						GameServer.Database.SaveObject(player.Guild.theGuildDB);
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
				long OldRealmPoints = player.Guild.theGuildDB.RealmPoints;
				long NewRealmPoints = rpsArgs.RealmPoints;

				#region Buff Bonus to RPS
				if (player.Guild.theGuildDB.BuffType == 1)
				{
					player.RealmPoints += (long)Math.Ceiling(rpsArgs.RealmPoints * BUFF_RPS / 100);
					player.SaveIntoDatabase();

					player.Guild.theGuildDB.RealmPoints += (long)Math.Ceiling(rpsArgs.RealmPoints * BUFF_RPS / 100);
					GameServer.Database.SaveObject(player.Guild.theGuildDB);

					player.Out.SendMessage("You get an additional " + (long)Math.Ceiling(rpsArgs.RealmPoints * BUFF_RPS / 100) + " Realm Points due to your Guilds Buff!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
				#endregion

				long GuildLevel = player.Guild.GuildLevel;

				if ((OldRealmPoints < 100000000) && (player.Guild.theGuildDB.RealmPoints > 100000000))
				{
					// Report to Newsmgr
					string message = player.Guild.theGuildDB.GuildName + " [" + GlobalConstants.RealmToName((eRealm)player.Realm) + "] has reached 100,000,000 Realm Points!";
					NewsMgr.CreateNews(message, player.Realm, eNewsType.RvRGlobal, false);
				}

				#region Guild Levels
				if (player.Guild.GuildLevel < 120)
				{
					if (player.Guild.RealmPoints >= REALMPOINTS_FOR_GUILDLEVEL[(GuildLevel + 1)])
					{
						player.Guild.GainGuildLevel(1);
						GameServer.Database.SaveObject(player.Guild.theGuildDB);

						string message1 = "[Guild] Your guild has reached Level " + player.Guild.GuildLevel + "!";
						foreach (GamePlayer ply in player.Guild.ListOnlineMembers())
						{
							ply.Out.SendMessage(message1, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
						}
					}
				}
				#endregion

				if (player.Guild != null)
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
				if (player.Guild.theGuildDB.BuffType == 2)
				{
					player.BountyPoints += (long)Math.Ceiling(bpsArgs.BountyPoints * BUFF_BPS / 100);
					player.SaveIntoDatabase();

					player.Guild.theGuildDB.BountyPoints += (long)Math.Ceiling(bpsArgs.BountyPoints * BUFF_BPS / 100);
					GameServer.Database.SaveObject(player.Guild.theGuildDB);

					player.Out.SendMessage("You get an additional " + (long)Math.Ceiling(bpsArgs.BountyPoints * BUFF_BPS / 100) + " Bounty Points due to your Guilds Buff!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
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
				if (checkGuild.theGuildDB.BuffType != 0)
				{
					TimeSpan buffDate = DateTime.Now.Subtract(checkGuild.theGuildDB.BuffTime);

					//int TimeLeft;
					if (buffDate.Days > 0)
					{
						checkGuild.theGuildDB.BuffType = 0;

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
