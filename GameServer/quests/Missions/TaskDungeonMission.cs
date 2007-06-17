using System;
using System.Collections;

using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests
{
	public class TaskDungeonMission : AbstractMission
	{
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

		private eDungeonType m_dungeonType;
		public eDungeonType DungeonType
		{
			get { return m_dungeonType; }
		}

		private eTDMissionType m_missionType;
		public eTDMissionType TDMissionType
		{
			get { return m_missionType; }
		}

		private Region m_taskRegion;
		public Region TaskRegion
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
			GamePlayer player = owner as GamePlayer;

			if (owner is PlayerGroup)
				player = (owner as PlayerGroup).Leader;

			if (player == null)
				return;

			//check level range and get region id from it
			ushort rid = GetRegionFromLevel(player.Level, player.Realm);
			Region r = WorldMgr.GetRegion(rid);
			//build region instance
			m_taskRegion = new Region(r.TimeManager, r.RegionData);
			long mobCount = 0, merchantCount = 0, itemCount = 0, bindCount = 0;
			m_taskRegion.InstanceLevel = GetLevelFromPlayer(player);
			m_taskRegion.LoadFromDatabase(r.RegionData.Mobs, ref mobCount, ref merchantCount, ref itemCount, ref bindCount);
			m_taskRegion.Zones.AddRange(r.Zones);

			foreach (Mob mob in r.RegionData.Mobs)
			{
				if (mob.Name.ToLower() != mob.Name)
				{
					m_bossName = mob.Name;
					break;
				}
			}

			foreach (Mob mob in r.RegionData.Mobs)
			{
				if (Util.Chance(20))
				{
					m_targetName = mob.Name;
					break;
				}
			}

			int specificCount = 0;
			foreach (Mob mob in r.RegionData.Mobs)
			{
				if (mob.Name == m_targetName)
					specificCount++;
			}

			if (Util.Chance(50) && m_bossName != "")
				m_missionType = eTDMissionType.Boss;
			else if (Util.Chance(20) && m_targetName != "")
			{
				m_missionType = eTDMissionType.Specific;
				m_total = specificCount;
			}
			else
			{
				m_missionType = eTDMissionType.Clear;
				m_total = mobCount;
			}
		}

		private static byte GetLevelFromPlayer(GamePlayer player)
		{
			if (player.PlayerGroup == null)
			{
				/*
				 * solo
				 * player | dungeon
				 * 1 | 1
				 * 2 | 1
				 * 3 | 3
				 * 4 | 3?
				 * 5 | 5?
				 * 6 | 5?
				 * 7 | 7?
				 * 8 | 7?
				 * 9 | 9?
				 * 10 | 10?
				 */

				//to make this easier, we will use the players level
				return player.Level;
			}
			else
			{
				//to make this easier, we will use the players level  + group count
				return (byte)(player.Level + player.PlayerGroup.PlayerCount);
			}
		}

		//Burial Tomb
		private static ushort[] burial_tomb = new ushort[] { 293/*, 294, 295, 400, 401, 402, 403, 404*/ };
		//The Cursed Barrow
		private static ushort[] the_cursed_barrow = new ushort[] { 450/*, 451, 452, 453, 454*/ };
		//Damp Cavern
		private static ushort[] damp_cavern = new ushort[] { 300/*, 301, 302, 303, 304*/ };

		private static ushort GetRegionFromLevel(byte level, byte realm)
		{
#warning TODO, fill this properly for all levels
			//if (level <= 10)
			{
				switch (realm)
				{
					case 1:
						return GetRandomRegion(burial_tomb);
					case 2:
						return GetRandomRegion(the_cursed_barrow);
					case 3:
						return GetRandomRegion(damp_cavern);
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
								if (m_owner is GamePlayer)
								{
									(m_owner as GamePlayer).Out.SendMessage((m_total - m_current) + " " + m_targetName + " Left", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
								}
								else if (m_owner is PlayerGroup)
								{
									foreach (GamePlayer player in (m_owner as PlayerGroup).GetPlayersInTheGroup())
									{
										player.Out.SendMessage((m_total - m_current) + " " + m_targetName + " Left", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
									}
								}
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
							if (m_owner is GamePlayer)
							{
								(m_owner as GamePlayer).Out.SendMessage((m_total - m_current) + " Creatures Left", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
							}
							else if (m_owner is PlayerGroup)
							{
								foreach (GamePlayer player in (m_owner as PlayerGroup).GetPlayersInTheGroup())
								{
									player.Out.SendMessage((m_total - m_current) + " Creatures Left", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
								}
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
							else return "You have been asked to clear the nearby caves. " + (m_total - m_current) + " creatures left!";
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
				if (m_owner is PlayerGroup)
					player = (m_owner as PlayerGroup).Leader;
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
				if (m_owner is PlayerGroup)
					player = (m_owner as PlayerGroup).Leader;
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
			else if (m_owner is PlayerGroup)
			{
				foreach (GamePlayer player in (m_owner as PlayerGroup).GetPlayersInTheGroup())
				{
					player.Out.SendMessage("Mission Complete", eChatType.CT_ScreenCenter, eChatLoc.CL_ChatWindow);
				}
			}
			base.FinishMission();
		}
	}
}