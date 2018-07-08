using System.Collections;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{

    public class ChargeEffect : StaticEffect
    {
        private const int Duration = 15;
        private const string DelveString = "Grants unbreakable speed 3 for 15 second duration. Grants immunity to roots, stun, snare and mesmerize spells. Target will still take damage from snare/root spells that do damage.";
        private GameLiving _living;
        private long _startTick;
        private RegionTimer _expireTimer;

        public override void Start(GameLiving living)
        {
            _living = living;

            // Send messages
            if (_living is GamePlayer player)
            {
                player.Out.SendMessage("You begin to charge wildly!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            else if (_living is GameNPC)
            {
                if (((GameNPC)_living).Brain is IControlledBrain icb && icb.Body != null)
                {
                    GamePlayer playerowner = icb.GetPlayerOwner();
                    playerowner?.Out.SendMessage($"The {icb.Body.Name} charges its prey!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                return;
            }

            _startTick = living.CurrentRegion.Time;
            foreach (GamePlayer tPlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                tPlayer.Out.SendSpellEffectAnimation(living, living, 7035, 0, false, 1);
            }

            // sets player into combat mode
            living.LastAttackTickPvP = _startTick;
            ArrayList speedSpells = new ArrayList();
            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellEffect == false)
                    {
                        continue;
                    }

                    if ((effect as GameSpellEffect).Spell.SpellType == "SpeedEnhancement")
                    {
                        speedSpells.Add(effect);
                    }
                }
            }

            foreach (GameSpellEffect spell in speedSpells)
            {
                spell.Cancel(false);
            }

            _living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, PropertyCalc.MaxSpeedCalculator.Speed3);
            _living.TempProperties.setProperty("Charging", true);
            if (_living is GamePlayer gamePlayer)
            {
                gamePlayer.Out.SendUpdateMaxSpeed();
            }

            StartTimers();
            _living.EffectList.Add(this);
        }

        public override void Cancel(bool playerCancel)
        {
            _living.TempProperties.removeProperty("Charging");
            _living.EffectList.Remove(this);
            _living.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);

            // Send messages
            if (_living is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
                player.Out.SendMessage("You no longer seem so crazy!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else if (_living is GameNPC)
            {
                if (((GameNPC)_living).Brain is IControlledBrain icb && icb.Body != null)
                {
                    GamePlayer playerowner = icb.GetPlayerOwner();
                    playerowner?.Out.SendMessage($"The {icb.Body.Name} ceases its charge!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                }
            }

            StopTimers();
        }

        protected virtual void StartTimers()
        {
            StopTimers();
            _expireTimer = new RegionTimer(_living, new RegionTimerCallback(ExpiredCallback), Duration * 1000);
        }

        // Stops the timers for this effect
        protected virtual void StopTimers()
        {
            if (_expireTimer != null)
            {
                _expireTimer.Stop();
                _expireTimer = null;
            }
        }

        // The callback method when the effect expires
        protected virtual int ExpiredCallback(RegionTimer timer)
        {
            Cancel(false);
            return 0;
        }

        // Name of the effect
        public override string Name => "Charge";

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

        // Icon to show on players, can be id
        public override ushort Icon
        {
            get
            {
                if (_living is GameNPC)
                {
                    return 411;
                }
                else
                {
                    return 3034;
                }
            }
        }

        // Delve Info
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
                    delveInfoList.Add(seconds > 60
                        ? $"- {seconds / 60}:{seconds % 60:00} minutes remaining."
                        : $"- {seconds} seconds remaining.");
                }

                return delveInfoList;
            }
        }
    }
}
