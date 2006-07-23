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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct SpellEntity
	{
		private int m_id;
		private int m_amnesiaChance;
		private double m_castTime;
		private int m_clientEffect;
		private int m_concentration;
		private double m_damage;
		private int m_damageType;
		private string m_description;
		private int m_duration;
		private int m_effectGroup;
		private int m_frequency;
		private int m_icon;
		private int m_instrumentRequirement;
		private int m_lifeDrainReturn;
		private string m_message1;
		private string m_message2;
		private string m_message3;
		private string m_message4;
		private string m_name;
		private int m_power;
		private int m_pulse;
		private int m_pulsePower;
		private int m_radius;
		private int m_range;
		private int m_recastDelay;
		private int m_resurrectHealth;
		private int m_resurrectMana;
		private int m_spellGroup;
		private string m_target;
		private string m_type;
		private double m_value;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int AmnesiaChance
		{
			get { return m_amnesiaChance; }
			set { m_amnesiaChance = value; }
		}
		public double CastTime
		{
			get { return m_castTime; }
			set { m_castTime = value; }
		}
		public int ClientEffect
		{
			get { return m_clientEffect; }
			set { m_clientEffect = value; }
		}
		public int Concentration
		{
			get { return m_concentration; }
			set { m_concentration = value; }
		}
		public double Damage
		{
			get { return m_damage; }
			set { m_damage = value; }
		}
		public int DamageType
		{
			get { return m_damageType; }
			set { m_damageType = value; }
		}
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}
		public int Duration
		{
			get { return m_duration; }
			set { m_duration = value; }
		}
		public int EffectGroup
		{
			get { return m_effectGroup; }
			set { m_effectGroup = value; }
		}
		public int Frequency
		{
			get { return m_frequency; }
			set { m_frequency = value; }
		}
		public int Icon
		{
			get { return m_icon; }
			set { m_icon = value; }
		}
		public int InstrumentRequirement
		{
			get { return m_instrumentRequirement; }
			set { m_instrumentRequirement = value; }
		}
		public int LifeDrainReturn
		{
			get { return m_lifeDrainReturn; }
			set { m_lifeDrainReturn = value; }
		}
		public string Message1
		{
			get { return m_message1; }
			set { m_message1 = value; }
		}
		public string Message2
		{
			get { return m_message2; }
			set { m_message2 = value; }
		}
		public string Message3
		{
			get { return m_message3; }
			set { m_message3 = value; }
		}
		public string Message4
		{
			get { return m_message4; }
			set { m_message4 = value; }
		}
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public int Power
		{
			get { return m_power; }
			set { m_power = value; }
		}
		public int Pulse
		{
			get { return m_pulse; }
			set { m_pulse = value; }
		}
		public int PulsePower
		{
			get { return m_pulsePower; }
			set { m_pulsePower = value; }
		}
		public int Radius
		{
			get { return m_radius; }
			set { m_radius = value; }
		}
		public int Range
		{
			get { return m_range; }
			set { m_range = value; }
		}
		public int RecastDelay
		{
			get { return m_recastDelay; }
			set { m_recastDelay = value; }
		}
		public int ResurrectHealth
		{
			get { return m_resurrectHealth; }
			set { m_resurrectHealth = value; }
		}
		public int ResurrectMana
		{
			get { return m_resurrectMana; }
			set { m_resurrectMana = value; }
		}
		public int SpellGroup
		{
			get { return m_spellGroup; }
			set { m_spellGroup = value; }
		}
		public string Target
		{
			get { return m_target; }
			set { m_target = value; }
		}
		public string Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		public double Value
		{
			get { return m_value; }
			set { m_value = value; }
		}
	}
}
