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
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the berserk ability
	/// </summary>
	public class BerserkEffect : IGameEffect
	{
		/// <summary>
		/// The ability description
		/// </summary>
		protected const String delveString = "This ability transforms the player into a Vendo beast and makes each hit that lands a critical hit, at the expense of any defensive abilities. It can be used one every seven minutes.";

		/// <summary>
		/// The owner of the effect
		/// </summary>
		GamePlayer m_player;

		/// <summary>
		/// The timer that will cancel the effect
		/// </summary>
		protected RegionTimer m_expireTimer;

		/// <summary>
		/// Creates a new berserk effect
		/// </summary>
		public BerserkEffect()
		{
		}

		/// <summary>
		/// Start the berserk on a player
		/// </summary>
		public void Start(GamePlayer player)
		{
			m_player = player;
			m_player.Model = 582; // vendo Man

			StartTimers(); // start the timers before adding to the list!
			m_player.EffectList.Add(this);

			foreach (GamePlayer visiblePlayer in m_player.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				visiblePlayer.Out.SendEmoteAnimation(m_player, eEmote.MidgardFrenzy);
			}

			m_player.Out.SendMessage("You go into a berserker frenzy!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}	

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel) {
			StopTimers();

			m_player.EffectList.Remove(this);

			// TODO add proper transform animation
			m_player.Model = (ushort)m_player.CreationModel;			

			// there is no animation on end of the effect
			m_player.Out.SendMessage("Your berserker frenzy ends.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();

			m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpiredCallback), BerserkAbilityHandler.DURATION);
		}

		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		protected virtual void StopTimers()
		{
			if(m_expireTimer != null)
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
		public string Name { get { return "Berserk"; } }

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
		/// TODO find correct icon for berserk
		/// </summary>
		public ushort Icon { get { return 479; } }

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
					if(seconds > 60)
						delveInfoList.Add("- " + seconds/60 + ":" + (seconds%60).ToString("00") + " minutes remaining.");
					else
						delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList; 
			}
		}
	}
}
