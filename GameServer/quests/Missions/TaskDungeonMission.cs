using System;
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests
{
    public class TaskDungeonMission : AbstractMission
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public enum eTDMissionType
        {
            Clear = 0,
            Boss = 1,
            Specific = 2,
        }

        public enum eDungeonType
        {
            Melee = 0,
            Ranged = 1,
        }

        public eDungeonType DungeonType { get; } = eDungeonType.Melee;

        public eTDMissionType TdMissionType { get; }

        public TaskDungeonInstance TaskRegion { get; }

        public int Current { get; private set; }

        public long Total { get; }

        public string BossName { get; } = string.Empty;

        public string TargetName { get; } = string.Empty;

        private readonly bool[] _mobIsAlive;

        public TaskDungeonMission(object owner, eDungeonType dungeonType = eDungeonType.Ranged)
            : base(owner)
        {
            Log.Info("INFO: Successfully entered TaskDungeonMission!");
            GamePlayer player = owner as GamePlayer;

            if (owner is Group group)
            {
                player = group.Leader;

                // Assign the mission to the group.
                group.Mission = this;
            }

            if (player == null)
            {
                return;
            }

            // check level range and get region id from it
            ushort rid = GetRegionFromLevel(player.Level, player.Realm, dungeonType);

            TaskDungeonInstance instance = (TaskDungeonInstance)WorldMgr.CreateInstance(rid, typeof(TaskDungeonInstance));
            TaskRegion = instance;
            instance.Mission = this;

            // Dinberg: I've removed instance level, and have commented this out so it compiles.
            // I dont have time to implement the rest right now,
            // m_taskRegion.InstanceLevel = GetLevelFromPlayer(player);

            // Infact, this clearly isnt in use. I'll fix it to use the new instance system, and then itll work.
            // Do that later this week ^^.

            // Lets load the region from the InstanceXElementDB!

            // First we get the instance keyname.
            string keyname = $"TaskDungeon{rid}.1"; // TODO; variations, eg .2, .3 etc.
            instance.LoadFromDatabase(keyname);

            // Now, search for the boss and possible targets in the instance!
            foreach (var o in instance.Objects)
            {
                if (!(o is GameNPC npc))
                {
                    continue;
                }

                if (npc.Name.ToLower() != npc.Name)
                {
                    if (BossName == string.Empty)
                    {
                        BossName = npc.Name; // Some instances have multiple bosses, eg Gregorian - why break?
                    }
                    else if (Util.Chance(50))
                    {
                        BossName = npc.Name;
                    }
                } // else what if we aren't looking at a boss, but a normal mob?
                else
                    if (Util.Chance(20) || TargetName == string.Empty)
                {
                    TargetName = npc.Name;
                }
            }

            int specificCount = 0;

            // Draw the mission type before we do anymore counting...
            if (Util.Chance(40) && BossName != string.Empty)
            {
                TdMissionType = eTDMissionType.Boss;
            }
            else if (Util.Chance(20) && TargetName != string.Empty)
            {
                TdMissionType = eTDMissionType.Specific;
            }
            else
            {
                TdMissionType = eTDMissionType.Clear;
            }

            // Now, count if we need to.
            if (TdMissionType != eTDMissionType.Boss)
            {
                foreach (var o in instance.Objects)
                {
                    if (!(o is GameNPC entry))
                    {
                        continue;
                    }

                    // Now, if we want all mobs, get all mobs...
                    if (TdMissionType == eTDMissionType.Clear)
                    {
                        specificCount++;
                    }
                    else if (entry.Name == TargetName)
                    {
                        // only count target mobs for specific dungeons.
                        specificCount++;
                    }
                }
            }

            // append the count to the total!
            Total = specificCount;

            // Set the mission description again if owner is group, otherwise
            // mission description is always "Clear" before entering the dungeon.
            if (owner is Group)
            {
                UpdateMission();
            }

            _mobIsAlive = new bool[Total];
            for (int i = 0; i < Total; i++)
            {
                _mobIsAlive[i] = true;
            }
        }

        // Dinberg: removed this void. Handled in TaskDungeonInstance
        // private static byte GetLevelFromPlayer(GamePlayer player)

        // Burial Tomb
        private static readonly ushort[] BurialTombLong = { 295, 400, 402, 404 };
        private static readonly ushort[] BurialTombLaby = { 293, 294, 401, 403 };

        // Forgotten Mines
        private static readonly ushort[] ForgottenMinesLong = { 256, 257, 258, };
        private static readonly ushort[] ForgottenMinesLaby = { 410, 411, 412, 413, 414 };

        // Desecrated Ground
        private static readonly ushort[] DesecratedGroundsLong = { 297, 298, 409 };
        private static readonly ushort[] DesecratedGroundsLaby = { 296, 405, 406, 407, 408 };

        // Funerary Hall
        private static readonly ushort[] FuneraryHallLong = { 379, 382, 383, 419 };
        private static readonly ushort[] FuneraryHallLaby = { 415, 416, 417, 418 };

        // Sundered Tombs
        private static readonly ushort[] SunderedTombsLong = { 386, 387, 388, };
        private static readonly ushort[] SunderedTombsLaby = { 420, 421, 422, 423, 424 };

        // Damp Cavern
        private static readonly ushort[] DampCavernLong = { 278, 279, 280, 301 };
        private static readonly ushort[] DampCavernLaby = { 300, 302, 303, 304 };

        // Forgotten Sepulchre
        private static readonly ushort[] ForgottenSepulchreLong = { 281, 282, 283, 307 };
        private static readonly ushort[] ForgottenSepulchreLaby = { 305, 306, 308, 309 };

        // The Concealed Guardhouse
        private static readonly ushort[] TheConcealedGuardhouseLong = { 284, 285, 286 };
        private static readonly ushort[] TheConcealedGuardhouseLaby = { 310, 311, 312, 313, 314 };

        // The Gossamer Grotto
        private static readonly ushort[] TheGossamerGrottoLong = { 287, 288, 289 };
        private static readonly ushort[] TheGossamerGrottoLaby = { 315, 316, 317, 318, 319 };

        // Underground Tunnel
        private static readonly ushort[] UndergroundTunnelLong = { 290, 291, 292 };
        private static readonly ushort[] UndergroundTunnelLaby = { 320, 321, 322, 323, 324 };

        // The Cursed Barrow
        private static readonly ushort[] TheCursedBarrowLong = { 427, 428, 431, 454 };
        private static readonly ushort[] TheCursedBarrowLaby = { 450, 451, 452, 453 };

        // Dismal Grotto
        private static readonly ushort[] DismalGrottoLong = { 432, 441, 444, 459 };
        private static readonly ushort[] DismalGrottoLaby = { 455, 456, 457, 458 };

        // The Accursed Caves
        private static readonly ushort[] TheAccursedCavesLong = { 464, 477, 478, 481 };
        private static readonly ushort[] TheAccursedCavesLaby = { 460, 461, 462, 463 };

        // Dark Cavern
        private static readonly ushort[] DarkCavernLong = { 445, 448, 449 };
        private static readonly ushort[] DarkCavernLaby = { 465, 466, 467, 468, 469 };

        // Unused Mine
        private static readonly ushort[] UnusedMineLong = { 474, 482, 485, 486 };
        private static readonly ushort[] UnusedMineLaby = { 048, 471, 472, 473 };

        private static ushort GetRegionFromLevel(byte level, eRealm realm, eDungeonType dungeonType)
        {
            // return 286;

            // TODO: fill this properly for all levels
            // Done by Argoras: filled all the Values for possible Realms and levels
            if (level <= 10)
            {
                switch (realm)
                {
                    case eRealm.Albion:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(BurialTombLong);
                        }
                        else
                        {
                            return GetRandomRegion(BurialTombLaby);
                        }

                    case eRealm.Midgard:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(DampCavernLong);
                        }
                        else
                        {
                            return GetRandomRegion(DampCavernLaby);
                        }

                    case eRealm.Hibernia:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(TheCursedBarrowLong);
                        }
                        else
                        {
                            return GetRandomRegion(TheCursedBarrowLaby);
                        }
                }
            }
            else if (level > 10 && level <= 20)
            {
                switch (realm)
                {
                    case eRealm.Albion:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(ForgottenMinesLong);
                        }
                        else
                        {
                            return GetRandomRegion(ForgottenMinesLaby);
                        }

                    case eRealm.Midgard:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(ForgottenSepulchreLong);
                        }
                        else
                        {
                            return GetRandomRegion(ForgottenSepulchreLaby);
                        }

                    case eRealm.Hibernia:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(DismalGrottoLong);
                        }
                        else
                        {
                            return GetRandomRegion(DismalGrottoLaby);
                        }
                }
            }
            else if (level > 20 && level <= 30)
            {
                switch (realm)
                {
                    case eRealm.Albion:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(DesecratedGroundsLong);
                        }
                        else
                        {
                            return GetRandomRegion(DesecratedGroundsLaby);
                        }

                    case eRealm.Midgard:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(TheConcealedGuardhouseLong);
                        }
                        else
                        {
                            return GetRandomRegion(TheConcealedGuardhouseLaby);
                        }

                    case eRealm.Hibernia:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(TheAccursedCavesLong);
                        }
                        else
                        {
                            return GetRandomRegion(TheAccursedCavesLaby);
                        }
                }
            }
            else if (level > 30 && level <= 40)
            {
                switch (realm)
                {
                    case eRealm.Albion:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(FuneraryHallLong);
                        }
                        else
                        {
                            return GetRandomRegion(FuneraryHallLaby);
                        }

                    case eRealm.Midgard:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(TheGossamerGrottoLong);
                        }
                        else
                        {
                            return GetRandomRegion(TheGossamerGrottoLaby);
                        }

                    case eRealm.Hibernia:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(DarkCavernLong);
                        }
                        else
                        {
                            return GetRandomRegion(DarkCavernLaby);
                        }
                }
            }
            else if (level > 40)
            {
                switch (realm)
                {
                    case eRealm.Albion:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(SunderedTombsLong);
                        }
                        else
                        {
                            return GetRandomRegion(SunderedTombsLaby);
                        }

                    case eRealm.Midgard:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(UndergroundTunnelLong);
                        }
                        else
                        {
                            return GetRandomRegion(UndergroundTunnelLaby);
                        }

                    case eRealm.Hibernia:
                        if (dungeonType == eDungeonType.Ranged)
                        {
                            return GetRandomRegion(UnusedMineLong);
                        }
                        else
                        {
                            return GetRandomRegion(UnusedMineLaby);
                        }
                }
            }

            return 0;
        }

        private static ushort GetRandomRegion(ushort[] regions)
        {
            return regions[Util.Random(0, regions.Length - 1)];
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (e != GameLivingEvent.EnemyKilled)
            {
                return;
            }

            if (sender is GamePlayer player && player.CurrentRegion != TaskRegion)
            {
                return;
            }

            if (!(args is EnemyKilledEventArgs eargs))
            {
                return;
            }

            switch (TdMissionType)
            {
                case eTDMissionType.Boss:
                    {
                        if (eargs.Target.Name == BossName)
                        {
                            FinishMission();
                        }

                        break;
                    }

                case eTDMissionType.Specific:
                    {
                        if (eargs.Target.Name == TargetName)
                        {
                            if (_mobIsAlive[eargs.Target.ObjectID - 1])
                            {
                                _mobIsAlive[eargs.Target.ObjectID - 1] = false;
                                Current++;
                                UpdateMission();
                                if (Current == Total)
                                {
                                    FinishMission();
                                }
                            }
                        }

                        break;
                    }

                case eTDMissionType.Clear:
                    {
                        if (_mobIsAlive[eargs.Target.ObjectID - 1])
                        {
                            _mobIsAlive[eargs.Target.ObjectID - 1] = false;
                            Current++;
                            UpdateMission();
                            if (Current == Total)
                            {
                                FinishMission();
                            }
                        }

                        break;
                    }
            }
        }

        /*
         * [Task] You have been asked to kill Dralkden the Thirster in the nearby caves.
         * [Task] You have been asked to clear the nearby caves.
         * [Task] You have been asked to clear the nearby caves. 19 creatures left!
         * [Task] You have been asked to kill 6 acidic clouds in the nearby caves!
         */

        public override string Description
        {
            get
            {
                switch (TdMissionType)
                {
                    case eTDMissionType.Boss: return $"You have been asked to kill {BossName} in the nearby caves.";
                    case eTDMissionType.Specific: return $"You have been asked to kill {Total} {TargetName} in the nearby caves.";
                    case eTDMissionType.Clear:
                        {
                            // Additional check if region is null in case of group mission.
                            // Otherwise else condition is used with m_total = 0.
                            if ((Owner is GamePlayer player && player.CurrentRegion != TaskRegion) || (Owner is Group && TaskRegion == null))
                            {
                                return "You have been asked to clear the nearby caves.";
                            }

                            bool test = Total - Current == 1;
                            return $"You have been asked to clear the nearby caves. {Total - Current} creature{(test ? string.Empty : "s")} left!";
                        }

                    default: return $"No description for mission type {TdMissionType}";
                }
            }
        }

        public override long RewardRealmPoints => 0;

        public override long RewardMoney
        {
            get
            {
                GamePlayer player = (GamePlayer) Owner;

                if (Owner is Group group)
                {
                    player = group.Leader;
                }

                return player.Level * player.Level * 100;
            }
        }

        private int XpMagicNumber
        {
            get
            {
                switch (TdMissionType)
                {
                    case eTDMissionType.Clear: return 75;
                    case eTDMissionType.Boss:
                    case eTDMissionType.Specific: return 50;
                }

                return 0;
            }
        }

        public override long RewardXp
        {
            get
            {
                GamePlayer player = (GamePlayer) Owner;
                if (Owner is Group group)
                {
                    player = group.Leader;
                }

                long amount = XpMagicNumber * player.Level;
                if (player.Level > 1)
                {
                    amount += XpMagicNumber * (player.Level - 1);
                }

                return amount;
            }
        }

        public override void FinishMission()
        {
            if (Owner is GamePlayer gamePlayer)
            {
                gamePlayer.Out.SendMessage("Mission Complete", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
            }
            else if (Owner is Group pOwner)
            {
                foreach (GamePlayer player in pOwner.GetPlayersInTheGroup())
                {
                    player.Out.SendMessage("Mission Complete", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
                }
            }

            base.FinishMission();
        }
    }

    // This part is done by Dinberg, but the original source im not sure of. I'm trying to tweak it the the instance
    // system I've developed, and this script was partly finished so I adopted it ^^.
    public class TaskDungeonInstance : Instance
    {
        public TaskDungeonInstance(ushort ID, GameTimer.TimeManager time, RegionData dat)
            : base(ID, time, dat)
        {
        }

        /// <summary>
        /// Gets/Sets the Mission of this instance.
        /// </summary>
        public TaskDungeonMission Mission { get; set; }

        private int _level;

        // Change instance level...
        // I've checked, this should be called correctly: player will be added/removed in time.
        public override void OnPlayerEnterInstance(GamePlayer player)
        {
            base.OnPlayerEnterInstance(player);
            UpdateInstanceLevel();

            // The player will not yet be in the instance, so wont receive the relevant text.
            player.Out.SendMessage($"You have entered {Description}.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            player.Out.SendMessage($"This instance is currently level {_level}.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
        }

        public override void OnPlayerLeaveInstance(GamePlayer player)
        {
            base.OnPlayerLeaveInstance(player);
            UpdateInstanceLevel();

            // Expire task from player...
            if (player.Mission == Mission)
            {
                player.Mission.ExpireMission();
            }
        }

        // This void is outside of Instance,
        // because i want people to think carefully about how they change levels in their instance.
        public void UpdateInstanceLevel()
        {
            _level = (byte)GetInstanceLevel();

            // Set all mobs to that level...
            if (_level > 0)
            {
                foreach (GameObject obj in Objects)
                {
                    if (!(obj is GameNPC npc))
                    {
                        continue;
                    }

                    npc.Level = (byte)_level;
                }

                // Update to the players..
                foreach (GameClient client in WorldMgr.GetClientsOfRegion(ID))
                {
                    client?.Out.SendMessage($"This instance is now level {_level}", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }
        }

        /// <summary>
        /// Expire the missions - the instance has exploded.
        /// </summary>
        public override void OnCollapse()
        {
            // We expire the mission as players can no longer reach or access the region once collapsed.
            Mission?.ExpireMission();

            base.OnCollapse();
        }
    }
}
