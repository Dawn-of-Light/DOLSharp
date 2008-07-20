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
    /// Summary description for TurretPBAoESpellHandler.
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
            GameNPC target = selectedTarget as GameNPC;
            //[Ganrod]Nidel: Launch spell on Controlled Pet if target is null
            // or if taget isn't our controlled npc, like 1.90 EU off servers.
            if (target == null || !Caster.GetItsControlledNpc(target))
            {
                if (Caster.ControlledNpc != null && Caster.ControlledNpc.Body != null)
                {
                    if (!Caster.GetItsControlledNpc(target))
                    {
                        Caster.TargetObject = Caster.ControlledNpc.Body;
                        return base.CheckBeginCast(Caster.ControlledNpc.Body);
                    }
                }
                MessageToCaster("You must select your controlled Pet or Turret before casting this spell !",
                                eChatType.CT_SpellResisted);
                return false;
            }
		    return base.CheckBeginCast(target);
		}

		public override void FinishSpellCast(GameLiving target)
		{
			//Make pet Caster spell
			m_caster = Caster.TargetObject as GameLiving;

			if (m_caster == null) return;

			base.FinishSpellCast(target);
		}

		public override void StartSpell(GameLiving target)
		{
			base.StartSpell(Caster.TargetObject as GameLiving);
		}

		public override void DamageTarget(AttackData ad, bool showEffectAnimation)
		{
			base.DamageTarget(ad, showEffectAnimation);
			if(ad.Damage > 0 && ad.Target is GameNPC)
			{
				//pet will take aggro
				ad.Attacker = Caster;
				IAggressiveBrain aggroBrain = ((GameNPC) ad.Target).Brain as IAggressiveBrain;
				if(aggroBrain != null)
				{
					aggroBrain.AddToAggroList(ad.Attacker, ad.Attacker.Level*10);
				}
			}
		}
	}
}