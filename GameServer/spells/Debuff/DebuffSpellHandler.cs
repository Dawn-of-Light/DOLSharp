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

using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Any Non-Positive Effect Spell Handler can inherit this to have a common behavior
	/// Not Meant to be instanciated
	/// </summary>
	public abstract class DebuffSpellHandler : SpellHandler
	{
		/// <summary>
		/// Buff Should always have positive Effect...
		/// </summary>
		public override bool HasPositiveEffect {
			get { return false; }
		}
		
		/// <summary>
		/// Trigger Attack Timer on debuff start.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			if (effect.Owner.Realm == 0 || Caster.Realm == 0)
			{
				effect.Owner.LastAttackedByEnemyTickPvE = effect.Owner.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				effect.Owner.LastAttackedByEnemyTickPvP = effect.Owner.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		public override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			// debuff duration should only be reduced by resist base.
			int duration = base.CalculateEffectDuration(target, effectiveness);
			duration = (int)(duration * GetResistBaseRatio(target, effectiveness));

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			
			return duration;
		}
		
		public DebuffSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}