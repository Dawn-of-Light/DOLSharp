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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the stag ability
	/// </summary>
	public class StagEffect : IGameEffect
	{
		/*
        1.42
        Hibernian Heroes have receieved a new ability - "Spirit of the Hunt".
        Whenever this ability is used, the Hero will shapeshift into a fearsome
        stag-headed Huntsman from Celtic Lore, and will receive bonus hit points.
        There are four different levels of of this ability: Initiate(15th level),
        Member(25th level), Leader(35th level), and Master(45th level).
        While in this form, the hero has increased hit points - +20% for the
        15th level ability up to +50% for the 45th level ability. The ability
        lasts for thirty seconds - at the end of this time, the hero's
        maximum hits will return to normal, but he keeps any hit point
        gain from the ability in his current hits (but he cannot exceed his
        pre-buffed maximum). The ability can be used once every 30 minutes
        played. Please note that there is only one Huntsman creature -
        male and female Heroes will both shapeshift into the same creature.
        */

		// some time after a lurikeen model was added for luri's

		/// <summary>
		/// The ability description
		/// </summary>
		protected const String delveString = "Hero-only ability, that transforms a player into a stag-headed Huntsman. This lasts 30 seconds, and gives the player a burst in hitpoints. May be used once every 30 minutes. Initiate = 20% bonus to hitpoints. Member =30%, Leader = 40%, Master = 50%";

		/// <summary>
		/// The ability owner
		/// </summary>
		protected GamePlayer m_player;

		/// <summary>
		/// The used ability
		/// </summary>
		protected Ability m_ability;

		/// <summary>
		/// The amount of max health gained
		/// </summary>
		protected int m_amount;

		/// <summary>
		/// The timer that expires the ability
		/// </summary>
		protected RegionTimer m_expireTimer;

		/// <summary>
		/// Creates a new stag effect
		/// </summary>
		public StagEffect()
		{
		}

		/// <summary>
		/// Start the stag on player
		/// </summary>
		/// <param name="player">The player starting new effect</param>
		/// <param name="ab">The ability used to start new effect</param>
		public void Start(GamePlayer player, Ability ab)
		{
			m_ability = ab;
			m_player = player;

			if (!(player.Alive))
			{
				player.Out.SendMessage("You cannot use this ability while Dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.Mez)
			{
				player.Out.SendMessage("You cannot use this ability while Mezzed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.Stun)
			{
				player.Out.SendMessage("You cannot use this ability while Stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.Sitting)
			{
				player.Out.SendMessage("You cannot use this ability while Sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (m_player.Race == (int)eRace.Lurikeen) m_player.Model = 859;
			else m_player.Model = 583;

			double m_amountPercent = (ab.Level + 0.5 + Util.RandomDouble()) / 10; //+-5% random
			m_amount = (int)(player.CalculateMaxHealth(player.Level, player.Constitution) * m_amountPercent);

			m_player.BuffBonusCategory1[(int)eProperty.MaxHealth] += m_amount;
			m_player.Health += (int)(m_player.GetModified(eProperty.MaxHealth) * m_amountPercent);
			if (m_player.Health > m_player.MaxHealth) m_player.Health = m_player.MaxHealth;
			m_player.Out.SendUpdatePlayer();

			foreach (GamePlayer visiblePlayer in m_player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visiblePlayer.Out.SendEmoteAnimation(m_player, eEmote.StagFrenzy);
			}

			StartTimers();

			m_player.Out.SendMessage("You channel the spirit of the Hunt!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			m_player.EffectList.Add(this);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public void Cancel(bool playerCancel)
		{
			StopTimers();

			m_player.Model = (ushort)m_player.PlayerCharacter.CreationModel;

			double m_amountPercent = m_amount / m_player.GetModified(eProperty.MaxHealth);
			int playerHealthPercent = m_player.HealthPercent;
			m_player.BuffBonusCategory1[(int)eProperty.MaxHealth] -= m_amount;
			m_player.Health = m_player.Alive ? (int)Math.Max(1, 0.01 * m_player.MaxHealth * playerHealthPercent) : 0;
			m_player.Out.SendUpdatePlayer();

			// there is no animation on end of the effect
			m_player.EffectList.Remove(this);
			m_player.Out.SendMessage("Your Spirit of the Hunt ends.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		protected virtual void StartTimers()
		{
			StopTimers();

			m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpiredCallback), StagAbilityHandler.DURATION);
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
		/// <param name="callingTimer">the gametimer of the effect</param>
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
		public ushort Icon { get { return 480; } }

		/// <summary>
		/// The internal unique effect ID
		/// </summary>
		private ushort m_id;

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID
		{
			get { return m_id; }
			set { m_id = value; }
		}

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
