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
 * - AI for turret caster, like 1.90 EU official servers
 */
using DOL.GS;

namespace DOL.AI.Brain
{
  public class TurretMainPetCasterBrain : TurretBrain
  {
    public TurretMainPetCasterBrain(GameLiving owner) : base(owner)
    {
    }

    public override void Think()
    {
      if(AggressionState == eAggressionState.Aggressive)
      {
        CheckPlayerAggro();
        CheckNPCAggro();
        AttackMostWanted();
        return;
      }

      if(AggressionState != eAggressionState.Passive)
      {
        AttackMostWanted();
      }
    }

    public override void Attack(GameObject target)
    {
      GameLiving defender = target as GameLiving;
      if(defender == null)
      {
        return;
      }

      if(!GameServer.ServerRules.IsAllowedToAttack(Body, defender, true))
      {
        return;
      }

      if(Body.IsCasting)
      {
        Body.StopCurrentSpellcast();
      }
      if(AggressionState == eAggressionState.Passive)
      {
        AggressionState = eAggressionState.Defensive;
        UpdatePetWindow();
      }
      m_orderAttackTarget = defender;

      AttackMostWanted();
      return;
    }

    protected override void AttackMostWanted()
    {
      if(!IsActive)
      {
        return;
      }

      GameLiving target = CalculateNextAttackTarget();
      if(target == null || !target.IsAlive)
      {
        return;
      }

      Body.TargetObject = target;
      TrustCast(((TurretPet) Body).TurretSpell);
    }

    protected override void OnFollowLostTarget(GameObject target)
    {
    }

    protected override void OnAttackedByEnemy(AttackData ad)
    {
      if(AggressionState != eAggressionState.Passive)
      {
        AttackMostWanted();
      }
    }
  }
}