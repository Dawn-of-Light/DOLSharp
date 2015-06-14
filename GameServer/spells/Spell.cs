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
using System.Text;
using System.Collections.Generic;
using System.Linq;

using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// 
	/// </summary>
	public class Spell : Skill, ICustomParamsValuable
	{
		protected readonly string m_description = "";
		protected readonly string m_target = "";
		protected readonly string m_spelltype = "-";
		protected readonly int m_range = 0;
		protected readonly int m_radius = 0;
		protected readonly double m_value = 0;
		protected readonly double m_damage = 0;
		protected readonly eDamageType m_damageType = eDamageType.Natural;
		protected readonly byte m_concentration = 0;
		protected readonly int m_duration = 0;
		protected readonly int m_frequency = 0;
		protected readonly int m_pulse = 0;
		protected readonly int m_pulse_power = 0;
		protected readonly int m_power = 0;
		protected readonly int m_casttime = 0;
		protected readonly int m_recastdelay = 0;
		protected readonly int m_reshealth = 0;
		protected readonly int m_resmana = 0;
		protected readonly int m_lifedrain_return = 0;
		protected readonly int m_amnesia_chance = 0;
		protected readonly string m_message1 = "";
		protected readonly string m_message2 = "";
		protected readonly string m_message3 = "";
		protected readonly string m_message4 = "";
		protected readonly ushort m_effectID = 0;
		protected readonly int m_instrumentRequirement = 0;
		protected readonly int m_spellGroup = 0;
		protected readonly int m_effectGroup = 0;
		protected readonly int m_subSpellID = 0;
        protected readonly int m_sharedtimergroup = 0; 
		protected readonly bool m_moveCast = false;
		protected readonly bool m_uninterruptible = false;
		protected readonly bool m_isfocus = false;
        protected readonly bool m_minotaurspell = false;
		// warlocks
		protected readonly bool m_isprimary = false;
		protected readonly bool m_issecondary = false;
		protected bool m_inchamber = false;
		protected bool m_costpower = true;
		protected readonly bool m_allowbolt = false;
		protected int m_overriderange = 0;
		protected bool m_isShearable = false;
		// tooltip
		protected ushort m_tooltipId = 0;
		// params
		protected Dictionary<string, List<string>> m_paramCache = null;

		public Dictionary<string, List<string>> CustomParamsDictionary
		{
			get { return m_paramCache; }
		}
		
		#region member access properties
		#region warlocks
		public bool IsPrimary
		{
			get { return m_isprimary; }
		}

		public bool IsSecondary
		{
			get { return m_issecondary; }
		}

		public bool InChamber
		{
			get { return m_inchamber; }
			set { m_inchamber = value; }
		}

		public bool CostPower
		{
			get { return m_costpower; }
			set { m_costpower = value; }
		}

		public bool AllowBolt
		{
			get { return m_allowbolt; }
		}

		public int OverrideRange
		{
			get { return m_overriderange; }
			set { m_overriderange = value; }
		}
		#endregion
		public ushort ClientEffect
		{
			get { return m_effectID; }
		}

		public string Description
		{
			get { return m_description; }
		}

        public int SharedTimerGroup
        {
            get { return m_sharedtimergroup; }
        }

		public string Target
		{
			get { return m_target; }
		}

		public int Range
		{
			get
			{
				if (m_overriderange != 0 && m_range != 0)
					return m_overriderange;
				else
					return m_range;
			}
		}

		public int Power
		{
			get
			{
				if (!m_costpower)
					return 0;
				else
					return m_power;
			}
		}

		public int CastTime
		{
			get
			{
				if (m_inchamber)
					return 0;
				else
					return m_casttime;
			}
		}

		public double Damage
		{
			get { return m_damage; }
		}

		public eDamageType DamageType
		{
			get { return m_damageType; }

		}

		public string SpellType
		{
			get { return m_spelltype; }
		}

		public int Duration
		{
			get { return m_duration; }
		}

		public int Frequency
		{
			get { return m_frequency; }
		}

		public int Pulse
		{
			get { return m_pulse; }
		}

		public int PulsePower
		{
			get { return m_pulse_power; }
		}

		public int Radius
		{
			get { return m_radius; }
		}

		public int RecastDelay
		{
			get { return m_recastdelay; }
		}

		public int ResurrectHealth
		{
			get { return m_reshealth; }
		}

		public int ResurrectMana
		{
			get { return m_resmana; }
		}

		public double Value
		{
			get { return m_value; }
		}

		public byte Concentration
		{
			get { return m_concentration; }
		}

		public int LifeDrainReturn
		{
			get { return m_lifedrain_return; }
		}

		public int AmnesiaChance
		{
			get { return m_amnesia_chance; }
		}

		public string Message1 { get { return m_message1; } }
		public string Message2 { get { return m_message2; } }
		public string Message3 { get { return m_message3; } }
		public string Message4 { get { return m_message4; } }

		public override eSkillPage SkillType
		{
			get { return NeedInstrument ? eSkillPage.Songs : eSkillPage.Spells; }
		}

		public int InstrumentRequirement
		{
			get { return m_instrumentRequirement; }
		}

		public int Group
		{
			get { return m_spellGroup; }
		}

		public int EffectGroup
		{
			get { return m_effectGroup; }
		}

		public int SubSpellID
		{
			get { return m_subSpellID; }
		}

		public bool MoveCast
		{
			get { return m_moveCast; }
		}

		public bool Uninterruptible
		{
			get { return m_uninterruptible; }
		}
		
		public bool IsFocus
		{
			get { return m_isfocus; }
		}

		/// <summary>
		/// This spell can be sheared even if cast by self
		/// </summary>
		public bool IsShearable
		{
			get { return m_isShearable; }
			set { m_isShearable = value; }
		}

        /// <summary>
        /// Whether or not this spell is harmful.
        /// </summary>
        public bool IsHarmful
        {
            get
            {
                return (Target.ToLower() == "enemy" || Target.ToLower() == "area" ||
                    Target.ToLower() == "cone");
            }
        }
        
		#endregion

		/// <summary>
		/// Returns the string representation of the Spell
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(32)
				.Append("Name=").Append(Name)
				.Append(", ID=").Append(ID)
				.Append(", SpellType=").Append(SpellType)
				.ToString();
		}

        public Spell(DBSpell dbspell, int requiredLevel)
            : this(dbspell, requiredLevel, false)
        {
        }

        public Spell(DBSpell dbspell, int requiredLevel, bool minotaur)
			: base(dbspell.Name, dbspell.SpellID, (ushort)dbspell.Icon, requiredLevel, dbspell.TooltipId)
		{
			m_description = dbspell.Description;
			m_target = dbspell.Target;
			m_spelltype = dbspell.Type;
			m_range = dbspell.Range;
			m_radius = dbspell.Radius;
			m_value = dbspell.Value;
			m_damage = dbspell.Damage;
			m_damageType = (eDamageType)dbspell.DamageType;
			m_concentration = (byte)dbspell.Concentration;
			m_duration = dbspell.Duration * 1000;
			m_frequency = dbspell.Frequency * 100;
			m_pulse = dbspell.Pulse;
			m_pulse_power = dbspell.PulsePower;
			m_power = dbspell.Power;
			m_casttime = (int)(dbspell.CastTime * 1000);
			m_recastdelay = dbspell.RecastDelay * 1000;
			m_reshealth = dbspell.ResurrectHealth;
			m_resmana = dbspell.ResurrectMana;
			m_lifedrain_return = dbspell.LifeDrainReturn;
			m_amnesia_chance = dbspell.AmnesiaChance;
			m_message1 = dbspell.Message1;
			m_message2 = dbspell.Message2;
			m_message3 = dbspell.Message3;
			m_message4 = dbspell.Message4;
			m_effectID = (ushort)dbspell.ClientEffect;
			m_instrumentRequirement = dbspell.InstrumentRequirement;
			m_spellGroup = dbspell.SpellGroup;
			m_effectGroup = dbspell.EffectGroup;
			m_subSpellID = dbspell.SubSpellID;
			m_moveCast = dbspell.MoveCast;
			m_uninterruptible = dbspell.Uninterruptible;
			m_isfocus = dbspell.IsFocus;
			// warlocks
			m_isprimary = dbspell.IsPrimary;
			m_issecondary = dbspell.IsSecondary;
			m_allowbolt = dbspell.AllowBolt;
            m_sharedtimergroup = dbspell.SharedTimerGroup;
            m_minotaurspell = minotaur;
            // Params
            m_paramCache = InitParamCache(dbspell.CustomValues);
		}

		/// <summary>
		/// Make a copy of a spell but change the spell type
		/// Usefull for customization of spells by providing custom spell handelers
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="spellType"></param>
		public Spell(Spell spell, string spellType) :
			base(spell.Name, spell.ID, (ushort)spell.Icon, spell.Level, spell.InternalID)
		{
			m_description = spell.Description;
			m_target = spell.Target;
			m_spelltype = spellType; // replace SpellType
			m_range = spell.Range;
			m_radius = spell.Radius;
			m_value = spell.Value;
			m_damage = spell.Damage;
			m_damageType = spell.DamageType;
			m_concentration = spell.Concentration;
			m_duration = spell.Duration;
			m_frequency = spell.Frequency;
			m_pulse = spell.Pulse;
			m_pulse_power = spell.PulsePower;
			m_power = spell.Power;
			m_casttime = spell.CastTime;
			m_recastdelay = spell.RecastDelay;
			m_reshealth = spell.ResurrectHealth;
			m_resmana = spell.ResurrectMana;
			m_lifedrain_return = spell.LifeDrainReturn;
			m_amnesia_chance = spell.AmnesiaChance;
			m_message1 = spell.Message1;
			m_message2 = spell.Message2;
			m_message3 = spell.Message3;
			m_message4 = spell.Message4;
			m_effectID = spell.ClientEffect;
			m_icon = spell.Icon;
			m_instrumentRequirement = spell.InstrumentRequirement;
			m_spellGroup = spell.Group;
			m_effectGroup = spell.EffectGroup;
			m_subSpellID = spell.SubSpellID;
			m_moveCast = spell.MoveCast;
			m_uninterruptible = spell.Uninterruptible;
			m_isfocus = spell.IsFocus;
			m_isprimary = spell.IsPrimary;
			m_issecondary = spell.IsSecondary;
			m_allowbolt = spell.AllowBolt;
			m_sharedtimergroup = spell.SharedTimerGroup;
			m_minotaurspell = spell.m_minotaurspell;
            // Params
			m_paramCache = new Dictionary<string, List<string>>(spell.m_paramCache);
		}

		/// <summary>
		/// Make a shallow copy of this spell
		/// Always make a copy before attempting to modify any of the spell values
		/// </summary>
		/// <returns></returns>
		public virtual Spell Copy()
		{
			return (Spell)MemberwiseClone();
		}

		public override Skill Clone()
		{
			return (Spell)MemberwiseClone();
		}
		
		/// <summary>
		/// Fill in spell delve information.
		/// </summary>
		/// <param name="delve"></param>
		public virtual void Delve(List<String> delve)
		{
			delve.Add(String.Format("Function: {0}", Name));
			delve.Add("");
			delve.Add(Description);
			delve.Add("");
			DelveEffect(delve);
			DelveTarget(delve);

			if (Range > 0)
				delve.Add(String.Format("Range: {0}", Range));

			if (Duration > 0 && Duration < 65535)
				delve.Add(String.Format("Duration: {0}", 
					(Duration >= 60000) 
					? String.Format("{0}:{1} min", (int) (Duration / 60000), Duration % 60000)
					: String.Format("{0} sec", Duration / 1000)));

			delve.Add(String.Format("Casting time: {0}",
				(CastTime == 0) ? "instant" : String.Format("{0} sec", CastTime)));

			if (Target.ToLower() == "enemy" || Target.ToLower() == "area" || Target.ToLower() == "cone")
				delve.Add(String.Format("Damage: {0}", 
					GlobalConstants.DamageTypeToName((eDamageType)DamageType)));

			delve.Add("");
		}

		private void DelveEffect(List<String> delve)
		{
		}

		private void DelveTarget(List<String> delve)
		{
			String target;
			switch (Target)
			{
				case "Enemy":
					target = "Targetted";
					break;
				default:
					target = Target;
					break;
			}

			delve.Add(String.Format("Target: {0}", target));
		}

		#region Spell Helpers
		
        /// <summary>
        /// Whether or not the spell is instant cast.
        /// </summary>
        public bool IsInstantCast
        {
            get
            {
                return (CastTime <= 0);
            }
        }
        
        /// <summary>
        /// Wether or not the spell is Point Blank Area of Effect
        /// </summary>
        public bool IsPBAoE
        {
        	get
        	{
        		return (Range == 0 && IsAoE);
        	}
        }
        
        /// <summary>
        /// Wether or not this spell need Instrument (and is a Song)
        /// </summary>
        public bool NeedInstrument
        {
        	get
        	{
        		return InstrumentRequirement != 0;
        	}
        }
        
        /// <summary>
        /// Wether or not this spell is an Area of Effect Spell
        /// </summary>
        public bool IsAoE
        {
        	get
        	{
        		return Radius > 0;
        	}
        }
        
        /// <summary>
        /// Wether this spell Has valid SubSpell 
        /// </summary>
        public bool HasSubSpell
        {
        	get
        	{
        		return SubSpellID > 0 || MultipleSubSpells.Count > 0;
        	}
        }
        
        /// <summary>
        /// Wether this spell has a recast delay (cooldown)
        /// </summary>
        public bool HasRecastDelay
        {
        	get
        	{
        		return RecastDelay > 0;
        	}
        }
        
        /// <summary>
        /// Wether this spell is concentration based
        /// </summary>
        public bool IsConcentration
        {
        	get
        	{
        		return Concentration > 0;
        	}
        }
        
        /// <summary>
        /// Wether this spell has power usage.
        /// </summary>
        public bool UsePower
        {
        	get
        	{
        		return Power != 0;
        	}
        }

        /// <summary>
        /// Wether this spell has pulse power usage.
        /// </summary>
        public bool UsePulsePower
        {
        	get
        	{
        		return PulsePower != 0;
        	}
        }
        
        /// <summary>
        /// Wether this Spell is a pulsing spell (Song/Chant)
        /// </summary>
        public bool IsPulsing
        {
        	get
        	{
        		return Pulse != 0;
        	}
        }
        
        /// <summary>
        /// Wether this Spell is a Song/Chant
        /// </summary>
        public bool IsChant
        {
        	get
        	{
        		return Pulse != 0 && !IsFocus;
        	}
        }
        
        /// <summary>
        /// Wether this Spell is a Pulsing Effect (Dot/Hot...)
        /// </summary>
        public bool IsPulsingEffect
        {
        	get
        	{
        		return Frequency > 0 && !IsPulsing;
        	}
        }
        #endregion
		#region utils

		public ushort InternalIconID
		{
			get
			{
				return this.GetParamValue<ushort>("InternalIconID");
			}
		}
		
		public IList<int> MultipleSubSpells
		{
			get
			{
				return this.GetParamValues<int>("MultipleSubSpellID").Where(id => id > 0).ToList();
			}
		}
		
		public bool AllowCoexisting
		{
			get
			{
				return this.GetParamValue<bool>("AllowCoexisting");
			}
		}
        
		/// <summary>
		/// Initialize Param Cache from DB Relation Collection.
		/// </summary>
		/// <param name="customValues"></param>
		protected static Dictionary<string, List<string>> InitParamCache(DBSpellXCustomValues[] customValues)
		{
			Dictionary<string, List<string>> paramCache = null;
			
			if (customValues != null && customValues.Length > 0)
			{
				// create dict
				paramCache = new Dictionary<string, List<string>>();
				
				foreach (DBSpellXCustomValues val in customValues)
				{
					if (!paramCache.ContainsKey(val.KeyName))
						paramCache.Add(val.KeyName, new List<string>());
					
					paramCache[val.KeyName].Add(val.Value);
				}
			}
			
			return paramCache;
		}
				
		#endregion
	}
	
}
