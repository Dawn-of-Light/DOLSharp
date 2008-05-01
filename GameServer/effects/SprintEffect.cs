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
using DOL.Language;

using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for sprint ability
	/// </summary>
	public sealed class SprintEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// The timer that reduce the endurance every interval
		/// </summary>
		RegionTimer m_tickTimer;

		/// <summary>
		/// The amount of timer ticks player was not moving
		/// </summary>
		int m_idleTicks = 0;

		/// <summary>
		/// Start the sprinting on player
		/// </summary>
		public override void Start(GameLiving target)
		{
			base.Start(target);
			if (m_tickTimer != null)
			{
				m_tickTimer.Stop();
				m_tickTimer = null;
			}
			m_tickTimer = new RegionTimer(target);
			m_tickTimer.Callback = new RegionTimerCallback(PulseCallback);
			m_tickTimer.Start(1);
		}

		/// <summary>
		/// Stop the effect on target
		/// </summary>
		public override void Stop()
		{
			base.Stop();
			if (m_tickTimer != null)
			{
				m_tickTimer.Stop();
				m_tickTimer = null;
			}
		}

		/// <summary>
		/// Sprint "pulse"
		/// </summary>
		/// <param name="callingTimer"></param>
		/// <returns></returns>
		public int PulseCallback(RegionTimer callingTimer)
		{
			int nextInterval;

			if (m_owner.IsMoving)
				m_idleTicks = 0;
			else m_idleTicks++;

			if (m_owner.Endurance - 5 <= 0 || m_idleTicks >= 6)
			{
				Cancel(false);
				nextInterval = 0;
			}
			else
			{
				nextInterval = Util.Random(600, 1400);
				if (m_owner.IsMoving)
				{
					int amount = 5;

					LongWindAbility ra = m_owner.GetAbility(typeof(LongWindAbility)) as LongWindAbility;
					if (ra != null)
						amount = 5 - ra.GetAmountForLevel(ra.Level);

					m_owner.Endurance -= amount;
				}
			}
			return nextInterval;
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			base.Cancel(playerCancel);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Sprint(false);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.SprintEffect.Name"); } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public override int RemainingTime { get { return 1000; } } // always 1 for blink effect

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 0x199; } }
	}
}
