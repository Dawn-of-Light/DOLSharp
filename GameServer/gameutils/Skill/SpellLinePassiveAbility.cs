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

using DOL.GS.Spells;
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// SpellLinePassiveAbility is a Specific Ability that Will trigger Self-Buff when activated based on Attached Spell Line
	/// Level Change should trigger cast of higher level spells, and cancel previous ones
	/// </summary>
	public class SpellLinePassiveAbility : SpellLineAbstractAbility
	{
		
		public override void Activate(GameLiving living, bool sendUpdates)
		{
			base.Activate(living, sendUpdates);
			
			var spell = CurrentSpell;
			var line = CurrentSpellLine;
			
			if (line != null && spell != null && spell.Target.ToLower().Equals("self"))
			{
				living.CastSpell(spell, line);
			}
		}
		
		public override void OnLevelChange(int oldLevel, int newLevel = 0)
		{
			base.OnLevelChange(oldLevel, newLevel);
			
			// deactivate old spell and activate new one
			if (m_activeLiving != null)
			{
				var oldSpell = GetSpellForLevel(oldLevel);
				
				if (oldSpell != null)
				{
					var pulsing = m_activeLiving.FindPulsingSpellOnTarget(oldSpell);
					if (pulsing != null)
						pulsing.Cancel(false);
					
					var effect = m_activeLiving.FindEffectOnTarget(oldSpell);
					if (effect != null)
						effect.Cancel(false);
				}
				
				var spell = CurrentSpell;
				var line = CurrentSpellLine;
				if (line != null && spell != null && spell.Target.ToLower().Equals("self"))
				{
					m_activeLiving.CastSpell(spell, line);
				}
			}
		}
		
		public override void Deactivate(GameLiving living, bool sendUpdates)
		{
			var spell = CurrentSpell;
			var line = CurrentSpellLine;
			
			// deactivate spell
			if (m_activeLiving != null && line != null && spell != null)
			{				
					var pulsing = m_activeLiving.FindPulsingSpellOnTarget(spell);
					if (pulsing != null)
						pulsing.Cancel(false);
					
					var effect = m_activeLiving.FindEffectOnTarget(spell);
					if (effect != null)
						effect.Cancel(false);
			}
			
			base.Deactivate(living, sendUpdates);
		}
		
		public SpellLinePassiveAbility(DBAbility dba, int level)
			: base(dba, level)
		{
		}
	}
}
