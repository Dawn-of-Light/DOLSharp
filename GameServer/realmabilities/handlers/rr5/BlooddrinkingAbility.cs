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
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// BloodDrinking Ability SB RR5 RA
    /// </summary>
    /// /// <author>Stexx</author>
    public class BloodDrinkingAbility : RR5RealmAbility
    {
        public const int DURATION = 30 * 1000; // 30 secs
        public const double HEALPERCENT = 20; // 20% heal
        public const int EFFECT = 1567;

        public BloodDrinkingAbility(DBAbility dba, int level) : base(dba, level) { }

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
                BloodDrinkingEffect BloodDrinking = (BloodDrinkingEffect)player.EffectList.GetOfType<BloodDrinkingEffect>();
                if (BloodDrinking != null)
                    BloodDrinking.Cancel(false);

                new BloodDrinkingEffect().Start(player);
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("Cause the Shadowblade to be healed for 20% of all damage he does for 30 seconds");
            list.Add("");
            list.Add("Target: Self");
            list.Add("Duration: 30 sec");
            list.Add("Casting time: Instant");
            list.Add("Re-use : 5 minutes");

        }

    }
}
