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
    /// Minion Rescue RA
    /// </summary>
    public class MinionRescueAbility : RR5RealmAbility
    {
        public const int DURATION = 6 * 1000;
        public const int SpellRadius = 500;

        public MinionRescueAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player == null)
                return;

            foreach (GamePlayer Targetplayer in player.GetPlayersInRadius((ushort)SpellRadius))
            {
                if (Targetplayer != null && Targetplayer.IsAlive && GameServer.ServerRules.IsAllowedToAttack(player, Targetplayer, true))
                {
                    MinionRescueEffect raEffect = new MinionRescueEffect();
                    raEffect.Start(player);
                    DisableSkill(living);
                    return;
                }
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add("Summon pets that will follow and stun enemies.");
            list.Add("");
            list.Add("Target: Enemy");
            list.Add("Duration: 6 sec");
            list.Add("Casting time: Instant");
        }

    }
}
