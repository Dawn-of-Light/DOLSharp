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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandler("Taunt")]
	public class TauntSpellHandler : SpellHandler
	{
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;
			
			// no animation on stealthed players
			if (target is GamePlayer)
				if ( target.IsStealthed ) 
					return;
			
			SendEffectAnimation(target, 0, false, 1);

			// Create attack data.
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			DamageTarget(ad, false);

			// Interrupt only if target is actually casting
			if (target.IsCasting && Spell.Target.ToLower() != "cone")
				target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
		}

		protected override void OnSpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);

			// Interrupt only if target is actually casting
			if (target.IsCasting && Spell.Target.ToLower() != "cone")
				target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
		}

		public override void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			base.DamageTarget(ad, showEffectAnimation, attackResult);

			if (ad.Target is GameNPC && Spell.Value > 0)
			{
				IOldAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
				{
					// this amount is a wild guess - Tolakram
					aggroBrain.AddToAggroList(Caster, Math.Max(1, (int)(Spell.Value * Caster.Level * 0.1)));
					//log.DebugFormat("Damage: {0}, Taunt Value: {1}, Taunt Amount {2}", ad.Damage, Spell.Value, Math.Max(1, (int)(Spell.Value * Caster.Level * 0.1)));
				}
			}

			m_lastAttackData = ad;
		}

		public TauntSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}

		public override string ShortDescription
			=> $"Taunts target, increasing your threat against it by {Spell.Value}.";
	}
}
