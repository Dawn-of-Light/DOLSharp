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
	/// Speed Decrease Increasing on Each Pulse
	/// </summary>
	[SpellHandlerAttribute("IncreasingEffectivenessSpeedDecrease")]
	public class IncreasingEffectivenessSpeedDecreaseHandler : SingleStatDebuff
	{
		/// <summary>
		/// Helper for Pulse Increase
		/// </summary>
		private PulseIncreaseHelper m_pulseHelper;

		/// <summary>
		/// Debuff Effectiveness
		/// </summary>
		public override eProperty Property1 { get { return eProperty.LivingEffectiveness; } }
		
		/// <summary>
		/// Increase Pulse Count when spell Pulse.
		/// </summary>
		public override void OnSpellPulse(PulsingSpellEffect effect)
		{
			if(m_pulseHelper.OnSpellPulse(effect))
			{
				base.OnSpellPulse(effect);
			}
		}
		
		/// <summary>
		/// Apply with effectiveness increase
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, m_pulseHelper.GetIncreasedEffectiveness(effectiveness));
		}
		
		/// <summary>
		/// Add snare to effectiveness Debuff
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			effect.Owner.BuffBonusMultCategory1.Set(eProperty.MaxSpeed, effect, 1.0-Spell.Value*effect.Effectiveness*0.01);
			
			if(effect.Owner is GamePlayer)
				((GamePlayer)effect.Owner).Out.SendUpdateMaxSpeed();
			
			base.OnEffectStart(effect);
		}
		
		/// <summary>
		/// Remove Debuff
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			effect.Owner.BuffBonusMultCategory1.Remove(eProperty.MaxSpeed, effect);
			
			if(effect.Owner is GamePlayer)
				((GamePlayer)effect.Owner).Out.SendUpdateMaxSpeed();
			
			return base.OnEffectExpires(effect, noMessages);
		} 
		
		public IncreasingEffectivenessSpeedDecreaseHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			m_pulseHelper = new PulseIncreaseHelper(this);
		}
	}
}
