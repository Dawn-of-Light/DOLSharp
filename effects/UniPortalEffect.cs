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
using System.Text;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The effect associated with the UniPortal teleport spell.
	/// </summary>
	/// <author>Aredhel</author>
	class UniPortalEffect : TimedEffect, IGameEffect
	{
		/// <summary>
		/// Create a new portal effect.
		/// </summary>
		public UniPortalEffect(int duration) 
			: base(duration) { }

		/// <summary>
		/// Effect icon.
		/// </summary>
		public override ushort Icon
		{
			get { return 4310; }
		}

		/// <summary>
		/// Effect name.
		/// </summary>
		public override string Name
		{
			get { return "Uni-Portal Effect"; }
		}
	}
}
