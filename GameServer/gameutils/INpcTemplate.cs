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
using System.Collections;

namespace DOL.GS
{
	/// <summary>
	/// Interface for all NPC templates
	/// </summary>
	public interface INpcTemplate
	{
		/// <summary>
		/// Gets the npc template ID
		/// </summary>
		int TemplateId { get; }

		/// <summary>
		/// Gets the template npc name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the template npc guild name
		/// </summary>
		string GuildName { get; }

		/// <summary>
		/// Gets the template npc model
		/// </summary>
		ushort Model { get; }

		/// <summary>
		/// Gets the template npc size
		/// </summary>
		byte Size { get; }

		/// <summary>
		/// Gets the template npc max speed
		/// </summary>
		short MaxSpeed { get; }

		/// <summary>
		/// Gets the template npc flags
		/// </summary>
		uint Flags { get; }

		/// <summary>
		/// Gets the template npc inventory
		/// </summary>
		IGameInventory Inventory { get; }

		/// <summary>
		/// Gets the template npc melee damage type
		/// </summary>
		eDamageType MeleeDamageType { get; }

		/// <summary>
		/// Gets the template npc parry chance
		/// </summary>
		byte ParryChance { get; }

		/// <summary>
		/// Gets the template npc evade chance
		/// </summary>
		byte EvadeChance { get; }

		/// <summary>
		/// Gets the template npc block chance
		/// </summary>
		byte BlockChance { get; }

		/// <summary>
		/// Gets the template npc left hand swing chance
		/// </summary>
		byte LeftHandSwingChance { get; }

		/// <summary>
		/// Gets the template npc spells
		/// </summary>
		IList Spells { get; }

		/// <summary>
		/// Gets the template npc styles
		/// </summary>
		IList Styles { get; }

		/// <summary>
		/// Gets the template npc spelllines
		/// </summary>
		IList SpellLines { get; }
		/// <summary>
		/// Gets the template npc abilities
		/// </summary>
		IList Abilities { get; }

		/// <summary>
		/// Gets the template npc Strength 
		///</summary>
		int Strength { get; }

		/// <summary>
		/// Gets the template npc Constitution 
		///</summary>
		int Constitution { get; }

		/// <summary>
		/// Gets the template npc Dexterity 
		///</summary>
		int Dexterity { get; }

		/// <summary>
		/// Gets the template npc Quickness 
		///</summary>
		int Quickness { get; }

		/// <summary>
		/// Gets the template npc Piety 
		///</summary>
		int Piety { get; }

		/// <summary>
		/// Gets the template npc Intelligence 
		///</summary>
		int Intelligence { get; }

		/// <summary>
		/// Gets the template npc Empathy 
		///</summary>
		int Empathy { get; }

		/// <summary>
		/// Gets the template npc Charisma 
		///</summary>
		int Charisma { get; }
	}
}