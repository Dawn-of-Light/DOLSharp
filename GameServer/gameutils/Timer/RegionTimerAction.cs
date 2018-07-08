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

namespace DOL.GS
{
    /// <summary>
    /// RegionTimerAction is a dedicated Timer handling Lamba's to define Action Trigger
    /// </summary>
    public class RegionTimerAction<T> : GameTimer where T : GameObject
    {
        private Func<T, int> m_onTick;

        /// <summary>
        /// Get or Set the On Tick Function, Returning Next Interval Value
        /// Return 0 to stop the callback
        /// </summary>
        public Func<T, int> OnTickEvent {
            get { return m_onTick; }
            set { m_onTick = value; }
        }

        /// <summary>
        /// Source of GameTimer Event
        /// </summary>
        private readonly T m_source;

        /// <summary>
        /// Build a Stopped RegionTimerAction with GameObject Source.
        /// </summary>
        /// <param name="source">GameObject Source</param>
        public RegionTimerAction(T source)
            : base(source.CurrentRegion.TimeManager)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            m_source = source;
        }

        /// <summary>
        /// Build a Stopped RegionTimerAction with given Source and Function
        /// </summary>
        /// <param name="source">GameObject Source</param>
        /// <param name="onTick">OnTick Function returning next Interval</param>
        public RegionTimerAction(T source, Func<T, int> onTick)
            : this(source)
        {
            m_onTick = onTick;
        }

        /// <summary>
        /// Build a Stopped RegionTimerAction with given Source and Action
        /// </summary>
        /// <param name="source">GameObject Source</param>
        /// <param name="onTick">OnTick Action returning next Interval</param>
        public RegionTimerAction(T source, Action<T> onTick)
            : this(source)
        {
            m_onTick = obj => { onTick.Invoke(m_source); return 0; };
        }

        /// <summary>
        /// Build and start a RegionTimerAction with given source, delay and Action
        /// </summary>
        /// <param name="source">GameObject Source</param>
        /// <param name="delay">Delay before starting the first Tick</param>
        /// <param name="onTick">OnTick Action that will be runned once.</param>
        public RegionTimerAction(T source, int delay, Action<T> onTick)
            : this(source)
        {
            m_onTick = obj => { onTick.Invoke(m_source); return 0; };

            Start(delay);
        }

        /// <summary>
        /// Build and start a RegionTimerAction with given source, delay and Function
        /// </summary>
        /// <param name="source">GameObject Source</param>
        /// <param name="delay">Delay before starting the first Tick</param>
        /// <param name="onTick">OnTick Function returning next Interval</param>
        public RegionTimerAction(T source, int delay, Func<T, int> onTick)
            : this(source)
        {
            m_onTick = onTick;

            Start(delay);
        }

        /// <summary>
        /// Don't Start Timer unless a method is set !
        /// </summary>
        /// <param name="initialDelay"></param>
        public override void Start(int initialDelay)
        {
            if (m_onTick == null)
            {
                throw new ArgumentNullException("onTick");
            }

            base.Start(initialDelay);
        }

        /// <summary>
        /// Call the OnTick Properties and set new Interval
        /// </summary>
        protected override void OnTick()
        {
            if (m_onTick != null)
            {
                Interval = m_onTick.Invoke(m_source);
            }
            else
            {
                throw new ArgumentNullException("onTick");
            }
        }
    }
}
