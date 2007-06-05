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
		/// The owner of the effect
		/// </summary>
		GamePlayer m_player;

		/// <summary>
		/// The internal unique effect ID
		/// </summary>
		ushort m_id;

		/// <summary>
		/// The amount of timer ticks player was not moving
		/// </summary>
		int m_idleTicks = 0;

		/// <summary>
		/// Creates a new sprint effect
		/// </summary>
		public SprintEffect()
		{
		}

		/// <summary>
		/// Start the sprinting on player
		/// </summary>
		public void Start(GamePlayer player)
		{
			m_player = player;
			if (m_tickTimer != null)
			{
				m_tickTimer.Stop();
				m_tickTimer = null;
			}
			m_tickTimer = new RegionTimer(player);
			m_tickTimer.Callback = new RegionTimerCallback(PulseCallback);
			m_tickTimer.Start(1000);
			player.EffectList.Add(this);
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

			if (m_player.IsMoving)
				m_idleTicks = 0;
			else m_idleTicks++;

			if (m_player.Endurance - 5 <= 0 || m_idleTicks >= 6)
			{
				Cancel(false);
				nextInterval = 0;
			}
			else
			{
				nextInterval = Util.Random(600, 1400);
				if (m_player.IsMoving)
				{
					int amount = 5;

					LongWindAbility ra = m_player.GetAbility(typeof(LongWindAbility)) as LongWindAbility;
					if (ra != null)
						amount = 5 - ra.GetAmountForLevel(ra.Level);

					m_player.Endurance -= amount;
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
			m_player.Sprint(false);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return "Sprint"; } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public override int RemainingTime { get { return 1000; } } // always 1 for blink effect

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 0x199; } }

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID { get { return m_id; } set { m_id = value; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo { get { return new ArrayList(0); } }
	}
}
