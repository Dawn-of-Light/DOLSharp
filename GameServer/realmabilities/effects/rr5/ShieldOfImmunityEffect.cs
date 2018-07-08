using System;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Mastery of Concentration
    /// </summary>
    public class ShieldOfImmunityEffect : TimedEffect
    {
        private GameLiving _owner;

        public ShieldOfImmunityEffect()
            : base(20000)
        {
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            if (target is GamePlayer player)
            {
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
                }
            }

            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving living))
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            if (ad == null || ad.Damage < 1)
            {
                return;
            }

            if (ad.IsMeleeAttack || ad.AttackType == AttackData.eAttackType.Ranged || (ad.AttackType == AttackData.eAttackType.Spell && attackedByEnemy.AttackData.SpellHandler.Spell.SpellType == "Archery"))
            {
                int absorb = (int)(ad.Damage * 0.9);
                int critic = (int)(ad.CriticalDamage * 0.9);
                ad.Damage -= absorb;
                ad.CriticalDamage -= critic;
                if (living is GamePlayer gamePlayer)
                {
                    gamePlayer.Out.SendMessage($"Your Shield of Immunity absorbs {absorb + critic} points of damage", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                if (ad.Attacker is GamePlayer player)
                {
                    player.Out.SendMessage($"{living.Name}\'s Shield of Immunity absorbs {absorb + critic} points of damage", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
            }
        }

        public override void Stop()
        {
            GameEventMgr.RemoveHandler(_owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            base.Stop();
        }

        public override string Name => "Shield of Immunity";

        public override ushort Icon => 3047;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Shield that absorbs 90% melee/archer damage for 20 seconds."
                };

                return list;
            }
        }
    }
}