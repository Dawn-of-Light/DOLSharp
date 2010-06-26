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
 * - Useless using removed
 * - Get Main Pet tank or Main Pet caster by spell damage type
 */
using DOL.AI.Brain;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
  /// <summary>
  /// Spell handler to summon a animist pet.
  /// </summary>
  /// <author>IST</author>
  [SpellHandler("SummonAnimistPet")]
  public class SummonAnimistMainPet : SummonAnimistPet
  {
    public SummonAnimistMainPet(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
    {
    }

    public override bool CheckBeginCast(GameLiving selectedTarget)
    {
      if(Caster is GamePlayer && Caster.ControlledBrain != null)
      {
        MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
        return false;
      }
      return base.CheckBeginCast(selectedTarget);
    }

    protected override IControlledBrain GetPetBrain(GameLiving owner)
    {
      if(Spell.DamageType == 0)
      {
        return new TurretMainPetCasterBrain(owner);
      }
      //[Ganrod] Nidel: Spell.DamageType : 1 for tank pet
      if(Spell.DamageType == (eDamageType) 1)
      {
        return new TurretMainPetTankBrain(owner);
      }
      return base.GetPetBrain(owner);
    }
  }
}