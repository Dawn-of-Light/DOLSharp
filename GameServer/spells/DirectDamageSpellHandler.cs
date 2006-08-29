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
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("DirectDamage")]
	public class DirectDamageSpellHandler : SpellHandler
	{
		/// <summary>
		/// Execute direct damage spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute direct effect
		/// </summary>
		/// <param name="target">target that gets the damage</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{			
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;

			// calc damage
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			DamageTarget(ad, true);
			SendDamageMessages(ad);			
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
		}

		/// <summary>
		/// Calculates chance of spell getting resisted
		/// </summary>
		/// <param name="target">the target of the spell</param>
		/// <returns>chance that spell will be resisted for specific target</returns>
		public override int CalculateSpellResistChance(GameLiving target)
		{
			// hit chance can't be lower than 55% (?)
			return Math.Min(45, 100-CalculateToHitChance(target));
		}

		// constructor
		public DirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
