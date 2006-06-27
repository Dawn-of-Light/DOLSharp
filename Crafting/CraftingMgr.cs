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
		Jewellery = 5,
		MetalWorking = 6,
		LeatherCrafting = 7,
		ClothWorking = 8,
		GemCutting = 9,
		HerbalCrafting = 10,
		Tailoring = 11,
		Fletching = 12,
		SpellCrafting = 13,
		WoodWorking = 14,

		_Last = 14,
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

		/// <summary>
		/// get a crafting skill by the enum index
		/// </summary>
		/// <param name="skill"></param>
		/// <returns></returns>
		public static AbstractCraftingSkill getSkillbyEnum(eCraftingSkill skill)
		{
			int index = (int)skill - 1;
			if (index < 0 || index >= m_craftingskills.Length)
				return null;
			return m_craftingskills[index] as AbstractCraftingSkill;
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
			m_craftingskills[(int)eCraftingSkill.Jewellery - 1] = new Jewellery();
			m_craftingskills[(int)eCraftingSkill.SiegeCrafting - 1] = new SiegeCrafting();
			m_craftingskills[(int)eCraftingSkill.Tailoring - 1] = new Tailoring();
			m_craftingskills[(int)eCraftingSkill.WeaponCrafting - 1] = new WeaponCrafting();

			m_craftingskills[(int)eCraftingSkill.ClothWorking - 1] = new ClothWorking();
			m_craftingskills[(int)eCraftingSkill.GemCutting - 1] = new GemCutting();
			m_craftingskills[(int)eCraftingSkill.HerbalCrafting - 1] = new HerbalCrafting();
			m_craftingskills[(int)eCraftingSkill.LeatherCrafting - 1] = new LeatherCrafting();
			m_craftingskills[(int)eCraftingSkill.MetalWorking - 1] = new MetalWorking();
			m_craftingskills[(int)eCraftingSkill.WoodWorking - 1] = new WoodWorking();

			//Advanced skill
			m_craftingskills[(int)eCraftingSkill.Alchemy - 1] = new Alchemy();
			m_craftingskills[(int)eCraftingSkill.SpellCrafting - 1] = new SpellCrafting();

			return true;
		}

		#region Global craft functions

		/// <summary>
		/// Return the crafting skill needed to work on the item
		/// </summary>
		public static eCraftingSkill GetSecondaryCraftingSkillToWorkOnItem(InventoryItem item)
		{
			switch(item.Object_Type)
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

				default : 
					return eCraftingSkill.NoCrafting;
			}	
		}

		/// <summary>
		/// Return the approximative craft level of the item
		/// </summary>
		public static int GetItemCraftLevel(InventoryItem item)
		{
			switch(item.Object_Type)
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
					switch(item.Item_Type)
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

				default :
					return 0;
			}
		}

		#endregion

	}
}
