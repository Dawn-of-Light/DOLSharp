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
	/// the master for alchemy
	/// </summary>
	[NPCGuildScript("Alchemists Master")]
	public class AlchemistsMaster : CraftNPC
	{
		private static readonly eCraftingSkill[] m_trainedSkills = 
		{
			eCraftingSkill.SpellCrafting,
			eCraftingSkill.Alchemy,
			eCraftingSkill.GemCutting,
			eCraftingSkill.HerbalCrafting,
			eCraftingSkill.Jewellery,
			eCraftingSkill.SiegeCrafting,
		};

		public override eCraftingSkill[] TrainedSkills
		{
			get { return m_trainedSkills; }
		}

		public override string GUILD_ORDER
		{
			get { return "Alchemists"; }
		}

		public override string GUILD_CRAFTERS
		{
			get { return "Alchemists"; }
		}

		/// <summary>
		/// The eCraftingSkill
		/// </summary>
		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.Alchemy; }
		}

		/// <summary>
		/// The text for join order
		/// </summary>
		public override string InitialEntersentence
		{
			get { return "Would you like to join the Order of [" + GUILD_ORDER + "]? As a Alchemist, you have the ability to make dyes, poisons, and various magical potions. You also have the ability to make magical tinctures that can add both offensive and defensive magical effects to crafted items. You have some skill in Spellcraft, as well as being able to Jewelcraft with great skill"; }
		}
	}
}