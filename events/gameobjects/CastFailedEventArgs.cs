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
using DOL.Events;
using DOL.GS.Spells;

namespace DOL.Events
{
	/// <summary>
	/// Arguments for a cast failed event, stating the reason
	/// why a particular spell cast failed.
	/// </summary>
	/// <author>Aredhel</author>
	class CastFailedEventArgs : CastStartingEventArgs
	{
		public enum Reasons
		{
			TargetTooFarAway,
			TargetNotInView,
			AlreadyCasting,
			CrowdControlled
		};
				
		/// <summary>
		/// Constructs arguments for a cast failed event.
		/// </summary>
		public CastFailedEventArgs(ISpellHandler handler, Reasons reason) 
			: base(handler)
		{
			this.m_reason = reason;
		}

		private Reasons m_reason;

		/// <summary>
		/// The reason why the spell cast failed.
		/// </summary>
		public Reasons Reason
		{
			get { return m_reason; }
		}
	}
}
