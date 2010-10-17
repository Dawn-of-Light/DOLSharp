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
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	public class PlayerStatistics : IPlayerStatistics
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int TIME_BETWEEN_UPDATES = 60000; // 1 minute

        private static bool m_hasBeenRun = false;
        private static long m_lastUpdatedTime = 0;
        private static long m_timeToChange = 0;

        private static IList<string> toplist = new List<string>();
        private static string statsrp;
        private static string statslrp;
        private static string statskills;
        private static string statsdeath;
        private static string statsirs;
        private static string statsheal;
        private static string statsres;

        public class StatToCount
        {
            public string name;
            public uint count;

            public StatToCount(string name, uint count)
            {
                this.name = name;
                this.count = count;
            }
        }

		protected GamePlayer m_player = null;
		protected uint m_TotalRP = 0;
		protected uint m_RealmPointsEarnedFromKills = 0;
		protected ushort m_KillsThatHaveEarnedRPs = 0;
		protected ushort m_Deathblows = 0;
		protected ushort m_Deaths = 0;
		protected uint m_HitpointsHealed = 0;
		protected ushort m_RessurectionsPerformed = 0;
		protected DateTime m_LoginTime;

		#region accessors

		public GamePlayer Player
		{
			get { return m_player; }
		}

		public uint TotalRP
		{
			get { return m_TotalRP; }
			set
			{
				m_TotalRP = value;
			}
		}

		public uint RealmPointsEarnedFromKills
		{
			get { return m_RealmPointsEarnedFromKills; }
			set
			{
				m_RealmPointsEarnedFromKills = value;
			}
		}

		public ushort KillsThatHaveEarnedRPs
		{
			get { return m_KillsThatHaveEarnedRPs; }
			set
			{
				m_KillsThatHaveEarnedRPs = value;
			}
		}

		public ushort Deathblows
		{
			get { return m_Deathblows; }
			set
			{
				m_Deathblows = value;
			}
		}

		public ushort Deaths
		{
			get { return m_Deaths; }
			set
			{
				m_Deaths = value;
			}
		}

		public uint HitPointsHealed
		{
			get { return m_HitpointsHealed; }
			set
			{
				m_HitpointsHealed = value;
			}
		}

		public ushort RessurectionsPerformed
		{
			get { return m_RessurectionsPerformed; }
			set
			{
				m_RessurectionsPerformed = value;
			}
		}

		public DateTime LoginTime
		{
			get { return m_LoginTime; }
		}
		#endregion

		public PlayerStatistics(GamePlayer player)
		{
            DOL.GS.GameEvents.PlayerStatisticsEvent.CheckHandlers();
			m_player = player;
			m_LoginTime = DateTime.Now;
		}


        public static void CreateServerStats(GameClient client)
        {
            GamePlayer player = client.Player;

            if (m_lastUpdatedTime == 0)
            {
                m_lastUpdatedTime = player.CurrentRegion.Time;
            }

            m_timeToChange = (m_lastUpdatedTime + TIME_BETWEEN_UPDATES);

            if (player.CurrentRegion.Time < m_timeToChange && m_hasBeenRun)
            {
                return;
            }

            #region /stats top
            var chars = GameServer.Database.SelectObjects<DOLCharacters>("RealmPoints > 213881 ORDER BY RealmPoints DESC LIMIT 100"); // assuming we can get at least 20 players
            if (toplist.Count > 0)
            {
                toplist.Clear();
            }
            int count = 1;
            foreach (DOLCharacters chr in chars)
            {
                if (chr.IgnoreStatistics == false)
                {
                    var account = GameServer.Database.SelectObject<Account>("Name = '" + chr.AccountName + "'");

                    if (account != null && account.PrivLevel == 1)
                    {
                        toplist.Add("\n" + count.ToString() + " - [ " + chr.Name + " ] with " + String.Format("{0:0,0}", chr.RealmPoints) + " RP - [ " + (((chr.RealmLevel + 10) / 10) + "L" + ((chr.RealmLevel + 10) % 10)) + " ]");
                        if (++count > 20)
                            break;
                    }
                }
            }

            if (count == 1)
            {
                toplist.Add("None found!");
            }
            #endregion /stats top

            List<StatToCount> allstatsrp = new List<StatToCount>();
            List<StatToCount> allstatslrp = new List<StatToCount>();
            List<StatToCount> allstatskills = new List<StatToCount>();
            List<StatToCount> allstatsdeath = new List<StatToCount>();
            List<StatToCount> allstatsirs = new List<StatToCount>();
            List<StatToCount> allstatsheal = new List<StatToCount>();
            List<StatToCount> allstatsres = new List<StatToCount>();

            foreach (GameClient c in WorldMgr.GetAllPlayingClients())
            {
                if (c == null || c.Account.PrivLevel != 1 || c.Player.DBCharacter.IgnoreStatistics)
                    continue;

                PlayerStatistics stats = player.Statistics as PlayerStatistics;
                if (stats != null)
                {
                    if (c.Player.RealmLevel > 31)
                    {
                        allstatsrp.Add(new StatToCount(c.Player.Name, stats.TotalRP));
                        TimeSpan onlineTime = DateTime.Now.Subtract(stats.LoginTime);
                        allstatslrp.Add(new StatToCount(c.Player.Name, (uint)Math.Round(stats.RPsPerHour(stats.TotalRP, onlineTime))));
                        uint deaths = stats.Deaths; if (deaths < 1) deaths = 1;
                        allstatsirs.Add(new StatToCount(c.Player.Name, (uint)Math.Round((double)stats.TotalRP / (double)deaths)));
                    }
                    allstatskills.Add(new StatToCount(c.Player.Name, stats.KillsThatHaveEarnedRPs));
                    allstatsdeath.Add(new StatToCount(c.Player.Name, stats.Deathblows));
                    allstatsheal.Add(new StatToCount(c.Player.Name, stats.HitPointsHealed));
                    allstatsres.Add(new StatToCount(c.Player.Name, stats.RessurectionsPerformed));
                }
            }
            allstatsrp.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsrp.Reverse();
            allstatslrp.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatslrp.Reverse();
            allstatskills.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatskills.Reverse();
            allstatsdeath.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsdeath.Reverse();
            allstatsirs.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsirs.Reverse();
            allstatsheal.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsheal.Reverse();
            allstatsres.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsres.Reverse();

            statsrp = ""; statslrp = ""; statskills = ""; statsdeath = ""; statsirs = ""; statsheal = ""; statsres = "";
            for (int c = 0; c < allstatsrp.Count; c++) { if (c > 19 || allstatsrp[c].count < 1) break; statsrp += (c + 1) + ". " + allstatsrp[c].name + " with " + allstatsrp[c].count.ToString() + " RP\n"; }
            for (int c = 0; c < allstatslrp.Count; c++) { if (c > 19 || allstatslrp[c].count < 1) break; statslrp += (c + 1) + ". " + allstatslrp[c].name + " with " + allstatslrp[c].count.ToString() + " RP/hour\n"; }
            for (int c = 0; c < allstatskills.Count; c++) { if (c > 19 || allstatskills[c].count < 1) break; statskills += (c + 1) + ". " + allstatskills[c].name + " with " + allstatskills[c].count.ToString() + " kills\n"; }
            for (int c = 0; c < allstatsdeath.Count; c++) { if (c > 19 || allstatsdeath[c].count < 1) break; statsdeath += (c + 1) + ". " + allstatsdeath[c].name + " with " + allstatsdeath[c].count.ToString() + " deathblows\n"; }
            for (int c = 0; c < allstatsirs.Count; c++) { if (c > 19 || allstatsirs[c].count < 1) break; statsirs += (c + 1) + ". " + allstatsirs[c].name + " with " + allstatsirs[c].count.ToString() + " RP/death\n"; }
            for (int c = 0; c < allstatsheal.Count; c++) { if (c > 19 || allstatsheal[c].count < 1) break; statsheal += (c + 1) + ". " + allstatsheal[c].name + " with " + allstatsheal[c].count.ToString() + " HP\n"; }
            for (int c = 0; c < allstatsres.Count; c++) { if (c > 19 || allstatsres[c].count < 1) break; statsres += (c + 1) + ". " + allstatsres[c].name + " with " + allstatsres[c].count.ToString() + " res\n"; }

            m_lastUpdatedTime = player.CurrentRegion.Time;
            m_hasBeenRun = true;
        }

        /// <summary>
        /// Return a formatted string showing a players stats
        /// </summary>
        /// <returns></returns>
        public virtual string GetStatisticsMessage()
        {
            TimeSpan onlineTime = DateTime.Now.Subtract(LoginTime);

            string stringOnlineTime = "Online time: " + onlineTime.Days + " days, " + onlineTime.Hours + " hours, " + onlineTime.Minutes + " minutes, " + onlineTime.Seconds + " seconds\n";
            if (onlineTime.Days < 1)
                stringOnlineTime = "Online time: " + onlineTime.Hours + " hours, " + onlineTime.Minutes + " minutes, " + onlineTime.Seconds + " seconds\n";

            // First line should include all the options available for the stats command
            string message = "Options: /stats [ top | rp | kills | deathblows | irs | heal | rez | player <name|target> ]\n" +
                                    "Statistics for " + Player.Name + " this Session:\n" +
                                    "Total RP: " + TotalRP + "\n" +
                                    "RP earned from kills: " + RealmPointsEarnedFromKills + "\n" +
                                    "Kills that have earned RP: " + KillsThatHaveEarnedRPs + "\n" +
                                    "Deathblows: " + Deathblows + "\n" +
                                    "Deaths: " + Deaths + "\n" +
                                    "HP healed: " + HitPointsHealed + "\n" +
                                    "Resurrections performed: " + RessurectionsPerformed + "\n" +
                                    stringOnlineTime +
                                    "RP/hour: " + RPsPerHour(TotalRP, onlineTime) + "\n" +
                                    "Kills per death: " + Divide((uint)KillsThatHaveEarnedRPs, (uint)Deaths) + "\n" +
                                    "RP per kill: " + Divide(RealmPointsEarnedFromKills, (uint)KillsThatHaveEarnedRPs) + "\n" +
                                    "\"I Remain Standing...\": " + Divide(RealmPointsEarnedFromKills, (uint)Deaths) + "\n";
            return message;
        }


        /// <summary>
        /// Get serverwide player statistics
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual void DisplayServerStatistics(GameClient client, string command, string playerName)
        {
            CreateServerStats(client);

            switch (command)
            {
                case "top":
                    client.Out.SendCustomTextWindow("Top 20 Players", toplist);
                    break;
                case "rp":
                    client.Player.Out.SendMessage("Top 20 for Realm Points\n" + statsrp, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "lrp":
                    client.Player.Out.SendMessage("Top 20 for RP / Hour\n" + statslrp, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "kills":
                    client.Player.Out.SendMessage("Top 20 Killers\n" + statskills, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "deathblows":
                    client.Player.Out.SendMessage("Top 20 Deathblows\n" + statsdeath, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "irs":
                    client.Player.Out.SendMessage("Top 20 \"I Remain Standing\"\n" + statsirs, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "heal":
                    client.Player.Out.SendMessage("Top 20 Healers\n" + statsheal, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "rez":
                    client.Player.Out.SendMessage("Top 20 Resurrectors\n" + statsres, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
                case "player":
                    GameClient clientc = WorldMgr.GetClientByPlayerName(playerName, false, true);
                    if (clientc == null)
                    {
                        client.Player.Out.SendMessage("No player with name '" + playerName + "' found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    if (clientc.Player.StatsAnonFlag)
                    {
                        client.Player.Out.SendMessage(playerName + " doesn't want you to view his stats.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    client.Player.Out.SendMessage(clientc.Player.Statistics.GetStatisticsMessage(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;

                default:
                    client.Player.Out.SendMessage("Options: /stats [ top | rp | kills | deathblows | irs | heal | rez | player <name|target> ]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
            }

        }


		public static uint Divide(uint dividend, uint divisor)
		{
			if (divisor == 0)
				return dividend;
			else if (dividend == 0)
				return 0;
			else
				return (dividend / divisor);
		}

		public float RPsPerHour(uint realmPoints, TimeSpan time)
		{
			if (realmPoints == 0)
				return 0f;

			float days = (float)time.Days;
			float hours = (float)time.Hours;
			float minutes = (float)time.Minutes;
			float seconds = (float)time.Seconds;

			return (float)realmPoints / (days * 24 + hours + minutes / 60 + seconds / (60 * 60));
		}
	}
}

namespace DOL.GS.GameEvents
{
	public class PlayerStatisticsEvent
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool m_handlersLoaded = false;

        // Load these when the first player logs in
        // Coded like this so they won't be loaded if the server uses custom statistics
        public static void CheckHandlers()
        {
            if (m_handlersLoaded == false)
            {
                m_handlersLoaded = true;
                GameEventMgr.AddHandler(GameLivingEvent.GainedRealmPoints, new DOLEventHandler(GainedRealmPointsCallback));
                GameEventMgr.AddHandler(GameLivingEvent.Dying, new DOLEventHandler(DyingCallback));
                GameEventMgr.AddHandler(GameLivingEvent.CastFinished, new DOLEventHandler(FinishCastSpellCallback));
                GameEventMgr.AddHandler(GameLivingEvent.HealthChanged, new DOLEventHandler(HealthChangedCallback));
            }
        }

		public static void GainedRealmPointsCallback(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			GainedRealmPointsEventArgs gargs = args as GainedRealmPointsEventArgs;

			if (player == null || gargs == null)
				return;

			PlayerStatistics stats = player.Statistics as PlayerStatistics;

			if (stats == null)
				return;

			stats.TotalRP += (uint)gargs.RealmPoints;
		}

		public static void DyingCallback(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer dyingPlayer = sender as GamePlayer;
			DyingEventArgs dargs = args as DyingEventArgs;

			if (dyingPlayer == null || dargs == null)
				return;

			GamePlayer killer = dargs.Killer as GamePlayer;
			if (killer == null)
				return;

            PlayerStatistics killerStats = killer.Statistics as PlayerStatistics;
            PlayerStatistics dyingPlayerStats = dyingPlayer.Statistics as PlayerStatistics;

			if (killerStats == null || dyingPlayerStats == null)
				return;

			killerStats.Deathblows++;
			if (dyingPlayer.RealmPointsValue > 0)
			{
				killerStats.KillsThatHaveEarnedRPs++;
				killerStats.RealmPointsEarnedFromKills += RPsEarnedFromKill(killer, dyingPlayer);

                if (killer.Group != null)
                {
                    foreach (GamePlayer member in killer.Group.GetPlayersInTheGroup())
                    {
                        if (member != killer)
                        {
                            PlayerStatistics memberStats = member.Statistics as PlayerStatistics;
                            if (memberStats != null)
                            {
                                memberStats.KillsThatHaveEarnedRPs++;
                                memberStats.RealmPointsEarnedFromKills += RPsEarnedFromKill(member, dyingPlayer);
                            }
                        }
                    }
                }
			}

			dyingPlayerStats.Deaths++;
		}

		public static void FinishCastSpellCallback(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer caster = sender as GamePlayer;
			CastingEventArgs fargs = args as CastingEventArgs;

			if (caster == null || fargs == null)
				return;

			if (fargs.SpellHandler.Spell.SpellType == "Resurrect")
			{
				PlayerStatistics stats = caster.Statistics as PlayerStatistics;
                if (stats != null)
                {
                    stats.RessurectionsPerformed++;
                }
			}
		}

		public static void HealthChangedCallback(DOLEvent e, object sender, EventArgs args)
		{
			HealthChangedEventArgs hargs = args as HealthChangedEventArgs;
			if (hargs.ChangeType == GameLiving.eHealthChangeType.Spell)
			{
				GamePlayer player = hargs.ChangeSource as GamePlayer;
				if (player == null)
					return;

                PlayerStatistics stats = player.Statistics as PlayerStatistics;

                if (stats != null)
                {
                    stats.HitPointsHealed += (uint)hargs.ChangeAmount;
                }
			}
		}

		public static uint RPsEarnedFromKill(GamePlayer killer, GamePlayer killedPlayer)
		{
			long noExpSeconds = ServerProperties.Properties.RP_WORTH_SECONDS;
			if (killedPlayer.DBCharacter.DeathTime + noExpSeconds > killedPlayer.PlayedTime)
				return 0;

			float totaldmg = 0f;
			foreach (DictionaryEntry de in killedPlayer.XPGainers)
			{
				totaldmg += (float)de.Value;
			}

			foreach (DictionaryEntry de in killedPlayer.XPGainers)
			{
				GamePlayer key = de.Key as GamePlayer;
				if (killer == key)
				{
					if (!killer.IsWithinRadius(killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE))
						return 0;

					double damagePercent = (float)de.Value / totaldmg;
					if (!key.IsAlive)//Dead living gets 25% exp only
						damagePercent *= 0.25;

					int rpCap = key.RealmPointsValue * 2;
					uint realmPoints = (uint)(killedPlayer.RealmPointsValue * damagePercent);

					realmPoints = (uint)(realmPoints * (1.0 + 2.0 * (killedPlayer.RealmLevel - killer.RealmLevel) / 900.0));

					if (killer.Group != null && killer.Group.MemberCount > 1)
					{
						int count = 0;
						foreach (GamePlayer player in killer.Group.GetPlayersInTheGroup())
						{
							if (!player.IsWithinRadius(killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
							count++;
						}
						realmPoints = (uint)(realmPoints * (1.0 + count * 0.125));
					}
					if (realmPoints > rpCap)
						realmPoints = (uint)rpCap;

					return realmPoints;
				}
				else
					continue;
			}

			return 0;
		}
	}
}

