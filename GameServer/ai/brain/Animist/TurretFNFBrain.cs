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
/*
 * [Ganrod] Nidel 2008-07-08
 * - Turret find target if needed and Cast spell
 */
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Spells;


namespace DOL.AI.Brain
{
	public class TurretFNFBrain : TurretBrain
	{
		public TurretFNFBrain(GameLiving owner) : base(owner)
		{
		}

		/// <summary>
		/// [Ganrod] Nidel:
		/// Cast only Offensive or Defensive spell.
		/// <para>If Offensive spell is true, Defensive spell isn't casted.</para>
		/// </summary>
		public override void Think()
		{
			GamePlayer playerowner = GetPlayerOwner();
			if(!playerowner.CurrentUpdateArray[Body.ObjectID - 1])
			{
				playerowner.Out.SendObjectUpdate(Body);
				playerowner.CurrentUpdateArray[Body.ObjectID - 1] = true;
			}

			if(CheckSpells(eCheckSpellType.Offensive))
			{
				return;
			}
			if(CheckSpells(eCheckSpellType.Defensive))
			{
				// don't need target for radius spell
				if(((TurretPet) Body).TurretSpell.Radius == 0)
				{
					Body.TargetObject = GetDefensiveTarget(((TurretPet) Body).TurretSpell);
				}
				TrustCast(((TurretPet) Body).TurretSpell);
				return;
			}
			if(Body.IsAttacking)
			{
				Body.StopAttack();
			}

			if(Body.SpellTimer != null && Body.SpellTimer.IsAlive)
			{
				Body.SpellTimer.Stop();
			}
		}

		protected override void AttackMostWanted()
		{
			if(!IsActive)
			{
				return;
			}
			if(((TurretPet) Body).TurretSpell.Radius == 0)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
				GameLiving target = CalculateNextAttackTarget();
				if(target != null)
				{
					Body.TargetObject = target;
					TrustCast(((TurretPet) Body).TurretSpell);
				}
			}
			TrustCast(((TurretPet) Body).TurretSpell);
		}


	/// <summary>
	/// Get a random target from aggro table
	/// </summary>
	/// <returns></returns>
		protected override GameLiving CalculateNextAttackTarget()
	{
		List<GameLiving> livingList = new List<GameLiving>(3);
		lock(m_aggroTable.SyncRoot)
		{
			foreach(GameLiving living in m_aggroTable.Keys)
			{
				if(living.EffectList.GetOfType(typeof(NecromancerShadeEffect)) != null)
					continue;

				if(!living.IsAlive || living.CurrentRegion != Body.CurrentRegion || living.ObjectState != GameObject.eObjectState.Active)
					continue;

				if(WorldMgr.GetDistance(Body, living) > ((TurretPet) Body).TurretSpell.Range)
					continue;

				if(GetPlayerOwner().IsObjectGreyCon(living))
					continue;

				if(living.IsMezzed || living.IsStealthed)
					continue;

				GameSpellEffect root = SpellHandler.FindEffectOnTarget(living, "SpeedDecrease");
				if(root != null && root.Spell.Value == 99) continue;

				livingList.Add(living);
			}
		}
		m_aggroTable.Clear();
		if(livingList.Count > 0)
		{
			return livingList[Util.Random(livingList.Count - 1)];
		}
		return null;
	}

		/// <summary>
    /// Updates the pet window
    /// </summary>
    public override void UpdatePetWindow()
    {
    }
  }
}