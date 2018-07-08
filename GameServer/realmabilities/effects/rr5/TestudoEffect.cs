using System;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Mastery of Concentration
    /// </summary>
    public class TestudoEffect : TimedEffect
    {
        private GameLiving _owner;

        public TestudoEffect()
            : base(45000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            GamePlayer player = target as GamePlayer;
            if (player != null)
            {
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
                }
            }

            target.StopAttack();
            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            GameEventMgr.AddHandler(target, GameLivingEvent.AttackFinished, new DOLEventHandler(AttackEventHandler));
            if (player != null)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else
            {
                _owner.CurrentSpeed = _owner.MaxSpeed;
            }
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving living))
            {
                return;
            }

            InventoryItem shield = living.Inventory.GetItem(eInventorySlot.LeftHandWeapon);

            if (shield?.Object_Type != (int)eObjectType.Shield)
            {
                return;
            }

            if (living.TargetObject == null)
            {
                return;
            }

            if (living.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
            {
                return;
            }

            if (living.AttackWeapon.Hand == 1)
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            if (ad == null || ad.Attacker.Realm == 0)
            {
                return;
            }

            if (ad.Damage < 1)
            {
                return;
            }

            int absorb = (int)(ad.Damage * 0.9);
            int critic = (int)(ad.CriticalDamage * 0.9);
            ad.Damage -= absorb;
            ad.CriticalDamage -= critic;
            if (living is GamePlayer player)
            {
                player.Out.SendMessage($"Your Testudo Stance reduces the damage by {absorb + critic} points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }

            if (ad.Attacker is GamePlayer gamePlayer)
            {
                gamePlayer.Out.SendMessage($"{living.Name}\'s Testudo Stance reducec your damage by {absorb + critic} points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
        }

        private void AttackEventHandler(DOLEvent e, object sender, EventArgs args)
        {
            if (!(args is AttackFinishedEventArgs ag))
            {
                return;
            }

            if (ag.AttackData == null)
            {
                return;
            }

            switch (ag.AttackData.AttackResult)
            {
                case GameLiving.eAttackResult.Blocked:
                case GameLiving.eAttackResult.Evaded:
                case GameLiving.eAttackResult.Fumbled:
                case GameLiving.eAttackResult.HitStyle:
                case GameLiving.eAttackResult.HitUnstyled:
                case GameLiving.eAttackResult.Missed:
                case GameLiving.eAttackResult.Parried:
                    Stop(); break;
            }
        }

        public override void Stop()
        {
            GameEventMgr.RemoveHandler(_owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            GameEventMgr.RemoveHandler(_owner, GameLivingEvent.AttackFinished, new DOLEventHandler(AttackEventHandler));
            base.Stop();
            if (_owner is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else
            {
                _owner.CurrentSpeed = _owner.MaxSpeed;
            }
        }

        public override string Name => "Testudo";

        public override ushort Icon => 3067;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Warrior with shield equipped covers up and takes 90% less damage for all attacks for 45 seconds. Can only move at reduced speed (speed buffs have no effect) and cannot attack. Using a style will break testudo form. This ability is only effective versus realm enemies."
                };

                return list;
            }
        }
    }
}