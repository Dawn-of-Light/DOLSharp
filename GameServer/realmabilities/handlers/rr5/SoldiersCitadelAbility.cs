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
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Soldier's Citadel RA
    /// </summary>
    public class SoldiersCitadelAbility : RR5RealmAbility
    {
        public const int DURATION = 30 * 1000;
        public const int SECOND_DURATION = 15 * 1000;

        public SoldiersCitadelAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                SoldiersCitadelEffect SoldiersCitadel = (SoldiersCitadelEffect)player.EffectList.GetOfType(typeof(SoldiersCitadelEffect));
                if (SoldiersCitadel != null)
                    SoldiersCitadel.Cancel(false);

                new SoldiersCitadelEffect().Start(player);
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add("+50% block/parry 30s, -10% block/parry 15s.");
            list.Add("");
            list.Add("Target: Self");
            list.Add("Duration: 45 sec");
            list.Add("Casting time: Instant");
        }

    }
}
