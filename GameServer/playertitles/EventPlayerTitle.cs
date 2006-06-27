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
using DOL.Events;

namespace DOL.GS.PlayerTitles
{
	/// <summary>
	/// Base abstract class for typical player titles based on events.
	/// </summary>
	public abstract class EventPlayerTitle : SimplePlayerTitle
	{
		/// <summary>
		/// Constructs a new EventPlayerTitle instance.
		/// </summary>
		public EventPlayerTitle()
		{
			GameEventMgr.AddHandler(Event, new DOLEventHandler(EventCallback));
		}
		
		/// <summary>
		/// The event to hook.
		/// </summary>
		public abstract DOLEvent Event { get; }
		
		/// <summary>
		/// The event callback.
		/// </summary>
		/// <param name="e">The event fired.</param>
		/// <param name="sender">The event sender.</param>
		/// <param name="arguments">The event arguments.</param>
		protected virtual void EventCallback(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer p = sender as GamePlayer;
			if (p != null && IsSuitable(p))
				p.AddTitle(this);
		}
	}
}
