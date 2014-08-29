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
 
using DOL.GS;
using DOL.AI.Brain;
using DOL.Database;
 
namespace DOL.GS
{
 
    /// <summary>
    /// LootGeneratorSeals for DiamondSeal, EmeraldSeal, SapphireSeal
    /// At the moment this generator only adds Seals to the loot
    /// </summary>
    public class LootGeneratorSeals : LootGeneratorBase
    {
        private static ItemTemplate m_EmeraldSeal = GameServer.Database.FindObjectByKey<ItemTemplate>("EmeraldSeal");
        private static ItemTemplate m_SapphireSeal = GameServer.Database.FindObjectByKey<ItemTemplate>("SapphireSeal");
        private static ItemTemplate m_DiamondSeal = GameServer.Database.FindObjectByKey<ItemTemplate>("DiamondSeal");
       
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
				int killedcon = (int)player.GetConLevel(mob)+3;
				
				if(killedcon <= 0)
				        return loot;
				
				int lvl = mob.Level;
				if (lvl < 1) lvl = 1;
				
				double namedCount = ServerProperties.Properties.LOOTGENERATOR_SEALS_NAMED_COUNT; //default 1.5         
				int baseChance = ServerProperties.Properties.LOOTGENERATOR_SEALS_BASE_CHANCE; //default 70
				
				int levelEmerald = ServerProperties.Properties.LOOTGENERATOR_SEALS_MIN_DROPLVL_EMERALD; //default 10
				int levelSaphir = ServerProperties.Properties.LOOTGENERATOR_SEALS_MIN_DROPLVL_SAPHIR; //default 35
				int levelDiamond = ServerProperties.Properties.LOOTGENERATOR_SEALS_MIN_DROPLVL_DIAMOND; //default 50
				
				int maxcount;
				
				ItemTemplate sealToDrop;
				
				//Sealstype per level
				if (lvl >= levelDiamond)
				{
					sealToDrop = new ItemTemplate(m_DiamondSeal);
				}
				else if (lvl >= levelSaphir)
				{
					sealToDrop = new ItemTemplate(m_SapphireSeal);
				}
				else if(lvl >= levelEmerald)
				{
					sealToDrop = new ItemTemplate(m_EmeraldSeal);
				}
				else 
				{
					return loot;
				}								
				
				maxcount = Util.Random(1, lvl / 10);
				
				//BOSS'S BONUS
				if (!mob.Name[0].ToString().ToLower().Equals(mob.Name[0].ToString()))
				{
					sealToDrop.PackSize = Util.Random(2, 5);
				    maxcount = (int)Math.Round(maxcount*namedCount);
				}
				
				if(Util.Chance(baseChance+Math.Max(10, killedcon)))
				{
				        loot.AddFixed(sealToDrop, maxcount);
				}
            }
            catch
            {
                    return loot;
            }
            
            return loot;
        }  
    }
}

