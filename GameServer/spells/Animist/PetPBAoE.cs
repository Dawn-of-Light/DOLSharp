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
 * - BugFix: TurretPBAoE can cast on own pet and turret only.
 */
using DOL.AI.Brain;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
  /// <summary>
  /// Summary description for TauntSpellHandler.
  /// </summary>
  [SpellHandler("TurretPBAoE")]
  public class PetPBAoE : DirectDamageSpellHandler
  {
    public PetPBAoE(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine)
    {
    }

    public override bool CheckBeginCast(GameLiving selectedTarget)
    {
      if(!(Caster is GamePlayer))
      {
        return false;
      }
      if((Caster.ControlledNpc == null && Caster.PetCounter < 1) || Caster.ControlledNpc != null && !(Caster.ControlledNpc.Body.Brain is TurretBrain))
      {
        MessageToCaster("You must have a controlled Pet or Turret before casting this spell !", eChatType.CT_SpellResisted);
        return false;
      }
      GameNPC target = selectedTarget as GameNPC;
      //[Ganrod]Nidel: Launch spell on Controlled Pet if target is null, like 1.90 EU off servers.
      if(target == null && Caster.ControlledNpc != null)
      {
        return base.CheckBeginCast(Caster.ControlledNpc.Body);
      }
      if(target == null || target.Brain == null || !(target.Brain is TurretBrain) || (target.Brain as TurretBrain).GetPlayerOwner() != Caster)
      {
        MessageToCaster("You must select your controlled Pet or Turret before casting this spell !", eChatType.CT_SpellResisted);
        return false;
      }
      return base.CheckBeginCast(selectedTarget);
    }

    public override void StartSpell(GameLiving target)
    {
      base.StartSpell(Caster.TargetObject as GameLiving);
    }
  }
}