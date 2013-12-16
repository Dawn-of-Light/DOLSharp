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
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
    /// <summary>
    /// ShadowShroud Effect NS RR5 RA
    /// </summary>
    /// <author>Stexx</author>
    public class ShadowShroudEffect : TimedEffect
    {
        private GamePlayer EffectOwner;

        public ShadowShroudEffect()
            : base(RealmAbilities.ShadowShroudAbility.DURATION)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer)
            {
                EffectOwner = target as GamePlayer;
                foreach (GamePlayer p in EffectOwner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(EffectOwner, EffectOwner, ShadowShroudAbility.EFFECT, 0, false, 1);
                }

                EffectOwner.AbilityBonus[(int)eProperty.MissHit] += ShadowShroudAbility.MISSHITBONUS;
                GameEventMgr.AddHandler(EffectOwner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }
        }
        public override void Stop()
        {
            if (EffectOwner != null)
            {
                EffectOwner.AbilityBonus[(int)eProperty.MissHit] -= ShadowShroudAbility.MISSHITBONUS;
                GameEventMgr.RemoveHandler(EffectOwner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
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
            GamePlayer player = sender as GamePlayer;

            ShadowShroudEffect ShadowShroud = (ShadowShroudEffect)player.EffectList.GetOfType<ShadowShroudEffect>();
            if (ShadowShroud != null)
                ShadowShroud.Cancel(false);
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        protected void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
            if (args == null) return;
            if (args.AttackData == null) return;
          
            AttackData ad = args.AttackData;
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            double absorbPercent = ShadowShroudAbility.ABSPERCENT;
            int damageAbsorbed = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));
            if (damageAbsorbed > 0)
            {
                ad.Damage -= damageAbsorbed;
                //TODO correct messages
                MessageToLiving(ad.Target, string.Format("Shadow Shroud Ability absorbs {0} damage!", damageAbsorbed), eChatType.CT_Spell);
                MessageToLiving(ad.Attacker, string.Format("A barrier absorbs {0} damage of your attack!", damageAbsorbed), eChatType.CT_Spell);
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
            if (living is GamePlayer && message != null && message.Length > 0)
            {
                living.MessageToSelf(message, type);
            }
        }
        public override string Name { get { return "Shadow Shroud"; } }
        public override ushort Icon { get { return 1842; } }

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("Reduce all incoming damage by 10% and increase the Nightshade’s chance to be missed by 10% for 30 seconds");
                return list;
            }
        }
    }

}