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
    /// Effect handler for Fanatacism
    /// </summary>
    public class FanatacismEffect : TimedEffect
    {
        private const int Duration = 45 * 1000;
        private const int Value = 25;

        private GamePlayer _effectOwner;

        public FanatacismEffect()
            : base(Duration)
        { }

         public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _effectOwner = player;
                foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(_effectOwner, p, 7088, 0, false, 1);
                }

                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.MagicAbsorption] += Value;
            }
        }

        public override void Stop()
        {
            if (_effectOwner != null)
            {
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.MagicAbsorption] -= Value;
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
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
            Cancel(false);
        }

        public override string Name => "Fanatacism";

        public override ushort Icon => 7088;

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Grants a reduction in all spell damage taken for 45 seconds."
                };

                return list;
            }
        }
    }
}
