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
using System.Collections;

namespace DOL.GS.Effects
{

    public class GiftOfPerizorEffect : TimedEffect
    {
        private GamePlayer EffectOwner;

        public GiftOfPerizorEffect()
            : base(RealmAbilities.GiftOfPerizorAbility.DURATION)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(target, target, 7036, 0, false, 1);
            }
            EffectOwner = target as GamePlayer;
        }
        public override void Stop()
        {
            if (EffectOwner != null) EffectOwner.TempProperties.removeProperty("GiftOfPerizorOwner");
            base.Stop();
        }

        public override string Name { get { return "Gift Of Perizor"; } }
        public override ushort Icon { get { return 3090; } }

        // Delve Info
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add("Buff group with 25% damage reduction for 60 seconds, return damage reduced as power.");
                return list;
            }
        }
    }
}
