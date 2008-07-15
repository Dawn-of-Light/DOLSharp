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
 * - AI for turret tank, like 1.90 EU officiel servers
 * - Turret tank, cast spell and after cast, launch one body attack, again and again...
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
	  TurretPet pet = sender as TurretPet;
	  base.Notify(e, sender, args);
	  if (pet != null && e == GameLivingEvent.CastFinished && (pet.Brain is TurretMainPetTankBrain) && pet == Body && AggressionState != eAggressionState.Passive)
	  {
		if (Body.TargetObject != null)
		  {
			if (Body.IsCasting)
			{
				Body.StopCurrentSpellcast();
			}
		  	Body.StartAttack(Body.TargetObject);
		  }
		}
	  if (pet != null && e == GameLivingEvent.AttackFinished && (pet.Brain is TurretMainPetTankBrain) && pet.Brain == this && AggressionState != eAggressionState.Passive)
		{
		  Body.StopAttack();
		  AttackMostWanted();
		}
	}

  	protected override void AttackMostWanted()
    {
	  // Force to wait body attack before casting.
	  if(Body.AttackState)
		return;
      base.AttackMostWanted();
      if(Body.TargetObject != null)
      {
        Body.StartAttack(Body.TargetObject);
      }
    }
  }
}