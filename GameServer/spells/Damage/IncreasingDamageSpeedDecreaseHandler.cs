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
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Damage with Speed Decrease Increasing on Each Pulse
	/// </summary>
	[SpellHandlerAttribute("IncreasingDamageSpeedDecrease")]
	public class IncreasingDamageSpeedDecreaseSpellHandler : DamageSpeedDecreaseSpellHandler
	{
		/// <summary>
		/// Helper for Pulse Increase
		/// </summary>
		private PulseIncreaseHelper m_pulseHelper;
		
		/// <summary>
		/// Increase Pulse Count when spell Pulse.
		/// </summary>
		public override void OnSpellPulse(PulsingSpellEffect effect)
		{
			if(m_pulseHelper.OnSpellPulse(effect))
				base.OnSpellPulse(effect);
		}
		
		/// <summary>
		/// Increase damage with each pulse
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			effectiveness = m_pulseHelper.GetIncreasedEffectiveness(effectiveness);
			return base.CalculateDamageToTarget(target, effectiveness);
		}
		
		/// <summary>
		/// Sends the cast animation
		/// Special Fix for Heretic Focus
		/// </summary>
		/// <param name="castTime">The cast time</param>
		/// <param name="clientEffect">Cast Animation</param>
		public override void SendCastAnimation(ushort castTime, ushort clientEffect)
		{
			if (Spell.ID > 14063 && Spell.ID < 14072)
				clientEffect = (ushort)(clientEffect - 10);
			
			base.SendCastAnimation(castTime, clientEffect);
		}
        
		public IncreasingDamageSpeedDecreaseSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			m_pulseHelper = new PulseIncreaseHelper(this);
		}
	}
}
