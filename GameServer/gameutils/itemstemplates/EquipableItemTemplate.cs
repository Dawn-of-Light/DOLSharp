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
	public enum eMagicalEffectType : byte
	{
		NoEffect		= 0,
		ActiveEffect	= 1,
		ReactiveEffect	= 2,
		ChargeEffect	= 3,
		PoisonEffect	= 4,
	}

	public enum eMaterialLevel : byte
	{
		Bronze		= 0,
		Iron		= 1,
		Steel		= 2,
		Alloy		= 3,
		FineAlloy	= 4,
		Mithril		= 5,
		Adamantium	= 6,
		Asterite	= 7,
		Netherium	= 8,
		Arcanium	= 9,
	}
			
	/// <summary>
	/// Summary description for a EquipableItemTemplate
	/// </summary> 
	public abstract class EquipableItemTemplate : GenericItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The quality percent of this template object
		/// </summary>
		protected byte m_quality;

		/// <summary>
		/// The bonus percent of this template object
		/// </summary>
		protected byte m_bonus;

		/// <summary>
		/// The durability percent of this template object
		/// </summary>
		protected byte m_durability;

		/// <summary>
		/// The condition percent of this object (ex: 85.2111 percent)
		/// </summary>
		protected double m_condition;

		/// <summary>
		/// The material this template is made with
		/// </summary>
		protected eMaterialLevel m_materialLevel;

		/// <summary>
		/// The id of the proc spell of this template
		/// </summary>
		protected int m_procSpellID;

		/// <summary>
		/// The type of the proc this item have
		/// </summary>
		protected eMagicalEffectType m_procEffectType;

		/// <summary>
		/// The id of the charge spell this item have 
		/// </summary>
		protected int m_chargeSpellID;

		/// <summary>
		/// The type of the charge effect this item have
		/// </summary>
		protected eMagicalEffectType m_chargeEffectType;

		/// <summary>
		/// How much charge the template actually have
		/// </summary>
		protected byte m_charge;

		/// <summary>
		/// The max charge number the template can have
		/// </summary>
		protected byte m_maxCharge;

		/// <summary>
		/// The list of all magical bonus of this item
		/// </summary>
		protected Iesi.Collections.ISet m_magicalBonus;

		/// <summary>
		/// The list of all allowed class to wear this item
		/// </summary>
		protected Iesi.Collections.ISet m_allowedClass;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the quality percent of this template
		/// </summary>
		public byte Quality
		{
			get { return m_quality; }
			set	{ m_quality = value; }
		}

		/// <summary>
		/// Gets or sets the bonus percent of the template
		/// </summary>
		public byte Bonus
		{
			get { return m_bonus; }
			set	{ m_bonus = value; }
		}

		/// <summary>
		/// Gets or sets the durability percent of the template
		/// </summary>
		public byte Durability
		{
			get { return m_durability; }
			set	{ m_durability = value; }
		}

		/// <summary>
		/// Gets or sets the condition percent of the template (ex: 85.2111 percent)
		/// </summary>
		public double Condition
		{
			get { return m_condition; }
			set	{ m_condition = value; }
		}

		/// <summary>
		/// Gets or sets with what material this template is made with
		/// </summary>
		public eMaterialLevel MaterialLevel
		{
			get { return m_materialLevel; }
			set	{ m_materialLevel = value; }
		}

		/// <summary>
		/// Gets or sets the id of the proc spell of this template
		/// </summary>
		public int ProcSpellID
		{
			get { return m_procSpellID; }
			set	{ m_procSpellID = value; }
		}

		/// <summary>
		/// Gets or sets the type of the proc of the template
		/// </summary>
		public eMagicalEffectType ProcEffectType
		{
			get { return m_procEffectType; }
			set	{ m_procEffectType = value; }
		}

		/// <summary>
		/// Gets or sets the id of the charge effect of this template
		/// </summary>
		public int ChargeSpellID
		{
			get { return m_chargeSpellID; }
			set	{ m_chargeSpellID = value; }
		}

		
		/// <summary>
		/// Gets or sets the type of the charge effect of the template
		/// </summary>
		public eMagicalEffectType ChargeEffectType
		{
			get { return m_chargeEffectType; }
			set	{ m_chargeEffectType = value; }
		}

		/// <summary>
		/// Gets or sets how much charge the template actually have
		/// </summary>
		public byte Charge
		{
			get { return m_charge; }
			set	{ m_charge = value; }
		}

		/// <summary>
		/// Gets or sets the max charge number the template can have
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
	}
}
