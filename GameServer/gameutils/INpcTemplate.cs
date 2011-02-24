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
		/// Do we want the npctemplate replace db mob's values ?
		/// </summary>
		int ReplaceMobValues { get; }
		
		/// <summary>
		/// Gets the template 'physical' attributes
		/// </summary>
		string Name { get; }
		string GuildName { get; }
		string Model { get; }
		string Size { get; }
		string Level { get;}
		short MaxSpeed { get; }
		ushort Flags { get; }
		ushort Race { get; }
		ushort BodyType { get;}
		byte VisibleActiveWeaponSlot { get;}

		/// <summary>
		/// Gets the template npc inventory
		/// </summary>
		string Inventory { get; }

		/// <summary>
		/// List of items sold by this npc
		/// </summary>
		string ItemsListTemplateID { get; }

		/// <summary>
		/// Gets the template combat stats
		/// </summary>
		eDamageType MeleeDamageType { get; }
		byte ParryChance { get; }
		byte EvadeChance { get; }
		byte BlockChance { get; }
		byte LeftHandSwingChance { get; }

		/// <summary>
		/// Gets the template npc abilities
		/// </summary>
		IList Spells { get; }
		IList Styles { get; }
		IList SpellLines { get; }
		IList Abilities { get; }

		/// <summary>
		/// Gets the template npc stats
		///</summary>
		int Strength { get; }
		int Constitution { get; }
		int Dexterity { get; }
		int Quickness { get; }
		int Piety { get; }
		int Intelligence { get; }
		int Empathy { get; }
		int Charisma { get; }

		/// <summary>
		/// Gets the template npc aggro values
		/// </summary>
		byte AggroLevel { get;}
		int AggroRange { get;}
		
		/// <summary>
		/// The Mob's max distance from its spawn before return automatically
		/// if MaxDistance > 0 ... the amount is the normal value
		/// if MaxDistance = 0 ... no maxdistance check
		/// if MaxDistance less than 0 ... the amount is calculated in procent of the value and the aggrorange (in StandardMobBrain)
		/// </summary>
		int MaxDistance { get;}

		/// <summary>
		/// The mob's tether range; if mob is pulled farther than this distance
		/// it will return to its spawn point.
		/// if TetherRange > 0 ... the amount is the normal value
		/// if TetherRange less or equal 0 ... no tether check
		/// </summary>
		int TetherRange { get; }
	}
}
