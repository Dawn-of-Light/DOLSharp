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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName="Spell")]
	public class DBSpell : DataObject
	{
		protected string m_id_unique;
		protected int m_spellid;
		protected int m_effectid;
		protected int m_icon;
		protected string m_name;
		protected string m_description;
		protected string m_target="";

		protected string m_spelltype="";
		protected int m_range=0;
		protected int m_radius=0;
		protected double m_value=0;
		protected double m_damage=0;
		protected int m_damageType=0;
		protected int m_concentration=0;
		protected int m_duration=0;
		protected int m_pulse=0;
		protected int m_frequency=0;
		protected int m_pulse_power=0;
		protected int m_power=0;
		protected double m_casttime=0;
		protected int m_recastdelay=0;
		protected int m_reshealth=1;
		protected int m_resmana=0;
		protected int m_lifedrain_return=0;
		protected int m_amnesia_chance=0;
		protected string m_message1="";
		protected string m_message2="";
		protected string m_message3="";
		protected string m_message4="";
		protected int m_instrumentRequirement;
		protected int m_spellGroup;
		protected int m_effectGroup;

		static bool	m_autoSave;

		public DBSpell()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull=false,Unique=true)]
		public int SpellID
		{
			get
			{
				return m_spellid;
			}
			set
			{
				Dirty = true;
				m_spellid = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public int ClientEffect
		{
			get
			{
				return m_effectid;
			}
			set
			{
				Dirty = true;
				m_effectid = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public int Icon
		{
			get
			{
				return m_icon;
			}
			set
			{
				Dirty = true;
				m_icon = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				Dirty = true;
				m_description = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public string Target
		{
			get
			{
				return m_target;
			}
			set
			{
				Dirty = true;
				m_target = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Range
		{
			get
			{
				return m_range;
			}
			set
			{
				Dirty = true;
				m_range = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Power
		{
			get
			{
				return m_power;
			}
			set
			{
				Dirty = true;
				m_power = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public double CastTime
		{
			get
			{
				return m_casttime;
			}
			set
			{
				Dirty = true;
				m_casttime = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public double Damage
		{
			get
			{
				return m_damage;
			}
			set
			{
				Dirty = true;
				m_damage = value;
			}
		}


		[DataElement(AllowDbNull=true)]
		public int DamageType
		{
			get
			{
				return m_damageType;
			}
			set
			{
				Dirty = true;
				m_damageType = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public string Type
		{
			get
			{
				return m_spelltype;
			}
			set
			{
				Dirty = true;
				m_spelltype = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Duration
		{
			get
			{
				return m_duration;
			}
			set
			{
				Dirty = true;
				m_duration = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Frequency
		{
			get
			{
				return m_frequency;
			}
			set
			{
				Dirty = true;
				m_frequency = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Pulse
		{
			get
			{
				return m_pulse;
			}
			set
			{
				Dirty = true;
				m_pulse = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int PulsePower
		{
			get
			{
				return m_pulse_power;
			}
			set
			{
				Dirty = true;
				m_pulse_power = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				Dirty = true;
				m_radius = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int RecastDelay
		{
			get
			{
				return m_recastdelay;
			}
			set
			{
				Dirty = true;
				m_recastdelay = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int ResurrectHealth
		{
			get
			{
				return m_reshealth;
			}
			set
			{
				Dirty = true;
				m_reshealth = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int ResurrectMana
		{
			get
			{
				return m_resmana;
			}
			set
			{
				Dirty = true;
				m_resmana = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public double Value
		{
			get
			{
				return m_value;
			}
			set
			{
				Dirty = true;
				m_value = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int Concentration
		{
			get
			{
				return m_concentration;
			}
			set
			{
				Dirty = true;
				m_concentration = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int LifeDrainReturn
		{
			get
			{
				return m_lifedrain_return;
			}
			set
			{
				Dirty = true;
				m_lifedrain_return = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int AmnesiaChance
		{
			get
			{
				return m_amnesia_chance;
			}
			set
			{
				Dirty = true;
				m_amnesia_chance = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public string Message1
		{
			get { return m_message1; }
			set
			{
				Dirty = true;
				m_message1 = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public string Message2
		{
			get { return m_message2; }
			set
			{
				Dirty = true;
				m_message2 = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public string Message3
		{
			get { return m_message3; }
			set
			{
				Dirty = true;
				m_message3 = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public string Message4
		{
			get { return m_message4; }
			set
			{
				Dirty = true;
				m_message4 = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int InstrumentRequirement
		{
			get { return m_instrumentRequirement; }
			set
			{
				Dirty = true;
				m_instrumentRequirement = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int SpellGroup
		{
			get
			{
				return m_spellGroup;
			}
			set
			{
				Dirty = true;
				m_spellGroup = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int EffectGroup
		{
			get
			{
				return m_effectGroup;
			}
			set
			{
				Dirty = true;
				m_effectGroup = value;
			}
		}
	}
}
