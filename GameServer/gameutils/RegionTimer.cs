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
using System.Text;
using System.Threading;

namespace DOL.GS
{
	/// <summary>
	/// This delegate is the callback function for RegionTimers
	/// </summary>
	public delegate int RegionTimerCallback(RegionTimer callingTimer);

	/// <summary>
	/// Calls delegate on every tick
	/// </summary>
	public sealed class RegionTimer : GameTimer
	{
		/// <summary>
		/// The timer callback
		/// </summary>
		private RegionTimerCallback m_callback;

		/// <summary>
		/// Holds properties for this region timer
		/// </summary>
		private PropertyCollection m_properties;

		/// <summary>
		/// Gets or sets the timer callback
		/// </summary>
		public RegionTimerCallback Callback
		{
			get { return m_callback; }
			set { m_callback = value; }
		}

		/// <summary>
		/// Gets the properties of this timer
		/// </summary>
		public PropertyCollection Properties
		{
			get 
			{
				if (m_properties == null)
				{
					lock (this)
					{
						if (m_properties == null)
						{
							PropertyCollection properties = new PropertyCollection();
							Thread.MemoryBarrier();
							m_properties = properties;
						}
					}
				}
				return m_properties; 
			}
		}

		/// <summary>
		/// Constructs a new region timer
		/// </summary>
		/// <param name="timerOwner">The game object that is starting the timer</param>
		public RegionTimer(GameObject timerOwner) : base(timerOwner.CurrentRegion.TimeManager)
		{
		}

		/// <summary>
		/// Constructs a new region timer
		/// </summary>
		/// <param name="timerOwner">The game object that is starting the timer</param>
		/// <param name="callback">The callback function to call</param>
		public RegionTimer(GameObject timerOwner, RegionTimerCallback callback) : this(timerOwner)
		{
			m_callback = callback;
		}

		/// <summary>
		/// Constructs a new region timer and starts it with specified delay
		/// </summary>
		/// <param name="timerOwner">The game object that is starting the timer</param>
		/// <param name="callback">The callback function to call</param>
		/// <param name="delay">The interval in milliseconds when to call the callback</param>
		public RegionTimer(GameObject timerOwner, RegionTimerCallback callback, int delay) : this(timerOwner, callback)
		{
			Start(delay);
		}

		/// <summary>
		/// Constructs a new region timer
		/// </summary>
		/// <param name="time"></param>
		public RegionTimer(TimeManager time) : base(time)
		{
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected override void OnTick()
		{
			if (m_callback != null)
				Interval = m_callback(this);
		}

		/// <summary>
		/// Returns short information about the timer
		/// </summary>
		/// <returns>Short info about the timer</returns>
		public override string ToString()
		{
			RegionTimerCallback callback = m_callback;
			object target = null;
			if (callback != null)
				target = callback.Target;
			return new StringBuilder(128)
				.Append("callback: ").Append(callback==null ? "(null)" : callback.Method.Name)
				.Append(' ').Append(base.ToString())
				.Append(" target: ").Append(target==null ? "" : (target.GetType().FullName+" "))
				.Append('(').Append(target==null ? "null" : target.ToString())
				.Append(')')
				.ToString();
		}
	}
}
