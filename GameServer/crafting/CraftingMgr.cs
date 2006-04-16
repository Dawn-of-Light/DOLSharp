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
using DOL.GS.Database;
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
			if(skill == eCraftingSkill.NoCrafting) return null;
			return m_craftingskills[(int)skill - 1] as AbstractCraftingSkill;
		}

		/// <summary>
		/// Initialize the crafting system
		/// </summary>
		/// <returns></returns>
		public static bool Init()
		{
			// primary skills
			m_craftingskills[(int)eCraftingSkill.ArmorCrafting - 1] = new ArmorCrafting();
			m_craftingskills[(int)eCraftingSkill.Fletching - 1] = new Fletching();
			m_craftingskills[(int)eCraftingSkill.Tailoring - 1] = new Tailoring();
			m_craftingskills[(int)eCraftingSkill.WeaponCrafting - 1] = new WeaponCrafting();
			// secondary skills
			m_craftingskills[(int)eCraftingSkill.Jewellery - 1] = new Jewellery();
			m_craftingskills[(int)eCraftingSkill.SiegeCrafting - 1] = new SiegeCrafting();
			m_craftingskills[(int)eCraftingSkill.ClothWorking - 1] = new ClothWorking();
			m_craftingskills[(int)eCraftingSkill.GemCutting - 1] = new GemCutting();
			m_craftingskills[(int)eCraftingSkill.HerbalCrafting - 1] = new HerbalCrafting();
			m_craftingskills[(int)eCraftingSkill.LeatherCrafting - 1] = new LeatherCrafting();
			m_craftingskills[(int)eCraftingSkill.MetalWorking - 1] = new MetalWorking();
			m_craftingskills[(int)eCraftingSkill.WoodWorking - 1] = new WoodWorking();
			//advanced skill
			m_craftingskills[(int)eCraftingSkill.Alchemy - 1] = new Alchemy();
			m_craftingskills[(int)eCraftingSkill.SpellCrafting - 1] = new SpellCrafting();

			return true;
		}

		#region Global craft functions

		/// <summary>
		/// Return the crafting skill needed to work on the item
		/// </summary>
		public static eCraftingSkill GetSecondaryCraftingSkillToWorkOnItem(EquipableItem item)
		{
			if(item is Armor && ((Armor)item).ArmorLevel == eArmorLevel.VeryLow)
			{ // cloth
				return eCraftingSkill.ClothWorking;
			}

			if(item is Armor && (((Armor)item).ArmorLevel == eArmorLevel.Low || ((Armor)item).ArmorLevel == eArmorLevel.Medium))
			{ // leather , studded
				return eCraftingSkill.LeatherCrafting;
			}

			if(item is RangedWeapon || item is Instrument)
			{ // ranged weapon , instruments
				return eCraftingSkill.WoodWorking;
			}

			if(item is Weapon)
			{
				return eCraftingSkill.MetalWorking;
			}
			
			return eCraftingSkill.NoCrafting;	
		}

		/// <summary>
		/// Return the approximative craft level of the item
		/// </summary>
		public static int GetItemCraftLevel(EquipableItem item)
		{
			if(item is Armor)
			{
				int baseLevel = 15 + item.Level * 20; // gloves
				foreach(eInventorySlot slot in item.EquipableSlot)
				{
					if(slot == eInventorySlot.HeadArmor) return baseLevel + 15;
					if(slot == eInventorySlot.FeetArmor) return baseLevel + 30;
					if(slot == eInventorySlot.LegsArmor) return baseLevel + 50;
					if(slot == eInventorySlot.ArmsArmor) return baseLevel + 65;
					if(slot == eInventorySlot.TorsoArmor) return baseLevel + 80;
				}
				return baseLevel;
			}

			return 15 + (item.Level - 1) * 20;		
		}

		#endregion

	}
}
