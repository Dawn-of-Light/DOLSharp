using System.Collections.Specialized;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class PerfectRecoveryAbility : TimedRealmAbility
    {
        public PerfectRecoveryAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _resurrectValue = 5;
        private const string ResurrectCasterProperty = "RESURRECT_CASTER";
        private readonly ListDictionary _resTimersByLiving = new ListDictionary();

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (!(living is GamePlayer player))
            {
                return;
            }

            GamePlayer targetPlayer = null;
            bool isGoodTarget = true;

            if (player.TargetObject == null)
            {
                isGoodTarget = false;
            }
            else
            {
                targetPlayer = player.TargetObject as GamePlayer;

                if (targetPlayer == null ||
                    targetPlayer.IsAlive ||
                    GameServer.ServerRules.IsSameRealm(living, (GameLiving) player.TargetObject, true) == false)
                {
                    isGoodTarget = false;
                }
            }

            if (isGoodTarget == false)
            {
                player.Out.SendMessage("You have to target a dead member of your realm!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _resurrectValue = 10; break;
                    case 2: _resurrectValue = 25; break;
                    case 3: _resurrectValue = 50; break;
                    case 4: _resurrectValue = 75; break;
                    case 5: _resurrectValue = 100; break;
                }
            }
            else
            {
                switch (Level)
                {
                    case 2: _resurrectValue = 50; break;
                    case 3: _resurrectValue = 100; break;
                }
            }

            if (targetPlayer.TempProperties.getProperty<object>(ResurrectCasterProperty, null) is GameLiving)
            {
                player.Out.SendMessage("Your target is already considering a resurrection!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!player.IsWithinRadius(targetPlayer, (int)(1500 * player.GetModified(eProperty.SpellRange) * 0.01)))

            {
                player.Out.SendMessage("You are too far away from your target to use this ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
            
            SendCasterSpellEffectAndCastMessage(living, 7019, true);
            DisableSkill(living);

            // Lifeflight:
            // don't rez just yet
            // ResurrectLiving(targetPlayer, player);
            // we need to add a dialogue response to the rez, copying from the rez spellhandler
            targetPlayer.TempProperties.setProperty(ResurrectCasterProperty, living);
            RegionTimer resurrectExpiredTimer = new RegionTimer(targetPlayer) {Callback = new RegionTimerCallback(ResurrectExpiredCallback)};
            resurrectExpiredTimer.Properties.setProperty("targetPlayer", targetPlayer);
            resurrectExpiredTimer.Start(15000);
            lock (_resTimersByLiving.SyncRoot)
            {
                _resTimersByLiving.Add(player.TargetObject, resurrectExpiredTimer);
            }

            // send resurrect dialog
            targetPlayer.Out.SendCustomDialog($"Do you allow {living.GetName(0, true)} to resurrect you\nwith {_resurrectValue} percent hits?", new CustomDialogResponse(ResurrectResponceHandler));
        }

        // Lifeflight add
        /// <summary>
        /// Resurrects target if it accepts
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        protected virtual void ResurrectResponceHandler(GamePlayer player, byte response)
        {
            // DOLConsole.WriteLine("resurrect responce: " + response);
            GameTimer resurrectExpiredTimer;
            lock (_resTimersByLiving.SyncRoot)
            {
                resurrectExpiredTimer = (GameTimer)_resTimersByLiving[player];
                _resTimersByLiving.Remove(player);
            }

            resurrectExpiredTimer?.Stop();

            GameLiving rezzer = (GameLiving)player.TempProperties.getProperty<object>(ResurrectCasterProperty, null);
            if (!player.IsAlive)
            {
                if (rezzer == null)
                {
                    player.Out.SendMessage("No one is currently trying to resurrect you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    if (response == 1)
                    {
                        ResurrectLiving(player, rezzer); // accepted
                    }
                    else
                    {
                        player.Out.SendMessage("You decline to be resurrected.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        // Dont need to refund anything with PR
                        // m_caster.Mana += CalculateNeededPower(player);
                        // but we do need to give them PR back
                        // Lifeflight: Seems like the best way to do this is to send a 0 duration to DisableSkill, which will enable to ability
                        rezzer.DisableSkill(this, 0);
                    }
                }
            }

            player.TempProperties.removeProperty(ResurrectCasterProperty);
        }

        // Lifeflight add
        /// <summary>
        /// Cancels resurrection after some time
        /// </summary>
        /// <param name="callingTimer"></param>
        /// <returns></returns>
        protected virtual int ResurrectExpiredCallback(RegionTimer callingTimer)
        {
            GamePlayer player = (GamePlayer)callingTimer.Properties.getProperty<object>("targetPlayer", null);
            if (player == null)
            {
                return 0;
            }

            player.TempProperties.removeProperty(ResurrectCasterProperty);
            player.Out.SendMessage("Your resurrection spell has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return 0;
        }

        public void ResurrectLiving(GamePlayer resurrectedPlayer, GameLiving rezzer)
        {
            if (rezzer.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (rezzer.CurrentRegionID != resurrectedPlayer.CurrentRegionID)
            {
                return;
            }

            resurrectedPlayer.Health = resurrectedPlayer.MaxHealth * _resurrectValue / 100;
            resurrectedPlayer.Mana = resurrectedPlayer.MaxMana * _resurrectValue / 100;
            resurrectedPlayer.Endurance = resurrectedPlayer.MaxEndurance * _resurrectValue / 100; // no endurance after any rez
            resurrectedPlayer.MoveTo(rezzer.CurrentRegionID, rezzer.X, rezzer.Y, rezzer.Z, rezzer.Heading);

            GameLiving living = resurrectedPlayer;
            GameTimer resurrectExpiredTimer;
            lock (_resTimersByLiving.SyncRoot)
            {
                resurrectExpiredTimer = (GameTimer)_resTimersByLiving[living];
                _resTimersByLiving.Remove(living);
            }

            resurrectExpiredTimer?.Stop();

            resurrectedPlayer.StopReleaseTimer();
            resurrectedPlayer.Out.SendPlayerRevive(resurrectedPlayer);
            resurrectedPlayer.UpdatePlayerStatus();

            GameSpellEffect effect = SpellHandler.FindEffectOnTarget(resurrectedPlayer, GlobalSpells.PvEResurrectionIllnessSpellType);
            effect?.Cancel(false);

            GameSpellEffect effecttwo = SpellHandler.FindEffectOnTarget(resurrectedPlayer, GlobalSpells.RvRResurrectionIllnessSpellType);
            effecttwo?.Cancel(false);

            resurrectedPlayer.Out.SendMessage("You have been resurrected by " + rezzer.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            // Lifeflight: this should make it so players who have been ressurected don't take damage for 5 seconds
            RezDmgImmunityEffect rezImmune = new RezDmgImmunityEffect();
            rezImmune.Start(resurrectedPlayer);

            // Lifeflight: We need to reward rez RPs
            if (rezzer is GamePlayer casterPlayer)
            {
                long rezRps = resurrectedPlayer.LastDeathRealmPoints * (_resurrectValue + 50) / 1000;
                if (rezRps > 0)
                {
                    casterPlayer.GainRealmPoints(rezRps);
                }
                else
                {
                    casterPlayer.Out.SendMessage("The player you resurrected was not worth realm points on death.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    casterPlayer.Out.SendMessage("You thus get no realm points for the resurrect.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }
    }
}

