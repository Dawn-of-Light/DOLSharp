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

namespace DOL.GS
{
	/// <summary>
	/// the master for armorcrafting
	/// </summary>
	[NPCGuildScript("Weaponsmiths Master")]
	public class WeaponCraftingMaster : CraftNPC
	{
		private static readonly eCraftingSkill[] m_trainedSkills = 
		{
			eCraftingSkill.ArmorCrafting,
			eCraftingSkill.ClothWorking,
			eCraftingSkill.Fletching,
			eCraftingSkill.LeatherCrafting,
			eCraftingSkill.SiegeCrafting,
			eCraftingSkill.Tailoring,
			eCraftingSkill.WeaponCrafting,
			eCraftingSkill.MetalWorking,
			eCraftingSkill.WoodWorking,
		};

		public override eCraftingSkill[] TrainedSkills
		{
			get { return m_trainedSkills; }
		}

		public override string GUILD_ORDER
		{
			get
			{
				return "Weaponcrafters";
			}
		}

		public override string ACCEPTED_BY_ORDER_NAME
		{
			get
			{
				return "Weaponsmiths";
			}
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.WeaponCrafting; }
		}

		public override string InitialEntersentence
		{
			get
			{
				return "Would you like to join the Order of [Weaponcrafters]? As a crafter of weapons, you can expect to forge swords, axes, hammers and spears. While you will excel in weapon crafting and have good skills in armor crafting, you can expect great difficulty in tailoring and fletching. A well-trained weapon crafter also has a great bit of skill to perform siege crafting should it be of need.";
			}
		}
	}
}