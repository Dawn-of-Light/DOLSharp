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
using DOL.Events;
using DOL.GS;

namespace DOL.AI.Brain
{
  public class TurretMainPetTankBrain : TurretMainPetCasterBrain
  {
    public TurretMainPetTankBrain(GameLiving owner) : base(owner) { }

	public override void Notify(DOLEvent e, object sender, System.EventArgs args)
	{
		base.Notify(e, sender, args);
		if(AggressionState != eAggressionState.Passive)
		{
			if(e == GameLivingEvent.CastFinished || e == GameLivingEvent.AttackFinished)
			{
				TurretPet pet = sender as TurretPet;
				if(pet == null || pet != Body || !(pet.Brain is TurretMainPetTankBrain))
					return;

				if(e == GameLivingEvent.CastFinished)
				{
					if(Body.TargetObject != null)
					{
						//Force to stop spell casting
						if(Body.IsCasting)
						{
							Body.StopCurrentSpellcast();
						}
						if(Body.SpellTimer != null && Body.SpellTimer.IsAlive)
						{
							Body.SpellTimer.Stop();
						}
						Body.StartAttack(Body.TargetObject);
					}
					return;
				}
				if(e == GameLivingEvent.AttackFinished)
				{
					Body.StopAttack();
					AttackMostWanted();
				}
			}
		}
	}

  	protected override void AttackMostWanted()
	{
		// Force to wait body attack before casting.
		if(Body.AttackState)
		{
			return;
		}
		CheckSpells(eCheckSpellType.Offensive);
	}

  	protected override void OnAttackedByEnemy(AttackData ad)
	{
	  if (AggressionState != eAggressionState.Passive)
	  {
		AddToAggroList(ad.Attacker, (ad.Attacker.Level + 1) << 1);
	  	AttackMostWanted();
	  }
	}
  }
}