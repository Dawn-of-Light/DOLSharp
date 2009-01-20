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
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Valhalla's Blessing RA
    /// </summary>
    public class ValhallasBlessingAbility : RR5RealmAbility
    {
        public const int DURATION = 30 * 1000;
        private const int SpellRadius = 1500;

        public ValhallasBlessingAbility(DBAbility dba, int level) : base(dba, level) { }

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
                ArrayList targets = new ArrayList();
                if (player.Group == null)
                    targets.Add(player);
                else
                {
                    foreach (GamePlayer grpplayer in player.Group.GetPlayersInTheGroup())
                    {
                        if (WorldMgr.CheckDistance(grpplayer, player, SpellRadius) && grpplayer.IsAlive)
                            targets.Add(grpplayer);
                    }
                }
                foreach (GamePlayer target in targets)
                {
                    //send spelleffect
                    if (!target.IsAlive) continue;
                    ValhallasBlessingEffect ValhallasBlessing = (ValhallasBlessingEffect)target.EffectList.GetOfType(typeof(ValhallasBlessingEffect));
                    if (ValhallasBlessing != null)
                        ValhallasBlessing.Cancel(false);
                    new ValhallasBlessingEffect().Start(target);
                }
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add("Spells/Styles used by group have has a chance of not costing power or endurance. 30s duration, 10min RUT.");
            list.Add("");
            list.Add("Target: Group");
            list.Add("Duration: 30s");
            list.Add("Casting time: Instant");
        }

    }
}


