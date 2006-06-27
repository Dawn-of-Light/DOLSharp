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
//using DOL.GS.Database;
//using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// the master for spell crafting
	/// </summary>
	[NPCGuildScript("Spell Crafting Master")]
	public class SpellCraftingMaster : CraftNPC
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
			get { return "Spell Crafting"; }
		}

		public override string GUILD_CRAFTERS
		{
			get { return "Spell Crafter"; }
		}

		public override eCharacterClass[] AllowedClass
		{
			get
			{
				return new eCharacterClass[]
					{
						eCharacterClass.Cabalist,
						eCharacterClass.Cleric,
						eCharacterClass.Necromancer,
						eCharacterClass.Sorcerer,
						eCharacterClass.Theurgist,
						eCharacterClass.Wizard,
						eCharacterClass.Animist,
						eCharacterClass.Bainshee,
						eCharacterClass.Druid,
						eCharacterClass.Eldritch,
						eCharacterClass.Enchanter,
						eCharacterClass.Mentalist,
						eCharacterClass.Bonedancer,
						eCharacterClass.Healer,
						eCharacterClass.Runemaster,
						eCharacterClass.Shaman,
						eCharacterClass.Spiritmaster,
						eCharacterClass.Warlock,
					};
			}
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.SpellCrafting; }
		}

		public override string InitialEntersentence
		{ //TODO :good sentence
			get { return "Would you like to join the Order of [" + GUILD_ORDER + "]? As a Spell Crafter, you have the ability to make gemmes that can add magical effects to crafted items. You have some skill in Alchemy, as well as being able to Jewelcraft with great skill"; }
		}
	}
}