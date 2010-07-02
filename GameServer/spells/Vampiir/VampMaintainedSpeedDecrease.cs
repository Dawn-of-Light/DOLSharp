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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler for speed decreasing spells.  Special for vampiirs
	/// </summary>
	[SpellHandler("VampSpeedDecrease")]
	public class VampMaintainedSpeedDecrease : SpeedDecreaseSpellHandler
	{
		public VampMaintainedSpeedDecrease(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		/// <summary>
		/// Creates the corresponding spell effect for the spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			//We don't want an immunity timer for this
			return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
		}

		/// <summary>
		/// Check each pulse to see if spell can be maintained
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectPulse(GameSpellEffect effect)
		{
			GameLiving t = effect.Owner;
			GameLiving target = t.TargetObject as GameLiving;

			if (m_caster.Mana < Spell.PulsePower)
			{
				effect.Cancel(false);
				return;
			}

			if (!m_caster.IsAlive || 
				!effect.Owner.IsAlive || 
				!m_caster.IsWithinRadius(effect.Owner, Spell.Range) || 
				m_caster.IsMezzed || 
				m_caster.IsStunned || 
				effect.Owner.ObjectState != GameObject.eObjectState.Active ||
				(m_caster.TargetObject is GameLiving ? effect.Owner != m_caster.TargetObject as GameLiving : true))
			{
				effect.Cancel(false);
				return;
			}
			if (!m_caster.TargetInView)
			{
				effect.Cancel(false);
				return;
			}

			base.OnEffectPulse(effect);

			m_caster.Mana -= effect.Spell.PulsePower;
		}


		protected override void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			// Spell can be used in combat, do nothing
		}

	}
}
