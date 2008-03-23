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
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// base for all effects that are timed and should stop on itself
	/// </summary>
	public class TimedEffect : StaticEffect
	{
		private readonly object m_LockObject = new object();

		protected int m_duration;

		/// <summary>
		/// The timer that will cancel the effect
		/// </summary>
		protected RegionTimer m_expireTimer;

		/// <summary>
		/// create timed effect that will last the given timespan in milliseconds
		/// </summary>
		/// <param name="timespan"></param>
		public TimedEffect(int timespan)
		{
			m_duration = timespan;
		}

		/// <summary>
		/// Start the timed effect on target
		/// </summary>
		/// <param name="target">The effect target</param>
		public override void Start(GameLiving target)
		{
			lock (m_LockObject)
			{
				if (m_expireTimer == null)
				{
					m_expireTimer = new RegionTimer(target, new RegionTimerCallback(ExpiredCallback), m_duration);
				}
				base.Start(target);
			}
		}

		/// <summary>
		/// Stop the timed effect on owner
		/// </summary>
		public override void Stop()
		{
			lock (m_LockObject)
			{
				if (m_expireTimer != null)
				{
					m_expireTimer.Stop();
					m_expireTimer = null;
				}
				base.Stop();
			}
		}

		private int ExpiredCallback(RegionTimer timer)
		{
			Stop();
			return 0;
		}

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public override int RemainingTime
		{
			get
			{
				RegionTimer timer = m_expireTimer;
				if (timer == null || !timer.IsAlive)
					return 0;
				return timer.TimeUntilElapsed;
			}
		}

		public override IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				int seconds = RemainingTime / 1000;
				if (seconds > 0)
				{
					list.Add(" "); //empty line
					if (seconds > 60)
						list.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.MinutesRemaining", (seconds / 60), (seconds % 60).ToString("00")));
					else
						list.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.DelveInfo.SecondsRemaining", seconds));
				}
				return list;
			}
		}
	}
}
