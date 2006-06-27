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
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	public enum eObjectType : byte
	{
		GenericItem = 0,
		GenericWeapon = 1,

		//Albion weapons
		CrushingWeapon = 2,
		SlashingWeapon = 3,
		ThrustWeapon = 4,
		ShortBow = 5, // hib too
		TwoHandedWeapon = 6,
		PolearmWeapon = 7,
		Staff = 8, // hib and mid too
		Longbow = 9,
		Crossbow = 10,
		FlexibleWeapon = 24,

		//Midgard weapons
		Sword = 11,
		Hammer = 12,
		Axe = 13,
		Spear = 14,
		CompositeBow = 15,
		ThrownWeapon = 16,
		LeftAxe = 17,
		HandToHand = 25,

		//Hibernia weapons
		RecurvedBow = 18,
		Blades = 19,
		Blunt = 20,
		Piercing = 21,
		LargeWeapon = 22,
		CelticSpear = 23,
		Scythe = 26,

		//Armor
		_FirstArmor = 31,
		GenericArmor = 31,
		Cloth = 32,
		Leather =33,
		Studded = 34,
		Chain = 35,
		Plate = 36,
		Reinforced = 37,
		Scale = 38,
		_LastArmor = 38,

		//Misc
		Magical = 41,
		Shield = 42,
		Arrow = 43,
		Bolt = 44,
		Instrument = 45,
		Poison = 46,
		AlchemyTincture = 47,
		SpellcraftGem = 48,
		GardenObject = 49,
		SiegeBalista = 50,
		SiegeCatapult = 51,
		SiegeCauldron = 52,
		SiegeRam = 53,
		SiegeTrebuchet = 54,
        SiegeAmmunition = 55,
	}


	/// <summary>
	/// Summary description for a GenericItemTemplate
	/// </summary> 
	public class GenericItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The unique generic item template identifier
		/// </summary>
		protected string m_id;

		/// <summary>
		/// The item template name
		/// </summary>
		protected string m_name;

		/// <summary>
		/// The item template level
		/// </summary>
		protected byte m_level;

		/// <summary>
		/// The item template weight (in 1/10 pounds)
		/// </summary>
		protected int m_weight;

		/// <summary>
		/// The item template value (in copper)
		/// </summary>
		protected long m_value;

		/// <summary>
		/// The item template realm
		/// </summary>
		protected eRealm m_realm;

		/// <summary>
		/// The item template model
		/// </summary>
		protected int m_model;

		/// <summary>
		/// Does the template will generate saleable items
		/// </summary>
		protected bool m_isSaleable;

		/// <summary>
		/// Does the template will generate tradable items
		/// </summary>
		protected bool m_isTradable;

		/// <summary>
		/// Does the template will generate dropable items
		/// </summary>
		protected bool m_isDropable;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the unique generic item template identifier
		/// </summary>
		public string ItemTemplateID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Gets or sets the name of the template
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set	{ m_name = value; }
		}

		/// <summary>
		/// Gets or sets the level of the template
		/// </summary>
		public byte Level
		{
			get { return m_level; }
			set	{ m_level = value; }
		}

		/// <summary>
		/// Gets or sets the weight of the template (in 1/10 pounds)
		/// </summary>
		public int Weight
		{
			get { return m_weight; }
			set	{ m_weight = value; }
		}

		/// <summary>
		/// Gets or sets the value of the template (in copper)
		/// </summary>
		public long Value
		{
			get { return m_value; }
			set	{ m_value = value; }
		}

		/// <summary>
		/// Gets or sets the realm of the template
		/// </summary>
		public eRealm Realm
		{
			get { return m_realm; }
			set	{ m_realm = value; }
		}

		/// <summary>
		/// Gets or sets the graphic model of the template
		/// </summary>
		public int Model
		{
			get { return m_model; }
			set	{ m_model = value; }
		}

		/// <summary>
		/// Gets or sets if the template will generate saleable items
		/// </summary>
		public bool IsSaleable
		{
			get { return m_isSaleable; }
			set	{ m_isSaleable = value; }
		}

		
		/// <summary>
		/// Gets or sets if the template will generate tradable items
		/// </summary>
		public bool IsTradable
		{
			get { return m_isTradable; }
			set	{ m_isTradable = value; }
		}

		/// <summary>
		/// Gets or sets if the template will generate dropable items
		/// </summary>
		public bool IsDropable
		{
			get { return m_isDropable; }
			set	{ m_isDropable = value; }
		}
		#endregion

		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public virtual eObjectType ObjectType
		{
			get { return eObjectType.GenericItem; }
		}

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public virtual GenericItem CreateInstance()
		{
			GenericItem item = new GenericItem();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight;
			item.Value = m_value;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.QuestName = "";
			item.CrafterName = "";
			return item;
		}
	}
}
