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
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Searing pet RA
    /// </summary>
    public class SearingPetAbility : RR5RealmAbility
    {
        public const int DURATION = 19 * 1000;

        public SearingPetAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player != null && player.ControlledNpcBrain != null && player.ControlledNpcBrain.Body != null)
            {
                GameNPC pet = player.ControlledNpcBrain.Body as GameNPC;
                if (pet.IsAlive)
                {
                    SearingPetEffect SearingPet = (SearingPetEffect)pet.EffectList.GetOfType(typeof(SearingPetEffect));
                    if (SearingPet != null) SearingPet.Cancel(false);
                    new SearingPetEffect(player).Start(pet);
                }
                DisableSkill(living);
            }
            else if (player != null)
            {
                player.Out.SendMessage("You must have a controlled pet to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.DisableSkill(this, 3 * 1000);
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 120;
        }

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add(" PBAoE Pet pulsing effect, 350units, 25 damage, 6 ticks, 2min RUT.");
            list.Add("");
            list.Add("Target: Pet");
            list.Add("Duration: 18s");
            list.Add("Casting time: Instant");
        }

    }
}


