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
 * - Class for turret, like 1.90 EU official servers: Turret isn't interrupted
 */

using DOL.AI.Brain;

namespace DOL.GS
{
  public class TurretPet : GamePet
  {
    public TurretPet(INpcTemplate template)
      : base(template)
    {
    }

  	private Spell turretSpell;

  	/// <summary>
    /// Get first spell only
    /// </summary>
    public Spell TurretSpell
  	{
  		get { return turretSpell; }
  		set { turretSpell = value; }
  	}

	public override void StartMeleeAttack(GameObject attackTarget)
	{
	  if (attackTarget == null)
		return;

	  if (Brain is IControlledBrain)
	  {
		if ((Brain as IControlledBrain).AggressionState == eAggressionState.Passive)
		  return;
		GamePlayer playerowner;
		if ((playerowner = ((IControlledBrain)Brain).GetPlayerOwner()) != null)
		  playerowner.Stealth(false);
	  }

	  TargetObject = attackTarget;
	  if (TargetObject.Realm == 0 || Realm == 0)
		m_lastAttackTickPvE = m_CurrentRegion.Time;
	  else
		m_lastAttackTickPvP = m_CurrentRegion.Time;

	  if (m_attackers.Count == 0)
	  {
		if (SpellTimer == null)
		  SpellTimer = new SpellAction(this);
		if (!SpellTimer.IsAlive)
		  SpellTimer.Start(1);
	  }
	
	  if(Brain is TurretMainPetTankBrain)
	  {
		base.StartMeleeAttack(TargetObject);
	  }
	}

  	/// <summary>
    /// [Ganrod] Nidel: Don't interrupt turret cast.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="attackType"></param>
    /// <param name="attacker"></param>
    public override void StartInterruptTimer(int duration, AttackData.eAttackType attackType, GameLiving attacker)
    {
      return;
    }
  }
}
