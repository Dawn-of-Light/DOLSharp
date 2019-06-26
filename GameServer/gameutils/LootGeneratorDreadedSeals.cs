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
using System.Reflection;
using DOL.GS;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Keeps;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// LootGeneratorDreadedSeals
    /// At the moment this generator only adds dreaded seals to the loot
    /// </summary>
    public class LootGeneratorDreadedSeals : LootGeneratorBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ItemTemplate m_GlowingDreadedSeal = GameServer.Database.FindObjectByKey<ItemTemplate>("glowing_dreaded_seal");
        private static readonly ItemTemplate m_SanguineDreadedSeal = GameServer.Database.FindObjectByKey<ItemTemplate>("sanguine_dreaded_seal");

        /// <summary>       
        /// Generate loot for given mob
        /// </summary>
        /// <param name="mob"></param>
        /// <param name="killer"></param>
        /// <returns></returns>
        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList loot = base.GenerateLoot(mob, killer);

            try
            {
                GamePlayer player = killer as GamePlayer;
                if (killer is GameNPC && ((GameNPC)killer).Brain is IControlledBrain)
                    player = ((ControlledNpcBrain)((GameNPC)killer).Brain).GetPlayerOwner();
                if (player == null)
                    return loot;

                switch (mob)
                {
                    // Certain mobs have a 100% drop chance of multiple seals at once
                    case GuardLord lord:
                        if (lord.IsTowerGuard || lord.Component.AbstractKeep.BaseLevel < 50)
                            loot.AddFixed(m_SanguineDreadedSeal, 1);  // Guaranteed drop, but towers and BGs only merit 1 seal.
                        else
                            loot.AddFixed(m_SanguineDreadedSeal, 5 * lord.Component.Keep.Level);
                        break;
                    default:
                        if (mob.Name.ToUpper() == "LORD AGRAMON")
                            loot.AddFixed(m_SanguineDreadedSeal, 10);
                        else if (mob.Level >= ServerProperties.Properties.LOOTGENERATOR_DREADEDSEALS_STARTING_LEVEL)
                        {
                        int iPercentDrop = (mob.Level - ServerProperties.Properties.LOOTGENERATOR_DREADEDSEALS_STARTING_LEVEL)
	                        * ServerProperties.Properties.LOOTGENERATOR_DREADEDSEALS_DROP_CHANCE_PER_LEVEL
	                        + ServerProperties.Properties.LOOTGENERATOR_DREADEDSEALS_BASE_CHANCE;

                        if (!mob.Name.ToLower().Equals(mob.Name)) // Named mobs are more likely to drop a seal
	                        iPercentDrop = (int)Math.Round(iPercentDrop * ServerProperties.Properties.LOOTGENERATOR_DREADEDSEALS_NAMED_CHANCE);

                        if (Util.Random(9999) < iPercentDrop)
	                        loot.AddFixed(m_GlowingDreadedSeal, 1);
                        }
                        break;
	            }// switch
            }//try
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            return loot;
        }
	}
}
