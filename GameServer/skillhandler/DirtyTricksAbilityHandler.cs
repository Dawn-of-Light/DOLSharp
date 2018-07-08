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
using System.Collections.Generic;
using System.Reflection;

using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;
using DOL.Language;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Sprint Ability clicks
    /// </summary>
    [SkillHandler(Abilities.DirtyTricks)]
    public class DirtyTricksAbilityHandler : IAbilityActionHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// The ability reuse time in seconds
        /// </summary>
        private const int ReuseTimer = 60000 * 7; // 7 minutes

        /// <summary>
        /// The ability effect duration in seconds
        /// </summary>
        private const int Duration = 30;

        /// <summary>
        /// Execute dirtytricks ability
        /// </summary>
        /// <param name="ab">The used ability</param>
        /// <param name="player">The player that used the ability</param>
        public void Execute(Ability ab, GamePlayer player)
        {
            if (player == null)
            {
                if (Log.IsWarnEnabled)
                {
                    Log.Warn("Could not retrieve player in DirtyTricksAbilityHandler.");
                }

                return;
            }

            if (!player.IsAlive)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseDead"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsMezzed)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsStunned)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsSitting)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            player.DisableSkill(ab, ReuseTimer);
            DirtyTricksEffect dt = new DirtyTricksEffect(Duration * 1000);
            dt.Start(player);
        }
    }
}

namespace DOL.GS.Effects
{
    /// <summary>
    /// TripleWield
    /// </summary>
    public class DirtyTricksEffect : TimedEffect
    {
        public DirtyTricksEffect(int duration)
            : base(duration)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            GamePlayer player = target as GamePlayer;

            GameEventMgr.AddHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
        }

        public override void Stop()
        {
            base.Stop();
            GamePlayer player = Owner as GamePlayer;
            GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
        }

        protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(arguments is AttackFinishedEventArgs atkArgs))
            {
                return;
            }

            if (atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
                && atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitStyle)
            {
                return;
            }

            GameLiving target = atkArgs.AttackData.Target;

            if (target?.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (target.IsAlive == false)
            {
                return;
            }

            if (!(sender is GameLiving attacker))
            {
                return;
            }

            if (attacker.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (attacker.IsAlive == false)
            {
                return;
            }

            if (atkArgs.AttackData.IsOffHand)
            {
                return; // only react to main hand
            }

            if (atkArgs.AttackData.Weapon == null)
            {
                return; // no weapon attack
            }

            DTdetrimentalEffect dt = target.EffectList.GetOfType<DTdetrimentalEffect>();
            if (dt == null)
            {
                new DTdetrimentalEffect().Start(target);

                // Log.Debug("Effect Started from dirty tricks handler on " + target.Name);
            }
        }

        public override string Name => LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Skill.Ability.DirtyTricks.Name");

        public override ushort Icon => 478;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    LanguageMgr.GetTranslation(((GamePlayer) Owner).Client, "Skill.Ability.DirtyTricks.Description")
                };

                list.AddRange(base.DelveInfo);
                return list;
            }
        }
    }
}

namespace DOL.GS.Effects
{
    /// <summary>
    /// The helper class for the berserk ability
    /// </summary>
    public class DTdetrimentalEffect : StaticEffect
    {
        /// <summary>
        /// The ability description
        /// </summary>
        private const string DelveString = "Causes target's fumble rate to increase.";

        /// <summary>
        /// The owner of the effect
        /// </summary>
        private GameLiving _player;

        /// <summary>
        /// The timer that will cancel the effect
        /// </summary>
        private RegionTimer _expireTimer;

        /// <summary>
        /// Start the berserk on a player
        /// </summary>
        public override void Start(GameLiving living)
        {
            _player = living;

            // Log.Debug("Effect Started from DT detrimental effect on " + m_player.Name);
            StartTimers(); // start the timers before adding to the list!
            _player.EffectList.Add(this);
            _player.DebuffCategory[(int)eProperty.FumbleChance] += 50;

            // foreach (GamePlayer visiblePlayer in m_player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            //  {
            //  }
            if (living is GamePlayer player)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.DirtyTricks.EffectStart"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// Called when effect must be canceled
        /// </summary>
        public override void Cancel(bool playerCancel)
        {
            if (playerCancel) // not cancelable by teh player
            {
                return;
            }

            // Log.Debug("Effect Canceled from DT Detrimental effect on "+ m_player.Name);
            StopTimers();
            _player.EffectList.Remove(this);
            _player.DebuffCategory[(int)eProperty.FumbleChance] -= 50;
            if (_player is GamePlayer player)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.DirtyTricks.EffectCancel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// Starts the timers for this effect
        /// </summary>
        protected virtual void StartTimers()
        {
            StopTimers();
            _expireTimer = new RegionTimer(_player, new RegionTimerCallback(ExpiredCallback), 10000);
        }

        /// <summary>
        /// Stops the timers for this effect
        /// </summary>
        protected virtual void StopTimers()
        {
            if (_expireTimer != null)
            {
                // DOLConsole.WriteLine("effect stop expire on "+Owner.Name+" "+this.InternalID);
                _expireTimer.Stop();
                _expireTimer = null;
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
        public override string Name
        {
            get
            {
                if ((Owner as GamePlayer)?.Client != null)
                {
                    return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Skill.Ability.DirtyTricks.Name");
                }

                return LanguageMgr.GetTranslation(LanguageMgr.DefaultLanguage, "Skill.Ability.DirtyTricks.Name");
            }
        }

        /// <summary>
        /// Remaining Time of the effect in milliseconds
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
        /// Icon to show on players, can be id
        /// </summary>
        public override ushort Icon => 478;

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>
                {
                    DelveString
                };

                int seconds = RemainingTime / 1000;
                if (seconds > 0)
                {
                    delveInfoList.Add(" "); // empty line
                    if (seconds > 60)
                    {
                        delveInfoList.Add($"- {seconds / 60}:{seconds % 60:00}{LanguageMgr.GetTranslation(((GamePlayer) Owner).Client, "Skill.Ability.MinRemaining")}");
                    }
                    else
                    {
                        delveInfoList.Add($"- {seconds}{LanguageMgr.GetTranslation(((GamePlayer) Owner).Client, "Skill.Ability.SecRemaining")}");
                    }
                }

                return delveInfoList;
            }
        }
    }
}
