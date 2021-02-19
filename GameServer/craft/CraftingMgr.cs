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
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Enum of all crafting skill (related to client file)
	/// </summary>
	public enum eCraftingSkill : int
	{
		NoCrafting = 0,
		WeaponCrafting = 1,
		ArmorCrafting = 2,
		SiegeCrafting = 3,
		Alchemy = 4,
		MetalWorking = 6,
		LeatherCrafting = 7,
		ClothWorking = 8,
		GemCutting = 9,
		HerbalCrafting = 10,
		Tailoring = 11,
		Fletching = 12,
		SpellCrafting = 13,
		WoodWorking = 14,
		BasicCrafting = 15,
		_Last = 15,
	}

	/// <summary>
	/// Description résumée de CraftingMgr.
	/// </summary>
	public class CraftingMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Hold all crafting skill
		/// </summary>
		protected static AbstractCraftingSkill[] m_craftingskills = new AbstractCraftingSkill[(int)eCraftingSkill._Last];

		#region SellBack Price Control

		public static void UpdateSellBackPrice(DBCraftedItem recipe, ItemTemplate itemToCraft, IList<Tuple<ItemTemplate, int>> rawListandCounts)
		{

			bool updatePrice = true;

			if (itemToCraft.Name.EndsWith("metal bars") ||
				itemToCraft.Name.EndsWith("leather square") ||
				itemToCraft.Name.EndsWith("cloth square") ||
				itemToCraft.Name.EndsWith("wooden boards"))
				updatePrice = false;

			if (itemToCraft.PackageID.Contains("NoPriceUpdate"))
				updatePrice = false;

			long totalprice = 0;
			if (updatePrice)
			{
				long pricetoset = 0;
				if (recipe.CraftingSkillType == 6 || recipe.CraftingSkillType == 7 || recipe.CraftingSkillType == 8 ||
					recipe.CraftingSkillType == 14)
					pricetoset = Math.Abs((long)(totalprice * 2 * ServerProperties.Properties.CRAFTING_SECONDARYCRAFT_SELLBACK_PERCENT) / 100);
				else
					pricetoset = Math.Abs(totalprice * 2 * ServerProperties.Properties.CRAFTING_SELLBACK_PERCENT / 100);

				if (pricetoset > 0 && itemToCraft.Price != pricetoset)
				{
					long currentPrice = itemToCraft.Price;
					itemToCraft.Price = pricetoset;
					itemToCraft.AllowUpdate = true;
					itemToCraft.Dirty = true;
					if (GameServer.Database.SaveObject(itemToCraft))
						log.Error("Craft Price Correction: " + itemToCraft.Id_nb + " rawmaterials price= " + totalprice + " Actual Price= " + currentPrice + ". Corrected price to= " + pricetoset);
					else
						log.Error("Craft Price Correction Not SAVED: " + itemToCraft.Id_nb + " rawmaterials price= " + totalprice + " Actual Price= " + currentPrice + ". Corrected price to= " + pricetoset);
					GameServer.Database.UpdateInCache<ItemTemplate>(itemToCraft.Id_nb);
					itemToCraft.Dirty = false;
					itemToCraft.AllowUpdate = false;
				}
			}
		}

		#endregion SellBack Price Control

		public static IList<Tuple<ItemTemplate, int>> GetRawMaterialsItemAndNecessaryCount(GamePlayer player, DBCraftedItem recipe , IList<DBCraftedXItem> rawmaterials)
		{
			return ConstructRawMaterialsItemListAndCount(player, recipe, rawmaterials);
		}
		private static IList<Tuple<ItemTemplate, int>> ConstructRawMaterialsItemListAndCount(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawmaterials)
		{
			IList<Tuple<ItemTemplate, int>> rawMatsAndCountTuple = new List<Tuple<ItemTemplate, int>>();
			foreach (DBCraftedXItem material in rawmaterials)
			{
				ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);
				if (template != null)
				{
					rawMatsAndCountTuple.Add(new Tuple<ItemTemplate, int>(template, material.Count));
				}
				else
				player.Out.SendMessage("Can't find a material (" + material.IngredientId_nb + ") needed for this recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				log.Error("Cannot find raw material ItemTemplate: " + material.IngredientId_nb + " needed for recipe: " + recipe.CraftedItemID);
			}
			return rawMatsAndCountTuple;
		}
		/// <summary>
		/// get a crafting skill by the enum index
		/// </summary>
		/// <param name="skill"></param>
		/// <returns></returns>
		public static AbstractCraftingSkill getSkillbyEnum(eCraftingSkill skill)
		{
			if (skill == eCraftingSkill.NoCrafting) return null;
			return m_craftingskills[(int)skill - 1] as AbstractCraftingSkill;
		}

		/// <summary>
		/// Initialize the crafting system
		/// </summary>
		/// <returns></returns>
		public static bool Init()
		{
			// skill
			m_craftingskills[(int)eCraftingSkill.ArmorCrafting - 1] = new ArmorCrafting();
			m_craftingskills[(int)eCraftingSkill.Fletching - 1] = new Fletching();
			m_craftingskills[(int)eCraftingSkill.SiegeCrafting - 1] = new SiegeCrafting();
			m_craftingskills[(int)eCraftingSkill.Tailoring - 1] = new Tailoring();
			m_craftingskills[(int)eCraftingSkill.WeaponCrafting - 1] = new WeaponCrafting();

			m_craftingskills[(int)eCraftingSkill.ClothWorking - 1] = new ClothWorking();
			m_craftingskills[(int)eCraftingSkill.GemCutting - 1] = new GemCutting();
			m_craftingskills[(int)eCraftingSkill.HerbalCrafting - 1] = new HerbalCrafting();
			m_craftingskills[(int)eCraftingSkill.LeatherCrafting - 1] = new LeatherCrafting();
			m_craftingskills[(int)eCraftingSkill.MetalWorking - 1] = new MetalWorking();
			m_craftingskills[(int)eCraftingSkill.WoodWorking - 1] = new WoodWorking();
			m_craftingskills[(int)eCraftingSkill.BasicCrafting - 1] = new BasicCrafting();

			//Advanced skill
			m_craftingskills[(int)eCraftingSkill.Alchemy - 1] = new Alchemy();
			m_craftingskills[(int)eCraftingSkill.SpellCrafting - 1] = new SpellCrafting();

			return true;
		}

		#region Global craft functions

		/// <summary>
		/// Return the crafting skill which created the item
		/// </summary>
		public static eCraftingSkill GetCraftingSkill(InventoryItem item)
		{
			if (!item.IsCrafted)
				return eCraftingSkill.NoCrafting;

			switch (item.Object_Type)
			{
				case (int)eObjectType.Cloth:
				case (int)eObjectType.Leather:
					return eCraftingSkill.Tailoring;

				case (int)eObjectType.Studded:
				case (int)eObjectType.Reinforced:
				case (int)eObjectType.Chain:
				case (int)eObjectType.Scale:
				case (int)eObjectType.Plate:
					return eCraftingSkill.ArmorCrafting;

				// all weapon
				case (int)eObjectType.Axe:
				case (int)eObjectType.Blades:
				case (int)eObjectType.Blunt:
				case (int)eObjectType.CelticSpear:
				case (int)eObjectType.CrushingWeapon:
				case (int)eObjectType.Flexible:
				case (int)eObjectType.Hammer:
				case (int)eObjectType.HandToHand:
				case (int)eObjectType.LargeWeapons:
				case (int)eObjectType.LeftAxe:
				case (int)eObjectType.Piercing:
				case (int)eObjectType.PolearmWeapon:
				case (int)eObjectType.Scythe:
				case (int)eObjectType.Shield:
				case (int)eObjectType.SlashingWeapon:
				case (int)eObjectType.Spear:
				case (int)eObjectType.Sword:
				case (int)eObjectType.ThrustWeapon:
				case (int)eObjectType.TwoHandedWeapon:
					return eCraftingSkill.WeaponCrafting;

				case (int)eObjectType.CompositeBow:
				case (int)eObjectType.Crossbow:
				case (int)eObjectType.Fired:
				case (int)eObjectType.Instrument:
				case (int)eObjectType.Longbow:
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.Staff:
					return eCraftingSkill.Fletching;

				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.Poison:
					return eCraftingSkill.Alchemy;

				case (int)eObjectType.SpellcraftGem:
					return eCraftingSkill.SpellCrafting;

				case (int)eObjectType.SiegeBalista:
				case (int)eObjectType.SiegeCatapult:
				case (int)eObjectType.SiegeCauldron:
				case (int)eObjectType.SiegeRam:
				case (int)eObjectType.SiegeTrebuchet:
					return eCraftingSkill.SiegeCrafting;

				default:
					return eCraftingSkill.NoCrafting;
			}
		}

		/// <summary>
		/// Return the crafting skill needed to work on the item
		/// </summary>
		public static eCraftingSkill GetSecondaryCraftingSkillToWorkOnItem(InventoryItem item)
		{
			switch (item.Object_Type)
			{
				case (int)eObjectType.Cloth:
					return eCraftingSkill.ClothWorking;

				case (int)eObjectType.Leather:
				case (int)eObjectType.Studded:
					return eCraftingSkill.LeatherCrafting;

				// all weapon
				case (int)eObjectType.Axe:
				case (int)eObjectType.Blades:
				case (int)eObjectType.Blunt:
				case (int)eObjectType.CelticSpear:
				case (int)eObjectType.CrushingWeapon:
				case (int)eObjectType.Flexible:
				case (int)eObjectType.Hammer:
				case (int)eObjectType.HandToHand:
				case (int)eObjectType.LargeWeapons:
				case (int)eObjectType.LeftAxe:
				case (int)eObjectType.Piercing:
				case (int)eObjectType.PolearmWeapon:
				case (int)eObjectType.Scythe:
				case (int)eObjectType.Shield:
				case (int)eObjectType.SlashingWeapon:
				case (int)eObjectType.Spear:
				case (int)eObjectType.Sword:
				case (int)eObjectType.ThrustWeapon:
				case (int)eObjectType.TwoHandedWeapon:
				// all other armor
				case (int)eObjectType.Chain:
				case (int)eObjectType.Plate:
				case (int)eObjectType.Reinforced:
				case (int)eObjectType.Scale:
					return eCraftingSkill.MetalWorking;

				case (int)eObjectType.CompositeBow:
				case (int)eObjectType.Crossbow:
				case (int)eObjectType.Fired:
				case (int)eObjectType.Instrument:
				case (int)eObjectType.Longbow:
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.Staff:
					return eCraftingSkill.WoodWorking;

				default:
					return eCraftingSkill.NoCrafting;
			}
		}

		/// <summary>
		/// Return the approximative craft level of the item
		/// </summary>
		public static int GetItemCraftLevel(InventoryItem item)
		{
			switch (item.Object_Type)
			{
				case (int)eObjectType.Cloth:
				case (int)eObjectType.Leather:
				case (int)eObjectType.Studded:
				case (int)eObjectType.Chain:
				case (int)eObjectType.Plate:
				case (int)eObjectType.Reinforced:
				case (int)eObjectType.Scale:
					{
						int baseLevel = 15 + item.Level * 20; // gloves
						switch (item.Item_Type)
						{
							case (int)eInventorySlot.HeadArmor: // head
								return baseLevel + 15;

							case (int)eInventorySlot.FeetArmor: // feet
								return baseLevel + 30;

							case (int)eInventorySlot.LegsArmor: // legs
								return baseLevel + 50;

							case (int)eInventorySlot.ArmsArmor: // arms
								return baseLevel + 65;

							case (int)eInventorySlot.TorsoArmor: // torso
								return baseLevel + 80;

							default:
								return baseLevel;
						}
					}

				case (int)eObjectType.Axe:
				case (int)eObjectType.Blades:
				case (int)eObjectType.Blunt:
				case (int)eObjectType.CelticSpear:
				case (int)eObjectType.CrushingWeapon:
				case (int)eObjectType.Flexible:
				case (int)eObjectType.Hammer:
				case (int)eObjectType.HandToHand:
				case (int)eObjectType.LargeWeapons:
				case (int)eObjectType.LeftAxe:
				case (int)eObjectType.Piercing:
				case (int)eObjectType.PolearmWeapon:
				case (int)eObjectType.Scythe:
				case (int)eObjectType.Shield:
				case (int)eObjectType.SlashingWeapon:
				case (int)eObjectType.Spear:
				case (int)eObjectType.Sword:
				case (int)eObjectType.ThrustWeapon:
				case (int)eObjectType.TwoHandedWeapon:

				case (int)eObjectType.CompositeBow:
				case (int)eObjectType.Crossbow:
				case (int)eObjectType.Fired:
				case (int)eObjectType.Instrument:
				case (int)eObjectType.Longbow:
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.Staff:
					return 15 + (item.Level - 1) * 20;

				default:
					return 0;
			}
		}

		#endregion

	}
}
