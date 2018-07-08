using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using DOL.Events;
using DOL.Database;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    public class IchorOfTheDeepAbility : TimedRealmAbility
    {
        public IchorOfTheDeepAbility(DBAbility dba, int level) : base(dba, level) { }

        private RegionTimer _expireTimerId;
        private RegionTimer _rootExpire;
        private int _dmgValue;
        private int _duration;
        private GamePlayer _caster;

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            _caster = living as GamePlayer;
            if (_caster == null)
            {
                return;
            }

            if (_caster.TargetObject == null)
            {
                _caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return;
            }

            if (!_caster.TargetInView)
            {
                _caster.Out.SendMessage($"{_caster.TargetObject.Name} is not in view.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return;
            }

            if (!_caster.IsWithinRadius(_caster.TargetObject, 1875))
            {
                _caster.Out.SendMessage($"{_caster.TargetObject.Name} is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return;
            }

            if (_expireTimerId != null && _expireTimerId.IsAlive)
            {
                _caster.Out.SendMessage("You are already casting this ability.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                        case 1: _dmgValue = 150; _duration = 10000; break;
                        case 2: _dmgValue = 275; _duration = 15000; break;
                        case 3: _dmgValue = 400; _duration = 20000; break;
                        case 4: _dmgValue = 500; _duration = 25000; break;
                        case 5: _dmgValue = 600; _duration = 30000; break;
                        default: return;
                }
            }

                // 150 dam/10 sec || 400/20  || 600/30
                switch (Level)
                {
                        case 1: _dmgValue = 150; _duration = 10000; break;
                        case 2: _dmgValue = 400; _duration = 20000; break;
                        case 3: _dmgValue = 600; _duration = 30000; break;
                        default: return;
                }

            if (_caster.TargetObject is GameLiving target)
            {
                int primaryResistModifier = target.GetResist(eDamageType.Spirit);
                int secondaryResistModifier = target.SpecBuffBonusCategory[(int)eProperty.Resist_Spirit];
                int rootdet = (target.GetModified(eProperty.SpeedDecreaseDurationReduction) - 100) * -1;

                int resistModifier = 0;
                resistModifier += (int)((_dmgValue * (double)primaryResistModifier) * -0.01);
                resistModifier += (int)((_dmgValue + (double)resistModifier) * secondaryResistModifier * -0.01);

                if (target is GamePlayer)
                {
                    _dmgValue += resistModifier;
                }

                if (target is GameNPC)
                {
                    _dmgValue += resistModifier;
                }

                int rootmodifier = 0;
                rootmodifier += (int)((_duration * (double)primaryResistModifier) * -0.01);
                rootmodifier += (int)((_duration + (double)primaryResistModifier) * secondaryResistModifier * -0.01);
                rootmodifier += (int)((_duration + (double)rootmodifier) * rootdet * -0.01);

                _duration += rootmodifier;
            }

            if (_duration < 1)
            {
                _duration = 1;
            }

            foreach (GamePlayer iPlayer in _caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (iPlayer == _caster)
                {
                    iPlayer.MessageToSelf($"You cast {Name}!", eChatType.CT_Spell);
                }
                else
                {
                    iPlayer.MessageFromArea(_caster, $"{_caster.Name} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                iPlayer.Out.SendSpellCastAnimation(_caster, 7029, 20);
            }

            _expireTimerId = new RegionTimer(_caster, new RegionTimerCallback(EndCast), 2000);
        }

        protected virtual int EndCast(RegionTimer timer)
        {
            if (_caster.TargetObject == null)
            {
                _caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return 0;
            }

            if (_caster.IsMoving)
            {
                _caster.Out.SendMessage(LanguageMgr.GetTranslation(_caster.Client, "SpellHandler.CasterMove"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3000);
                return 0;
            }

            if (!_caster.IsWithinRadius(_caster.TargetObject, 1875))
            {
                _caster.Out.SendMessage(_caster.TargetObject.Name + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                _caster.DisableSkill(this, 3 * 1000);
                return 0;
            }

            if (!(_caster.TargetObject is GameLiving living))
            {
                timer.Stop();
                return 0;
            }

            if (living.EffectList.GetOfType<ChargeEffect>() == null && living.EffectList.GetOfType<SpeedOfSoundEffect>() != null)
            {
                living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
                _rootExpire = new RegionTimer(living, new RegionTimerCallback(RootExpires), _duration);
                GameEventMgr.AddHandler(living, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
                SendUpdates(living);
            }

            foreach (GamePlayer player in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(_caster, (GameLiving) _caster.TargetObject, 7029, 0, false, 1);
            }

            foreach (GameNPC mob in living.GetNPCsInRadius(500))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(_caster, mob, true))
                {
                    continue;
                }

                if (mob.HasAbility(Abilities.CCImmunity) || mob.HasAbility(Abilities.RootImmunity) || mob.HasAbility(Abilities.DamageImmunity))
                {
                    continue;
                }

                GameSpellEffect mez = SpellHandler.FindEffectOnTarget(mob, "Mesmerize");
                mez?.Cancel(false);

                mob.TakeDamage(_caster, eDamageType.Spirit, _dmgValue, 0);

                if (mob.EffectList.GetOfType<ChargeEffect>() == null && mob.EffectList.GetOfType<SpeedOfSoundEffect>() == null)
                {
                    mob.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
                    _rootExpire = new RegionTimer(mob, new RegionTimerCallback(RootExpires), _duration);
                    GameEventMgr.AddHandler(mob, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
                    SendUpdates(mob);
                }

                _caster.Out.SendMessage("You hit the " + mob.Name + " for " + _dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                foreach (GamePlayer player2 in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player2.Out.SendSpellEffectAnimation(_caster, mob, 7029, 0, false, 1);
                }
            }

            foreach (GamePlayer aeplayer in living.GetPlayersInRadius(500))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(_caster, aeplayer, true))
                {
                    continue;
                }

                GameSpellEffect mez = SpellHandler.FindEffectOnTarget(aeplayer, "Mesmerize");
                mez?.Cancel(false);

                aeplayer.TakeDamage(_caster, eDamageType.Spirit, _dmgValue, 0);
                aeplayer.StartInterruptTimer(3000, AttackData.eAttackType.Spell, _caster);

                if (aeplayer.EffectList.GetOfType<ChargeEffect>() == null && aeplayer.EffectList.GetOfType<SpeedOfSoundEffect>() == null)
                {
                    aeplayer.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
                    _rootExpire = new RegionTimer(aeplayer, new RegionTimerCallback(RootExpires), _duration);
                    GameEventMgr.AddHandler(aeplayer, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
                    SendUpdates(aeplayer);
                }

                _caster.Out.SendMessage($"You hit {aeplayer.Name} for {_dmgValue} damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                foreach (GamePlayer player3 in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player3.Out.SendSpellEffectAnimation(_caster, aeplayer, 7029, 0, false, 1);
                }
            }

            DisableSkill(_caster);
            timer.Stop();
            return 0;
        }

        protected virtual int RootExpires(RegionTimer timer)
        {
            if (timer.Owner is GameLiving living)
            {
                living.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
                SendUpdates(living);
            }

            timer.Stop();
            return 0;
        }

        /// <summary>
        /// Sends updates on effect start/stop
        /// </summary>
        /// <param name="owner"></param>
        protected static void SendUpdates(GameLiving owner)
        {
            if (owner.IsMezzed || owner.IsStunned)
            {
                return;
            }

            if (owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }

            if (owner is GameNPC npc)
            {
                short maxSpeed = npc.MaxSpeed;
                if (npc.CurrentSpeed > maxSpeed)
                {
                    npc.CurrentSpeed = maxSpeed;
                }
            }
        }

        /// <summary>
        /// Handles attack on buff owner
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackArgs == null)
            {
                return;
            }

            if (living == null)
            {
                return;
            }

            switch (attackArgs.AttackData.AttackResult)
            {
                case GameLiving.eAttackResult.HitStyle:
                case GameLiving.eAttackResult.HitUnstyled:
                    living.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
                    SendUpdates(living);
                    break;
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
