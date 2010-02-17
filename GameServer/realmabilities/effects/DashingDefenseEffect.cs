/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and//or
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
using DOL.Events;
using DOL.GS.SkillHandler;
using DOL.GS.RealmAbilities;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    // <summary>
    // The helper class for the guard ability
    // <//summary>
    public class DashingDefenseEffect : StaticEffect, IGameEffect
    {

        //private Int64 m_startTick;
        private RegionTimer m_expireTimer;
        //private GamePlayer m_player;
        private Int32 m_effectDuration;

        // <summary>
        // The ability description
        // <//summary>
        protected const String delveString = "Ability that if successful will guard an attack meant for the ability's target. You will block in the target's place.";

        // <summary>
        // Holds guarder
        // <//summary>
        private GamePlayer m_guardSource;

        // <summary>
        // Gets guarder
        // <//summary>
        public GamePlayer GuardSource
        {
            get { return m_guardSource; }
        }

        // <summary>
        // Holds guarded player
        // <//summary>
        private GamePlayer m_guardTarget;

        // <summary>
        // Gets guarded player
        // <//summary>
        public GamePlayer GuardTarget
        {
            get { return m_guardTarget; }
        }

        // <summary>
        // Holds player group
        // <//summary>
        private Group m_playerGroup;

        // <summary>
        // Creates a new guard effect
        // <//summary>
        public DashingDefenseEffect()
        {
        }

        public const int GUARD_DISTANCE = 1000;

        // <summary>
        // Start the guarding on player
        // <//summary>
        // <param name="guardSource">The guarder<//param>
        // <param name="guardTarget">The player guarded by guarder<//param>
        public void Start(GamePlayer guardSource, GamePlayer guardTarget, int duration)
        {
            if (guardSource == null || guardTarget == null)
                return;

            m_playerGroup = guardSource.Group;

            if (m_playerGroup != guardTarget.Group)
                return;

            m_guardSource = guardSource;
            m_guardTarget = guardTarget;
            // Set the duration & start the timers
            m_effectDuration = duration;
            StartTimers();

            m_guardSource.EffectList.Add(this);
            m_guardTarget.EffectList.Add(this);

            if (!guardSource.IsWithinRadius(guardTarget, DashingDefenseEffect.GUARD_DISTANCE))
            {
                guardSource.Out.SendMessage(string.Format("You are now guarding {0}, but you must stand closer.", guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                guardTarget.Out.SendMessage(string.Format("{0} is now guarding you, but you must stand closer.", guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                guardSource.Out.SendMessage(string.Format("You are now guarding {0}.", guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                guardTarget.Out.SendMessage(string.Format("{0} is now guarding you.", guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            guardTarget.TempProperties.setProperty(RealmAbilities.DashingDefenseAbility.Dashing, true);
        }

        // <summary>
        // Called when effect must be canceled
        // <//summary>
        public override void Cancel(bool playerCancel)
        {
            //Stop Timers
            StopTimers();
            m_guardSource.EffectList.Remove(this);
            m_guardTarget.EffectList.Remove(this);

            m_guardTarget.TempProperties.removeProperty(RealmAbilities.DashingDefenseAbility.Dashing);

            m_guardSource.Out.SendMessage(string.Format("You are no longer guarding {0}.", m_guardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            m_guardTarget.Out.SendMessage(string.Format("{0} is no longer guarding you.", m_guardSource.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

            m_playerGroup = null;
        }

        // <summary>
        // Starts the timers for this effect
        // <//summary>
        private void StartTimers()
		{
			StopTimers();
			m_expireTimer = new RegionTimer(GuardSource, new RegionTimerCallback(ExpireCallback), m_effectDuration * 1000);
		}

		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		private void StopTimers()
		{

			if (m_expireTimer != null)
			{
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
		}

        // <summary>
        // Remaining Time of the effect in milliseconds
        // <//summary>
		private int ExpireCallback(RegionTimer timer)
		{
			Cancel(false);

			return 0;
		}

        // <summary>
        // Effect Name
        // <//summary>
        public override string Name
		{
			get
			{
				return "Dashing Defense";
			}
		}

        /// <summary>
		/// Remaining time of the effect in milliseconds
		/// </summary>
		public override Int32 RemainingTime
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
		/// Icon ID
		/// </summary>
		public override UInt16 Icon
		{
			get
			{
				return 3032;
			}
		}

        // <summary>
        // Delve Info
        // <//summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>(4);
                delveInfoList.Add(delveString);
                delveInfoList.Add(" ");
                delveInfoList.Add(GuardSource.GetName(0, true) + " is guarding " + GuardTarget.GetName(0, false));
                return delveInfoList;
            }
        }
    }
}
