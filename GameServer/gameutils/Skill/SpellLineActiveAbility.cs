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

using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// Active Spell Line Ability Handler.
    /// Trigger Spell Casting in Described Spell Line using spell available at given Ability Level.
    /// </summary>
    public class SpellLineActiveAbility : SpellLineAbstractAbility
	{
		/// <summary>
		/// Execute Handler
		/// Cast the According Spell
		/// </summary>
		/// <param name="living">Living Executing Ability</param>
		public override void Execute(GameLiving living)
		{
			base.Execute(living);
			
			if (Spell != null && SpellLine != null)
				living.CastSpell(this);
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="dba"></param>
		/// <param name="level"></param>
		public SpellLineActiveAbility(DBAbility dba, int level)
			: base(dba, level)
		{
		}
	}
}
