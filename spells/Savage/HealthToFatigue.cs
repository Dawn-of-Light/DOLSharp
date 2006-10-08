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
            if (m_caster.Mana < CalculateNeededPower(selectedTarget))
            {
                if (!HasEnoughHealth()) 
			    {
				MessageToCaster("You don't have enough health to use this spell!" , eChatType.CT_Spell);
				return false;
			    }
                else if (m_caster.Endurance == m_caster.MaxEndurance)
                {
                    MessageToCaster("You already have full endurance!", eChatType.CT_Spell);
                    return false;
                }
                else
                    return true;
            }

			

			return base.CheckBeginCast (selectedTarget);
		}

		protected virtual bool HasEnoughHealth()
		{
            int HealthLost = Convert.ToInt32(m_caster.MaxHealth * (Spell.Power *0.01));
            if(m_caster.Health <= HealthLost)
                return false;
            return true;
		}

		/// <summary>
		/// Execute damage over time spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			base.FinishSpellCast(target);

            m_caster.Endurance += 5;

            int HealthLost = Convert.ToInt32(m_caster.MaxHealth * (Spell.Power * 0.01));
			if (m_caster.Health < HealthLost) 
				HealthLost = m_caster.MaxHealth - m_caster.Health; 
			
			m_caster.ChangeHealth(m_caster, GameLiving.eHealthChangeType.Spell, -HealthLost);
			GiveEndurance(m_caster, (int)m_spell.Value);			
		}
		
		protected virtual void GiveEndurance(GameLiving target, int amount)
		{
			
			if (target.Endurance >= amount)
				amount = target.MaxEndurance - target.Endurance;

			target.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, amount);
			MessageToCaster("You transfer "+amount+ " life to Endurance!" , eChatType.CT_Spell);
			
		}

		// constructor
		public HealthToEndurance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
