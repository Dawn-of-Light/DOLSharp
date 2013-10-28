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
    /// Overwhelm effect Infi RA
    /// </summary>
    /// /// <author>Stexx</author>
    public class OverwhelmEffect : TimedEffect
    {
        private GamePlayer EffectOwner;

        public OverwhelmEffect()
            : base(RealmAbilities.OverwhelmAbility.DURATION)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer)
            {
                EffectOwner = target as GamePlayer;
                foreach (GamePlayer p in EffectOwner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(EffectOwner, EffectOwner, OverwhelmAbility.EFFECT , 0, false, 1);
                }
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

            OverwhelmEffect Overwhelm = (OverwhelmEffect)player.EffectList.GetOfType<OverwhelmEffect>();
            if (Overwhelm != null)
                Overwhelm.Cancel(false);
        }

        public override string Name { get { return "Overwhelm"; } }
        public override ushort Icon { get { return 1841; } }

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("a 15% increased chance to bypass their target�s block, parry, and evade defenses for 30 seconds.");
                return list;
            }
        }
    }

}