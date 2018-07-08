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
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// The helper class for the guard ability
    /// </summary>
    public class MarkofPreyEffect : TimedEffect
    {
        private const int Duration = 30 * 1000;
        private const double Value = 5.1;

        private GamePlayer _effectOwner;
        private GamePlayer _effectCaster;
        private Group _playerGroup;

        public MarkofPreyEffect()
            : base(Duration)
        { }

        /// <summary>
        /// Start guarding the player
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="casterTarget"></param>
        public void Start(GamePlayer caster, GamePlayer casterTarget)
        {
            if (caster == null || casterTarget == null)
            {
                return;
            }

            _playerGroup = caster.Group;
            if (_playerGroup != casterTarget.Group)
            {
                return;
            }

            _effectCaster = caster;
            _effectOwner = casterTarget;
            foreach (GamePlayer p in _effectOwner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(_effectCaster, _effectOwner, 7090, 0, false, 1);
            }

            GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            if (_playerGroup != null)
            {
                GameEventMgr.AddHandler(_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
            }

            GameEventMgr.AddHandler(_effectOwner, GameLivingEvent.AttackFinished, new DOLEventHandler(AttackFinished));
            _effectOwner.Out.SendMessage("Your weapon begins channeling the strength of the vampiir!", DOL.GS.PacketHandler.eChatType.CT_Spell, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
            base.Start(casterTarget);
        }

        public override void Stop()
        {
            if (_effectOwner != null)
            {
                if (_playerGroup != null)
                {
                    GameEventMgr.RemoveHandler(_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
                }

                GameEventMgr.RemoveHandler(_effectOwner, GameLivingEvent.AttackFinished, new DOLEventHandler(AttackFinished));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                _playerGroup = null;
            }

            _effectOwner.Out.SendMessage("Your weapon returns to normal.", DOL.GS.PacketHandler.eChatType.CT_SpellExpires, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
            base.Stop();
        }

        /// <summary>
        /// Called when a player is inflicted in an combat action
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private void AttackFinished(DOLEvent e, object sender, EventArgs args)
        {
            if (!(args is AttackFinishedEventArgs atkArgs))
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

            var dpsCap = (1.2 + 0.3 * attacker.Level) * 0.7;

            double dps = Math.Min(Value, dpsCap);
            double damage = dps * atkArgs.AttackData.WeaponSpeed * 0.1;
            double damageResisted = damage * target.GetResist(eDamageType.Heat) * -0.01;

            AttackData ad = new AttackData
            {
                Attacker = attacker,
                Target = target,
                Damage = (int) (damage + damageResisted),
                Modifier = (int) damageResisted,
                DamageType = eDamageType.Heat,
                AttackType = AttackData.eAttackType.Spell,
                AttackResult = GameLiving.eAttackResult.HitUnstyled
            };

            target.OnAttackedByEnemy(ad);
            _effectCaster.ChangeMana(_effectOwner, GameLiving.eManaChangeType.Spell, ad.Damage);
            (attacker as GamePlayer)?.Out.SendMessage($"You hit {target.Name} for {ad.Damage} extra damage!", PacketHandler.eChatType.CT_Spell, PacketHandler.eChatLoc.CL_SystemWindow);

            attacker.DealDamage(ad);
        }

        /// <summary>
        /// Cancels effect if one of players disbands
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender">The group</param>
        /// <param name="args"></param>
        protected void GroupDisbandCallback(DOLEvent e, object sender, EventArgs args)
        {
            if (!(args is MemberDisbandedEventArgs eArgs))
            {
                return;
            }

            if (eArgs.Member == _effectOwner)
            {
                Cancel(false);
            }
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        protected void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            Cancel(false);
        }

        public override string Name => "Mark Of Prey";

        public override ushort Icon => 3089;

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Grants a 30 second damage add that stacks with all other forms of damage add. All damage done via the damage add will be returned to the Vampiir as power."
                };

                return list;
            }
        }
    }
}
