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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Helper Class for Pulse Increasing with default computation
	/// </summary>
	public class PulseIncreaseHelper
	{
		private SpellHandler m_spellHandler;
		
		private byte m_pulseCount;
		
		public byte PulseCount
		{
			get
			{
				return m_pulseCount;
			}
		}
		
		public PulseIncreaseHelper(SpellHandler handler)
		{
			m_spellHandler = handler;
			m_pulseCount = 0;
		}
		
		public void IncrementPulseCount()
		{
			++m_pulseCount;
		}
		
		public double GetIncreasedEffectiveness(double effectiveness)
		{
			double pulseEffectiveness = Math.Min( 1 + ((PulseCount - 1) * (m_spellHandler.Spell.ResurrectHealth*-0.01)), (m_spellHandler.Spell.ResurrectMana*-0.01));
			
			return effectiveness*pulseEffectiveness;
		}
		
		public bool OnSpellPulse(PulsingSpellEffect effect)
		{
			IncrementPulseCount();
			
			m_spellHandler.MessageToCaster("You concentrated on the spell!", eChatType.CT_Spell);
			return true;
		}
	}
}
