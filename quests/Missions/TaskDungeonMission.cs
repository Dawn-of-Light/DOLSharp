using System;
using System.Collections;
using System.Reflection;

using DOL.Database;
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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public enum eTDMissionType : int
		{
			Clear = 0,
			Boss = 1,
			Specific = 2,
		}

		public enum eDungeonType : int
		{
			Melee = 0,
			Ranged = 1,
		}

		private eDungeonType m_dungeonType = eDungeonType.Melee;
		public eDungeonType DungeonType
		{
			get { return m_dungeonType; }
		}

		private eTDMissionType m_missionType;
		public eTDMissionType TDMissionType
		{
			get { return m_missionType; }
		}

		private TaskDungeonInstance m_taskRegion;
        public TaskDungeonInstance TaskRegion
		{
			get { return m_taskRegion; }
		}

		private int m_current = 0;
		public int Current
		{
			get { return m_current; }
		}

		private long m_total = 0;
		public long Total
		{
			get { return m_total; }
		}

		private string m_bossName = "";
		public string BossName
		{
			get { return m_bossName; }
		}

		private string m_targetName = "";
		public string TargetName
		{
			get { return m_targetName; }
		}

		public TaskDungeonMission(object owner)
			: base(owner)
		{
            log.Info("INFO: Successfully entered TaskDungeonMission!");
			GamePlayer player = owner as GamePlayer;

            if (owner is Group)
            {
                player = (owner as Group).Leader;
                //Assign the mission to the group.
                (owner as Group).Mission = this;
            }

			if (player == null)
				return;

			//check level range and get region id from it
			ushort rid = GetRegionFromLevel(player.Level, player.Realm);

            TaskDungeonInstance instance = (TaskDungeonInstance)WorldMgr.CreateInstance(rid, typeof(TaskDungeonInstance));
            m_taskRegion = instance;
            instance.Mission = this;

			//Dinberg: I've removed instance level, and have commented this out so it compiles.
            //I dont have time to implement the rest right now, 
            //m_taskRegion.InstanceLevel = GetLevelFromPlayer(player);

            //Infact, this clearly isnt in use. I'll fix it to use the new instance system, and then itll work.
            //Do that later this week ^^.

            
            //Lets load the region from the InstanceXElementDB!


            //First we get the instance keyname.
            string keyname = "TaskDungeon" + rid + ".1"; //TODO; variations, eg .2, .3 etc.
            instance.LoadFromDatabase(keyname);

            //Now, search for the boss and possible targets in the instance!
            foreach (GameNPC npc in instance.Objects)
            {
                if (npc == null)
                    continue;

                if (npc.Name.ToLower() != npc.Name)
                {
                    if (m_bossName == "")
                        m_bossName = npc.Name; //Some instances have multiple bosses, eg Gregorian - why break?
                    else if (Util.Chance(50))
                        m_bossName = npc.Name;
                } //else what if we aren't looking at a boss, but a normal mob?
                else
                    if (Util.Chance(20) || m_targetName == "")
                        m_targetName = npc.Name;
            }

			int specificCount = 0;
            
            //Draw the mission type before we do anymore counting...
            if (Util.Chance(40) && m_bossName != "")
                m_missionType = eTDMissionType.Boss;
            else if (Util.Chance(20) && m_targetName != "")
                m_missionType = eTDMissionType.Specific;
            else
                m_missionType = eTDMissionType.Clear;
                
            //Now, count if we need to.
            if (m_missionType != eTDMissionType.Boss)
            {
                foreach (GameNPC entry in instance.Objects)
                {
                    if (entry == null)
                        continue;

                    //Now, if we want all mobs, get all mobs...
                    if (m_missionType == eTDMissionType.Clear)
                        specificCount++;
                    else if (entry.Name == m_targetName) 
                        //only count target mobs for specific dungeons.
                        specificCount++;
                }
            }

            //append the count to the total!
            m_total = specificCount;
		}

        //Dinberg: removed this void. Handled in TaskDungeonInstance
		//private static byte GetLevelFromPlayer(GamePlayer player)

		//Burial Tomb
		private static ushort[] burial_tomb = new ushort[] { 293/*, 294, 295, 400, 401, 402, 403, 404*/ };
		//The Cursed Barrow
		private static ushort[] the_cursed_barrow = new ushort[] { 450/*, 451, 452, 453, 454*/ };
		//Damp Cavern
		private static ushort[] damp_cavern = new ushort[] { 300/*, 301, 302, 303, 304*/ };

		private static ushort GetRegionFromLevel(byte level, eRealm realm)
		{
            return 286;
            /*
			//TODO: fill this properly for all levels
			//if (level <= 10)
			{
				switch (realm)
				{
					case eRealm.Albion:
						return GetRandomRegion(burial_tomb);
					case eRealm.Midgard:
						return GetRandomRegion(the_cursed_barrow);
					case eRealm.Hibernia:
						return GetRandomRegion(damp_cavern);
				}
			}
			return 0;*/
		}

		private static ushort GetRandomRegion(ushort[] regions)
		{
			return regions[Util.Random(0, regions.Length - 1)];
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e != GameLivingEvent.EnemyKilled)
				return;

			EnemyKilledEventArgs eargs = args as EnemyKilledEventArgs;

			switch (m_missionType)
			{
				case eTDMissionType.Boss:
					{
						if (eargs.Target.Name == m_bossName)
							FinishMission();
						break;
					}
				case eTDMissionType.Specific:
					{
						if (eargs.Target.Name == m_targetName)
						{
							m_current++;
							UpdateMission();
							if (m_current == m_total)
								FinishMission();
							else
							{
                                /* - Dinberg, disabled this. Messages extremely annoying.
								if (m_owner is GamePlayer)
								{
									(m_owner as GamePlayer).Out.SendMessage((m_total - m_current) + " " + m_targetName + " Left", eChatType.CT_ScreenCenter_And_CT_System, eChatLoc.CL_ChatWindow);
								}
								else if (m_owner is Group)
								{
									foreach (GamePlayer player in (m_owner as Group).GetPlayersInTheGroup())
									{
										player.Out.SendMessage((m_total - m_current) + " " + m_targetName + " Left", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
									}
								}
                                 */
							}
						}
						break;
					}
				case eTDMissionType.Clear:
					{
						m_current++;
						UpdateMission();
						if (m_current == m_total)
							FinishMission();
						else
						{
                            /*
							if (m_owner is GamePlayer)
							{
								(m_owner as GamePlayer).Out.SendMessage((m_total - m_current) + " Creatures Left", eChatType.CT_ScreenCenter_And_CT_System, eChatLoc.CL_ChatWindow);
							}
							else if (m_owner is Group)
							{
								foreach (GamePlayer player in (m_owner as Group).GetPlayersInTheGroup())
								{
									player.Out.SendMessage((m_total - m_current) + " Creatures Left", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
								}
							}
                             */
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
				switch (m_missionType)
				{
					case eTDMissionType.Boss: return "You have been asked to kill " + m_bossName + " in the nearby caves.";
					case eTDMissionType.Specific: return "You have been asked to kill " + m_total + " " + m_targetName + " in the nearby caves.";
					case eTDMissionType.Clear:
						{
                            if (m_owner is GamePlayer && (m_owner as GamePlayer).CurrentRegion != m_taskRegion)
                            {
                                return "You have been asked to clear the nearby caves.";
                            }
                            else
                            {
                                bool test = m_total - m_current == 1;
                                return "You have been asked to clear the nearby caves. " + (m_total - m_current) + " creature" + (test == true ? "" : "s") + " left!";
                            }
                        }
					default: return "No description for mission type " + m_missionType.ToString();
				}
			}
		}

		public override long RewardRealmPoints
		{
			get { return 0; }
		}

		public override long RewardMoney
		{
			get {
				GamePlayer player = m_owner as GamePlayer;
				if (m_owner is Group)
					player = (m_owner as Group).Leader;
				return player.Level * player.Level * 100;
			}
		}

		private int XPMagicNumber
		{
			get
			{
				switch (m_missionType)
				{
					case eTDMissionType.Clear: return 75;
					case eTDMissionType.Boss:
					case eTDMissionType.Specific: return 50;
				}
				return 0;
			}
		}

		public override long RewardXP
		{
			get
			{
				GamePlayer player = m_owner as GamePlayer;
				if (m_owner is Group)
					player = (m_owner as Group).Leader;
				long amount = XPMagicNumber * player.Level;
				if (player.Level > 1)
					amount += XPMagicNumber * (player.Level - 1);
				return amount;
				/*
				long XPNeeded = (m_owner as GamePlayer).ExperienceForNextLevel - (m_owner as GamePlayer).ExperienceForCurrentLevel;
				return (long)(XPNeeded * 0.50 / (m_owner as GamePlayer).Level); // 50% of total xp for level
				 */
			}
		}

		public override void FinishMission()
		{
			if (m_owner is GamePlayer)
			{
				(m_owner as GamePlayer).Out.SendMessage("Mission Complete", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
			}
			else if (m_owner is Group)
			{
				foreach (GamePlayer player in (m_owner as Group).GetPlayersInTheGroup())
				{
					player.Out.SendMessage("Mission Complete", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
				}
			}
			base.FinishMission();
		}
	}

    //This part is done by Dinberg, but the original source im not sure of. I'm trying to tweak it the the instance
    //system I've developed, and this script was partly finished so I adopted it ^^. 
    public class TaskDungeonInstance : Instance
    {
        public TaskDungeonInstance(ushort ID, GameTimer.TimeManager time, RegionData dat)
            : base(ID, time, dat)
        {
        }

        private TaskDungeonMission m_mission;
        /// <summary>
        /// Gets/Sets the Mission of this instance.
        /// </summary>
        public TaskDungeonMission Mission
        {
            get { return m_mission; }
            set { m_mission = value; }
        }

        private int m_level;

        //Change instance level...
        //I've checked, this should be called correctly: player will be added/removed in time.
        public override void OnPlayerEnterInstance(GamePlayer player)
        {
            base.OnPlayerEnterInstance(player);
            UpdateInstanceLevel();

            //The player will not yet be in the instance, so wont receive the relevant text.
            player.Out.SendMessage("You have entered " + Description + ".", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            player.Out.SendMessage("This instance is currently level " + m_level + ".", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
        }

        public override void OnPlayerLeaveInstance(GamePlayer player)
        {
            base.OnPlayerLeaveInstance(player);
            UpdateInstanceLevel();
            //Expire task from player...
            if (player.Mission == m_mission) player.Mission.ExpireMission();
        }


        //This void is outside of Instance,
        //because i want people to think carefully about how they change levels in their instance.
        public void UpdateInstanceLevel()
        {
            m_level = (byte)(GetInstanceLevel());
            //Set all mobs to that level...
            foreach (GameObject obj in Objects)
            {
                if (obj == null)
                    continue;

                GameNPC npc = obj as GameNPC;
                if (npc == null)
                    continue;

                npc.Level = (byte)m_level;
            }
            //Update to the players..
            foreach (GameClient client in WorldMgr.GetClientsOfRegion(ID))
                if (client != null)
                    client.Out.SendMessage("This instance is now level " + m_level, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Expire the missions - the instance has exploded.
        /// </summary>
        public override void OnCollapse()
        {
            //We expire the mission as players can no longer reach or access the region once collapsed.
            if (Mission != null)
            Mission.ExpireMission();
            base.OnCollapse();
        }
    }
}
