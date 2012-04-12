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

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for UseSpell event of GamePlayer.
	/// </summary>
	public class UseSpellEventArgs : EventArgs
	{
		private int m_spell;
        private int m_spellline;

        public UseSpellEventArgs(int spell, int spellline)
        {
            this.m_spell = spell;
            this.m_spellline = spellline;
        }
        public int SpellLevel
        {
            get { return m_spell; }
        }

        public int SpellLineIndex
        {
            get { return m_spellline; }
        }
	}
}
