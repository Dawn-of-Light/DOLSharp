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
 *
 * [NOTE:StephenxPimentel] The Die/AddToWorld functions are just an example.
 * Although this is a great base for individual Relic Protector Classes, and
 * it would be a very good idea for new classes to be derived from this
 *
 * e.g. Lab_Center_Relic : BaseProtector.
 * This way you can call the Locking/Unlocking Methods!
 *
 * Make sure that in your scripts u define "Relic" as the correct relic so
 * the Unlock/Lock Functions work properly!
 */

namespace DOL.GS
{
    public class BaseProtector : GameNPC
    {
        public static MinotaurRelic Relic { get; set; }

        public static GameNPC LockedEffect { get; set; }

        public static void UnlockRelic()
        {
            LockedEffect?.RemoveFromWorld();
        }

        // Never save the mob into the database either
        // or you will get double spawns on server start!
        public override void SaveIntoDatabase()
        {
        }

        public static bool LockRelic()
        {
            // make sure the relic exists before you lock it!
            if (Relic == null)
            {
                return false;
            }

            LockedEffect = new GameNPC
            {
                Model = 1583,
                Name = "LOCKED_RELIC",
                X = Relic.X,
                Y = Relic.Y,
                Z = Relic.Z,
                Heading = Relic.Heading,
                CurrentRegionID = Relic.CurrentRegionID,
                Flags = eFlags.CANTTARGET
            };
            LockedEffect.AddToWorld();

            return true;
        }

        // these mobs should NEVER respawn.
        public override int RespawnInterval => 0;

        public override void Die(GameObject killer)
        {
            /* NOTE: THIS IS JUST AN EXAMPLE!
             * Unlock the relic when the mob dies!

            UnlockRelic();
             */

            base.Die(killer);
        }

        public override bool AddToWorld()
        {
            LoadedFromScript = true;

            /* NOTE: THIS IS JUST AN EXAMPLE!
             * At Spawn: Define Relic with RelicID, then lock it!

            Relic = MinotaurRelicManager.GetRelic(1);
            LockRelic();
             */

            // You must also define the location that this mob should spawn
            // it will be spawned whenever the relic is spawned.
            return base.AddToWorld();
        }
    }
}
