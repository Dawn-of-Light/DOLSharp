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

namespace DOL.Database.TransferObjects
{
	/// <summary>
	/// 
	/// </summary>
	public class DbSpell
	{
		protected int m_id;
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

		public int SpellID
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

		public int ClientEffect
		{
			get
			{
				return m_effectid;
			}
			set
			{
				m_effectid = value;
			}
		}

		public int Icon
		{
			get
			{
				return m_icon;
			}
			set
			{
				m_icon = value;
			}
		}

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

		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_description = value;
			}
		}

		public string Target
		{
			get
			{
				return m_target;
			}
			set
			{
				m_target = value;
			}
		}

		public int Range
		{
			get
			{
				return m_range;
			}
			set
			{
				m_range = value;
			}
		}

		public int Power
		{
			get
			{
				return m_power;
			}
			set
			{
				m_power = value;
			}
		}

		public double CastTime
		{
			get
			{
				return m_casttime;
			}
			set
			{
				m_casttime = value;
			}
		}

		public double Damage
		{
			get
			{
				return m_damage;
			}
			set
			{
				m_damage = value;
			}
		}


		public int DamageType
		{
			get
			{
				return m_damageType;
			}
			set
			{
				m_damageType = value;
			}
		}

		public string Type
		{
			get
			{
				return m_spelltype;
			}
			set
			{
				m_spelltype = value;
			}
		}

		public int Duration
		{
			get
			{
				return m_duration;
			}
			set
			{
				m_duration = value;
			}
		}

		public int Frequency
		{
			get
			{
				return m_frequency;
			}
			set
			{
				m_frequency = value;
			}
		}

		public int Pulse
		{
			get
			{
				return m_pulse;
			}
			set
			{
				m_pulse = value;
			}
		}

		public int PulsePower
		{
			get
			{
				return m_pulse_power;
			}
			set
			{
				m_pulse_power = value;
			}
		}

		public int Radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				m_radius = value;
			}
		}

		public int RecastDelay
		{
			get
			{
				return m_recastdelay;
			}
			set
			{
				m_recastdelay = value;
			}
		}

		public int ResurrectHealth
		{
			get
			{
				return m_reshealth;
			}
			set
			{
				m_reshealth = value;
			}
		}

		public int ResurrectMana
		{
			get
			{
				return m_resmana;
			}
			set
			{
				m_resmana = value;
			}
		}

		public double Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}

		public int Concentration
		{
			get
			{
				return m_concentration;
			}
			set
			{
				m_concentration = value;
			}
		}

		public int LifeDrainReturn
		{
			get
			{
				return m_lifedrain_return;
			}
			set
			{
				m_lifedrain_return = value;
			}
		}

		public int AmnesiaChance
		{
			get
			{
				return m_amnesia_chance;
			}
			set
			{
				m_amnesia_chance = value;
			}
		}

		public string Message1
		{
			get 
			{ 
				return m_message1; 
			}
			set
			{
				m_message1 = value;
			}
		}

		public string Message2
		{
			get
			{ 
				return m_message2; 
			}
			set
			{
				m_message2 = value;
			}
		}

		public string Message3
		{
			get
			{
				return m_message3;
			}
			set
			{
				m_message3 = value;
			}
		}

		public string Message4
		{
			get
			{ 
				return m_message4;
			}
			set
			{
				m_message4 = value;
			}
		}

		public int InstrumentRequirement
		{
			get 
			{
				return m_instrumentRequirement;
			}
			set
			{
				m_instrumentRequirement = value;
			}
		}

		public int SpellGroup
		{
			get
			{
				return m_spellGroup;
			}
			set
			{
				m_spellGroup = value;
			}
		}

		public int EffectGroup
		{
			get
			{
				return m_effectGroup;
			}
			set
			{
				m_effectGroup = value;
			}
		}
	}
}
