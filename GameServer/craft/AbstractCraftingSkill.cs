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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.Language;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// AbstractCraftingSkill is the base class for all crafting skill
	/// </summary>
	public abstract class AbstractCraftingSkill
	{
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaration
		/// <summary>
		/// the maximum possible range within a player has to be to a forge , lathe ect to craft an item
		/// </summary>
		public const int CRAFT_DISTANCE = 512;

		/// <summary>
		/// The icone number used by this craft.
		/// </summary>
		private byte m_icon;

		/// <summary>
		/// The name show for this craft.
		/// </summary>
		private string m_name;

		/// <summary>
		/// The crafting skill id of this craft
		/// </summary>
		private eCraftingSkill m_eskill;

		/// <summary>
		/// The player currently crafting
		/// </summary>
		public const string PLAYER_CRAFTER = "PLAYER_CRAFTER";

		/// <summary>
		/// The recipe being crafted
		/// </summary>
		public const string RECIPE_BEING_CRAFTED = "RECIPE_BEING_CRAFTED";

		/// <summary>
		/// The list of raw materials for the recipe beign crafted
		/// </summary>
		public const string RECIPE_RAW_MATERIAL_LIST = "RECIPE_RAW_MATERIAL_LIST";

		public const int subSkillCap = 1300;

		public virtual string CRAFTER_TITLE_PREFIX
		{
			get
			{
				return "";
			}
		}
		public eCraftingSkill eSkill
		{
			get
			{
				return m_eskill;
			}
			set
			{
				m_eskill = value;
			}
		}
		public byte Icon
		{
			get
			{
				return m_icon;
			}
			set
			{
				m_icon = value;
			}
		}
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}
		#endregion

		#region First call function and callback

		/// <summary>
		/// Called when player tries to begin crafting an item
		/// </summary>
		public virtual void CraftItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
		{
			if (!CanPlayerStartToCraftItem(player, recipe, itemToCraft, rawMaterials))
			{
				return;
			}

			if (player.IsCrafting)
			{
				StopCraftingCurrentItem(player, itemToCraft);
				return;
			}

			int craftingTime = GetCraftingTime(player, recipe, rawMaterials);

			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.BeginWork", itemToCraft.Name, CalculateChanceToMakeItem(player, recipe).ToString()), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
			player.Out.SendTimerWindow(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.CurrentlyMaking", itemToCraft.Name), craftingTime);

			player.Stealth(false);
			
			StartCraftingTimerAndSetCallBackMethod(player, recipe, rawMaterials, craftingTime);
		}


		protected virtual void StartCraftingTimerAndSetCallBackMethod(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials, int craftingTime)
		{
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(MakeItem);
			player.CraftTimer.Properties.setProperty(PLAYER_CRAFTER, player);
			player.CraftTimer.Properties.setProperty(RECIPE_BEING_CRAFTED, recipe);
			player.CraftTimer.Properties.setProperty(RECIPE_RAW_MATERIAL_LIST, rawMaterials);
			player.CraftTimer.Start(craftingTime * 1000);
		}

		protected virtual void StopCraftingCurrentItem(GamePlayer player, ItemTemplate itemToCraft)
		{
			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.StopWork", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		protected virtual bool CanPlayerStartToCraftItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
		{
			if (!GameServer.ServerRules.IsAllowedToCraft(player, itemToCraft))
			{
				return false;
			}

			if (!CheckForTools(player, recipe, itemToCraft, rawMaterials))
			{
				return false;
			}

			if (!CheckSecondCraftingSkillRequirement(player, recipe, itemToCraft, rawMaterials))
			{
				return false;
			}

			if (!CheckRawMaterials(player, recipe, itemToCraft, rawMaterials))
			{
				return false;
			}

			if (player.IsMoving || player.IsStrafing)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.MoveAndInterrupt"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.InCombat)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.CantCraftInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Make the item when craft time is finished
		/// </summary>
		protected virtual int MakeItem(RegionTimer timer)
		{
			GamePlayer player = timer.Properties.getProperty<GamePlayer>(PLAYER_CRAFTER, null);
			DBCraftedItem recipe = timer.Properties.getProperty<DBCraftedItem>(RECIPE_BEING_CRAFTED, null);
			IList<DBCraftedXItem> rawMaterials = timer.Properties.getProperty<IList<DBCraftedXItem>>(RECIPE_RAW_MATERIAL_LIST, null);

			if (player == null || recipe == null || rawMaterials == null)
			{
				if (player != null) player.Out.SendMessage("Could not find recipe or item to craft!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				log.Error("Crafting.MakeItem: Could not retrieve player, recipe, or raw materials to craft from CraftTimer.");
				return 0;
			}

			ItemTemplate itemToCraft = GameServer.Database.FindObjectByKey<ItemTemplate>(recipe.Id_nb);
			if (itemToCraft == null)
			{
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();

			if (Util.Chance(CalculateChanceToMakeItem(player, recipe)))
			{
				if (!RemoveUsedMaterials(player, recipe, rawMaterials))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.MakeItem.NotAllMaterials"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					if (player.Client.Account.PrivLevel == 1)
						return 0;
				}

				BuildCraftedItem(player, recipe, itemToCraft);
				GainCraftingSkillPoints(player, recipe, rawMaterials);
			}
			else
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.MakeItem.LoseNoMaterials", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendPlaySound(eSoundType.Craft, 0x02);
			}
			return 0;
		}

		#endregion

		#region Requirement check

		/// <summary>
		/// Check if the player is near the needed tools (forge, lathe, etc)
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="recipe">the recipe being used</param>
		/// <param name="itemToCraft">the item to make</param>
		/// <param name="rawMaterials">a list of raw materials needed to create this item</param>
		/// <returns>true if required tools are found</returns>
		protected virtual bool CheckForTools(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
		{
			return true;
		}

		/// <summary>
		/// Check if the player has enough secondary crafting skill to build the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="recipe"></param>
		/// <returns></returns>
		public virtual bool CheckSecondCraftingSkillRequirement(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
		{
			int minimumLevel = GetSecondaryCraftingSkillMinimumLevel(recipe, itemToCraft);

			if (minimumLevel <= 0)
				return true; // no requirement needed

			foreach (DBCraftedXItem material in rawMaterials)
			{
				ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

				if (template == null)
				{
					player.Out.SendMessage("Can't find a material (" + material.IngredientId_nb + ") needed for recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					log.Error("Cannot find raw material ItemTemplate: " + material.IngredientId_nb + ") needed for recipe: " + recipe.CraftedItemID);
					return false;
				}

				switch (template.Model)
				{
					case 522:	//"cloth square"
					case 537:	//"heavy thread"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < minimumLevel)
							{
								player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoClothworkingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;
							}
							break;
						}

					case 521:	//"leather square"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.LeatherCrafting) < minimumLevel)
							{
								player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoLeathercraftingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;
							}
							break;
						}

					case 519:	//"metal bars"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < minimumLevel)
							{
								player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoMetalworkingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;
							}
							break;
						}

					case 520:	//"wooden boards"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < minimumLevel)
							{
								player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoWoodworkingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;
							}
							break;
						}
				}
			}
			return true;
		}

		/// <summary>
		/// Verify that player has the needed materials to craft an item
		/// </summary>
		public virtual bool CheckRawMaterials(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
		{
			ArrayList missingMaterials = null;

			lock (player.Inventory)
			{
				foreach (DBCraftedXItem material in rawMaterials)
				{
					ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);
					
					if (template == null)
					{
						player.Out.SendMessage("Can't find a material (" + material.IngredientId_nb + ") needed for this recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						log.Error("Cannot find raw material ItemTemplate: " + material.IngredientId_nb + ") needed for recipe: " + recipe.CraftedItemID);
						return false;
					}

					bool result = false;
					int count = material.Count;

					foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						if (item != null && item.Name == template.Name)
						{
							if (item.Count >= count)
							{
								result = true;
								break;
							}
							else
							{
								count -= item.Count;
							}
						}
					}

					if (result == false)
					{
						if (missingMaterials == null)
						{
							missingMaterials = new ArrayList(5);
						}

						missingMaterials.Add("(" + count + ") " + template.Name);
					}
				}

				if (missingMaterials != null)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckRawMaterial.NoIngredients", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckRawMaterial.YouAreMissing", itemToCraft.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					foreach (string materialName in missingMaterials)
					{
						player.Out.SendMessage(materialName, eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					}

					if (player.Client.Account.PrivLevel == (uint)ePrivLevel.Player) return false;
				}

				return true;
			}
		}

		#endregion

		#region Gain points

		/// <summary>
		/// Gain a point in the appropriate skills for a recipe and materials
		/// </summary>
		public virtual void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			foreach (DBCraftedXItem material in rawMaterials)
			{
				ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

				if (template != null)
				{
					switch (template.Model)
					{
						case 522:	//"cloth square"
						case 537:	//"heavy thread"
							{
								if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < subSkillCap)
								{
									player.GainCraftingSkill(eCraftingSkill.ClothWorking, 1);
								}
								break;
							}

						case 521:	//"leather square"
							{
								if (player.GetCraftingSkillValue(eCraftingSkill.LeatherCrafting) < subSkillCap)
								{
									player.GainCraftingSkill(eCraftingSkill.LeatherCrafting, 1);
								}
								break;
							}

						case 519:	//"metal bars"
							{
								if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < subSkillCap)
								{
									player.GainCraftingSkill(eCraftingSkill.MetalWorking, 1);
								}
								break;
							}

						case 520:	//"wooden boards"
							{
								if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < subSkillCap)
								{
									player.GainCraftingSkill(eCraftingSkill.WoodWorking, 1);
								}
								break;
							}
					}
				}
			}

			player.Out.SendUpdateCraftingSkills();
		}
		#endregion

		#region Use materials and created crafted item

		/// <summary>
		/// Remove used raw material from player inventory
		/// </summary>
		/// <param name="player"></param>
		/// <param name="recipe"></param>
		/// <returns></returns>
		public virtual bool RemoveUsedMaterials(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			Dictionary<int, int?> dataSlots = new Dictionary<int, int?>(10);

			lock (player.Inventory)
			{
				foreach (DBCraftedXItem material in rawMaterials)
				{
					ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

					if (template == null)
					{
						player.Out.SendMessage("Can't find a material (" + material.IngredientId_nb + ") needed for this recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						log.Error("RemoveUsedMaterials: Cannot find raw material ItemTemplate: " + material.IngredientId_nb + " needed for recipe: " + recipe.CraftedItemID);
						return false;
					}

					bool result = false;
					int count = material.Count;

					foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						if (item != null && item.Name == template.Name)
						{
							if (item.Count >= count)
							{
								if (item.Count == count)
								{
									dataSlots.Add(item.SlotPosition, null);
								}
								else
								{
									dataSlots.Add(item.SlotPosition, count);
								}
								result = true;
								break;
							}
							else
							{
								dataSlots.Add(item.SlotPosition, null);
								count -= item.Count;
							}
						}
					}
					if (result == false)
					{
						return false;
					}
				}
			}

			player.Inventory.BeginChanges();
			Dictionary<int, int?>.Enumerator enumerator = dataSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, int?> de = enumerator.Current;
				InventoryItem item = player.Inventory.GetItem((eInventorySlot)de.Key);
				if (item != null)
				{
					if (!de.Value.HasValue)
					{
						player.Inventory.RemoveItem(item);
					}
					else
					{
						player.Inventory.RemoveCountFromStack(item, de.Value.Value);
					}
					InventoryLogging.LogInventoryAction(player, "(craft)", eInventoryActionType.Craft, item.Template, de.Value.HasValue ? de.Value.Value : item.Count);
				}
			}
			player.Inventory.CommitChanges();

			return true;//all raw material removed and item created
		}

		/// <summary>
		/// Make the crafted item and add it to player's inventory
		/// </summary>
		/// <param name="player"></param>
		/// <param name="recipe"></param>
		/// <returns></returns>
		protected virtual void BuildCraftedItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft)
		{
			Dictionary<int, int> changedSlots = new Dictionary<int, int>(5); // key : > 0 inventory ; < 0 ground || value: < 0 = new item count; > 0 = add to old

			lock (player.Inventory)
			{
				int count = itemToCraft.PackSize < 1 ? 1 : itemToCraft.PackSize;
				foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					if (item == null)
						continue;

					if (item.Id_nb.Equals(itemToCraft.Id_nb) == false)
						continue;

					if (item.Count >= itemToCraft.MaxCount)
						continue;

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

				if (count > 0) // Add new object
				{
					eInventorySlot firstEmptySlot = player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
					if (firstEmptySlot == eInventorySlot.Invalid)
					{
						changedSlots.Add(-1, -count); // Create the item on the ground
					}
					else
					{
						changedSlots.Add((int)firstEmptySlot, -count); // Create the item in the free slot
					}
					count = 0;
				}

				InventoryItem newItem = null;

				player.Inventory.BeginChanges();

				Dictionary<int, int>.Enumerator enumerator = changedSlots.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, int> slot = enumerator.Current;
					int countToAdd = slot.Value;
					if (countToAdd > 0)	// Add to exiting item
					{
						newItem = player.Inventory.GetItem((eInventorySlot)slot.Key);
						if (newItem != null && player.Inventory.AddCountToStack(newItem, countToAdd))
						{
							InventoryLogging.LogInventoryAction("(craft)", player, eInventoryActionType.Other, newItem.Template, countToAdd);
							// count incremented, continue with next change
							continue;
						}
					}

					if (recipe.MakeTemplated)
					{
						string adjItem = itemToCraft.Id_nb+(GetQuality(player, recipe).ToString());
						ItemTemplate adjItemToCraft = GameServer.Database.FindObjectByKey<ItemTemplate>(adjItem);
						if (adjItemToCraft != null)
						{
							newItem = GameInventoryItem.Create(adjItemToCraft);
						}
						else
						{
							newItem = GameInventoryItem.Create(itemToCraft);
						}
					}
					else
					{
						ItemUnique unique = new ItemUnique(itemToCraft);
						GameServer.Database.AddObject(unique);
						newItem = GameInventoryItem.Create(unique);
						newItem.Quality = GetQuality(player, recipe);
					}

					newItem.IsCrafted = true;
					newItem.Creator = player.Name;
					newItem.Count = -countToAdd;

					if (slot.Key > 0)	// Create new item in the backpack
					{
						player.Inventory.AddItem((eInventorySlot)slot.Key, newItem);
						InventoryLogging.LogInventoryAction("(craft)", player, eInventoryActionType.Craft, newItem.Template, newItem.Count);
					}
					else					// Create new item on the ground
					{
						player.CreateItemOnTheGround(newItem);
						player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.BuildCraftedItem.BackpackFull", itemToCraft.Name));
					}
				}

				player.Inventory.CommitChanges();

				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.BuildCraftedItem.Successfully", itemToCraft.Name, newItem.Quality), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

				if (recipe.MakeTemplated == false && newItem.Quality == 100)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.BuildCraftedItem.Masterpiece"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					player.Out.SendPlaySound(eSoundType.Craft, 0x04);
				}
				else
				{
					player.Out.SendPlaySound(eSoundType.Craft, 0x03);
				}
			}
		}

		#endregion

		#region Calcul functions

		/// <summary>
		/// Calculate chance to succes
		/// </summary>
		public virtual int CalculateChanceToMakeItem(GamePlayer player, DBCraftedItem recipe)
		{
			int con = GetItemCon(player.GetCraftingSkillValue(m_eskill), recipe.CraftingLevel);
			if (con < -3)
				con = -3;
			if (con > 3)
				con = 3;

			switch (con)
			{
					// Chance to MAKE ! (100 - chance to fail)
				case -3:
					return 100;
				case -2:
					return 100;
				case -1:
					return 100;
				case 0:
					return 100 - 8;
				case 1:
					return 100 - 16;
				case 2:
					return 100 - 32;
				case 3:
					return 0;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Calculate chance to gain point
		/// </summary>
		public virtual int CalculateChanceToGainPoint(GamePlayer player, DBCraftedItem recipe)
		{
			int con = GetItemCon(player.GetCraftingSkillValue(m_eskill), recipe.CraftingLevel);
			if (con < -3)
				con = -3;
			if (con > 3)
				con = 3;
			int chance = 0;

			switch (con)
			{
				case -3:
					return 0;
				case -2:
					chance = 15;
					break;
				case -1:
					chance = 30;
					break;
				case 0:
					chance = 45;
					break;
				case 1:
					chance = 55;
					break;
				case 2:
					chance = 45;
					break;
				case 3:
					return 0;
				default:
					return 0;
			}

			// In capital cities bonuses to crafting apply (patch 1.86)
			if (player.CurrentRegion.IsCapitalCity)
			{
				chance += player.CraftingSkillBonus;

				if (chance < 0)
					chance = 0;
				if (chance > 100)
					chance = 100;
			}

			return chance;
		}

		/// <summary>
		/// Calculate crafting time
		/// </summary>
		public virtual int GetCraftingTime(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			double baseMultiplier = (recipe.CraftingLevel / 100) + 1;

			ushort materialsCount = 0;
			foreach (DBCraftedXItem material in rawMaterials)
			{
				materialsCount += (ushort)material.Count;
			}

			int craftingTime = (int)(baseMultiplier * materialsCount / 4);

			// Player does check for capital city bonus as well
			craftingTime = (int)(craftingTime / player.CraftingSpeed);

			//keep bonuses reduction in crafting time
			if (Keeps.KeepBonusMgr.RealmHasBonus(DOL.GS.Keeps.eKeepBonusType.Craft_Timers_5, (eRealm)player.Realm))
				craftingTime = (int)(craftingTime / 1.05);
			else if (Keeps.KeepBonusMgr.RealmHasBonus(DOL.GS.Keeps.eKeepBonusType.Craft_Timers_3, (eRealm)player.Realm))
				craftingTime = (int)(craftingTime / 1.03);

			int con = GetItemCon(player.GetCraftingSkillValue(m_eskill), recipe.CraftingLevel);
			double mod = 1.0;
			switch (con)
			{
				case -3:
					mod = 0.4;
					break;
				case -2:
					mod = 0.6;
					break;
				case -1:
					mod = 0.8;
					break;
				case 0:
					mod = 1.0;
					break;
				case 1:
					mod = 1.0;
					break;
				case 2:
					mod = 1.0;
					break;
				case 3:
					mod = 1.0;
					break;
			}

			craftingTime = (int)(craftingTime * mod);

			if (craftingTime < 1)
				craftingTime = 1;
			return craftingTime;
		}

		/// <summary>
		/// Calculate the minumum needed secondary crafting skill level to make the item
		/// </summary>
		public virtual int GetSecondaryCraftingSkillMinimumLevel(DBCraftedItem recipe, ItemTemplate itemToCraft)
		{
			return 0;
		}

		/// <summary>
		/// Calculate crafted item quality
		/// </summary>
		private int GetQuality(GamePlayer player, DBCraftedItem item)
		{
			// 2% chance to get masterpiece, 1:6 chance to get 94-99%, if legendary or if grey con
			// otherwise moving the most load towards 94%, the higher the item con to the crafter skill
			//1.87 patch raises min quality to 96%

			// legendary
			if (player.GetCraftingSkillValue(m_eskill) >= 1000)
			{
				if (Util.Chance(2))
				{
					return 100;	// 2% chance for master piece
				}
				return 96 + Util.Random(3);
			}

			int delta = GetItemCon(player.GetCraftingSkillValue(m_eskill), item.CraftingLevel);
			if (delta < -2)
			{
				if (Util.Chance(2))
					return 100; // grey items get 2% chance to be master piece
				return 96 + Util.Random(3); // handle grey items like legendary
			}

			// this is a type of roulette selection, imagine a roulette wheel where all chances get different sized
			// fields where the ball can drop, the bigger the field, the bigger the chance
			// field size is modulated by item con and can be also 0

			// possible chance allocation scheme for yellow item
			// 99:
			// 98:
			// 97: o
			// 96: oo
			// where one 'o' marks 100 size, this example results in 10% chance for yellow items to be 97% quality

			delta = delta * 100;

			int[] chancePart = new int[4]; // index ranges from 96%(0) to 99%(5)
			int sum = 0;
			for (int i = 0; i < 4; i++)
			{
				chancePart[i] = Math.Max((4 - i) * 100 - delta, 0);	// 0 minimum
				sum += chancePart[i];
			}

			// selection
			int rand = Util.Random(sum);
			for (int i = 3; i >= 0; i--)
			{
				if (rand < chancePart[i])
					return 96 + i;
				rand -= chancePart[i];
			}

			// if something still not clear contact Thrydon/Blue

			return 96;
		}


		/// <summary>
		/// get item con color compared to crafters skill
		/// </summary>
		/// <param name="crafterSkill"></param>
		/// <param name="itemCraftingLevel"></param>
		/// <returns>-3 grey, -2 green, -1 blue, 0 yellow, 1 orange, 2 red, 3 purple</returns>
		public int GetItemCon(int crafterSkill, int itemCraftingLevel)
		{
			int diff = itemCraftingLevel - crafterSkill;
			if (diff <= -50)
				return -3; // grey
			else if (diff <= -31)
				return -2; // green
			else if (diff <= -11)
				return -1; // blue
			else if (diff <= 0)
				return 0; // yellow
			else if (diff <= 19)
				return 1; // orange
			else if (diff <= 49)
				return 2; // red
			else
				return 3; // impossible
		}
		#endregion
	}
}
