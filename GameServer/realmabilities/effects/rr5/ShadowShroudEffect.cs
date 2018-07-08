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
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// ShadowShroud Effect NS RR5 RA
    /// </summary>
    /// <author>Stexx</author>
    public class ShadowShroudEffect : TimedEffect
    {
        private const int Duration = 30 * 1000;
        private const double Abspercent = 10; // 10% damage absorb
        private const int Effect = 1565;
        private const int MissHitBonus = 10; // 10% misshit bonus

        private GamePlayer _effectOwner;

        public ShadowShroudEffect()
            : base(Duration)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _effectOwner = player;
                foreach (GamePlayer p in _effectOwner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(_effectOwner, _effectOwner, Effect, 0, false, 1);
                }

                _effectOwner.AbilityBonus[(int)eProperty.MissHit] += MissHitBonus;
                GameEventMgr.AddHandler(_effectOwner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GameLivingEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }
        }

        public override void Stop()
        {
            if (_effectOwner != null)
            {
                _effectOwner.AbilityBonus[(int)eProperty.MissHit] -= MissHitBonus;
                GameEventMgr.RemoveHandler(_effectOwner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GameLivingEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }

            base.Stop();
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        protected void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player)
            {
                ShadowShroudEffect shadowShroud = player.EffectList.GetOfType<ShadowShroudEffect>();
                shadowShroud?.Cancel(false);
            }
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        protected void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(arguments is AttackedByEnemyEventArgs args))
            {
                return;
            }

            if (args.AttackData == null)
            {
                return;
            }

            AttackData ad = args.AttackData;
            if (!(sender is GameLiving))
            {
                return;
            }

            double absorbPercent = Abspercent;
            int damageAbsorbed = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));
            if (damageAbsorbed > 0)
            {
                ad.Damage -= damageAbsorbed;

                // TODO correct messages
                MessageToLiving(ad.Target, $"Shadow Shroud Ability absorbs {damageAbsorbed} damage!", eChatType.CT_Spell);
                MessageToLiving(ad.Attacker, $"A barrier absorbs {damageAbsorbed} damage of your attack!", eChatType.CT_Spell);
            }
        }

        /// <summary>
        /// sends a message to a living
        /// </summary>
        /// <param name="living"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void MessageToLiving(GameLiving living, string message, eChatType type)
        {
            if (living is GamePlayer && !string.IsNullOrEmpty(message))
            {
                living.MessageToSelf(message, type);
            }
        }

        public override string Name => "Shadow Shroud";

        public override ushort Icon => 1842;

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Reduce all incoming damage by 10% and increase the Nightshade’s chance to be missed by 10% for 30 seconds"
                };

                return list;
            }
        }
    }
}