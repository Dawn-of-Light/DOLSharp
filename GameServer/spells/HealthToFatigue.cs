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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Damage Over Time spell handler
	/// </summary>
	[SpellHandlerAttribute("HealthToEndurance")]
	public class HealthToEndurance : SpellHandler
	{

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (m_caster.Endurance == m_caster.MaxEndurance)
			{
				MessageToCaster("You already have full endurance!", eChatType.CT_Spell);
				return false;
			}

			return base.CheckBeginCast(selectedTarget);
		}

		/// <summary>
		/// Execute damage over time spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			base.FinishSpellCast(target);

			GiveEndurance(m_caster, (int)m_spell.Value);
			OnEffectExpires(null, true);
		}

		public override int CalculateEnduranceCost()
		{
			return 0;
		}

		protected virtual void GiveEndurance(GameLiving target, int amount)
		{
			if (target.Endurance >= amount)
				amount = target.MaxEndurance - target.Endurance;

			target.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, amount);
			MessageToCaster("You transfer " + amount + " life to Endurance!", eChatType.CT_Spell);
		}

		// constructor
		public HealthToEndurance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
