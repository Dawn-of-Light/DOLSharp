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
using System.Reflection;
using System.Collections;
using System.Text;
using System.Threading;
using DOL.GS;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class is the template class for all GameNPC
	/// </summary>
	public abstract class GameNPCTemplate
	{
		#region Name / Realm / Model (PersistantGameObject properties)

		/// <summary>
		/// The unique game object identifier (db unique id)
		/// </summary>
		private int m_id;

		/// <summary>
		/// Gets or sets the unique game object template identifier
		/// </summary>
		public int GameNPCTemplateID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// The name of the Object
		/// </summary>
		protected string m_Name;

		/// <summary>
		/// Holds the realm of this object
		/// </summary>
		protected byte m_Realm;

		/// <summary>
		/// The model of the Object
		/// </summary>
		protected int m_Model;

		/// <summary>
		/// Gets or Sets the current Name of the Object
		/// </summary>
		public virtual string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		/// <summary>
		/// Gets or Sets the current Realm of the Object
		/// </summary>
		public virtual byte Realm
		{
			get { return m_Realm; }
			set { m_Realm = value; }
		}

		/// <summary>
		/// Gets or Sets the current Model of the Object
		/// </summary>
		public virtual int Model
		{
			get { return m_Model; }
			set { m_Model = value; }
		}

		#endregion

		#region Level(min/max) (GameLivingBase properties)
        /// <summary>
        /// The min level of the Object
        /// </summary>
        protected byte m_minLevel;

        /// <summary>
        /// Gets or Sets the min level of the Object
        /// </summary>
        public virtual byte MinLevel
        {
            get { return m_minLevel; }
            set { m_minLevel = value; }
        }
	    
		/// <summary>
		/// The max level of the Object
		/// </summary>
		protected byte m_maxLevel;

		/// <summary>
		/// Gets or Sets the max level of the Object
		/// </summary>
		public virtual byte MaxLevel
		{
            get { return m_maxLevel; }
            set { m_maxLevel = value; }
		}
		#endregion

		#region GuildName / MaxSpeedBase (GameLiving properties)
		/// <summary>
		/// The guildname of this living
		/// </summary>
		protected string m_guildName;

		/// <summary>
		/// Gets or sets the guildname of this living
		/// </summary>
		public virtual string GuildName
		{
			get { return m_guildName; }
			set { m_guildName = value; }
		}

		/// <summary>
		/// The base maximum speed of this living
		/// </summary>
		protected int m_maxSpeedBase;

		/// <summary>
		/// Gets or sets the base max speed of this living
		/// </summary>
		public virtual int MaxSpeedBase
		{
			get { return m_maxSpeedBase; }
			set { m_maxSpeedBase = value; }
		}
		#endregion

		#region Size(min/max)/Flags/MeleeDamageType/EvadeChance/BlockChance/ParryChance/LeftHandSwingChance/InventoryID/OwnBrainTemplate (GameNPC properties)

        /// <summary>
        /// Holds the min size of the NPC
        /// </summary>
        protected byte m_minSize;

        /// <summary>
        /// Gets or sets the min size of the npc
        /// </summary>
        public byte MinSize
        {
            get { return m_minSize; }
            set { m_minSize = value; }
        }
	    
		/// <summary>
		/// Holds the max size of the NPC
		/// </summary>
		protected byte m_maxSize;

		/// <summary>
		/// Gets or sets the max size of the npc
		/// </summary>
		public byte MaxSize
		{
            get { return m_maxSize; }
            set { m_maxSize = value; }
		}

		/// <summary>
		/// Holds various flags of this npc
		/// </summary>
		protected byte m_flags;

		/// <summary>
		/// Gets or Sets the flags of this npc
		/// </summary>
		public virtual byte Flags
		{
			get { return m_flags; }
			set { m_flags = value; }
		}

		/// <summary>
		/// Stores the melee damage type of this NPC
		/// </summary>
		protected eDamageType m_meleeDamageType = eDamageType.Slash;

		/// <summary>
		/// Gets or sets the melee damage type of this NPC
		/// </summary>
		public virtual eDamageType MeleeDamageType
		{
			get { return m_meleeDamageType; }
			set { m_meleeDamageType = value; }
		}

		/// <summary>
		/// Stores the NPC evade chance
		/// </summary>
		protected byte m_evadeChance;

		/// <summary>
		/// Gets or sets the NPC evade chance
		/// </summary>
		public virtual byte EvadeChance
		{
			get { return m_evadeChance; }
			set { m_evadeChance = value; }
		}

		/// <summary>
		/// Stores the NPC block chance
		/// </summary>
		protected byte m_blockChance;

		/// <summary>
		/// Gets or sets the NPC block chance
		/// </summary>
		public virtual byte BlockChance
		{
			get { return m_blockChance; }
			set { m_blockChance = value; }
		}

		/// <summary>
		/// Stores the NPC parry chance
		/// </summary>
		protected byte m_parryChance;

		/// <summary>
		/// Gets or sets the NPC parry chance
		/// </summary>
		public virtual byte ParryChance
		{
			get { return m_parryChance; }
			set { m_parryChance = value; }
		}

		/// <summary>
		/// Stores the NPC left hand swing chance
		/// </summary>
		protected byte m_leftHandSwingChance;

		/// <summary>
		/// Gets or sets the NPC left hand swing chance
		/// </summary>
		public byte LeftHandSwingChance
		{
			get { return m_leftHandSwingChance; }
			set { m_leftHandSwingChance = value; }
		}

		/// <summary>
		/// Stores the InventoryID of this template
		/// </summary>
		protected int m_inventoryID;

		/// <summary>
		/// Gets or sets the InventoryID of this template
		/// </summary>
		public int InventoryID
		{
			get { return m_inventoryID; }
			set { m_inventoryID = value; }
		}

		/// <summary>
		/// Holds the own NPC brain
		/// </summary>
		protected ABrainTemplate m_ownBrainTemplate;

		/// <summary>
		/// Gets or set the own brain of this NPC
		/// </summary>
		private ABrainTemplate OwnBrainTemplate
		{
			get { return m_ownBrainTemplate; }
			set { m_ownBrainTemplate = value; }
		}
		#endregion

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public abstract GameObject CreateInstance();
	}
}
