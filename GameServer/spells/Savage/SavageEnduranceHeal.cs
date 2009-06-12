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
	///Handlers for the savage's special endurance heal that takes health instead of mana
	/// </summary>
	[SpellHandlerAttribute("SavageEnduranceHeal")]
	public class SavageEnduranceHeal : EnduranceHealSpellHandler
	{
		public SavageEnduranceHeal(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		protected override void RemoveFromStat(int value)
		{
			m_caster.Health -= value;
		}

		public override int PowerCost(GameLiving target)
		{
			int cost = 0;
			if (m_spell.Power < 0)
				cost = (int)(m_caster.MaxHealth * Math.Abs(m_spell.Power) * 0.01);
			else
				cost = m_spell.Power;
			return cost;
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			int cost = PowerCost(Caster);
			if (Caster.Health < cost)
			{
				MessageToCaster("You do not have enough health to cast that!", eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckBeginCast(Caster);
		}
	}
}
