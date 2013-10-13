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


//
// This is the Storm Unique Object Generator. changed to instanciate GeneratedUniqueItem
//
// Original version by Etaew
// Modified by Tolakram to add live like names and item models
//
// Released to the public on July 12th, 2010
//
// Please enjoy this generator and submit any fixes to the DOL project to benefit everyone.
// - Tolakram
//
// Updating to instance object of GeneratedUniqueITem by Leodagan on Aug 2013.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

using log4net;


namespace DOL.GS
{
	public class LootGeneratorUniqueItem : LootGeneratorBase
	{

		[CmdAttribute(
			"&genuniques",
			ePrivLevel.GM,
			"/genuniques ([TOA] || [L51] || [objecttype]) [itemtype] : generate 8 unique items")]
		public class LootGeneratorUniqueObjectCommandHandler : DOL.GS.Commands.AbstractCommandHandler, DOL.GS.Commands.ICommandHandler
		{
			public void OnCommand(GameClient client, string[] args)
			{

				try
				{

					for (int x = 0; x < 8; x++)
					{
						GeneratedUniqueItem item = null;

						if (args.Length > 1)
						{
							if (Convert.ToString(args[1]).ToUpper() == "TOA")
							{
								item = new GeneratedUniqueItem(true, client.Player.Realm, 51);
								item.GenerateItemQuality(GameObject.GetConLevel(client.Player.Level, 60));
							}
							else if (Convert.ToString(args[1]).ToUpper() == "L51")
							{
								item = new GeneratedUniqueItem(client.Player.Realm, 51);
								item.GenerateItemQuality(GameObject.GetConLevel(client.Player.Level, 50));
							}
							else
							{
								if (args.Length > 2)
									item = new GeneratedUniqueItem(client.Player.Realm, client.Player.Level, (eObjectType)Convert.ToInt32(args[1]), (eInventorySlot)Convert.ToInt32(args[2]));
								else
									item = new GeneratedUniqueItem(client.Player.Realm, client.Player.Level, (eObjectType)Convert.ToInt32(args[1]));
							}
						}
						
						item.AllowAdd = true;
						GameServer.Database.AddObject(item);
						InventoryItem invitem = GameInventoryItem.Create<ItemUnique>(item);
						client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
						client.Player.Out.SendMessage("Generated: " + item.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);

					}

				}
				catch (Exception)
				{
					DisplaySyntax(client);
				}
			}
		}

		[CmdAttribute(
			"&clearinventory",
			ePrivLevel.GM,
			"/clearinventory YES - clears your entire inventory!")]
		public class ClearInventoryCommandHandler : DOL.GS.Commands.AbstractCommandHandler, DOL.GS.Commands.ICommandHandler
		{
			public void OnCommand(GameClient client, string[] args)
			{
				// must add at least one parameter just to be safe
				if (args.Length > 1 && args[1].ToString() == "YES")
				{
					foreach (InventoryItem item in client.Player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
						client.Player.Inventory.RemoveItem(item);

					client.Out.SendMessage("Inventory cleared!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					DisplaySyntax(client);
				}
			}
		}
	
	
		//base chance in %
		public static ushort BASE_ROG_CHANCE = 15;
		
		//Named loot chance (added to base chance)
		public static ushort NAMED_ROG_CHANCE = 5;
		
		//base TOA chance in % (0 to disable TOA in other region than TOA)
		public static ushort BASE_TOA_CHANCE = 25;
			
		//Named TOA loot chance (added to named rog chance)
		public static ushort NAMED_TOA_CHANCE = 5;

		/// <summary>
		/// Generate loot for given mob
		/// </summary>
		/// <param name="mob"></param>
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

				// allow the leader to decide the loot realm
				if (player.Group != null)
					player = player.Group.Leader;
			
				double killedCon = player.GetConLevel(mob);
			
				//grey don't loot RoG
				if(killedCon <= -3)
						return loot;
			
				// chance to get a RoG Item
				int chance = BASE_ROG_CHANCE + ((int)killedCon + 3)*2;
				// toa item
				bool toachance = Util.Chance(BASE_TOA_CHANCE);
				
				if (IsMobInTOA(mob) && mob.Name.ToLower() != mob.Name && mob.Level >= 50)
				{
					// ToA named mobs have good chance to drop unique loot
					chance += NAMED_ROG_CHANCE+NAMED_TOA_CHANCE;
					toachance = true;
				}
				else if(IsMobInTOA(mob)) 
				{
					toachance = true;
				}
				else if (mob.Name.ToLower() != mob.Name) 
				{
					chance += NAMED_ROG_CHANCE;
					// Classic Named earn this
					toachance = true;
				}
			
				GeneratedUniqueItem item = new GeneratedUniqueItem(toachance, player.Realm, (byte)Math.Min(mob.Level+1, 51));
				item.AllowAdd = true;
				item.GenerateItemQuality(killedCon);
			
				if (player.Realm != 0 && Util.Chance(chance))
				{
					loot.AddFixed(item, 1);
				}
			}
			catch
			{
				return loot;
			}
			
			return loot;
			
		}
		
		public static bool IsMobInTOA(GameNPC mob)
		{
			if (mob.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
					return true;

			return false;
		}	
	
	}
}
