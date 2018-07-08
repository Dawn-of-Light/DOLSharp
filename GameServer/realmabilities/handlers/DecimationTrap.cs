using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Mastery of Concentration RA
    /// </summary>
    public class DecimationTrapAbility : TimedRealmAbility
    {
        /*private ushort region;
        private int x;
        private int y;
        private int z;*/
        private Area.Circle _traparea;
        private GameLiving _owner;
        private RegionTimer _ticktimer;
        private int _effectiveness;
        private ushort _region;

        public DecimationTrapAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            const ushort icon = 7026;
            _effectiveness = 0;
            _owner = living;

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _effectiveness = 300; break;
                    case 2: _effectiveness = 450; break;
                    case 3: _effectiveness = 600; break;
                    case 4: _effectiveness = 750; break;
                    case 5: _effectiveness = 900; break;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _effectiveness = 300; break;
                    case 2: _effectiveness = 600; break;
                    case 3: _effectiveness = 900; break;
                }
            }

            if (living.GroundTarget == null)
            {
                return;
            }

            if (!living.IsWithinRadius(living.GroundTarget, 1500))
            {
                return;
            }

            if (!(living is GamePlayer player))
            {
                return;
            }

            if (player.RealmAbilityCastTimer != null)
            {
                player.RealmAbilityCastTimer.Stop();
                player.RealmAbilityCastTimer = null;
                player.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }

            foreach (GamePlayer p in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellCastAnimation(living, icon, 20);
            }

            player.RealmAbilityCastTimer = new RegionTimer(player)
            {
                Callback = new RegionTimerCallback(StartSpell)
            };

            player.RealmAbilityCastTimer.Start(2000);
        }

        public override bool CheckRequirement(GamePlayer player)
        {
            return player.Level >= 40;
        }

        private int StartSpell(RegionTimer timer)
        {
            if (!_owner.IsAlive)
            {
                return 0;
            }

            _traparea = new Area.Circle("decimation trap", _owner.X, _owner.Y, _owner.Z, 50);

            _owner.CurrentRegion.AddArea(_traparea);
            _region = _owner.CurrentRegionID;

            GameEventMgr.AddHandler(_traparea, AreaEvent.PlayerEnter, new DOLEventHandler(EventHandler));
            _ticktimer = new RegionTimer(_owner)
            {
                Callback = new RegionTimerCallback(OnTick)
            };

            _ticktimer.Start(600000);
            getTargets();
            DisableSkill(_owner);

            return 0;
        }

        private int OnTick(RegionTimer timer)
        {
            removeHandlers();
            return 0;
        }

        protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(arguments is AreaEventArgs args))
            {
                return;
            }

            if (!(args.GameObject is GameLiving living))
            {
                return;
            }

            if (!GameServer.ServerRules.IsAllowedToAttack(_owner, living, true))
            {
                return;
            }

            getTargets();
        }

        private void removeHandlers()
        {
            _owner.CurrentRegion.RemoveArea(_traparea);
            GameEventMgr.RemoveHandler(_traparea, AreaEvent.PlayerEnter, new DOLEventHandler(EventHandler));
        }

        private void getTargets()
        {
            foreach (GamePlayer target in WorldMgr.GetPlayersCloseToSpot(_region, _traparea.X, _traparea.Y, _traparea.Z, 350))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(_owner, target, true))
                {
                    DamageTarget(target);
                }
            }

            foreach (GameNPC target in WorldMgr.GetNPCsCloseToSpot(_region, _traparea.X, _traparea.Y, _traparea.Z, 350))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(_owner, target, true))
                {
                    DamageTarget(target);
                }
            }
        }

        private void DamageTarget(GameLiving target)
        {
            if (!GameServer.ServerRules.IsAllowedToAttack(_owner, target, true))
            {
                return;
            }

            if (!target.IsAlive)
            {
                return;
            }

            if (_ticktimer.IsAlive)
            {
                _ticktimer.Stop();
                removeHandlers();
            }

            int dist = target.GetDistanceTo(new Point3D(_traparea.X, _traparea.Y, _traparea.Z));
            double mod = 1;
            if (dist > 0)
            {
                mod = 1 - ((double)dist / 350);
            }

            int basedamage = (int)(_effectiveness * mod);
            int resist = (int)(basedamage * target.GetModified(eProperty.Resist_Energy) * -0.01);
            int damage = basedamage + resist;

            if (_owner is GamePlayer player)
            {
                player.Out.SendMessage($"You hit {target.Name} for {damage}({resist}) points of damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
            }

            if (target is GamePlayer targetPlayer)
            {
                if (targetPlayer.IsStealthed)
                {
                    targetPlayer.Stealth(false);
                }
            }

            foreach (GamePlayer p in target.GetPlayersInRadius(false, WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(_owner, target, 7026, 0, false, 1);
                p.Out.SendCombatAnimation(_owner, target, 0, 0, 0, 0, 0x14, target.HealthPercent);
            }

            // target.TakeDamage(owner, eDamageType.Energy, damage, 0);
            AttackData ad = new AttackData
            {
                AttackResult = GameLiving.eAttackResult.HitUnstyled,
                Attacker = _owner,
                Target = target,
                DamageType = eDamageType.Energy,
                Damage = damage
            };

            target.OnAttackedByEnemy(ad);
            _owner.DealDamage(ad);
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                list.Add("Trap that deals the following damage in an 350 radius");
                list.Add("Level 1: 300 Damage");
                list.Add("Level 2: 450 Damage");
                list.Add("Level 3: 600 Damage");
                list.Add("Level 4: 750 Damage");
                list.Add("Level 5: 900 Damage");
                list.Add(string.Empty);
                list.Add("Range 1500");
                list.Add("Target: Ground Target");
                list.Add("Radius: 350");
                list.Add("Casting time: 2 seconds");
            }
            else
            {
                list.Add("Trap that deals the following damage in an 350 radius");
                list.Add("Level 1: 300 Damage");
                list.Add("Level 2: 600 Damage");
                list.Add("Level 3: 900 Damage");
                list.Add(string.Empty);
                list.Add("Range 1500");
                list.Add("Target: Ground Target");
                list.Add("Radius: 350");
                list.Add("Casting time: 2 seconds");
            }
        }
    }
}