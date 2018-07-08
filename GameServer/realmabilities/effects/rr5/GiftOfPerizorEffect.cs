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

using System.Collections.Generic;

namespace DOL.GS.Effects
{

    public class GiftOfPerizorEffect : TimedEffect
    {
        private const int Duration = 60 * 1000;

        private GamePlayer _effectOwner;

        public GiftOfPerizorEffect()
            : base(Duration)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(target, target, 7036, 0, false, 1);
            }

            _effectOwner = target as GamePlayer;
        }

        public override void Stop()
        {
            _effectOwner?.TempProperties.removeProperty("GiftOfPerizorOwner");

            base.Stop();
        }

        public override string Name => "Gift Of Perizor";

        public override ushort Icon => 3090;

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Buff group with 25% damage reduction for 60 seconds, return damage reduced as power."
                };

                return list;
            }
        }
    }
}
