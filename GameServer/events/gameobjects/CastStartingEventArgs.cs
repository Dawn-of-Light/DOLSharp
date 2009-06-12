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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the CastSpell event of GameLivings
	/// </summary>
	public class CastStartingEventArgs : EventArgs
	{

		/// <summary>
		/// The Spell Handler
		/// </summary>
		private ISpellHandler m_handler;

		/// <summary>
		/// The target of the spell
		/// </summary>
		private GameLiving m_target = null;

		/// <summary>
		/// Constructs a new cast event args
		/// </summary>
		public CastStartingEventArgs(ISpellHandler handler)
		{
			this.m_handler = handler;
		}

		/// <summary>
		/// Constructs a new cast event args
		/// </summary>
		public CastStartingEventArgs(ISpellHandler handler, GameLiving target)
		{
			this.m_handler = handler;
			this.m_target = target;
		}

		/// <summary>
		/// Gets the handler
		/// </summary>
		public ISpellHandler SpellHandler
		{
			get { return m_handler; }
		}

		/// <summary>
		/// Gets the target
		/// </summary>
		public GameLiving Target
		{
			get { return m_target; }
		}
	}
}