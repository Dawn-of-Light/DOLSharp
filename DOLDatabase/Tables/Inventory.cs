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

using DOL.Database;
using DOL.Database.Attributes;
using System.Collections.Generic;
using log4net;
using System.Reflection;
using System.Timers;

namespace DOL.Database
{
	/// <summary>
	/// The InventoryItem table holds all values from the
	/// ItemTemplate table and also some more values that
	/// are neccessary to store the inventory position
	/// </summary>
	[DataTable(TableName = "Inventory")]
	public class InventoryItem : DataObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Inventory fields
		protected string m_ownerID; 		// owner id
		[DataElement(AllowDbNull = false, Index = true)]
		public string OwnerID
		{
			get	{return m_ownerID;}
			set	{Dirty = true;m_ownerID = value;}
		}
		
		protected ushort m_ownerLot; 		// house lot owner id
		[DataElement(AllowDbNull = true)]
		public ushort OwnerLot
		{
			get{return m_ownerLot;}
			set{Dirty = true;m_ownerLot = value;}
		}
		
		protected string m_itemplate_id;		// id for ItemTemplate
		[DataElement(AllowDbNull = true, Index = true)]
		public string ITemplate_Id
		{
			get{return m_itemplate_id;}
			set{Dirty = true;m_itemplate_id = value;}
		}
		protected string m_utemplate_id;		// id for ItemUnique
		[DataElement(AllowDbNull = true, Index = true)]
		public string UTemplate_Id
		{
			get{return m_utemplate_id;}
			set{Dirty = true;m_utemplate_id = value;}
		}
		
		protected bool m_iscrafted;			// iscrafted or looted ?
		[DataElement(AllowDbNull = true)]
		public bool IsCrafted {
			get { return m_iscrafted; }
			set { Dirty = true; m_iscrafted = value; }
		}
		
		protected string m_creator;			// crafter or mob dropping it, but also quest, etc...
		[DataElement(AllowDbNull = true)]
		public string Creator {
			get { return m_creator; }
			set { Dirty = true; m_creator = value; }
		}
		
		protected int m_slot_pos;			// slot in inventory
		[DataElement(AllowDbNull = true)]
		public int SlotPosition
		{
			get{return m_slot_pos;}
			set{Dirty = true;m_slot_pos = value;}
		}
		
		protected int m_count; 				// count of items, for stack
		[DataElement(AllowDbNull = true)]
		public int Count
		{
			get{return m_count;}
			set{Dirty = true;m_count = value;}
		}
		
		protected int m_sellPrice;			// sell price in CM
		[DataElement(AllowDbNull = true)]
		public int SellPrice
		{
			get{return m_sellPrice;}
			set{Dirty = true;m_sellPrice = value;}
		}
		
		protected long m_experience;			// artefact experience
		[DataElement(AllowDbNull = true)]
		public virtual long Experience
		{
			get { return m_experience; }
			set{Dirty = true;m_experience = value;}
		}
		
		// Itemtemplate fields
		// apparence fields
		protected int m_color;
		[DataElement(AllowDbNull = true)]
		public int Color {
			get { return m_color; }
			set {Dirty = true; m_color = value; }
		}
		protected int m_emblem;
		[DataElement(AllowDbNull = true)]
		public int Emblem {
			get { return m_emblem; }
			set { Dirty = true;m_emblem = value; }
		}
		protected byte m_extension;
		[DataElement(AllowDbNull = true)]
		public byte Extension {
			get { return m_extension; }
			set { Dirty = true;m_extension = value; }
		}
		
		// item health
		protected int m_condition;
		[DataElement(AllowDbNull = false)]
		public int Condition {
			get { return m_condition; }
			set { Dirty = true;m_condition = value; }
		}
		protected int m_durability;
		[DataElement(AllowDbNull = false)]
		public int Durability {
			get { return m_durability; }
			set { Dirty = true;m_durability = value; }
		}
		
		// poison & current charges
		protected int m_poisonSpellID;
		[DataElement(AllowDbNull = true)]
		public int PoisonSpellID {
			get { return m_poisonSpellID; }
			set { Dirty = true;m_poisonSpellID = value; }
		}
		protected int m_poisonMaxCharges;
		[DataElement(AllowDbNull = true)]
		public int PoisonMaxCharges {
			get { return m_poisonMaxCharges; }
			set { Dirty = true;m_poisonMaxCharges = value; }
		}
		protected int m_poisonCharges;
		[DataElement(AllowDbNull = true)]
		public int PoisonCharges {
			get { return m_poisonCharges; }
			set { Dirty = true;m_poisonCharges = value; }
		}
		protected int m_charges;
		[DataElement(AllowDbNull = true)]
		public int Charges {
			get { return m_charges; }
			set { Dirty = true;m_charges = value; }
		}
		protected int m_charges1;
		[DataElement(AllowDbNull = true)]
		public int Charges1 {
			get { return m_charges1; }
			set { Dirty = true;m_charges1 = value; }
		}
		
		private DateTime m_lastUsedDateTime;	// last used DT
		public int CanUseAgainIn
		{
			get
			{
				try
				{
					TimeSpan elapsed = DateTime.Now.Subtract(m_lastUsedDateTime);
					TimeSpan reuse = new TimeSpan(0, 0, Template.CanUseEvery);
					return (reuse.CompareTo(elapsed) < 0)
						? 0
						: Template.CanUseEvery - elapsed.Seconds - 60 * elapsed.Minutes - 3600 * elapsed.Hours;
				}
				catch (ArgumentOutOfRangeException)
				{
					return 0;
				}
			}
			set
			{
				m_lastUsedDateTime = DateTime.Now.AddSeconds(value - Template.CanUseEvery);
				Dirty = true;
			}
		}
		
		private int m_cooldown;				// item cooldown
		[DataElement(AllowDbNull = false)]
		public int Cooldown
		{
			get { return CanUseAgainIn; }
			set { Dirty =true;m_cooldown = value; }
		}

		[Relation(LocalField = "ITemplate_Id", RemoteField = "Id_nb", AutoLoad = true, AutoDelete=false)]
		public ItemTemplate ITWrapper
		{
			get { return Template as ItemTemplate; }
			set { Template = value as ItemTemplate; }
		}

		[Relation(LocalField = "UTemplate_Id", RemoteField = "Id_nb", AutoLoad = true, AutoDelete=true)]
		public ItemUnique IUWrapper
		{
			get { return Template as ItemUnique; }
			set { Template = value as ItemTemplate; }
		}

		protected ItemTemplate m_item;
		public virtual ItemTemplate Template
		{
			get{
				return m_item;
			}
			set{
				Dirty = true;
				m_item = value;
			}
		}
		#endregion
		
		/// <summary>
		/// Without parameter, the inventoryitem 'll never be saved
		/// </summary>
		public InventoryItem()
		{
			AutoSave = false;
			m_ownerID = null;
			m_emblem = 0;
			m_count = 1;
			m_sellPrice = 0;
			m_experience = 0;
			m_iscrafted = false;
			m_creator = string.Empty;
			m_itemplate_id = null;
			m_utemplate_id = null;
			ITWrapper = new ItemTemplate();
			ITWrapper.AutoSave = false;
		}

		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemTemplate
		/// ItemTemplate table will never be affected
		/// An Inventoryitem created on the basis of a new ItemTemplate()
		/// or from an Artifact will never be saved too.
		/// </summary>
		/// <param name="itemTemplate"></param>
		public InventoryItem(ItemTemplate template):base()
		{
			m_ownerID = null;
			Template = template;
			m_itemplate_id = template.Id_nb;
			m_utemplate_id = null;
			m_color = template.Color;
			m_emblem = template.Emblem;
			m_count = template.PackSize;
			m_extension = template.Extension;
			m_condition = template.MaxCondition;
			m_durability = template.MaxDurability;
			m_charges = template.MaxCharges;
			m_charges1 = template.MaxCharges1;
		}
		
		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemUnique
		/// ItemUnique will always be created in the ItemUnique table
		/// </summary>
		/// <param name="itemTemplate"></param>
		public InventoryItem(ItemUnique template):base()
		{
			m_ownerID = null;
			Template = (ItemTemplate)template;
			Template.Dirty = true;
			m_utemplate_id = template.Id_nb;
			m_itemplate_id = null;
			m_color = template.Color;
			m_emblem = template.Emblem;
			m_count = template.PackSize;
			m_extension = template.Extension;
			m_condition = template.MaxCondition;
			m_durability = template.MaxDurability;
			m_charges = template.MaxCharges;
			m_charges1 = template.MaxCharges1;
		}
		
		/// <summary>
		/// Creates a new Inventoryitem based on the given InventoryItem
		/// </summary>
		/// <param name="inventoryItem"></param>
		public InventoryItem(InventoryItem template)
		{
			Template = template.Template;
			m_itemplate_id = template.ITemplate_Id;
			m_utemplate_id = template.UTemplate_Id;
			m_color = template.Color;
			m_extension = template.Extension;
			m_slot_pos = template.SlotPosition;
			m_count = template.Count;
			m_creator = template.Creator;
			m_iscrafted = template.IsCrafted;
			m_sellPrice = template.SellPrice;
			m_condition = template.Condition;
			m_durability = template.Durability;
			m_emblem = template.Emblem;
			m_cooldown = template.Cooldown;
			m_charges = template.Charges;
			m_charges1 = template.Charges1;
			m_poisonCharges = template.PoisonCharges ;
			m_poisonMaxCharges = template.PoisonMaxCharges ;
			m_poisonSpellID = template.PoisonSpellID;
			m_experience = template.Experience;
		}
		
		public void SetCooldown()
		{
			CanUseAgainIn = m_cooldown;
		}

		/// <summary>
		/// Repair cost for this item in the current state.
		/// </summary>
		public virtual long RepairCost
		{
			get
			{
				return (( Template.MaxCondition -  Condition) * Template.Price) / Template.MaxCondition;
			}
		}
		
		/// <summary>
		/// Whether to save this object or not.
		/// </summary>
		public override bool Dirty
		{
			get
			{
				// null ownerid items are never saved
				if (string.IsNullOrEmpty(m_ownerID))
					return false;
				
				// ghost item
				if (string.IsNullOrEmpty(m_utemplate_id) && string.IsNullOrEmpty(m_itemplate_id))
					return false;
				
				// Items with reuse timers will ALWAYS be saved.
				return ( base.Dirty || Template.CanUseEvery > 0);
			}
			set
			{
				// null ownerid items are never saved
				if (string.IsNullOrEmpty(m_ownerID))
					base.Dirty =  false;
				
				// ghost item
				if (string.IsNullOrEmpty(m_utemplate_id) && string.IsNullOrEmpty(m_itemplate_id))
					base.Dirty = false;
				else
					base.Dirty = value;
			}
		}
		
		#region ItemTemplate wrapper fields
		// ItemTemplate wrapper, should be removed
		// The proper use in the code must be (inventory).Template.Property, instead of (inventory).Property
		public int Level
		{
			get { return Template.Level; }
			set { Template.Level = value; }
		}

		// dur_con
		public int MaxCondition
		{
			get { return Template.MaxCondition; }
			set { Template.MaxCondition = value; }
		}
		public int MaxDurability
		{
			get { return Template.MaxDurability; }
			set { Template.MaxDurability = value; }
		}

		public int Weight
		{
			get { return Template.Weight * m_count; }
			set { Template.Weight = value; }
		}

		// weapon/armor
		public int DPS_AF
		{
			get { return Template.DPS_AF; }
			set { Template.DPS_AF = value; }
		}
		public int SPD_ABS
		{
			get { return Template.SPD_ABS; }
			set { Template.SPD_ABS = value; }
		}
		public int Hand
		{
			get { return Template.Hand; }
			set { Template.Hand = value; }
		}
		public int Type_Damage
		{
			get { return Template.Type_Damage; }
			set { Template.Type_Damage = value; }
		}
		public int Object_Type
		{
			get { return Template.Object_Type; }
			set { Template.Object_Type = value; }
		}
		public int Item_Type
		{
			get { return Template.Item_Type; }
			set { Template.Item_Type = value; }
		}
		
		// properties
		public bool IsDropable
		{
			get { return Template.IsDropable; }
			set { Template.IsDropable = value; }
		}

		public bool IsPickable
		{
			get { return Template.IsPickable; }
			set { Template.IsPickable = value; }
		}
		public bool IsTradable
		{
			get { return Template.IsTradable; }
			set { Template.IsTradable = value; }
		}
		public bool IsIndestructible {
			get { return Template.IsIndestructible; }
			set { Template.IsIndestructible = value; }
		}
		public bool IsNotLosingDur {
			get { return Template.IsNotLosingDur; }
			set { Template.IsNotLosingDur = value; }
		}
		
		// stack
		public int MaxCount
		{
			get { return Template.MaxCount; }
			set { Template.MaxCount = value; }
		}
		public int PackSize
		{
			get { return Template.PackSize; }
			set { Template.PackSize = value; }
		}
		
		// proc & charges
		public int ProcSpellID
		{
			get { return Template.ProcSpellID; }
			set { Template.ProcSpellID = value; }
		}
		public int MaxCharges
		{
			get { return Template.MaxCharges; }
			set { Template.MaxCharges = value;}
		}
		public int ProcSpellID1
		{
			get { return Template.ProcSpellID1; }
			set { Template.ProcSpellID1 = value; }
		}
		public int SpellID
		{
			get { return Template.SpellID; }
			set { Template.SpellID = value; }
		}
		public int SpellID1
		{
			get { return Template.SpellID1; }
			set { Template.SpellID1 = value; }
		}
		public int MaxCharges1
		{
			get { return Template.MaxCharges1; }
			set { Template.MaxCharges1 = value; }
		}
		
		public int CanUseEvery
		{
			get { return Template.CanUseEvery; }
			set { Template.CanUseEvery = value; }
		}
		
		public int Realm
		{
			get { return Template.Realm; }
			set { Template.Realm = value; }
		}
		public string AllowedClasses
		{
			get { return Template.AllowedClasses; }
			set { Template.AllowedClasses = value; }
		}
		public string Name
		{
			get { return Template.Name; }
			set { Template.Name = value; }
		}
		
		public string Id_nb
		{
			get {return Template.Id_nb; }
			set { Template.Id_nb = value;}
		}
		public int Model
		{
			get { return Template.Model; }
			set { Template.Model = value;}
		}
		public int Effect
		{
			get { return Template.Effect; }
			set { Template.Effect = value;}
		}
		public int Quality
		{
			get { return Template.Quality; }
			set { Template.Quality = value; }
		}
		public long Price
		{
			get { return Template.Price; }
			set { Template.Price = value; }
		}
		
		public int ExtraBonus
		{
			get { return Template.ExtraBonus; }
			set { Template.ExtraBonus = value; }
		}
		public int Bonus
		{
			get { return Template.Bonus; }
			set { Template.Bonus = value; }
		}
		public int Bonus1
		{
			get { return Template.Bonus1; }
			set { Template.Bonus1 = value; }
		}
		public int Bonus2
		{
			get { return Template.Bonus2; }
			set { Template.Bonus2 = value; }
		}
		public int Bonus3
		{
			get { return Template.Bonus3; }
			set { Template.Bonus3 = value; }
		}
		public int Bonus4
		{
			get { return Template.Bonus4; }
			set { Template.Bonus4 = value; }
		}
		public int Bonus5
		{
			get { return Template.Bonus5; }
			set { Template.Bonus5 = value; }
		}
		public int Bonus6
		{
			get { return Template.Bonus6; }
			set { Template.Bonus6 = value; }
		}
		public int Bonus7
		{
			get { return Template.Bonus7; }
			set { Template.Bonus7 = value; }
		}
		public int Bonus8
		{
			get { return Template.Bonus8; }
			set { Template.Bonus8 = value; }
		}
		public int Bonus9
		{
			get { return Template.Bonus9; }
			set { Template.Bonus9 = value; }
		}
		public int Bonus10
		{
			get { return Template.Bonus10; }
			set { Template.Bonus10 = value; }
		}
		public int ExtraBonusType
		{
			get { return Template.ExtraBonusType; }
			set { Template.ExtraBonusType = value; }
		}
		public int Bonus1Type
		{
			get { return Template.Bonus1Type; }
			set { Template.Bonus1Type = value; }
		}
		public int Bonus2Type
		{
			get { return Template.Bonus2Type; }
			set { Template.Bonus2Type = value; }
		}
		public int Bonus3Type
		{
			get { return Template.Bonus3Type; }
			set { Template.Bonus3Type = value; }
		}
		public int Bonus4Type
		{
			get { return Template.Bonus4Type; }
			set { Template.Bonus4Type = value; }
		}
		public int Bonus5Type
		{
			get { return Template.Bonus5Type; }
			set { Template.Bonus5Type = value; }
		}
		public int Bonus6Type
		{
			get { return Template.Bonus6Type; }
			set { Template.Bonus6Type = value; }
		}
		public int Bonus7Type
		{
			get { return Template.Bonus7Type; }
			set { Template.Bonus7Type = value; }
		}
		public int Bonus8Type
		{
			get { return Template.Bonus8Type; }
			set { Template.Bonus8Type = value; }
		}
		public int Bonus9Type
		{
			get { return Template.Bonus9Type; }
			set { Template.Bonus9Type = value; }
		}
		public int Bonus10Type
		{
			get { return Template.Bonus10Type; }
			set { Template.Bonus10Type = value; }
		}
		
		public string Description
		{
			get { return Template.Description;}
			set { Template.Description = value;}
		}
		public int BonusLevel
		{
			get { return Template.BonusLevel;}
			set { Template.BonusLevel = value;}
		}
		public bool CanDropAsLoot
		{
			get { return Template.CanDropAsLoot;}
			set { Template.CanDropAsLoot = value;}
		}
		
		//not used
		//protected int m_flags;
		//protected string m_packageID;
		#endregion
		
		// Wrapped methods/accessors from ItemTemplate
		public byte DurabilityPercent
		{
			get
			{
				return (byte)((Template.MaxDurability > 0) ?Durability * 100 / Template.MaxDurability : 0);
			}
		}

		public byte ConditionPercent
		{
			get	{
				return (byte)Math.Round((Template.MaxCondition > 0) ? (double)Condition / Template.MaxCondition * 100 : 0);
			}
		}
		public bool IsMagical
		{
			get{ return Template.IsMagical;}
		}
		public bool IsStackable
		{
			get{ return Template.IsStackable;}
		}
		public virtual string GetName(int article, bool firstLetterUppercase)
		{
			return Template.GetName(article,firstLetterUppercase);
		}
	}
}