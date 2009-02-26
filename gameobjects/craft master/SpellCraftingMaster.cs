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
	/// the master for spell crafting
	/// </summary>
	[NPCGuildScript("Spellcrafters Master")]
	public class SpellCraftingMaster : CraftNPC
	{
		private static readonly eCraftingSkill[] m_trainedSkills = 
		{
			eCraftingSkill.SpellCrafting,
			eCraftingSkill.Alchemy,
			eCraftingSkill.GemCutting,
			eCraftingSkill.HerbalCrafting,
			eCraftingSkill.SiegeCrafting,
		};

		public override eCraftingSkill[] TrainedSkills
		{
			get { return m_trainedSkills; }
		}

		public override string GUILD_ORDER
		{
			get
			{
				return "Spellcrafters";
			}
		}

		public override string ACCEPTED_BY_ORDER_NAME
		{
			get
			{
				return "Spellcrafters";
			}
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.SpellCrafting; }
		}

		public override string InitialEntersentence
		{
			get
			{
				return "Would you like to join the Order of [Spellcrafters]? As a Spellcrafter, you have the ability to create gems that can be imbued onto crafted items to give them new amazing magical properties.  You also have some skill in Alchemy, as well as being able to Jewelcraft with great skill.";
;			}
		}
	}
}