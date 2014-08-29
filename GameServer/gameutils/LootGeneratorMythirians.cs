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
 
using DOL.GS;
using DOL.AI.Brain;
using DOL.Database;
 
namespace DOL.GS
{
 
	/// <summary>
	/// Loot Generator for Game Mythirian
	/// Will Randomly pick a Mythirian to loot based on a preset Chance, and Mythirian Champion Level.
	/// </summary>
	public class LootGeneratorMythirians : LootGeneratorBase
	{
		
		private static IList<ItemTemplate> m_MythirianListCL6 = GameServer.Database.SelectObjects<ItemTemplate>("ClassType LIKE 'DOL.GS.GameMythirian' AND Type_Damage = 6");
		private static IList<ItemTemplate> m_MythirianListCL8 = GameServer.Database.SelectObjects<ItemTemplate>("ClassType LIKE 'DOL.GS.GameMythirian' AND Type_Damage = 8");
		private static IList<ItemTemplate> m_MythirianListCL10 = GameServer.Database.SelectObjects<ItemTemplate>("ClassType LIKE 'DOL.GS.GameMythirian' AND Type_Damage = 10");
		
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
				
				
				int minCL6 = ServerProperties.Properties.LOOTGENERATOR_MYTHIRIAN_MIN_DROPLVL_CL6;
				int minCL8 = ServerProperties.Properties.LOOTGENERATOR_MYTHIRIAN_MIN_DROPLVL_CL8;
				int minCL10 = ServerProperties.Properties.LOOTGENERATOR_MYTHIRIAN_MIN_DROPLVL_CL10;
				int baseChance = ServerProperties.Properties.LOOTGENERATOR_MYTHIRIAN_BASE_CHANCE;
				
				ItemTemplate mythirianToDrop;
				
				if(lvl >= minCL10) 
				{
					mythirianToDrop = PickRandomMythirian(m_MythirianListCL10);
				}
				else if(lvl >= minCL8)
				{
					mythirianToDrop = PickRandomMythirian(m_MythirianListCL8);
				}
				else if(lvl >= minCL6)
				{
					mythirianToDrop = PickRandomMythirian(m_MythirianListCL6);
				}
				else
				{
					return loot;
				}

				//BOSS'S BONUS
				if (!mob.Name[0].ToString().ToLower().Equals(mob.Name[0].ToString()))
				{
					baseChance = ServerProperties.Properties.LOOTGENERATOR_MYTHIRIAN_NAMED_CHANCE;
				}
				
				if(Util.Chance(baseChance+Math.Max(10, killedcon)))
				{
				        loot.AddFixed(mythirianToDrop, 1);
				}
            }
            catch
            {
            	return loot;
            }
            
            return loot;
        }
        
        private ItemTemplate PickRandomMythirian(IList<ItemTemplate> mythirianFromList) 
        {
        	int randomPick = Util.Random(mythirianFromList.Count-1);
        	
        	return new ItemTemplate(mythirianFromList[randomPick]);
        }
	}
}
