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

using System;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database.DataTransferObjects
{
	public class DbItemTemplate
	{
		protected int m_durability;
		protected int m_maxdurability;
		protected int m_condition;
		protected int m_maxcondition;
		protected int m_quality;
		protected int m_maxquality;
		protected int m_dps_af;
		protected int m_spd_abs;
		protected int m_hand;
		protected int m_type_damage;
		protected int m_object_type;
		protected int m_item_type;
		protected int m_color;
		protected int m_emblem;
		protected int m_effect;
		protected byte m_extension;
		protected int m_bonus;
		protected int m_bonus1;
		protected int m_bonus2;
		protected int m_bonus3;
		protected int m_bonus4;
		protected int m_bonus5;
		protected int m_extrabonus;
		protected int m_bonusType;
		protected int m_bonus1Type;
		protected int m_bonus2Type;
		protected int m_bonus3Type;
		protected int m_bonus4Type;
		protected int m_bonus5Type;
		protected int m_extrabonusType;
		protected bool m_isPickable;
		protected string m_ClassWhenHitted;
		protected string m_ParamClassWhenHitted;
		protected string m_ClassWhenHit;
		protected string m_ParamClassWhenHit;
		protected string m_ClassWhenActivate;
		protected string m_ParamClassWhenActivate;

		//Removed and replace a by a flexible system with class and param
		//protected int m_charges;
		//protected int m_maxCharges;
		//protected int m_spellID;
		//protected int m_procSpellID;

		/// <summary>
		/// The unique generic item template identifier
		/// </summary>
		protected int m_id;

		/// <summary>
		/// The unique generic item template identifier
		/// TODO REMOVE
		/// </summary>
		protected string m_templateId;

		/// <summary>
		/// The max count of the template
		/// </summary>
		protected int m_maxCount;

		/// <summary>
		/// The number of instance to buy
		/// </summary>
		protected int m_packSize;

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

		/// <summary>
		/// Gets or sets the unique generic item template identifier
		/// TODO Remove
		/// </summary>
		public string ItemTemplateID
		{
			get { return m_templateId; }
			set { m_templateId = value; }
		}

		/// <summary>
		/// Gets or sets the unique generic item template identifier
		/// </summary>
		public int ID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// Gets or sets the name of the template
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// Gets or sets the level of the template
		/// </summary>
		public byte Level
		{
			get { return m_level; }
			set { m_level = value; }
		}

		/// <summary>
		/// Gets or sets the weight of the template (in 1/10 pounds)
		/// </summary>
		public int Weight
		{
			get { return m_weight; }
			set { m_weight = value; }
		}

		/// <summary>
		/// Gets or sets the value of the template (in copper)
		/// </summary>
		public long Value
		{
			get { return m_value; }
			set { m_value = value; }
		}

		/// <summary>
		/// Gets or sets the realm of the template
		/// </summary>
		public eRealm Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}

		/// <summary>
		/// Gets or sets the graphic model of the template
		/// </summary>
		public int Model
		{
			get { return m_model; }
			set { m_model = value; }
		}

		/// <summary>
		/// Gets or sets if the template will generate saleable items
		/// </summary>
		public bool IsSaleable
		{
			get { return m_isSaleable; }
			set { m_isSaleable = value; }
		}


		/// <summary>
		/// Gets or sets if the template will generate tradable items
		/// </summary>
		public bool IsTradable
		{
			get { return m_isTradable; }
			set { m_isTradable = value; }
		}

		/// <summary>
		/// Gets or sets if the template will generate dropable items
		/// </summary>
		public bool IsDropable
		{
			get { return m_isDropable; }
			set { m_isDropable = value; }
		}

		/// <summary>
		/// Gets or sets how much items could be stack in the same inventory slot
		/// </summary>
		public int MaxCount
		{
			get { return m_maxCount; }
			set { m_maxCount = value; }
		}

		/// <summary>
		/// Gets or sets how much items will be buy
		/// </summary>
		public int PackSize
		{
			get { return m_packSize; }
			set { m_packSize = value; }
		}


	}
}
