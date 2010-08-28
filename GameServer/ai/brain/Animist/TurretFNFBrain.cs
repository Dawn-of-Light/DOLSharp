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
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.Spells;

namespace DOL.AI.Brain
{
	public class TurretFNFBrain : TurretBrain
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public TurretFNFBrain(GameLiving owner) : base(owner)
		{
		}

		/// <summary>
		/// Get a random target from aggro table
		/// </summary>
		/// <returns></returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			List<GameLiving> newTargets = new List<GameLiving>();
			List<GameLiving> oldTargets = new List<GameLiving>();
			base.CalculateNextAttackTarget();
			lock(m_aggroTable.SyncRoot)
			{
				foreach(GameLiving living in m_aggroTable.Keys)
				{
					if(!living.IsAlive || living.CurrentRegion != Body.CurrentRegion || living.ObjectState != GameObject.eObjectState.Active)
						continue;

					if (living.IsMezzed || living.IsStealthed)
						continue;

					if (!Body.IsWithinRadius(living, MAX_AGGRO_DISTANCE, true))
						continue;

					if (!Body.IsWithinRadius(living, ((TurretPet)Body).TurretSpell.Range, true))
						continue;

					if (((TurretPet)Body).TurretSpell.SpellType != "SpeedDecrease" && SpellHandler.FindEffectOnTarget(living, "SpeedDecrease") != null)
						continue;

					if (((TurretPet)Body).TurretSpell.SpellType == "SpeedDecrease" && living.HasAbility(Abilities.RootImmunity))
						continue;

					newTargets.Add(living);
				}
			}

			foreach (GamePlayer living in Body.GetPlayersInRadius((ushort)((TurretPet)Body).TurretSpell.Range, Body.CurrentRegion.IsDungeon ? false : true))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(Body, living, true))
                    continue;

                if (living.IsPvPInvulnerability)
                    continue;

                if (!living.IsAlive || living.CurrentRegion != Body.CurrentRegion || living.ObjectState != GameObject.eObjectState.Active)
                    continue;

                if (living.IsMezzed || living.IsStealthed)
                    continue;

                if (((TurretPet)Body).TurretSpell.SpellType != "SpeedDecrease" && SpellHandler.FindEffectOnTarget(living, "SpeedDecrease") != null)
                    continue;

				if (LivingHasEffect(living, ((TurretPet)Body).TurretSpell))
				{
					oldTargets.Add(living);
				}
				else
				{
					newTargets.Add(living as GameLiving);
				}
            }

			foreach (GameNPC living in Body.GetNPCsInRadius((ushort)((TurretPet)Body).TurretSpell.Range, Body.CurrentRegion.IsDungeon ? false : true))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(Body, living, true))
                    continue;

                if (!living.IsAlive || living.CurrentRegion != Body.CurrentRegion || living.ObjectState != GameObject.eObjectState.Active)
                    continue;

                if (living.IsMezzed || living.IsStealthed)
                    continue;

                if (((TurretPet)Body).TurretSpell.SpellType != "SpeedDecrease" && SpellHandler.FindEffectOnTarget(living, "SpeedDecrease") != null)
                    continue;

                if (((TurretPet)Body).TurretSpell.SpellType == "SpeedDecrease" && (living.HasAbility(Abilities.RootImmunity) || living.HasAbility(Abilities.DamageImmunity)))
                    continue;

				if (LivingHasEffect(living, ((TurretPet)Body).TurretSpell))
				{
					oldTargets.Add(living);
				}
				else
				{
					newTargets.Add(living as GameLiving);
				}
			}

			// always favor previous targets and new targets that have not been attacked first, then re-attack old targets

            if (newTargets.Count > 0)
			{
				return newTargets[Util.Random(newTargets.Count - 1)];
			}
			else if (oldTargets.Count > 0)
			{
				return oldTargets[Util.Random(oldTargets.Count - 1)];
			}

			m_aggroTable.Clear();
			return null;
		}

		protected override void OnAttackedByEnemy(AttackData ad)
		{
			AddToAggroList(ad.Attacker, (ad.Attacker.Level + 1) << 1);
		}

		/// <summary>
    /// Updates the pet window
    /// </summary>
    public override void UpdatePetWindow()
    {
    }
  }
}