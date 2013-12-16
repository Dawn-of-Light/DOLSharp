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
namespace DOL.Events
{
	/// <summary>
	/// This class defines an abstract event in DOL
	/// It needs to be overridden in order to create
	/// custom events.
	/// </summary>
	public abstract class DOLEvent
	{
		/// <summary>
		/// The event name
		/// </summary>
		protected string m_EventName;
		
		/// <summary>
		/// Constructs a new event
		/// </summary>
		/// <param name="name">The name of the event</param>
		public DOLEvent(string name)
		{
			m_EventName = name;
		}

		/// <summary>
		/// Gets the name of this event
		/// </summary>
		public string Name
		{
			get { return m_EventName; }
		}

		/// <summary>
		/// Returns the string representation of this event
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "DOLEvent("+m_EventName+")";
		}

		/// <summary>
		/// Returns true if the event target is valid for this event
		/// </summary>
		/// <param name="o">The object that is hooked</param>
		/// <returns></returns>
		public virtual bool IsValidFor(object o)
		{
			return true;
		}
	}
}
