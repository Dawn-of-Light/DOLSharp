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
 * [StephenxPimentel: Example usage of Mino-Relic protectors]
 * I've created this script to help show what needs to be in
 * each protector to make it work properly. 
 * 
 * Please make note of the AddToWorld function,
 * aswell as the Die function.
 * 
 * Add To World:
 * - you MUST set the mobs spawn location
 * - you MUST name your mob
 * - you MUST set the Relic associated by Relic ID, or Relic InternalID.
 * - you MUST call the LockRelic(); method.
 * 
 * Die:
 * - you MUST call the UnlockRelic(); method.
 * 
 * 
 * Other than these things, you are free to do whatever you want.
 * Classes should inherit from "BaseProtector"
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS
{
    //all classes should inherit from BaseProtector.
    public class ArrektosProtector : BaseProtector
    {
        public const string ALREADY_GOT_HELP = "ALREADY_GOT_HELP";

        public override bool AddToWorld()
        {
            //foreman fogo doesn't leave the room.
            TetherRange = 1000;

            X = 49293;
            Y = 42208;
            Z = 27562;
            Heading = 2057;
            CurrentRegionID = 245;

            Flags = 0;

            Level = 56;
            Model = 2249; //undead Minotaur
            Name = "Forge Foreman Fogo";
            Size = 65;

            //get the relic by its ID, and lock it!
            Relic = MinotaurRelicManager.GetRelic(1);
            LockRelic();



            TempProperties.setProperty(ALREADY_GOT_HELP, false);

            return base.AddToWorld();
        }
        public override void StartAttack(GameObject target)
        {
            base.StartAttack(target);

            if (!TempProperties.getProperty<bool>(ALREADY_GOT_HELP))
            {
                foreach (GameNPC npc in GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    //on initial attack, all fireborn in range add!
                    if (npc.Name == "minotaur fireborn")
                    npc.StartAttack(target);
                }

                TempProperties.setProperty(ALREADY_GOT_HELP, true);
            }
        }
        public override void Die(GameObject killer)
        {
            base.Die(killer);

            TempProperties.setProperty(ALREADY_GOT_HELP, false);

            //when the protector is dead, the relic should be unlocked!
            UnlockRelic();

            //another thing is that most of these mobs drop 1 time drops
            //i haven't added support for this, but someone will eventually.
        }
    }
}
