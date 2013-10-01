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

using DOL.Language;

namespace DOL.GS
{
	[NPCGuildScript("Basic Crafters Master")]
	public class BasicCraftingMaster: CraftNPC
	{
		private static readonly eCraftingSkill[] m_trainedSkills = {
			eCraftingSkill.Alchemy,
			eCraftingSkill.ArmorCrafting,
			eCraftingSkill.BasicCrafting,
			eCraftingSkill.ClothWorking,
			eCraftingSkill.Fletching,
			eCraftingSkill.GemCutting,
			eCraftingSkill.HerbalCrafting,
			eCraftingSkill.LeatherCrafting,
			eCraftingSkill.MetalWorking,
			eCraftingSkill.SiegeCrafting,
			eCraftingSkill.SpellCrafting,
			eCraftingSkill.Tailoring,
			eCraftingSkill.WeaponCrafting,
			eCraftingSkill.WoodWorking
		};

		public override eCraftingSkill[] TrainedSkills
		{
			get { return m_trainedSkills; }
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.BasicCrafting; }
		}

		public override string GUILD_ORDER
		{
            get { return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "BasicCraftingMaster.GuildOrder"); }
		}

		public override string ACCEPTED_BY_ORDER_NAME
		{
            get { return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "BasicCraftingMaster.AcceptedByOrderName"); }
		}
		public override string InitialEntersentence
		{
			// Dunnerholl : Basic Crafting Master does not give the option to rejoin this craft
			get 
			{ 
				return null; 
			}
		}
	}
}
