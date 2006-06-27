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
using System.Collections;

namespace DOL.GS.Database
{
    /// <summary>
    /// NPC template for mob and npc
    /// </summary>
	public class DBNpcTemplate
	{
 		private int 		m_id;
 		private string		m_name = "";
 		private string		m_guildName = "";
		private int			m_model;
		private byte		m_size=50;
		private short		m_maxSpeed=50;
		private string		m_equipmentTemplateID = "";
		private byte		m_meleeDamageType = 1;
		private byte 		m_parryChance;
		private byte 		m_evadeChance;
		private byte 		m_blockChance;
		private byte 		m_leftHandSwingChance;
		private string		m_spells = "";//TODO : new table?
		private string		m_styles = "";//TODO : new table?
        private int m_gender;
        private int m_race = 0;
        private int m_factionID;
        private int m_realm;
        private int m_level;
        private int m_maximumHealth;
        private bool m_isGhost;
        private bool m_isNameHidden;
        private bool m_isTargetable;
        private bool m_isFlying;
        private bool m_isStealth;
        private int m_aggroLevel;
        private int m_aggroRange;
        private string m_brainType = "";
        private string m_brainParams = "";
        private int m_STRBonusPercent;
        private int m_CONBonusPercent;
        private int m_DEXBonusPercent;
        private int m_QUIBonusPercent;
        private int m_INTBonusPercent;
        private int m_PIEBonusPercent;
        private int m_EMPBonusPercent;
        private int m_CHABonusPercent;

        /// <summary>
        /// ID of template
        /// </summary>
		public int NpcTemplateID
		{
			get
			{ 
				return m_id; 
			}
			set
			{
				m_id = value;
			}
		}

        /// <summary>
        /// Name of template
        /// </summary>
		public string Name
		{
			get 
			{ 
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

        /// <summary>
        /// Guild name of template
        /// </summary>
		public string GuildName
		{
			get 
			{ 
				return m_guildName; 
			}
			set
			{
				m_guildName = value;
			}
		}

        /// <summary>
        /// Model of template
        /// </summary>
		public int Model
		{
			get 
			{ 
				return m_model;
			}
			set
			{
				m_model = value;
			}
		}

        /// <summary>
        /// Size of template
        /// </summary>
		public byte Size
		{
			get 
			{ 
				return m_size;
			}
			set
			{
				m_size = value;
			}
		}

        /// <summary>
        /// MaxSpeed of template
        /// </summary>
		public short MaxSpeed
		{
			get 
			{ 
				return m_maxSpeed;
			}
			set
			{
				m_maxSpeed = value;
			}
		}

        /// <summary>
        /// Equipment of template
        /// </summary>
		public string EquipmentTemplateID
		{
			get
			{ 
				return m_equipmentTemplateID;
			}
			set
			{
				m_equipmentTemplateID = value;
			}
		}
		
        /// <summary>
        /// MeleeDamageType of template
        /// </summary>
		public byte MeleeDamageType
		{
			get
			{
				return m_meleeDamageType;
			}
			set
			{
				m_meleeDamageType = value;
			}
		}

        /// <summary>
        /// Parry chance of template
        /// </summary>
		public byte ParryChance
		{
			get
			{ 
				return m_parryChance;
			}
			set
			{
				m_parryChance = value;
			}
		}

        /// <summary>
        /// Evade chance of template
        /// </summary>
		public byte EvadeChance
		{
			get
			{ 
				return m_evadeChance; 
			}
			set
			{
				m_evadeChance = value;
			}
		}

        /// <summary>
        /// Block chance of template
        /// </summary>
		public byte BlockChance
		{
			get 
			{ 
				return m_blockChance;
			}
			set
			{
				m_blockChance = value;
			}
		}

        /// <summary>
        /// Left hand swing of template
        /// </summary>
		public byte LeftHandSwingChance
		{
			get
			{ 
				return m_leftHandSwingChance; 
			}
			set
			{
				m_leftHandSwingChance = value;
			}
		}

        /// <summary>
        /// Spells of template
        /// </summary>
		public string Spells
		{
			get 
			{
				return m_spells; 
			}
			set
			{
				m_spells = value;
			}
		}

        /// <summary>
        /// Gets or sets the styles.
        /// </summary>
        /// <value>The styles.</value>
		public string Styles
		{
			get
			{ 
				return m_styles; 
			}
			set
			{
				m_styles = value;
			}
		}

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>The gender.</value>
        public int Gender 
        { 
            get 
            { 
                return m_gender; 
            }
            set 
            {
                m_gender = value;
            }
        }

        /// <summary>
        /// Gets or sets the race.
        /// </summary>
        /// <value>The race.</value>
        public int Race
        {
            get 
            { 
                return m_race; 
            }
            set 
            {
                m_race = value;
            }
        }

        /// <summary>
        /// Gets or sets the faction ID.
        /// </summary>
        /// <value>The faction ID.</value>
        public int FactionID
        {
            get 
            {
                return m_factionID; 
            }
            set 
            {
                m_factionID = value;
            }
        }

        /// <summary>
        /// Gets or sets the realm.
        /// </summary>
        /// <value>The realm.</value>
        public int Realm
        {
            get 
            { 
                return m_realm; 
            }
            set 
            {
                m_realm = value;
            }
        }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get 
            { 
                return m_level; 
            }
            set 
            {
                m_level = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum health.
        /// </summary>
        /// <value>The maximum health.</value>
        public int MaximumHealth
        {
            get 
            { 
                return m_maximumHealth; 
            }
            set 
            {
                m_maximumHealth = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this npc is ghost.
        /// </summary>
        /// <value><c>true</c> if this npc is ghost; otherwise, <c>false</c>.</value>
        public bool IsGhost
        {
            get 
            { 
                return m_isGhost; 
            }
            set 
            {
                m_isGhost = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this npc is name hidden.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is name hidden; otherwise, <c>false</c>.
        /// </value>
        public bool IsNameHidden
        {
            get 
            { 
                return m_isNameHidden; 
            }
            set 
            {
                m_isNameHidden = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this npc is targetable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is targetable; otherwise, <c>false</c>.
        /// </value>
        public bool IsTargetable
        {
            get 
            { 
                return m_isTargetable; 
            }
            set 
            {
                m_isTargetable = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is flying.
        /// </summary>
        /// <value><c>true</c> if this instance is flying; otherwise, <c>false</c>.</value>
        public bool IsFlying
        {
            get 
            { 
                return m_isFlying; 
            }
            set 
            {
                m_isFlying = value;
            }
        }

        /// <summary>
        /// Gets or sets the aggro level.
        /// </summary>
        /// <value>The aggro level.</value>
        public int AggroLevel
        {
            get 
            { 
                return m_aggroLevel; 
            }
            set 
            {
                m_aggroLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the aggro range.
        /// </summary>
        /// <value>The aggro range.</value>
        public int AggroRange
        {
            get 
            { 
                return m_aggroRange; 
            }
            set 
            {
                m_aggroRange = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the brain.
        /// </summary>
        /// <value>The type of the brain.</value>
        public string BrainType
        {
            get 
            { 
                return m_brainType; 
            }
            set 
            {
                m_brainType = value;
            }
        }

        /// <summary>
        /// Gets or sets the brain params.
        /// </summary>
        /// <value>The brain params.</value>
        public string BrainParams
        {
            get 
            { 
                return m_brainParams; 
            }
            set 
            {
                m_brainParams = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is stealth.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is stealth; otherwise, <c>false</c>.
        /// </value>
        public bool IsStealth
        {
            get 
            { 
                return m_isStealth; 
            }
            set 
            {
                m_isStealth = value;
            }
        }

        /// <summary>
        /// Gets or sets the strength bonus percent.
        /// </summary>
        /// <value>The strength bonus percent.</value>
        public int STRBonusPercent
        {
            get
            {
                return m_STRBonusPercent;
            }
            set
            {
                m_STRBonusPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the constitution bonus percent.
        /// </summary>
        /// <value>The constitution bonus percent.</value>
        public int CONBonusPercent
        {
            get
            {
                return m_CONBonusPercent;
            }
            set
            {
                m_CONBonusPercent = value;
            }
        }


        /// <summary>
        /// Gets or sets the dexterity bonus percent.
        /// </summary>
        /// <value>The dexterity bonus percent.</value>
        public int DEXBonusPercent
        {
            get
            {
                return m_DEXBonusPercent;
            }
            set
            {
                m_DEXBonusPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the quickness bonus percent.
        /// </summary>
        /// <value>The quickness bonus percent.</value>
        public int QUIBonusPercent
        {
            get
            {
                return m_QUIBonusPercent;
            }
            set
            {
                m_QUIBonusPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the inteligence bonus percent.
        /// </summary>
        /// <value>The inteligence bonus percent.</value>
        public int INTBonusPercent
        {
            get
            {
                return m_INTBonusPercent;
            }
            set
            {
                m_INTBonusPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the piety bonus percent.
        /// </summary>
        /// <value>The piety bonus percent.</value>
        public int PIEBonusPercent
        {
            get
            {
                return m_PIEBonusPercent;
            }
            set
            {
                m_PIEBonusPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the empathy bonus percent.
        /// </summary>
        /// <value>The empathy bonus percent.</value>
        public int EMPBonusPercent
        {
            get
            {
                return m_EMPBonusPercent;
            }
            set
            {
                m_EMPBonusPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the charisma bonus percent.
        /// </summary>
        /// <value>The charisma bonus percent.</value>
        public int CHABonusPercent
        {
            get
            {
                return m_CHABonusPercent;
            }
            set
            {
                m_CHABonusPercent = value;
            }
        }
	}
}