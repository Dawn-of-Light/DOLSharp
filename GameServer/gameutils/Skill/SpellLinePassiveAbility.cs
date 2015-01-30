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
	/// Description of SpellLinePassiveAbility.
	/// </summary>
	public class SpellLinePassiveAbility : SpellLineAbstractAbility
	{
		public override void Activate(GameLiving living, bool sendUpdates)
		{
			base.Activate(living, sendUpdates);
			
			if (CurrentSpellLine != null && CurrentSpell != null && CurrentSpell.Target.ToLower().Equals("self"))
			{
				living.CastSpell(CurrentSpell, CurrentSpellLine);
			}
		}
		
		public override void OnLevelChange(int oldLevel, int newLevel = 0)
		{
			base.OnLevelChange(oldLevel, newLevel);
			// TODO Cancel Previous Spell And Start New One
		}
		
		public override void Deactivate(GameLiving living, bool sendUpdates)
		{
			base.Deactivate(living, sendUpdates);
			// TODO Cancel Current Spell
		}
		
		public SpellLinePassiveAbility(DBAbility dba, int level)
			: base(dba, level)
		{
		}
	}
}
