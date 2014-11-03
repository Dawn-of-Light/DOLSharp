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
using System.Collections.Generic;

using DOL.GS.Styles;

namespace DOL.GS
{
	/// <summary>
	/// This is a Live Spell Line Specialization, used for list caster, with baseline spell and specline spell appart
	/// Purely rely on base Specialization implementation only disabling weapon style (not applicable here)
	/// </summary>
	public class LiveSpellLineSpecialization : Specialization
	{
		public LiveSpellLineSpecialization(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
		
		/// <summary>
		/// No Styles for Spell Specs.
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		protected override List<Style> GetStylesForLiving(GameLiving living, int level)
		{
			return new List<Style>();
		}

	}
}
