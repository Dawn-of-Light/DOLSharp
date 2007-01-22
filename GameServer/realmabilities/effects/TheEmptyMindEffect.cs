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
using DOL.GS.SkillHandler;
using DOL.Events;

namespace DOL.GS.Effects
{
	// if you wonder why its blank its because its all in spell handler ;o
	/// <summary>
	/// The helper class for the SavageCrush ability
	/// </summary>
	public class TheEmptyMindEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// The ability description
		/// </summary>
		protected const String delveString = "Grants the user 45 seconds of increased resistances to all magical damage by the percentage listed.";

		/// <summary>
		/// The owner of the effect
		/// </summary>
		GamePlayer m_player;

		/// <summary>
		/// The timer that will cancel the effect
		/// </summary>
		protected RegionTimer m_expireTimer;

		/// <summary>
		/// The used ability
		/// </summary>
		protected Ability m_ability;

		/// <summary>
		/// Creates a new SavageCrush effect
		/// </summary>
		public TheEmptyMindEffect()
		{
		}

		/// <summary>
		/// Start the SavageCrush on a player
		/// </summary>
		public void Start(GamePlayer player, Ability ab)
		{
			m_ability = ab;
			m_player = player;

			if (!(player.IsAlive))
			{
				player.Out.SendMessage("You cannot use this while Dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsMezzed)
			{
				player.Out.SendMessage("You cannot use this while Mezzed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsStunned)
			{
				player.Out.SendMessage("You cannot use this while Stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsSitting)
			{
				player.Out.SendMessage("You must be standing to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			foreach (GamePlayer visiblePlayer in m_player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visiblePlayer.Out.SendSpellEffectAnimation(m_player, m_player, 1197, 0, false, 1);
			}

			StartTimers(); // start the timers before adding to the list!
			m_player.EffectList.Add(this);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			StopTimers();

			m_player.EffectList.Remove(this);

			// there is no animation on end of the effect
			m_player.Out.SendMessage("Your clearheaded state leaves you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();

			m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpiredCallback), RealmAbilities.TheEmptyMindAbility.m_duration);
		}

		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		protected virtual void StopTimers()
		{
			if (m_expireTimer != null)
			{
				//DOLConsole.WriteLine("effect stop expire on "+Owner.Name+" "+this.InternalID);
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
		}

		/// <summary>
		/// The callback method when the effect expires
		/// </summary>
		/// <param name="callingTimer">the regiontimer of the effect</param>
		/// <returns>the new intervall (0) </returns>
		protected virtual int ExpiredCallback(RegionTimer callingTimer)
		{
			Cancel(false);
			return 0;
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public string Name { get { return m_ability.Name; } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public int RemainingTime
		{
			get
			{
				RegionTimer timer = m_expireTimer;
				if (timer == null || !timer.IsAlive)
					return 0;
				return timer.TimeUntilElapsed;
			}
		}

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public ushort Icon
		{
			get { return 7008; }
		}

		/// <summary>
		/// Stores the internal effect ID
		/// </summary>
		ushort m_id;

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID { get { return m_id; } set { m_id = value; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(4);
				delveInfoList.Add(delveString);

				int seconds = RemainingTime / 1000;
				if (seconds > 0)
				{
					delveInfoList.Add(" "); //empty line
					if (seconds > 60)
						delveInfoList.Add("- " + seconds / 60 + ":" + (seconds % 60).ToString("00") + " minutes remaining.");
					else
						delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList;
			}
		}
	}
}
