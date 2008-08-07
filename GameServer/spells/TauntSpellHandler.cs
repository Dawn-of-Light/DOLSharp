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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Summary description for TauntSpellHandler.
	/// </summary>
	[SpellHandler("Taunt")]
	public class TauntSpellHandler : SpellHandler
	{
		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;

			SendEffectAnimation(target, 0, false, 1);

			// calc damage
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			//DamageTarget(ad, true);
			SendDamageMessages(ad);
		
			// Interrupt only if target is actually casting
			if (target.IsCasting && Spell.Target.ToLower()!="cone")
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
				
			if (target is GameNPC)
			{
				GameNPC npc = (GameNPC)target;
				IAggressiveBrain aggroBrain = npc.Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
		}

		/// <summary>
		/// When spell was resisted
		/// </summary>
		/// <param name="target">the target that resisted the spell</param>
		protected override void OnSpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);

			// Interrupt only if target is actually casting
			if (target.IsCasting && Spell.Target.ToLower()!="cone")
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}

		public TauntSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}
