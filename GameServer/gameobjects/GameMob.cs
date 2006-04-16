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
using System.Reflection;
using System.Collections;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{

	/// <summary>
	/// This class represents a monster in the game!
	/// </summary>
	public class GameMob : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	
		#region Respawn
		/// <summary>
		/// The time to wait before each mob respawn
		/// </summary>
		protected int m_respawnInterval;

		/// <summary>
		/// The Respawn Interval of this mob in milliseconds
		/// </summary>
		public virtual int RespawnInterval
		{
			get { return m_respawnInterval; }
			set { m_respawnInterval = value; }
		}

		/// <summary>
		/// Starts the Respawn Timer
		/// </summary>
		public virtual void StartRespawn()
		{
			int respawnInt = RespawnInterval;
			if(respawnInt == 0) return;

			if(respawnInt < 0)
			{
				//Standard 5-8 mins
				if (Level <= 65 || Realm != 0)
				{
					respawnInt = Util.Random(5*60000)+3*60000;
				}
				else
				{
					int minutes = Level - 65 + 15;
					respawnInt = minutes*60000;
				}
			}

			if (m_respawnAction != null)
				m_respawnAction.Stop();
			m_respawnAction = new RespawnAction(this);
			m_respawnAction.Start(respawnInt);
		}

		/// <summary>
		/// The timer that will be started when the npc die
		/// </summary>
		protected RespawnAction m_respawnAction;

		/// <summary>
		/// The timed respawn action
		/// </summary>
		protected class RespawnAction : RegionAction
		{
			/// <summary>
			/// Constructs a new pray action
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public RespawnAction(GameMob actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Callback method for the pray-timer
			/// </summary>
			protected override void OnTick()
			{
				GameMob mob = (GameMob)m_actionSource;

				if(mob.ObjectState == eObjectState.Active) return;

				log.Error("RespawnAction.OnTick");

				//Heal this mob, move it to the spawnlocation
				mob.Health = mob.MaxHealth;
				mob.Mana = mob.MaxMana;
				mob.EndurancePercent = 100;
				mob.Position = mob.SpawnPosition;
				mob.Heading = mob.SpawnHeading;
				mob.AddToWorld();
			}
		}

		/// <summary>
		/// Called when this living dies
		/// </summary>
		public override void Die(GameLiving killer)
		{
			base.Die(killer);

			GamePlayer playerKiller = killer as GamePlayer;
			if (playerKiller != null && Faction != null)
			{
				playerKiller.IncreaseAggroLevel(Faction);
				foreach(Faction currentFaction in Faction.FriendFactions)
				{
					playerKiller.IncreaseAggroLevel(currentFaction);
				}
				foreach(Faction currentFaction in Faction.EnemyFactions)
				{
					playerKiller.DecreaseAggroLevel(currentFaction);
				}
			}

			StartRespawn();
		}
		#endregion
		
		#region Loot

		/// <summary>
		/// Holds the unique id of the loot list to use when the mob must drop
		/// </summary>
		protected int m_lootListID;

		/// <summary>
		/// Get and set the unique id of the loot list to use when the mob must drop
		/// </summary>
		public int LootListID
		{
			get { return m_lootListID; }
			set { m_lootListID = value; }
		}

		#endregion

		#region Charm
		/// <summary>
		/// The property that holds charmed tick if any
		/// </summary>
		public const string CHARMED_TICK_PROP = "CharmedTick";

		/// <summary>
		/// The duration of no exp after charmed, in game ticks
		/// </summary>
		public const int CHARMED_NOEXP_TIMEOUT = 60000;

		/// <summary>
		/// Removes the npc from the world
		/// </summary>
		/// <returns>true if the npc has been successfully removed</returns>
		public override bool RemoveFromWorld()
		{
			if (!base.RemoveFromWorld()) return false;
	
			TempProperties.removeProperty(CHARMED_TICK_PROP);
			return true;
		}

		#endregion

		#region Faction
		/// <summary>
		/// In the db we save the factionD but we must
		/// link the object with the faction instance
		/// stored in the factionMgr
		///(it is only used internaly by NHIbernate)
		/// </summary>
		/// <value>The faction id</value>
		private int FactionID
		{
			get { return m_faction != null ? m_faction.FactionID : 0; }
			set { m_faction = FactionMgr.GetFaction(value); }
		}

		/// <summary>
		/// Holds the Faction of the NPC
		/// </summary>
		protected Faction m_faction;

		/// <summary>
		/// Gets the Faction of the NPC
		/// </summary>
		public Faction Faction
		{
			get { return m_faction; }
			set { m_faction = value; }
		}

		#endregion
		
		#region GetAggroLevelString
		/// <summary>
		/// How friendly this mob is to player
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>aggro state as string</returns>
		public override string GetAggroLevelString(GamePlayer player, bool firstLetterUppercase)
		{
			// "aggressive towards you!", "hostile towards you.", "neutral towards you.", "friendly."
			// TODO: correct aggro strings

			if(GameServer.ServerRules.IsSameRealm(this, player, true))
			{
				if(firstLetterUppercase) return "Friendly";
				else return "friendly";
			}

			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if(aggroBrain != null && Faction != null)
			{
				int aggroLevel = aggroBrain.AggroLevel + player.GetFactionAggroLevel(Faction);
				if(aggroLevel > 0)
				{
					if(firstLetterUppercase) return "Aggressive";
					else return "aggressive";
				}
				else if(aggroLevel > -25)
				{
					if(firstLetterUppercase) return "Hostile";
					else return "hostile";
				}
				else if(aggroLevel > -50)
				{
					if(firstLetterUppercase) return "Neutral";
					else return "neutral";
				}
				else
				{
					if(firstLetterUppercase) return "Friendly";
					else return "friendly";
				}
			}

			return base.GetAggroLevelString(player, firstLetterUppercase);
		}

		#endregion
	}
}
