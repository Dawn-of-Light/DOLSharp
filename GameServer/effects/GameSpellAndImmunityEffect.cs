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
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Spell Effect assists SpellHandler with duration spells with immunity
	/// </summary>
	public class GameSpellAndImmunityEffect : GameSpellEffect
	{
		/// <summary>
		/// The amount of times this effect started
		/// </summary>
		protected int m_startedCount;

		/// <summary>
		/// Creates a new game spell effect
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="duration"></param>
		/// <param name="pulseFreq"></param>
		public GameSpellAndImmunityEffect(ISpellHandler handler, int duration, int pulseFreq) : this(handler, duration, pulseFreq, 1)
		{
		}

		/// <summary>
		/// Creates a new game spell effect
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="duration"></param>
		/// <param name="pulseFreq"></param>
		/// <param name="effectiveness"></param>
		public GameSpellAndImmunityEffect(ISpellHandler handler, int duration, int pulseFreq, double effectiveness) : base(handler, duration, pulseFreq, effectiveness)
		{
			m_startedCount = 0;
		}

		/// <summary>
		/// The callback method when the effect expires
		/// </summary>
		protected override void ExpiredCallback()
		{
			if (!ImmunityState)
			{
				Cancel(false);
			}
			else
			{
				StopTimers();
				m_owner.EffectList.Remove(this);
			}
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected override void StartTimers()
		{
			int duration = m_duration;
			if (m_startedCount > 0)
			{
				duration /= Math.Min(20, m_startedCount*2);
				if (duration < 1) duration = 1;
			}
			m_duration = duration;
			m_startedCount++;
			base.StartTimers();
		}

		/// <summary>
		/// Cancels the effect
		/// </summary>
		/// <param name="playerCanceled">true if canceled by the player</param>
		public override void Cancel(bool playerCanceled)
		{
			if (m_owner == null) return;

			if (playerCanceled)
			{
				if (Owner is GamePlayer)
					((GamePlayer) Owner).Out.SendMessage("You can't remove this effect!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			lock (m_owner.EffectList) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (m_expired)
				{
					// do not allow removing immunity on alive living
					if (!m_owner.IsAlive)
						m_owner.EffectList.Remove(this);
					return;
				}

				StopTimers();
				m_expired = true;
				int duration = m_handler.OnEffectExpires(this, false);
				if (duration > 0)
				{
					m_duration = duration;
					m_timer = new PulsingEffectTimer(this);
					m_timer.Interval = 0;
					m_timer.Start(duration);
					m_owner.EffectList.OnEffectsChanged(this);
				}
				else
				{
					m_owner.EffectList.Remove(this);
				}
			}
		}

		/// <summary>
		/// Gets the amount of times this effect started
		/// </summary>
		public int StartedCount
		{
			get { return m_startedCount; }
		}

		/// <summary>
		/// True if effect is in immunity state
		/// </summary>
		public bool ImmunityState
		{
			get { return m_expired; }
			set { m_expired = value; }
		}
	}
}
