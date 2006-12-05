//save bytes int -> ushort

using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.Events;
using DOL.GS.Scripts;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Spells;

namespace DOL.GS.Scripts
{
    public class PlayerStatistic
    {
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static bool m_loggingEnabled = true;
        private static string PLAYER_STATISTICS_PROPERTY = "PLAYER_STATISTICS_PROPERTY";

        protected GamePlayer m_Player = null;
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
            get { return m_Player; }
        }

        public uint TotalRP
        {
            get { return m_TotalRP; }
            set
            {
                m_TotalRP = value;

                if (log.IsDebugEnabled)
                    if (m_Player != null)
                        log.Info("Statistics: " + m_Player.Name + " now has " + value + " TotalRP");
                    else
                        log.Info("Statistics: (m_Player is null) now has " + value + " TotalRP");
            }
        }

        public uint RealmPointsEarnedFromKills
        {
            get { return m_RealmPointsEarnedFromKills; }
            set
            {
                m_RealmPointsEarnedFromKills = value;

                if(log.IsDebugEnabled)
                    if(m_Player!=null)
                        log.Info("Statistics: "+m_Player.Name+" now has "+value+" Realmpoints");
                    else
                        log.Info("Statistics: (m_Player is null) now has "+value+" Realmpoints");
            }
        }

        public ushort KillsThatHaveEarnedRPs
        {
            get { return m_KillsThatHaveEarnedRPs; }
            set
            {
                m_KillsThatHaveEarnedRPs = value;

                if (log.IsDebugEnabled)
                    if (m_Player != null)
                        log.Info("Statistics: " + m_Player.Name + " now has " + value + " kills in total.");
                    else
                        log.Info("Statistics: (m_Player is null) now has " + value + " kills in total.");
            }
        }

        public ushort Deathblows
        {
            get { return m_Deathblows; }
            set
            {
                m_Deathblows = value;

                if(log.IsDebugEnabled)
                    if (m_Player != null)
                        log.Info("Statistics: " + m_Player.Name + " now has " + value + " Deathblows.");
                    else
                        log.Info("Statistics: (m_Player is null) now has " + value + " Deathblows.");
            }
        }

        public ushort Deaths
        {
            get { return m_Deaths; }
            set
            {
                m_Deaths = value;

                if (log.IsDebugEnabled)
                    if (m_Player != null)
                        log.Info("Statistics: " + m_Player.Name + " now has " + value + " Deaths.");
                    else
                        log.Info("Statistics: (m_Player is null) now has " + value + " Deaths.");
            }
        }

        public uint HitPointsHealed
        {
            get { return m_HitpointsHealed; }
            set
            {
                m_HitpointsHealed = value;

                if (log.IsDebugEnabled)
                    if (m_Player != null)
                        log.Info("Statistics: " + m_Player.Name + " now has healed " + value + " Hitpoints.");
                    else
                        log.Info("Statistics: (m_Player is null) now has healed " + value + " Hitpoints.");
            }
        }

        public ushort RessurectionsPerformed
        {
            get { return m_RessurectionsPerformed; }
            set
            {
                m_RessurectionsPerformed = value;

                if (log.IsDebugEnabled)
                    if (m_Player != null)
                        log.Info("Statistics: " + m_Player.Name + " now has performed " + value + " Ressurections.");
                    else
                        log.Info("Statistics: (m_Player is null) now has performed " + value + " Ressurections.");
            }
        }

        public DateTime LoginTime
        {
            get { return m_LoginTime; }
        }
        #endregion

        public PlayerStatistic(GamePlayer player)
        {
            m_Player = player;
            m_LoginTime = DateTime.Now;
            SetStatistic(Player, this);
        }

        public static void SetStatistic(GamePlayer player, PlayerStatistic stats)
        {
            player.TempProperties.setProperty(PLAYER_STATISTICS_PROPERTY, stats);
        }

        public static PlayerStatistic GetStatistic(GamePlayer player)
        {
            PlayerStatistic stats = player.TempProperties.getObjectProperty(PLAYER_STATISTICS_PROPERTY, null) as PlayerStatistic;
            return stats;
        }

        public static string GetStatsMessage(GamePlayer player)
        {
            PlayerStatistic stats = PlayerStatistic.GetStatistic(player);

            if (stats == null)
                return "";

            TimeSpan onlineTime = DateTime.Now.Subtract(stats.LoginTime);

            string stringOnlineTime = "Online time: " + onlineTime.Days + " days, " + onlineTime.Hours + " hours, " + onlineTime.Minutes + " minutes, " + onlineTime.Seconds + " seconds\n";
            if (onlineTime.Days < 1)
                stringOnlineTime = "Online time: " + onlineTime.Hours + " hours, " + onlineTime.Minutes + " minutes, " + onlineTime.Seconds + " seconds\n";

            string message =        "Statistics for " + player.Name + " this Session:\n" +
                                    "Total RP: " + stats.TotalRP + "\n" +
                                    "RP earned from kills: " + stats.RealmPointsEarnedFromKills + "\n" +
                                    "Kills that have earned RP: " + stats.KillsThatHaveEarnedRPs + "\n" +
                                    "Deathblows: " + stats.Deathblows + "\n" +
                                    "Deaths: " + stats.Deaths + "\n" +
                                    "HP healed: " + stats.HitPointsHealed + "\n" +
                                    "Resurrections performed: " + stats.RessurectionsPerformed + "\n" +
                                    stringOnlineTime +
                                    "RP/hour: " + stats.RPsPerHour(stats.TotalRP, onlineTime) + "\n" +
                                    "Kills per death: " + Divide((uint)stats.KillsThatHaveEarnedRPs, (uint)stats.Deaths) + "\n" +
                                    "RP per kill: " + Divide(stats.RealmPointsEarnedFromKills, (uint)stats.KillsThatHaveEarnedRPs) + "\n" +
                                    "\"I Remain Standing...\": " + Divide(stats.RealmPointsEarnedFromKills, (uint)stats.Deaths) + "\n";
			return message;
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

        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(GameEnteredCallback));
            GameEventMgr.AddHandler(GameLivingEvent.GainedRealmPoints, new DOLEventHandler(GainedRealmPointsCallback));
            GameEventMgr.AddHandler(GameLivingEvent.Dying, new DOLEventHandler(DyingCallback));
            GameEventMgr.AddHandler(GameLivingEvent.CastFinished, new DOLEventHandler(FinishCastSpellCallback));
            GameEventMgr.AddHandler(GameLivingEvent.HealthChanged, new DOLEventHandler(HealthChangedCallback));
        }

        [ScriptUnloadedEvent]
        public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(GameEnteredCallback));
            GameEventMgr.RemoveHandler(GameLivingEvent.GainedRealmPoints, new DOLEventHandler(GainedRealmPointsCallback));
            GameEventMgr.RemoveHandler(GameLivingEvent.Dying, new DOLEventHandler(DyingCallback));
            GameEventMgr.RemoveHandler(GameLivingEvent.CastFinished, new DOLEventHandler(FinishCastSpellCallback));
            GameEventMgr.RemoveHandler(GameLivingEvent.HealthChanged, new DOLEventHandler(HealthChangedCallback));
        }

        public static void GameEnteredCallback(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null)
                return;

            if (PlayerStatistic.GetStatistic(player) != null)
                return;
            else
                new PlayerStatistic(player);
        }

        public static void GainedRealmPointsCallback(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;
            GainedRealmPointsEventArgs gargs = args as GainedRealmPointsEventArgs;

            if (player == null || gargs == null)
                return;

            PlayerStatistic stats = PlayerStatistic.GetStatistic(player);

            if (stats == null)
                return;

            stats.TotalRP += (uint)gargs.RealmPoints;

            PlayerStatistic.SetStatistic(player, stats);
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

            PlayerStatistic killerStats = PlayerStatistic.GetStatistic(killer);
            PlayerStatistic dyingPlayerStats = PlayerStatistic.GetStatistic(dyingPlayer);

            if (killerStats == null || dyingPlayerStats == null)
                return;

            killerStats.Deathblows++;
            if (dyingPlayer.RealmPointsValue > 0)
            {
                killerStats.KillsThatHaveEarnedRPs++;
                killerStats.RealmPointsEarnedFromKills += RPsEarnedFromKill(killer, dyingPlayer);
            }

            dyingPlayerStats.Deaths++;

            PlayerStatistic.SetStatistic(dyingPlayer, dyingPlayerStats);
            PlayerStatistic.SetStatistic(killer, killerStats);
        }

        public static void FinishCastSpellCallback(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer caster = sender as GamePlayer;
			CastSpellEventArgs fargs = args as CastSpellEventArgs;

            if (caster == null || fargs == null)
                return;

            if (fargs.SpellHandler.Spell.SpellType == "Resurrect")
            {
                PlayerStatistic stats = PlayerStatistic.GetStatistic(caster);
                stats.RessurectionsPerformed++;
                PlayerStatistic.SetStatistic(caster, stats);
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
                PlayerStatistic stats = PlayerStatistic.GetStatistic(player);
                if (stats == null)
                    return;
                stats.HitPointsHealed += (uint)hargs.ChangeAmount;
                PlayerStatistic.SetStatistic(player, stats);
            }
        }

        public static uint RPsEarnedFromKill(GamePlayer killer, GamePlayer killedPlayer)
        {
            long noExpSeconds = ServerProperties.Properties.RP_WORTH_SECONDS;
            if (killedPlayer.PlayerCharacter.DeathTime + noExpSeconds > killedPlayer.PlayedTime)
                return 0;

            float totaldmg = 0f;
            foreach (DictionaryEntry de in killedPlayer.XPGainers)
            {
                totaldmg += (float)de.Value;
            }

            foreach(DictionaryEntry de in killedPlayer.XPGainers)
            {
                GamePlayer key = de.Key as GamePlayer;
                if (killer == key)
                {
                    if (!WorldMgr.CheckDistance(killer, killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        return 0;

                    double damagePercent = (float)de.Value / totaldmg;
                    if (!key.IsAlive)//Dead living gets 25% exp only
                        damagePercent *= 0.25;

                    int rpCap = key.RealmPointsValue * 2;
                    uint realmPoints = (uint)(killedPlayer.RealmPointsValue * damagePercent);

                    realmPoints = (uint)(realmPoints * (1.0 + 2.0 * (killedPlayer.RealmLevel - killer.RealmLevel) / 900.0));

                    if (killer.PlayerGroup != null && killer.PlayerGroup.PlayerCount > 1)
					{
							int count = 0;
							foreach (GamePlayer player in killer.PlayerGroup.GetPlayersInTheGroup())
							{
								if (!WorldMgr.CheckDistance(player, killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
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

