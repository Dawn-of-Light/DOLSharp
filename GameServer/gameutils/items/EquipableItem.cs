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
using DOL.GS.Scripts;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a EquipableItem
	/// </summary> 
	public abstract class EquipableItem : GenericItem
	{
		#region Declaraction
		/// <summary>
		/// The quality percent of this object
		/// </summary>
		private byte m_quality;

		/// <summary>
		/// The bonus percent of this object
		/// </summary>
		private byte m_bonus;

		/// <summary>
		/// The durability percent of this object
		/// </summary>
		private byte m_durability;

		/// <summary>
		/// The condition percent of this object (ex: 85.2111 percent)
		/// </summary>
		private double m_condition;

		/// <summary>
		/// The material this object is made with
		/// </summary>
		private eMaterialLevel m_materialLevel;

		/// <summary>
		/// The id of the proc spell of this object
		/// </summary>
		private int m_procSpellID;

		/// <summary>
		/// The type of the proc this item have
		/// </summary>
		private eMagicalEffectType m_procEffectType;

		/// <summary>
		/// The id of the charge spell this item have 
		/// </summary>
		private int m_chargeSpellID;

		/// <summary>
		/// The type of the charge effect this item have
		/// </summary>
		private eMagicalEffectType m_chargeEffectType;

		/// <summary>
		/// How much charge this item actually have
		/// </summary>
		private byte m_charge;

		/// <summary>
		/// The max charge number the item can have
		/// </summary>
		private byte m_maxCharge;

		/// <summary>
		/// The list of all magical bonus of this item
		/// </summary>
		private Iesi.Collections.ISet m_magicalBonus;

		/// <summary>
		/// The list of all allowed class to wear this item
		/// </summary>
		private Iesi.Collections.ISet m_allowedClass;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the quality percent of this item
		/// </summary>
		public byte Quality
		{
			get { return m_quality; }
			set	{ m_quality = value; }
		}

		/// <summary>
		/// Gets or sets the bonus percent of the item
		/// </summary>
		public byte Bonus
		{
			get { return m_bonus; }
			set	{ m_bonus = value; }
		}

		/// <summary>
		/// Gets or sets the durability percent of the item
		/// </summary>
		public byte Durability
		{
			get { return m_durability; }
			set	{ m_durability = value; }
		}

		/// <summary>
		/// Gets or sets the condition percent of the item (ex: 85.2111 percent)
		/// </summary>
		public double Condition
		{
			get { return m_condition; }
			set	{ m_condition = value; }
		}

		/// <summary>
		/// Gets or sets with what material this item is made with
		/// </summary>
		public eMaterialLevel MaterialLevel
		{
			get { return m_materialLevel; }
			set	{ m_materialLevel = value; }
		}

		/// <summary>
		/// Gets or sets the id of the proc spell of this item
		/// </summary>
		public int ProcSpellID
		{
			get { return m_procSpellID; }
			set	{ m_procSpellID = value; }
		}

		/// <summary>
		/// Gets or sets the type of the proc of the item
		/// </summary>
		public eMagicalEffectType ProcEffectType
		{
			get { return m_procEffectType; }
			set	{ m_procEffectType = value; }
		}

		/// <summary>
		/// Gets or sets the id of the charge effect of this item
		/// </summary>
		public int ChargeSpellID
		{
			get { return m_chargeSpellID; }
			set	{ m_chargeSpellID = value; }
		}

		
		/// <summary>
		/// Gets or sets the type of the charge effect of the item
		/// </summary>
		public eMagicalEffectType ChargeEffectType
		{
			get { return m_chargeEffectType; }
			set	{ m_chargeEffectType = value; }
		}

		/// <summary>
		/// Gets or sets how much charge this item actually have
		/// </summary>
		public byte Charge
		{
			get { return m_charge; }
			set	{ m_charge = value; }
		}

		/// <summary>
		/// Gets or sets the max charge number this item can have
		/// </summary>
		public byte MaxCharge
		{
			get { return m_maxCharge; }
			set	{ m_maxCharge = value; }
		}

		/// <summary>
		/// Gets or sets the list of magical bonus of this item
		/// </summary>
		public Iesi.Collections.ISet MagicalBonus
		{
			get
			{
				if(m_magicalBonus == null) m_magicalBonus = new Iesi.Collections.HybridSet();
				return m_magicalBonus;
			}
			set	{ m_magicalBonus = value; }
		}

		/// <summary>
		/// Gets or sets the list of all class allowed to wear this item
		/// </summary>
		public Iesi.Collections.ISet AllowedClass
		{
			get
			{
				if(m_allowedClass == null) m_allowedClass = new Iesi.Collections.HybridSet();
				return m_allowedClass;
			}
			set	{ m_allowedClass = value; }
		}

		#endregion
	
		#region Function

		/// <summary>
		/// Property that holds tick when charged item was used last time
		/// </summary>
		public const string LAST_CHARGED_ITEM_USE_TICK = "LastChargedItemUsedTick";

		/// <summary>
		/// Called when the item is used
		/// </summary>
		/// <param name="type">0:quick bar click || 1:/use || 2:/use2</param>	
		public override bool OnItemUsed(byte type)
		{
			if(! base.OnItemUsed(type)) return false;
			
			if(type == 1)
			{
				if(ChargeEffectType == eMagicalEffectType.ChargeEffect && ChargeSpellID != 0)
				{
					if (Charge < 1)
					{
						Owner.Out.SendMessage("Your item have no more charges.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					}
					else if(SlotPosition <= (int) eInventorySlot.FirstBackpack)
					{
						Owner.Out.SendMessage("You can't use this item from your backpack. Equip it before!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						long lastChargedItemUseTick = Owner.TempProperties.getLongProperty(LAST_CHARGED_ITEM_USE_TICK, 0L);
						long changeTime = Owner.Region.Time - lastChargedItemUseTick;
						if(changeTime < 60000 * 3) //3 minutes reuse timer
						{
							Owner.Out.SendMessage("You must wait " + (60000 * 3 - changeTime)/1000 + " more second before discharge another object!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							SpellLine chargeEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
							if (chargeEffectLine != null)
							{
								IList spells = SkillBase.GetSpellList(chargeEffectLine.KeyName);
								if (spells != null)
								{
									foreach (Spell spell in spells)
									{
										if (spell.ID == ChargeSpellID)
										{
											if(spell.Level <= Level)
											{
												ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spell, chargeEffectLine);
												if (spellHandler != null)
												{
													spellHandler.StartSpell(Owner.TargetObject as GameLiving);
													Charge--;
													Owner.TempProperties.setProperty(LAST_CHARGED_ITEM_USE_TICK, Owner.Region.Time);
												}
												else
												{
													Owner.Out.SendMessage("Charge effect ID " + spell.ID + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												}
											}
											break;
										}
									}
								}
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Called when the item hit something or is hitted by something
		/// </summary>
		public virtual void OnItemHit()
		{
			if(Condition > 70) // min con is 70%
			{
				int baseSwing = 60 + 10 * (byte)MaterialLevel; // How much swing are needed with a yellow item to lose 1% con
				// eMaterialLevel.Bronze = 60;  eMaterialLevel.Arcanium = 150;
				
				double con = GameObject.GetConLevel(Owner.Level, Level);
				double decaySpeed = (50 / 4 * con + 100) /100; // all 4 level diff the item decay is 50% faster
			
				if (Condition < 90) { decaySpeed *= 2; } // two time faster if con less than 90%

				// Subtract condition
				Condition = Math.Max(70 , Condition - decaySpeed / baseSwing); // min con is 70%
			}
		}

		/// <summary>
		/// Called when the item is equipped
		/// </summary>
		public virtual void OnItemEquipped()
		{
			
		}

		/// <summary>
		/// Called when the item is unequipped
		/// </summary>
		public virtual void OnItemUnequipped()
		{
			
		}
		#endregion

		/// <summary>
		/// Gets all inventory slots where the item can be equipped
		/// </summary>
		public abstract eInventorySlot[] EquipableSlot
		{
			get;
		}
		
	}
}
