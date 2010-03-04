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
using System.Text;
using DOL.GS.PacketHandler;
using DOL.Database;
using log4net;
using System.Reflection;
using DOL.Events;
using DOL.GS.ServerProperties;
using DOL.AI.Brain;


namespace DOL.GS
{
	/// <summary>
	/// Base class for all dragon type mobs. A dragon will use various abilities,
	/// e.g. spells and timed mob spawns; to allow for a variety of spell types,
	/// mob types and mob numbers you will have to derive your own dragon class
	/// and override some of the methods and properties provided in this abstract
	/// class.
	/// </summary>
	/// <author>Aredhel</author>
	public abstract class GameDragon : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		
		/// <summary>
		/// Set the dragon strength in percent of its max abilities
		/// </summary>
		private int m_DragonStrength;
		public virtual int DragonStrength
		{
			get { return m_DragonStrength;}
			set { m_DragonStrength = value;}
			
		}
		
		/// <summary>
		/// Announcements for Breath, Glare and death.
		/// </summary>
		protected String[] m_breathAnnounce;
		protected String m_glareAnnounce;
		protected String[] m_deathAnnounce;

		/// <summary>
		/// Creates a new instance of GameDragon.
		/// </summary>
		public GameDragon()
			: base()
		{
			// Put announcements in here, should make localisation easier. Write your own
			// default constructor if you need to change these.

			m_breathAnnounce = new String[] { "You feel a rush of air flow past you as {0} inhales deeply!", 
				"{0} takes another powerful breath as he prepares to unleash a raging inferno upon you!", 
				"{0} bellows in rage and glares at all of the creatures attacking him.", 
				"{0} noticeably winces from his wounds as he attempts to prepare for yet another life-threatening attack!" };
			m_glareAnnounce = "{0} glares at {1}!";
			m_deathAnnounce = new String[] { "The earth lurches beneath your feet as {0} staggers and topples to the ground.",
				"A glowing light begins to form on the mound that served as {0}'s lair." };

			TetherRange = 2500;	// TODO: Can be removed once there is an NPCTemplate.
			m_DragonStrength = ServerProperties.Properties.SET_DIFFICULTY_ON_EPIC_ENCOUNTERS;
		}

		public ushort LairRadius
		{
			get { return 2000; }
		}

		/// <summary>
		/// Create dragon's lair after it was loaded from the DB.
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			String[] dragonName = Name.Split(new char[] { ' ' });
			WorldMgr.GetRegion(CurrentRegionID).AddArea(new Area.Circle(String.Format("{0}'s Lair",
				dragonName[0]),
				X, Y, 0, LairRadius + 200));
		}

		public override int AttackRange
		{
			get { return 400; }
			set { }
		}

		public override double GetArmorAF(eArmorSlot slot)
		{
			return 1000*m_DragonStrength/100;
		}

		public override double GetArmorAbsorb(eArmorSlot slot)
		{
			// 85% ABS is cap.

			return 0.85*m_DragonStrength/100;
		}

		/// <summary>
		/// Returns the dragon's resist to a given damage type.
		/// </summary>
		/// <param name="damageType"></param>
		/// <returns></returns>
		public override int GetResist(eDamageType damageType)
		{
			// 35% vulnerable to melee, 1% to everything else.

			switch (damageType)
			{
				case eDamageType.Slash : 
				case eDamageType.Crush :
				case eDamageType.Thrust: return 65*m_DragonStrength/100;
				default: return 99*m_DragonStrength/100;
			}
		}

		public override int MaxHealth
		{
			get
			{
				return 30000*m_DragonStrength/100;
			}
		}

		public override double AttackDamage(InventoryItem weapon)
		{
			return base.AttackDamage(weapon) * 1.0 *m_DragonStrength/100;
		}

		public override int MaxSpeedBase
		{
			get { return 191 + (Level * 2); }
			set { m_maxSpeedBase = value; }
		}

		public override int RespawnInterval
		{
			get
			{
				int highmod = Level + 50;
				int lowmod = Level / 3;
				int result = Util.Random(lowmod, highmod);
				return result * 60 * 1000 *m_DragonStrength/100;
			}
		}

		/// <summary>
		/// Invoked when the dragon dies.
		/// </summary>
		/// <param name="killer">The living that got the killing blow.</param>
		public override void Die(GameObject killer)
		{
			// debug
			if (killer == null)
				log.Error("Dragon Killed: killer is null!");
			else
				log.Debug("Dragon Killed: killer is " + killer.Name + ", attackers:");

			ArrayList clone = m_attackers.Clone() as ArrayList;

			foreach (GameLiving attacker in clone)
			{
				if (attacker != null)
					log.Debug(attacker.Name);
			}

			// due to issues with attackers the following code will send a notify to all in area in order to force quest credit
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Notify(GameLivingEvent.EnemyKilled, killer, new EnemyKilledEventArgs(this));
			}

			base.Die(killer);

			foreach (String message in m_deathAnnounce)
			{
				BroadcastMessage(String.Format(message, Name));
			}

			ReportNews(killer);
		}

		#region Damage & Heal Events

		/// <summary>
		/// Take some amount of damage inflicted by another GameObject.
		/// </summary>
		/// <param name="source">The object inflicting the damage.</param>
		/// <param name="damageType">The type of damage.</param>
		/// <param name="damageAmount">The amount of damage inflicted.</param>
		/// <param name="criticalAmount">The critical amount of damage inflicted</param>
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			m_healthPercentOld = HealthPercent;
			base.TakeDamage(source, damageType, damageAmount, criticalAmount);
			Brain.Notify(GameObjectEvent.TakeDamage, this,
				new TakeDamageEventArgs(source, damageType, damageAmount, criticalAmount));
		}

		/// <summary>
		/// Take action upon someone healing the enemy.
		/// </summary>
		/// <param name="enemy">The living that was healed.</param>
		/// <param name="healSource">The source of the heal.</param>
		/// <param name="changeType">The way the living was healed.</param>
		/// <param name="healAmount">The amount that was healed.</param>
		public override void EnemyHealed(GameLiving enemy, GameObject healSource, eHealthChangeType changeType, int healAmount)
		{
			base.EnemyHealed(enemy, healSource, changeType, healAmount);
			Brain.Notify(GameLivingEvent.EnemyHealed, this,
				new EnemyHealedEventArgs(enemy, healSource, changeType, healAmount));
		}

		#endregion

		#region Tether

		/// <summary>
		/// Return to spawn point, dragon can't be attacked while it's
		/// on it's way.
		/// </summary>
		public override void WalkToSpawn()
		{
			EvadeChance = 100;
			base.WalkToSpawn();
		}

		/// <summary>
		/// Handle event notifications.
		/// </summary>
		/// <param name="e">The event that occured.</param>
		/// <param name="sender">The sender of the event.</param>
		public override void Notify(DOLEvent e, object sender)
		{
			base.Notify(e, sender);

			// When dragon arrives at its spawn point, make it vulnerable again.

			if (e == GameNPCEvent.ArriveAtTarget)
				EvadeChance = 0;
		}

		#endregion

		#region Health

		private int m_healthPercentOld = 100;

		/// <summary>
		/// The amount of health before the most recent attack.
		/// </summary>
		public int HealthPercentOld
		{
			get { return m_healthPercentOld; }
			protected set { m_healthPercentOld = value; }
		}

		#endregion

		#region Custom Methods

		/// <summary>
		/// Broadcast relevant messages to the raid.
		/// </summary>
		/// <param name="message">The message to be broadcast.</param>
		public void BroadcastMessage(String message)
		{
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE + 1500))
				player.Out.SendMessage(message, eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Post a message in the server news and award a dragon kill point for
		/// every XP gainer in the raid.
		/// </summary>
		/// <param name="killer">The living that got the killing blow.</param>
		protected void ReportNews(GameObject killer)
		{
			int numPlayers = AwardDragonKillPoint();
			String message = String.Format("{0} has been slain by a force of {1} warriors from the realm of {2}",
				Name, numPlayers, GlobalConstants.RealmToName((eRealm)killer.Realm));
			NewsMgr.CreateNews(message, killer.Realm, eNewsType.PvE, true);
		}

		/// <summary>
		/// Award dragon kill point for each XP gainer.
		/// </summary>
		/// <returns>The number of people involved in the kill.</returns>
		protected int AwardDragonKillPoint()
		{
			int count = 0;
			lock (this.XPGainers.SyncRoot)
			{
				foreach (System.Collections.DictionaryEntry de in this.XPGainers)
				{
					GameObject obj = (GameObject)de.Key;
					if (obj is GamePlayer)
					{
						GamePlayer player = obj as GamePlayer;
						player.KillsDragon++;
						count++;
					}
				}
			}
			return count;
		}

		#endregion

		#region Add Spawns

		/// <summary>
		/// Spawn adds that will despawn again after some time has passed.
		/// For your own implementation call SpawnTimedAdd with your own
		/// template ID (which could, in turn, use a different mob class than 
		/// GameNPC and thus have a different brain implementation) and mob level.
		/// </summary>
		public virtual bool CheckAddSpawns()
		{
			// Prevent brain from doing this over and over.

			HealthPercentOld = HealthPercent;
			return false;
		}

		private INpcTemplate m_addTemplate;

		/// <summary>
        /// Create an add from the specified template.
		/// </summary>
		/// <param name="templateID"></param>
		/// <param name="level"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="uptime"></param>
		/// <returns></returns>
		protected GameNPC SpawnTimedAdd(int templateID, int level, int x, int y, int uptime, bool isRetriever)
		{
			GameNPC add = null;

			try
			{
				if (m_addTemplate == null || m_addTemplate.TemplateId != templateID)
				{
					m_addTemplate = NpcTemplateMgr.GetTemplate(templateID);
				}

				// Create add from template.
				// The add will automatically despawn after 30 seconds.

				if (m_addTemplate != null)
				{
					add = new GameNPC(m_addTemplate);

					if (isRetriever)
					{
						add.SetOwnBrain(new RetrieverMobBrain());
					}
					add.CurrentRegion = this.CurrentRegion;
					add.Heading = (ushort)(Util.Random(0, 4095));
					add.Realm = 0;
					add.X = x;
					add.Y = y;
					add.Z = 0;
					add.CurrentSpeed = 0;
					add.Level = (byte)level;
					add.RespawnInterval = -1;
					add.AddToWorld();
					new DespawnTimer(this, add, uptime * 1000);
				}
			}
			catch
			{
				log.Warn(String.Format("Unable to get template for {0}", Name));
			}
			return add;
		}

		/// <summary>
		/// Provides a timer to remove an NPC from the world after some
		/// time has passed.
		/// </summary>
		protected class DespawnTimer : GameTimer
		{
			private GameNPC m_npc;

			/// <summary>
			/// Constructs a new DespawnTimer.
			/// </summary>
			/// <param name="timerOwner">The owner of this timer.</param>
			/// <param name="npc">The GameNPC to despawn when the time is up.</param>
			/// <param name="delay">The time after which the add is supposed to despawn.</param>
			public DespawnTimer(GameObject timerOwner, GameNPC npc, int delay)
				: base(timerOwner.CurrentRegion.TimeManager)
			{
				m_npc = npc;
				Start(delay);
			}

			/// <summary>
			/// Called on every timer tick.
			/// </summary>
			protected override void OnTick()
			{
				// Remove the NPC from the world.

				if (m_npc != null)
				{
					m_npc.Delete();
					m_npc = null;
				}
			}
		}

		/// <summary>
		/// The number of players in the dragon's lair.
		/// </summary>
		public int PlayersInLair
		{
			get
			{
				int count = 0;
				foreach (GamePlayer player in GetPlayersInRadius(LairRadius))
					count++;
				return count;
			}
		}

		/// <summary>
		/// Invoked when retriever type mob has reached its target location.
		/// </summary>
		/// <param name="sender">The retriever mob.</param>
		public virtual void OnRetrieverArrived(GameNPC sender)
		{
		}

		#endregion

		#region Breath & Resist Debuff

		protected Spell m_breathSpell;

		/// <summary>
		/// The Breath spell. Override this property in your dragon implementation
		/// and assign the spell to m_breathSpell.
		/// </summary>
		protected abstract Spell Breath
		{
			get;
		}

		protected Spell m_resistDebuffSpell;

		/// <summary>
		/// The resist debuff spell. Override this property in your dragon implementation
		/// and assign the spell to m_debuffSpell.
		/// </summary>
		protected abstract Spell ResistDebuff
		{
			get;
		}

		/// <summary>
		/// Check whether or not to cast Breath.
		/// </summary>
		public bool CheckBreath()
		{
			PrepareToBreathe();	// Has a 100% chance to cast.
			return true;
		}

		/// <summary>
		/// Announce the Breath attack and start the 5 second timer.
		/// </summary>
		private void PrepareToBreathe()
		{
			// Prevent brain from casting this over and over.

			HealthPercentOld = HealthPercent;	
			int messageNo = Util.Random(1, m_breathAnnounce.Length) - 1;
			BroadcastMessage(String.Format(m_breathAnnounce[messageNo], Name));
			new RegionTimer(this, new RegionTimerCallback(CastBreath), 5000);
		}

		/// <summary>
		/// Cast Breath on the raid (AoE damage and AoE resist debuff).
		/// </summary>
		/// <param name="timer">The timer that started this cast.</param>
		/// <returns></returns>
		private int CastBreath(RegionTimer timer)
		{
			CastSpell(Breath, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
			CastSpell(ResistDebuff, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
			return 0;
		}

		#endregion

		#region Glare

		protected Spell m_glareSpell;

		/// <summary>
		/// The Glare spell. Override this property in your dragon implementation
		/// and assign the spell to m_glareSpell.
		/// </summary>
		protected abstract Spell Glare
		{
			get;
		}

		private const int m_glareChance = 50;

		/// <summary>
		/// Chance to cast Glare when a potential target has been detected.
		/// </summary>
		protected int GlareChance
		{
			get { return m_glareChance; }
		}

		private GamePlayer m_glareTarget;

		/// <summary>
		/// The target for the next glare attack.
		/// </summary>
		private GamePlayer GlareTarget
		{
			get { return m_glareTarget; }
			set { m_glareTarget = value; PrepareToGlare(); }
		}

		/// <summary>
		/// Check whether or not to glare at this target.
		/// </summary>
		/// <param name="target">The potential target.</param>
		/// <returns>Whether or not the spell was cast.</returns>
		public bool CheckGlare(GamePlayer target)
		{
			if (target == null || GlareTarget != null) return false;
			bool success = Util.Chance(GlareChance);
			if (success)
				GlareTarget = target;
			return success;
		}

		/// <summary>
		/// Announce the glare attack and start the 5 second timer.
		/// </summary>
		private void PrepareToGlare()
		{
			if (GlareTarget == null) return;
			TurnTo(GlareTarget);
			BroadcastMessage(String.Format(m_glareAnnounce, Name, GlareTarget.Name));
			new RegionTimer(this, new RegionTimerCallback(CastGlare), 5000);
		}

		/// <summary>
		/// Cast glare on the target.
		/// </summary>
		/// <param name="timer">The timer that started this cast.</param>
		/// <returns></returns>
		private int CastGlare(RegionTimer timer)
		{
			// Turn around to the target and cast glare, then go back to the original
			// target, if one exists.

			GameObject oldTarget = TargetObject;
			TargetObject = GlareTarget;
			TurnTo(GlareTarget);
			CastSpell(Glare, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
			GlareTarget = null;
			if (oldTarget != null) TargetObject = oldTarget;
			return 0;
		}

		#endregion

		#region Melee Debuff

		protected Spell m_meleeDebuffSpell;

		/// <summary>
		/// The melee debuff spell. Override this property in your dragon implementation
		/// and assign the spell to m_meleeDebuffSpell.
		/// </summary>
		protected abstract Spell MeleeDebuff
		{
			get;
		}

		private const int m_meleeDebuffChance = 1;

		/// <summary>
		/// Chance to cast a melee debuff.
		/// </summary>
		protected int MeleeDebuffChance
		{
			get { return m_meleeDebuffChance; }
		}

		/// <summary>
		/// Check whether or not to melee debuff this target.
		/// </summary>
		/// <param name="target">The potential target.</param>
		/// <returns>Whether or not the spell was cast.</returns>
		public bool CheckMeleeDebuff(GamePlayer target)
		{
			if (target == null) return false;
			bool success = Util.Chance(MeleeDebuffChance);
			if (success)
				CastMeleeDebuff(target);
			return success;
		}

		/// <summary>
		/// Cast a melee specific debuff on the target.
		/// </summary>
		/// <param name="target"></param>
		private void CastMeleeDebuff(GamePlayer target)
		{
			CastSpell(MeleeDebuff, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
		}

		#endregion

		#region Ranged Debuff

		protected Spell m_rangedDebuffSpell;

		/// <summary>
		/// The melee debuff spell. Override this property in your dragon implementation
		/// and assign the spell to m_rangedDebuffSpell.
		/// </summary>
		protected abstract Spell RangedDebuff
		{
			get;
		}

		private const int m_rangedDebuffChance = 1;

		/// <summary>
		/// Chance to cast nearsight.
		/// </summary>
		protected int RangedDebuffChance
		{
			get { return m_rangedDebuffChance; }
		}

		/// <summary>
		/// Check whether or not to cast a debuff on a ranged target.
		/// </summary>
		/// <param name="target">The potential target.</param>
		/// <returns>Whether or not the spell was cast.</returns>
		public bool CheckRangedDebuff(GamePlayer target)
		{
			if (target == null) return false;
			bool success = Util.Chance(RangedDebuffChance);
			if (success)
				CastRangedDebuff(target);
			return success;
		}

		/// <summary>
		/// Cast nearsight on the target.
		/// </summary>
		/// <param name="target"></param>
		private void CastRangedDebuff(GamePlayer target)
		{
			CastSpell(RangedDebuff, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
		}

		#endregion

		#region Throw

		private const int m_throwChance = 5;

		/// <summary>
		/// Chance to throw a player.
		/// </summary>
		protected int ThrowChance
		{
			get { return m_throwChance; }
		}

		/// <summary>
		/// Check whether or not to throw this target into the air.
		/// </summary>
		/// <param name="target">The potential target.</param>
		/// <returns>Whether or not the target was thrown.</returns>
		public bool CheckThrow(GamePlayer target)
		{
			if (target == null || !target.IsAlive || target.IsStunned)
				return false;

			bool success = Util.Chance(ThrowChance);

			if (success)
				ThrowPlayer(target);

			return success;
		}

		/// <summary>
		/// Hurl a player into the air.
		/// </summary>
		/// <param name="target">The player to hurl into the air.</param>
		private void ThrowPlayer(GamePlayer target)
		{
			BroadcastMessage(String.Format("{0} is hurled into the air!", target.Name));
			
			// Face the target, then push it 600 units up and 400-600 units backwards.

			TurnTo(target);

			Point3D targetPosition = TargetPosition(target, 600, Heading, Util.Random(400, 600) );

			target.MoveTo(target.CurrentRegionID, targetPosition.X, targetPosition.Y, targetPosition.Z, target.Heading);
		}

		/// <summary>
		/// Calculate the target position with given height and displacement.
		/// </summary>
		/// <param name="x">Current object X position.</param>
		/// <param name="y">Current object Y position.</param>
		/// <param name="z">Current object Z position.</param>
		/// <param name="height">Height the object is to be lifted to.</param>
		/// <param name="heading">The direction the object is displaced in.</param>
		/// <param name="displacement">The amount the object is displaced by.</param>
		/// <returns></returns>
		private Point3D TargetPosition( IPoint3D target, int height, int heading, int displacement)
		{
            Point3D targetPoint;

            targetPoint = new Point3D( target.GetPointFromHeading( (ushort)heading, displacement ), target.Z + height );

			return targetPoint;
		}

		#endregion
		
		#region Stun

		private Spell m_stun;

		/// <summary>
		/// The stun spell.
		/// </summary>
		protected virtual Spell Stun
		{
			get
			{
				if (m_stun == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 4123;
					spell.Icon = 4123;
					spell.Duration = 30*m_DragonStrength/100;
					spell.Description = "Stun";
					spell.Name = "Paralyzing Horror";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Damage = 500;
					spell.DamageType = 13;
					spell.RecastDelay = 10;
					spell.SpellID = 6000;
					spell.Target = "Enemy";
					spell.Type = "Stun";
					spell.Message1 = "You cannot move!";
					spell.Message2 = "{0} cannot seem to move!";
					m_stun = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_stun);
				}
				return m_stun;
			}
		}

		private const int m_stunChance = 10;

		/// <summary>
		/// Chance to cast stun.
		/// </summary>
		protected int StunChance
		{
			get { return m_stunChance; }
		}

		/// <summary>
		/// Try to get an AoE stun off.
		/// </summary>
		/// <param name="firstTime">
		/// Whether or not this is the first stun 
		/// (first stun will cast with 100% chance).
		/// </param>
		/// <returns>Whether or not the stun was cast.</returns>
		public bool CheckStun(bool firstTime)
		{
			if (GetSkillDisabledDuration(Stun) == 0 && (firstTime || Util.Chance(StunChance)))
			{
				PrepareToStun();
				return true;
			}
			return false;
		}

		private void PrepareToStun()
		{
			// No announcement for this seemingly.
			new RegionTimer(this, new RegionTimerCallback(CastStun), 5000);
		}

		/// <summary>
		/// Start the 5 second timer for the stun.
		/// </summary>
		private int CastStun(RegionTimer timer)
		{
			CastSpell(Stun, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
			return 0;
		}

		#endregion		
	}
}


