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
using DOL.GS;
using log4net;
using System.Reflection;
using DOL.Events;


namespace DOL.AI.Brain
{
	/// <summary>
	/// AI for dragon like NPCs.
	/// </summary>
    public class DragonBrain : StandardMobBrain
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Create a new DragonBrain.
        /// </summary>
        public DragonBrain()
            : base()
        {
        }

		/// <summary>
		/// The brain main loop. Do necessary health checks first and take
		/// any actions, if necessary. If everything's fine either pick a
		/// player to Glare at or to throw around.
		/// </summary>
        public override void Think()
        {
			// Don't call base.Think() for now, I don't trust it...
			// While returning to the spawn point we don't need to think.

			if (Body.IsReturningHome) return;

			// When dragon regenerates to full health reset stage to 10.

			if (Body.HealthPercent == 100 && Stage < 10)
				Stage = 10;

			// If we aren't already aggroing something, look out for
			// someone we can aggro on and attack right away.

			if (!IsAggroing && AggroLevel > 0)
			{
				CheckPlayerAggro();
				if (IsAggroing)
				{
					AttackMostWanted();
					return;
				}
			}
			
            // If dragon has run out of tether range, clear aggro list and let it 
            // return to its spawn point.
 
            if (CheckTether())
            {
                Body.StopFollow();
                ClearAggroList();
                Body.WalkToSpawn();
                return;
            }

			if (CheckHealth()) return;
			if (PickGlareTarget()) return;
			PickThrowTarget();
        }

		/// <summary>
		/// Called whenever the dragon's body sends something to its brain.
		/// </summary>
		/// <param name="e">The event that occured.</param>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The event details.</param>
		public override void Notify(DOL.Events.DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);
			if (sender == Body)
			{
				GameDragon dragon = sender as GameDragon;
				if (e == GameObjectEvent.TakeDamage)
				{
					if (CheckHealth()) return;

					// Someone hit the dragon. If the attacker is in melee range, there
					// is a chance the dragon will cast a debuff specific to melee
					// classes on him, if not, well, dragon will try to get its Glare off...

					GameObject source = (args as TakeDamageEventArgs).DamageSource;
					if (WorldMgr.CheckDistance(dragon, source, dragon.AttackRange))
						dragon.CheckMeleeDebuff(source as GamePlayer);
					else dragon.CheckGlare(source as GamePlayer);
				}
				else if (e == GameLivingEvent.EnemyHealed)
				{
					// Someone healed an enemy. If the healer is in melee range, there
					// is a chance the dragon will cast a debuff specific to ranged
					// classes on him, if not, there's still Glare...

					GameObject source = (args as EnemyHealedEventArgs).HealSource;
					if (WorldMgr.CheckDistance(dragon, source, dragon.AttackRange))
						dragon.CheckRangedDebuff(source as GamePlayer);
					else dragon.CheckGlare(source as GamePlayer);
				}
			}
			else if (e == GameNPCEvent.ArriveAtTarget && sender != null)
			{
				// Message from another NPC, such as a retriever,
				// for example.

				log.Info(String.Format("DragonBrain.Notify: ArriveAtTarget({0})", (sender as GameObject).Name));
				(Body as GameDragon).OnRetrieverArrived(sender as GameNPC);
			}
        }

        #region Tether

        /// <summary>
        /// Check whether dragon is out of tether range.
        /// </summary>
        /// <returns>True if dragon has reached the end of its tether.</returns>
        private bool CheckTether()
        {
            GameDragon dragon = Body as GameDragon;
            if (dragon == null) return false;
            return !WorldMgr.CheckDistance(dragon, dragon.SpawnX, dragon.SpawnY, dragon.SpawnZ, dragon.TetherRange);
        }

        #endregion

        #region Health Check

		private int m_stage = 10;

		/// <summary>
		/// This keeps track of the stage the encounter is in, so players
		/// don't have to go through all the PBAoE etc. again, just because
		/// the dragon regains a small amount of health. Starts at 10 (full
		/// health) and drops to 0.
		/// </summary>
		public int Stage
		{
			get { return m_stage; }
			set { if (value >= 0 && value <= 10) m_stage = value; }
		}

        /// <summary>
		/// Actions to be taken into consideration when health drops.
		/// </summary>
		/// <returns>Whether any action was taken.</returns>
		private bool CheckHealth()
		{
			GameDragon dragon = Body as GameDragon;
			if (dragon == null) return false;

			int healthOld = dragon.HealthPercentOld / 10;
			int healthNow = dragon.HealthPercent / 10;

			// Stun when health drops below 30%.

			if (healthNow < 3)
			{
				if (dragon.CheckStun(healthNow < healthOld))
					return true;
			}

			if (healthNow < healthOld && Stage > healthNow)
			{
				Stage = healthNow;

				// Breathe at 89%/79%/69%/49% and 9%.

				switch (healthNow)
				{
					case 8:
					case 7:
					case 6:
					case 4:
					case 0: if (dragon.CheckBreath())
							return true;
						break;
				}

				// Spawn adds at 49% and 19% (hunch).

				switch (healthNow)
				{
					case 5:
					case 3: if (dragon.CheckAddSpawns())
							return true;
						break;
				}
			}
			return false;
		}

		#endregion

		#region Glare

		/// <summary>
		/// Try to find a potential target for Glare.
		/// </summary>
		/// <returns>Whether or not a target was picked.</returns>
		private bool PickGlareTarget()
		{
			GameDragon dragon = Body as GameDragon;
			if (dragon == null) return false;

            lock (m_aggroTable.SyncRoot)
            {
				foreach (DictionaryEntry dictEntry in m_aggroTable)
				{
					GamePlayer player = dictEntry.Key as GamePlayer;
					if (player != null && !WorldMgr.CheckDistance(dragon, player, dragon.AttackRange))
						return dragon.CheckGlare(player);
				}
            }
			return false;
		}

		#endregion

        #region Throw

		/// <summary>
		/// Pick a target to hurl into the air.
		/// </summary>
		/// <returns>Whether or not a target was picked.</returns>
		private bool PickThrowTarget()
		{
			GameDragon dragon = Body as GameDragon;
			if (dragon == null) return false;

			ArrayList inRangePlayers = new ArrayList();
			foreach (GamePlayer player in dragon.GetPlayersInRadius((ushort) dragon.AttackRange))
				if (player.IsAlive) inRangePlayers.Add(player);
			if (inRangePlayers.Count > 0)
				return dragon.CheckThrow((GamePlayer)(inRangePlayers[Util.Random(1, inRangePlayers.Count) - 1]));
			return false;
		}

        #endregion
    }
}
