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
	/// <summary>
	/// Summary description for a GenericItem
	/// </summary> 
	public class GenericItem : GenericItemBase
	{
		#region Declaraction

		/// <summary>
		/// The item name
		/// </summary>
		private string m_name;

		/// <summary>
		/// The item level
		/// </summary>
		private byte m_level;

		/// <summary>
		/// The item weight (in 1/10 pounds)
		/// </summary>
		private int m_weight;

		/// <summary>
		/// The item value (in copper)
		/// </summary>
		private long m_value;

		/// <summary>
		/// The item realm
		/// </summary>
		private eRealm m_realm;

		/// <summary>
		/// Does the item is saleable
		/// </summary>
		private bool m_isSaleable;

		/// <summary>
		/// Does the item tradable
		/// </summary>
		private bool m_isTradable;

		/// <summary>
		/// Does the item is dropable
		/// </summary>
		private bool m_isDropable;

		/// <summary>
		/// The quest name of the item
		/// </summary>
		private string m_questName;

		/// <summary>
		/// The crafter name of the item
		/// </summary>
		private string m_crafterName;

		/// <summary>
		/// The owner of this item
		/// </summary>
		private GamePlayer m_owner;

		#endregion

		#region Get and Set

		/// <summary>
		/// Gets or sets the name of the item
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set	{ m_name = value; }
		}

		/// <summary>
		/// Gets or sets the level of the item
		/// </summary>
		public byte Level
		{
			get { return m_level; }
			set	{ m_level = value; }
		}

		/// <summary>
		/// Gets or sets the weight of the item (in 1/10 pounds)
		/// </summary>
		public int Weight
		{
			get { return m_weight; }
			set	{ m_weight = value; }
		}

		/// <summary>
		/// Gets or sets the value of the item (in copper)
		/// </summary>
		public long Value
		{
			get { return m_value; }
			set	{ m_value = value; }
		}

		/// <summary>
		/// Gets or sets the realm of the item
		/// </summary>
		public eRealm Realm
		{
			get { return m_realm; }
			set	{ m_realm = value; }
		}

		/// <summary>
		/// Gets or sets if the item is saleable
		/// </summary>
		public bool IsSaleable
		{
			get { return m_isSaleable; }
			set	{ m_isSaleable = value; }
		}

		
		/// <summary>
		/// Gets or sets if the item is tradable
		/// </summary>
		public bool IsTradable
		{
			get { return m_isTradable; }
			set	{ m_isTradable = value; }
		}

		/// <summary>
		/// Gets or sets if the item is dropable
		/// </summary>
		public bool IsDropable
		{
			get { return m_isDropable; }
			set	{ m_isDropable = value; }
		}

		/// <summary>
		/// Gets or sets the quest name associed with this item
		/// </summary>
		public string QuestName
		{
			get 
			{ 
				if(m_questName == null) m_questName = "";
					return m_questName; 
			}
			set	{ m_questName = value; }
		}

		/// <summary>
		/// Gets or sets the crafter name associed with this item
		/// </summary>
		public string CrafterName
		{
			get
			{
				if(m_crafterName == null) m_crafterName = "";	
					return m_crafterName;
			}
			set	{ m_crafterName = value; }
		}

		/// <summary>
		/// Gets or sets the owner of this item
		/// </summary>
		public GamePlayer Owner
		{
			get { return m_owner; }
			set	{ m_owner = value; }
		}
		#endregion

		#region Functions
		/// <summary>
		/// Called when the item is used
		/// </summary>
		/// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
		public virtual bool OnItemUsed(byte type)
		{
			if(!Owner.Alive)
			{
				Owner.Out.SendMessage("You can't fire or use this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if(!(this is RangedWeapon))
			{
				Owner.Out.SendMessage("You attempt to use " + Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			return true;
		}

		#endregion

		/// <summary>
		/// Gets the object type of the item (for test use class type instead of this propriety)
		/// </summary>
		public virtual eObjectType ObjectType
		{
			get { return eObjectType.GenericItem; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public virtual IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();

				list.Add(" ");//empty line
				if (QuestName != null && QuestName != "")
				{
					list.Add("(This is a part of "+QuestName+" quest. Do not destroy this object or you will not be able to complete the quest.)");
				}

				if (CrafterName != null && CrafterName != "")
				{
					list.Add("Crafter : " + CrafterName);
				}
				
				list.Add("Realm : " + Realm);
				
				return list;
			}
		}
	}
}
