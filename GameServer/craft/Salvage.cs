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
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The class holding all salvage functions
	/// </summary>
	public class Salvage
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaration

		/// <summary>
		/// The SalvageYield entry for the item being salvaged
		/// </summary>
		protected const string SALVAGE_YIELD = "SALVAGE_YIELD";

		/// <summary>
		/// The item being salvaged
		/// </summary>
		protected const string SALVAGED_ITEM = "SALVAGED_ITEM";

		#endregion

		#region First call function and callback

		/// <summary>
		/// Begin salvaging an inventory item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static int BeginWork(GamePlayer player, InventoryItem item)
		{
            SalvageYield salvageYield = null;

			if (!IsAllowedToBeginWork(player, item))
			{
				return 0;
			}

			int salvageLevel = CraftingMgr.GetItemCraftLevel(item) / 100;
			if(salvageLevel > 9) salvageLevel = 9; // max 9

			string sql = "";

			if (item.SalvageYieldID == 0)
			{
				sql = "ObjectType=" + item.Object_Type + " AND SalvageLevel=" + salvageLevel;
			}
			else
			{
				sql = "ID=" + item.SalvageYieldID;
			}

			if (ServerProperties.Properties.USE_SALVAGE_PER_REALM)
			{
				// Some items use realm, some do not, so allow a find of either a set realm, or 0
				sql += " AND (Realm=" + item.Realm + " OR Realm=0)";
			}

			salvageYield = GameServer.Database.SelectObject<SalvageYield>(sql);
			ItemTemplate material = null;

			if (salvageYield != null && string.IsNullOrEmpty(salvageYield.MaterialId_nb) == false)
			{
				material = GameServer.Database.FindObjectByKey<ItemTemplate>(salvageYield.MaterialId_nb);

				if (material == null)
				{
					player.Out.SendMessage("Can't find material (" + material.Id_nb + ") needed to salvage this item!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					log.ErrorFormat("Salvage Error for ID: {0}:  Material not found: {1}", salvageYield.ID, material.Id_nb);
				}
			}

            if (material == null)
			{
				if (salvageYield == null && item.SalvageYieldID > 0)
				{
					player.Out.SendMessage("This items salvage recipe (" + item.SalvageYieldID + ") not implemented yet.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					log.ErrorFormat("SalvageYield ID {0} not found for item: {1}", item.SalvageYieldID, item.Name);
				}
				else if (salvageYield == null)
				{
					player.Out.SendMessage("Salvage recipe not found for this item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					log.ErrorFormat("Salvage Lookup Error: ObjectType: {0}, Item: {1}", item.Object_Type, item.Name);
				}
				return 0;
			}

			if (player.IsMoving || player.IsStrafing)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.InterruptSalvage"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			if (player.IsStealthed)
			{
				player.Stealth(false);
			}
			
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.BeginSalvage", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			// clone the yield entry and update values to work with this salvage (not saved to the DB)
			SalvageYield yield = salvageYield.Clone() as SalvageYield;

			if (item.SalvageYieldID == 0 || yield.Count == 0)
			{
				// Calculated salvage values
				int count = GetMaterialYield(player, item, yield, material);
				if (count < 1)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.NoSalvage", item.Name + ". The material returned amount is zero"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
			}

			player.Out.SendTimerWindow(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.Salvaging", item.Name), yield.Count);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(Proceed);
			player.CraftTimer.Properties.setProperty(AbstractCraftingSkill.PLAYER_CRAFTER, player);
			player.CraftTimer.Properties.setProperty(SALVAGED_ITEM, item);
			player.CraftTimer.Properties.setProperty(SALVAGE_YIELD, yield);

			player.CraftTimer.Start(yield.Count * 1000);
			return 1;
		}

		/// <summary>
		/// Begin salvaging a siege weapon
		/// </summary>
		/// <param name="player"></param>
		/// <param name="siegeWeapon"></param>
		/// <returns></returns>
		public static int BeginWork(GamePlayer player, GameSiegeWeapon siegeWeapon)
		{
			if (siegeWeapon == null)
				return 0;
			// Galenas
			siegeWeapon.ReleaseControl();
			siegeWeapon.RemoveFromWorld();
			bool error = false;
            DBCraftedItem recipe = GameServer.Database.SelectObject<DBCraftedItem>("Id_nb ='" + siegeWeapon.ItemId + "'");

			if (recipe == null)
            {
				player.Out.SendMessage("Error retrieving salvage data!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				log.Error("Salvage Siege Error: DBCraftedItem is null for" + siegeWeapon.ItemId);
				return 1;
            }

			IList<DBCraftedXItem> rawMaterials = GameServer.Database.SelectObjects<DBCraftedXItem>("`CraftedItemId_nb` = '" + recipe.Id_nb + "'");

			if (rawMaterials == null || rawMaterials.Count == 0)
            {
				player.Out.SendMessage("No raw materials provided for this siege weapon!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				log.Error("Salvage Siege Error: No Raw Materials found for " + siegeWeapon.ItemId);
				return 1;
            }

            if (player.IsCrafting)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.IsAllowedToBeginWork.EndCurrentAction"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }
			InventoryItem item;
			ItemTemplate template;
			foreach (DBCraftedXItem material in rawMaterials)
			{
				template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

				if (template == null)
				{
					player.Out.SendMessage("Missing raw material " + material.IngredientId_nb + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					log.Error("Salvage Siege Error: Raw Material not found " + material.IngredientId_nb);
					return 1;
				}

				item = GameInventoryItem.Create<ItemTemplate>(template);
				item.Count = material.Count;
				if (!player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
				{
					error = true;
					break;
				}
			}

			if (error)
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.NoRoom"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return 1;
		}

		/// <summary>
		/// Called when craft time is finished
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		protected static int Proceed(RegionTimer timer)
		{
			GamePlayer player = timer.Properties.getProperty<GamePlayer>(AbstractCraftingSkill.PLAYER_CRAFTER, null);
			InventoryItem itemToSalvage = timer.Properties.getProperty<InventoryItem>(SALVAGED_ITEM, null);
			SalvageYield yield = timer.Properties.getProperty<SalvageYield>(SALVAGE_YIELD, null);
			int materialCount = yield.Count;

			if (player == null || itemToSalvage == null || yield == null || materialCount == 0)
			{
				player.Out.SendMessage("Error retrieving salvage data for this item!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				log.Error("Salvage: There was a problem getting back salvage info from the craft timer.");
				return 0;
			}

			ItemTemplate rawMaterial = null;

			if (string.IsNullOrEmpty(yield.MaterialId_nb) == false)
			{
				rawMaterial = GameServer.Database.FindObjectByKey<ItemTemplate>(yield.MaterialId_nb);
			}

			if (rawMaterial == null)
			{
				player.Out.SendMessage("Error finding the raw material needed to salvage this item!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				log.Error("Salvage: Error finding raw material " + yield.MaterialId_nb);
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();

			player.Inventory.RemoveItem(itemToSalvage); // clean the free of the item to salvage
			
			Hashtable changedSlots = new Hashtable(5); // value: < 0 = new item count; > 0 = add to old
			lock(player.Inventory)
			{
				int count = materialCount;
				foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					if (item == null) continue;
					if (item.Id_nb != rawMaterial.Id_nb) continue;
					if (item.Count >= item.MaxCount) continue;

					int countFree = item.MaxCount - item.Count;
					if (count > countFree)
					{
						changedSlots.Add(item.SlotPosition, countFree); // existing item should be changed
						count -= countFree;
					}
					else
					{
						changedSlots.Add(item.SlotPosition, count); // existing item should be changed
						count = 0;
						break;
					}
				}

				if(count > 0) // Add new object
				{
					eInventorySlot firstEmptySlot = player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
					changedSlots.Add((int)firstEmptySlot, -count); // Create the item in the free slot (always at least one)
					count = 0;
				}
				
			}

			InventoryItem newItem = null;

			player.Inventory.BeginChanges();
			foreach(DictionaryEntry de in changedSlots)
			{
				int countToAdd = (int) de.Value;
				if(countToAdd > 0)	// Add to exiting item
				{
					newItem = player.Inventory.GetItem((eInventorySlot)de.Key);
					player.Inventory.AddCountToStack(newItem, countToAdd);
				}
				else
				{
					newItem = GameInventoryItem.Create<ItemTemplate>(rawMaterial);
					newItem.Count = -countToAdd;
					player.Inventory.AddItem((eInventorySlot)de.Key, newItem);
				}
			}

			player.Inventory.CommitChanges();
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.Proceed.GetBackMaterial", materialCount, rawMaterial.Name, itemToSalvage.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			
			return 0;
		}
		
		#endregion
		
		#region Requirement check

		/// <summary>
		/// Check if the player can begin to salvage an item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsAllowedToBeginWork(GamePlayer player, InventoryItem item)
		{
			if (player.InCombat)
			{
				player.Out.SendMessage("You can't salvage while in combat.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (item.IsNotLosingDur || item.IsIndestructible)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.NoSalvage", item.Name + ".  This item is indestructible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			
			if(item.SlotPosition < (int)eInventorySlot.FirstBackpack || item.SlotPosition > (int)eInventorySlot.LastBackpack)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.IsAllowedToBeginWork.BackpackItems"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			eCraftingSkill skill = CraftingMgr.GetSecondaryCraftingSkillToWorkOnItem(item);
			if(skill == eCraftingSkill.NoCrafting)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.BeginWork.NoSalvage", item.Name + ".  You do not have the required secondary skill"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.IsCrafting)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.IsAllowedToBeginWork.EndCurrentAction"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.GetCraftingSkillValue(skill) < (0.75 * CraftingMgr.GetItemCraftLevel(item)))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Salvage.IsAllowedToBeginWork.NotEnoughSkill", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}
		
		#endregion

		#region Calcul functions

        /// <summary>
        /// Calculate the count per Object_Type
        /// </summary>
        public static int GetCountForSalvage(InventoryItem item, SalvageYield salvage, ItemTemplate rawMaterial)
        {
            long maxCount = 0;

			if (rawMaterial == null)
				return 0;

            #region Weapons

            switch ((eObjectType)item.Object_Type)
            {
                case eObjectType.RecurvedBow:
                case eObjectType.CompositeBow:
                case eObjectType.Longbow:
                case eObjectType.Crossbow:
                case eObjectType.Staff:
                case eObjectType.Fired:
                    maxCount += 36;
                    break;
                case eObjectType.Thrown:
                case eObjectType.CrushingWeapon:
                case eObjectType.SlashingWeapon:
                case eObjectType.ThrustWeapon:
                case eObjectType.Flexible:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.Piercing:
                case eObjectType.Sword:
                case eObjectType.Hammer:
                case eObjectType.LeftAxe:
                case eObjectType.Axe:
                case eObjectType.HandToHand:
                    {
                        int dps = item.DPS_AF;
                        if (dps > 520)
                            maxCount += 10;
                        else
                            maxCount += 5;
                        break;
                    }
                case eObjectType.TwoHandedWeapon:
                case eObjectType.PolearmWeapon:
                case eObjectType.LargeWeapons:
                case eObjectType.CelticSpear:
                case eObjectType.Scythe:
                case eObjectType.Spear:
                    {
                        int dps = item.DPS_AF;
                        if (dps > 520)
                            maxCount += 15;
                        else
                            maxCount += 10;
                    }
                    break;
                case eObjectType.Shield:
                    switch (item.Type_Damage)
                    {
                        case 1:
                            maxCount += 5;
                            break;
                        case 2:
                            maxCount += 8;
                            break;
                        case 3:
                            maxCount += 12;
                            break;
                        default:
                            maxCount += 5;
                            break;
                    }
                    break;
                case eObjectType.Instrument:
                    switch (item.Type_Damage)
                    {
                        case 1:
                            maxCount += 5;
                            break;
                        case 2:
                            maxCount += 8;
                            break;
                        case 3:
                            maxCount += 12;
                            break;
                        default:
                            maxCount += 5;
                            break;

                    }
                    break;

                #endregion Weapons

            #region Armor

                case eObjectType.Cloth:
                case eObjectType.Leather:
                case eObjectType.Reinforced:
                case eObjectType.Studded:
                case eObjectType.Scale:
                case eObjectType.Chain:
                case eObjectType.Plate:
                    switch (item.Item_Type)
                    {
                        case Slot.HELM:
                            maxCount += 12;
                            break;
                        case Slot.TORSO:
                            maxCount += 17;
                            break;
                        case Slot.LEGS:
                            maxCount += 15;
                            break;

                        case Slot.ARMS:
                            maxCount += 10;
                            break;

                        case Slot.HANDS:
                            maxCount += 6;
                            break;
                        case Slot.FEET:
                            maxCount += 5;
                            break;
                        default:
                            maxCount += 5;
                            break;
                    }
                    break;
            }
        #endregion Armor

            #region Modifications

            if (maxCount < 1)
                maxCount = (int)(item.Price * 0.45 / rawMaterial.Price);

            int toadd = 0;

            if (item.Quality > 97 && !item.IsCrafted)
                for (int i = 97; i < item.Quality;)
                {
                    toadd += 3;
                    i++;
                }

            if (item.Price > 300000 && !item.IsCrafted)
            {
                long i = item.Price;
                i = item.Price / 100000;
                toadd += (int)i;
            }

            if (toadd > 0)
                maxCount += toadd;

            #region SpecialFix MerchantList

            if (item.Bonus8 > 0)
                if (item.Bonus8Type == 0 || item.Bonus8Type.ToString() == "")
                    maxCount = item.Bonus8;

            #endregion SpecialFix MerchantList

            if (item.Condition != item.MaxCondition && item.Condition < item.MaxCondition)
            {
                long usureoverall = (maxCount * ((item.Condition / 5) / 1000)) / 100; // assume that all items have 50000 base con
                maxCount = usureoverall;
            }

            if (maxCount < 1)
                maxCount = 1;
            else if (maxCount > 500)
                maxCount = 500;

            #endregion Modifications

            return (int)maxCount;
        }

		/// <summary>
		/// Return the material yield for this salvage.
		/// </summary>
		public static int GetMaterialYield(GamePlayer player, InventoryItem item, SalvageYield salvageYield, ItemTemplate rawMaterial)
		{
            int maxCount = 0;

			if (rawMaterial == null)
				return 0;

			if (ServerProperties.Properties.USE_NEW_SALVAGE)
			{
				maxCount = GetCountForSalvage(item, salvageYield, rawMaterial);
			}
			else
			{
				maxCount = (int)(item.Price * 0.45 / rawMaterial.Price); // crafted item return max 45% of the item value in material

				if (item.IsCrafted)
				{
					maxCount = (int)Math.Ceiling((double)maxCount / 2);
				}
			}

			int playerPercent = player.GetCraftingSkillValue(CraftingMgr.GetSecondaryCraftingSkillToWorkOnItem(item)) * 100 / CraftingMgr.GetItemCraftLevel(item);

			if (playerPercent > 100)
			{
				playerPercent = 100;
			}
			else if (playerPercent < 75)
			{
				playerPercent = 75;
			}

			int minCount = (int)(((maxCount - 1) / 25f) * playerPercent) - ((3 * maxCount) - 4); //75% => min = 1; 100% => min = maxCount;

			salvageYield.Count = Util.Random(minCount, maxCount);
			return salvageYield.Count;
		}

		#endregion
	}
}
