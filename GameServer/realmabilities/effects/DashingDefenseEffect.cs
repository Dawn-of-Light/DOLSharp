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
using DOL.GS.PacketHandler;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    // <summary>
    // The helper class for the guard ability
    // <//summary>
    public class DashingDefenseEffect : StaticEffect
    {

        // private Int64 m_startTick;
        private RegionTimer _expireTimer;

        // private GamePlayer m_player;
        private int _effectDuration;

        // <summary>
        // The ability description
        // <//summary>
        private const string DelveString = "Ability that if successful will guard an attack meant for the ability's target. You will block in the target's place.";

        // <summary>
        // Holds guarder
        // <//summary>

        // <summary>
        // Gets guarder
        // <//summary>
        public GamePlayer GuardSource { get; private set; }

        // <summary>
        // Holds guarded player
        // <//summary>

        // <summary>
        // Gets guarded player
        // <//summary>
        public GamePlayer GuardTarget { get; private set; }

        // <summary>
        // Holds player group
        // <//summary>
        private Group _playerGroup;

        public const int GuardDistance = 1000;

        // <summary>
        // Start the guarding on player
        // <//summary>
        // <param name="guardSource">The guarder<//param>
        // <param name="guardTarget">The player guarded by guarder<//param>
        public void Start(GamePlayer guardSource, GamePlayer guardTarget, int duration)
        {
            if (guardSource == null || guardTarget == null)
            {
                return;
            }

            _playerGroup = guardSource.Group;

            if (_playerGroup != guardTarget.Group)
            {
                return;
            }

            GuardSource = guardSource;
            GuardTarget = guardTarget;

            // Set the duration & start the timers
            _effectDuration = duration;
            StartTimers();

            GuardSource.EffectList.Add(this);
            GuardTarget.EffectList.Add(this);

            if (!guardSource.IsWithinRadius(guardTarget, GuardDistance))
            {
                guardSource.Out.SendMessage($"You are now guarding {guardTarget.GetName(0, false)}, but you must stand closer.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                guardTarget.Out.SendMessage($"{guardSource.GetName(0, true)} is now guarding you, but you must stand closer.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                guardSource.Out.SendMessage($"You are now guarding {guardTarget.GetName(0, false)}.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                guardTarget.Out.SendMessage($"{guardSource.GetName(0, true)} is now guarding you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            guardTarget.TempProperties.setProperty(RealmAbilities.DashingDefenseAbility.Dashing, true);
        }

        // <summary>
        // Called when effect must be canceled
        // <//summary>
        public override void Cancel(bool playerCancel)
        {
            // Stop Timers
            StopTimers();
            GuardSource.EffectList.Remove(this);
            GuardTarget.EffectList.Remove(this);

            GuardTarget.TempProperties.removeProperty(RealmAbilities.DashingDefenseAbility.Dashing);

            GuardSource.Out.SendMessage($"You are no longer guarding {GuardTarget.GetName(0, false)}.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            GuardTarget.Out.SendMessage($"{GuardSource.GetName(0, true)} is no longer guarding you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            _playerGroup = null;
        }

        // <summary>
        // Starts the timers for this effect
        // <//summary>
        private void StartTimers()
        {
            StopTimers();
            _expireTimer = new RegionTimer(GuardSource, new RegionTimerCallback(ExpireCallback), _effectDuration * 1000);
        }

        /// <summary>
        /// Stops the timers for this effect
        /// </summary>
        private void StopTimers()
        {

            if (_expireTimer != null)
            {
                _expireTimer.Stop();
                _expireTimer = null;
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
        public override string Name => "Dashing Defense";

        /// <summary>
        /// Remaining time of the effect in milliseconds
        /// </summary>
        public override int RemainingTime
        {
            get
            {
                RegionTimer timer = _expireTimer;
                if (timer == null || !timer.IsAlive)
                {
                    return 0;
                }

                return timer.TimeUntilElapsed;
            }
        }

        /// <summary>
        /// Icon ID
        /// </summary>
        public override ushort Icon => 3032;

        // <summary>
        // Delve Info
        // <//summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>(4)
                {
                    DelveString,
                    " ",
                    $"{GuardSource.GetName(0, true)} is guarding {GuardTarget.GetName(0, false)}"
                };

                return delveInfoList;
            }
        }
    }
}
