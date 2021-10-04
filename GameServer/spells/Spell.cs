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
using DOL.AI.Brain;

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
		protected double m_value = 0;
		protected double m_damage = 0;
		protected readonly eDamageType m_damageType = eDamageType.Natural;
		protected readonly byte m_concentration = 0;
		protected int m_duration = 0;
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
			set { m_paramCache = value; }
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

		public virtual string Target
		{
			get { return m_target; }
		}

		public virtual int Range
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
			set { m_damage = value; }
		}

		public eDamageType DamageType
		{
			get { return m_damageType; }

		}

		public virtual string SpellType
		{
			get { return m_spelltype; }
		}

		public int Duration
		{
			get { return m_duration; }
            set { m_duration = value; }
		}

		public virtual int Frequency
		{
			get { return m_frequency; }
		}

		public virtual int Pulse
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
			set { m_value = value; }
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

		public int SubSpellId { get; }

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
		
		public virtual bool IsFocus
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
        /// Is this spell harmful?
        /// </summary>
        public bool IsHarmful
        {
            get
            {
				switch (Target.ToUpper())
				{
					case "ENEMY":
					case "AREA":
					case "CONE":
						return true;
					default:
						return false;
				}
            }
        }

		/// <summary>
		/// Is this spell Helpful?
		/// </summary>
		public bool IsHelpful
		{
			get { return !IsHarmful; }
		}

		/// <summary>
		/// Is this a buff spell?
		/// </summary>
		public bool IsBuff
		{
			get
			{
				return (IsHelpful && (Duration > 0 || Concentration > 0));
			}
		}

		/// <summary>
		/// Is this a healing spell?
		/// </summary>
		public bool IsHealing
		{
			get
			{
				switch (SpellType.ToUpper())
				{
					case "CUREPOISON":
					case "CUREDISEASE":
					case "COMBATHEAL":
					case "HEAL":
					case "HEALOVERTIME":
					case "HEALTHREGENBUFF":
					case "MERCHEAL":
					case "OMNIHEAL":
					case "PBAEHEAL":
					case "SPREADHEAL":
					case "SUMMONHEALINGELEMENTAL":
						return true;
					default:
						return false;
				}
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
            this.InitFromCollection<DBSpellXCustomValues>(dbspell.CustomValues, param => param.KeyName, param => param.Value);
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
		/// Check if a target has this spell's effect already
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool TargetHasEffect(GameObject target)
		{
			return Duration > 0 && StandardMobBrain.LivingHasEffect(target as GameLiving, this);
		}

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
        /// Whether or not the spell is Point Blank Area of Effect
        /// </summary>
        public bool IsPBAoE
        {
        	get
        	{
        		return (Range == 0 && IsAoE);
        	}
        }

		/// <summary>
		/// Whether or not this spell need Instrument (and is a Song)
		/// </summary>
		public bool NeedInstrument
        {
        	get
        	{
        		return InstrumentRequirement != 0;
        	}
        }

		/// <summary>
		/// Whether or not this spell is an Area of Effect Spell
		/// </summary>
		public bool IsAoE
        {
        	get
        	{
        		return Radius > 0;
        	}
        }

		/// <summary>
		/// Whether this spell Has valid SubSpell 
		/// </summary>
		public bool HasSubSpell
        {
        	get
        	{
        		return SubSpellID > 0 || MultipleSubSpells.Count > 0;
        	}
        }

		/// <summary>
		/// Whether this spell has a recast delay (cooldown)
		/// </summary>
		public bool HasRecastDelay
        {
        	get
        	{
        		return RecastDelay > 0;
        	}
        }

		/// <summary>
		/// Whether this spell is concentration based
		/// </summary>
		public bool IsConcentration
        {
        	get
        	{
        		return Concentration > 0;
        	}
        }

		/// <summary>
		/// Whether this spell has power usage.
		/// </summary>
		public bool UsePower
        {
        	get
        	{
        		return Power != 0;
        	}
        }

		/// <summary>
		/// Whether this spell has pulse power usage.
		/// </summary>
		public bool UsePulsePower
        {
        	get
        	{
        		return PulsePower != 0;
        	}
        }

		/// <summary>
		/// Whether this Spell is a pulsing spell (Song/Chant)
		/// </summary>
		public bool IsPulsing
        {
        	get
        	{
        		return Pulse != 0;
        	}
        }

		/// <summary>
		/// Whether this Spell is a Song/Chant
		/// </summary>
		public bool IsChant
        {
        	get
        	{
        		return Pulse != 0 && !IsFocus;
        	}
        }

		/// <summary>
		/// Whether this Spell is a Pulsing Effect (Dot/Hot...)
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

		//Eden delve methods that i've added to
		public string GetDelveFunction()
		{
			switch (SpellType)
			{
				case "DummySpell": // test for abilitySpells
				case "RvrResurrectionIllness":
				case "PveResurrectionIllness": return "light";

				case "Charm": return "charm";
				case "CureMezz": return "remove_eff";
				case "Lifedrain": return "lifedrain";
				case "PaladinArmorFactorBuff":
				case "ArmorFactorBuff": return "shield";
				case "ArmorAbsorptionBuff": return "absorb";
				case "DirectDamageWithDebuff": return "nresist_dam";
				case "DamageSpeedDecrease":
				case "SpeedDecrease": return "snare";
				case "Bolt": return "bolt";

				case "Amnesia": return "amnesia";

				case "QuicknessDebuff":
				case "ConstitutionDebuff":
				case "StrengthDebuff":
				case "DexterityDebuff": return "nstat";

				case "QuicknessBuff":
				case "ConstitutionBuff":
				case "StrengthBuff":
				case "DexterityBuff": return "stat";

				case "Disease": return "disease";
				case "DamageOverTime": return "dot";

				case "Confusion":
				case "Mesmerize":
				case "Nearsight":
				case "PetSpeedEnhancement":
				case "SpeedEnhancement":
				case "SpeedOfTheRealm":
				case "CombatSpeedBuff":
				case "CombatSpeedDebuff":
				case "Bladeturn": return "combat";

				case "DirectDamage":
					if (Duration == 1) // change this , field in DB? IsGTAoE bool - Unty
					{
						return "storm";
					}
					return "direct";

				case "AcuityDebuff":
				case "StrengthConstitutionDebuff":
				case "DexterityConstitutionDebuff":
				case "WeaponSkillConstitutionDebuff":
				case "DexterityQuicknessDebuff": return "ntwostat";

				case "AcuityBuff":
				case "StrengthConstitutionBuff":
				case "DexterityConstitutionBuff":
				case "WeaponSkillConstitutionBuff":
				case "DexterityQuicknessBuff": return "twostat";

				case "BodyResistDebuff":
				case "ColdResistDebuff":
				case "EnergyResistDebuff":
				case "HeatResistDebuff":
				case "MatterResistDebuff":
				case "SpiritResistDebuff":
				case "SlashResistDebuff":
				case "ThrustResistDebuff":
				case "CrushResistDebuff":
				case "EssenceSear": return "nresistance";

				case "BodySpiritEnergyBuff":
				case "HeatColdMatterBuff":
				case "BodyResistBuff":
				case "ColdResistBuff":
				case "EnergyResistBuff":
				case "HeatResistBuff":
				case "MatterResistBuff":
				case "SpiritResistBuff":
				case "SlashResistBuff":
				case "ThrustResistBuff":
				case "CrushResistBuff": return "resistance";

				case "HealthRegenBuff":
				case "EnduranceRegenBuff":
				case "PowerRegenBuff": return "enhancement";

				case "MesmerizeDurationBuff": return "mez_dampen";

				case "CombatHeal": // guess for now

				case "SubSpellHeal": // new for ability value - Unty
				case "Heal": return "heal";

				case "Resurrect": return "raise_dead";
				case "DamageAdd": return "dmg_add";

				case "CureNearsight":
				case "CurePoison":
				case "CureDisease": return "rem_eff_ty";
				case "SpreadHeal": return "spreadheal";

				case "SummonAnimistFnF":
				case "SummonAnimistPet":
				case "SummonCommander":
				case "SummonMinion":
				case "SummonSimulacrum":
				case "SummonDruidPet":
				case "SummonHunterPet":
				case "SummonNecroPet":
				case "SummonUnderhill":
				case "SummonTheurgistPet": return "summon";

				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear": return "buff_shear";

				case "StyleStun":
				case "StyleBleeding":
				case "StyleSpeedDecrease":
				case "StyleCombatSpeedDebuff": return "add_effect";

				case "SiegeArrow":
				case "ArrowDamageTypes":
				case "Archery": return "archery";

				case "HereticDoTLostOnPulse": return "direct_inc";

				case "DefensiveProc": return "def_proc";

				case "OffensiveProcPvE":
				case "OffensiveProc": return "off_proc";
				case "AblativeArmor": return "hit_buffer";

				case "Stun": return "paralyze";
				case "HealOverTime": return "regen";
				case "DamageShield": return "dmg_shield";
				case "Taunt": return "taunt";

				case "MeleeDamageDebuff": return "ndamage";
				case "ArmorAbsorptionDebuff": return "nabsorb";
			}
			return "0";
		}

		public int GetDelveAmountIncrease()
		{
			switch (SpellType)
			{
				case "HereticDoTLostOnPulse": return 50;
			}
			return 0;
		}

		public int GetDelveAbility()
		{
			switch (SpellType)
			{
				case "MesmerizeDurationBuff": return Target == "Self" ? 4 : 3072;
				case "DamageAdd":
				case "ArmorAbsorptionBuff":
				case "BodyResistBuff":
				case "DefensiveProc":
				case "OffensiveProc":
				case "ColdResistBuff":
				case "EnergyResistBuff":
				case "HeatResistBuff":
				case "MatterResistBuff":
				case "SpiritResistBuff":
				case "SlashResistBuff":
				case "ThrustResistBuff":
				case "CrushResistBuff":
				case "ArmorFactorBuff": return 4;
				case "SiegeArrow": return 1024;

				case "BodySpiritEnergyBuff":
				case "HeatColdMatterBuff": return Pulse > 0 ? 2052 : 3076;

				case "SubSpellHeal": return 2049;
				case "PowerRegenBuff":
				case "EnduranceRegenBuff":
				case "HealthRegenBuff":
					if (Pulse > 0)
					{
						return 3072;
					}
					return 4;
				case "AblativeArmor": return 3072;
				case "Archery":
					if (Name.StartsWith("Power Shot"))
					{
						return 1088;
					}
					return 1024;
				case "ArcheryDoT": return 1;

				case "ArmorAbsorptionDebuff":
				case "MeleeDamageDebuff": return 8;
			}
			return 0;
		}

		public int GetDelvePowerLevel(int level)
		{
			switch (SpellType)
			{
				case "Confusion": return (int)Value + 100;

				case "SummonAnimistFnF":
				case "SummonAnimistPet":
				case "SummonCommander":
				case "SummonMinion":
				case "SummonSimulacrum":
				case "SummonDruidPet":
				case "SummonHunterPet":
				case "SummonNecroPet":
				case "SummonTheurgistPet":
				case "DamageOverTime": return -(int)Damage;

				case "Charm": return Pulse == 1 ? (int)Damage : -(int)Damage;

				case "CombatSpeedBuff": return -(int)(Value * 2);

				case "StyleBleeding": return (int)Damage;
				case "StyleSpeedDecrease": return (int)(100 - Value);

				case "CombatSpeedDebuff":
				case "StyleCombatSpeedDebuff": return -(int)Value;
			}
			return level;
		}

		public int GetDelveTargetType()
		{
			switch (Target)
			{
				case "Realm": return 7;
				case "Self": return 0;
				case "Enemy": return 1;
				case "Pet": return 6;
				case "Group": return 3;
				case "Area": return 9;

				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear": return 10;
				//case "OffensiveProcPvE": return 14 PvE only -Unty
				default: return 0;
			}
		}

		public int GetDelvePowerCost()
		{
			switch (SpellType)
			{
				//case "ArmorAbsorptionDebuff"
				case "BodySpiritEnergyBuff":
				case "HeatColdMatterBuff": return Pulse > 0 ? -PulsePower : Power;
				case "SiegeArrow":
				case "ArrowDamageTypes":
				case "Archery": return -Power;
			}
			return Power;
		}

		public int GetDelveLinkEffect()
		{
			if (SubSpellId > 0)
			{
				return (int)SubSpellId;
			}
			switch (SpellType)
			{
				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear": return IsAoE ? 7312 : 5595;
			}
			return 0;
		}

		public int GetDelveDurationType()
		{
			//2-seconds,4-conc,5-focus
			switch (SpellType)
			{
				case "HereticDoTLostOnPulse": return 5;
			}
			if (Duration > 0)
			{
				return 2;
			}
			if (Concentration > 0)
			{
				return 4;
			}
			return 0;
		}

		public int GetDelveDuration()
		{
			return Duration / 1000;
		}

		public int GetDelveDamageType()
		{
			switch (SpellType)
			{
				case "StyleSpeedDecrease":
				case "StyleCombatSpeedDebuff": return 0;
			}
			switch (DamageType)
			{
				case eDamageType.Slash: return 2;
				case eDamageType.Heat: return 10;
				case eDamageType.Cold: return 12;
				case eDamageType.Matter: return 15;
				case eDamageType.Body: return 16;
				case eDamageType.Spirit: return 17;
				case eDamageType.Energy: return 22;
			}
			return 0;
		}

		public int GetDelveBonus()
		{
			switch (SpellType)
			{
				case "Charm": return (int)Pulse == 1 ? 1 : 0;
				case "SummonAnimistFnF":
				case "SummonAnimistPet":
				case "SummonCommander":
				case "SummonMinion":
				case "SummonSimulacrum":
				case "SummonDruidPet":
				case "SummonHunterPet":
				case "SummonNecroPet":
				case "SummonUnderhill":
				case "SummonTheurgistPet": return 1;

				case "Lifedrain": return LifeDrainReturn / 10;
				case "DamageSpeedDecrease":
				case "SpeedDecrease": return (int)(100 - Value);
				case "Amnesia": return AmnesiaChance;
				case "QuicknessDebuff":
				case "QuicknessBuff":
				case "ConstitutionDebuff":
				case "ConstitutionBuff":
				case "StrengthDebuff":
				case "StrengthBuff":
				case "DexterityDebuff":
				case "DexterityBuff":
				case "AcuityDebuff":
				case "AcuityBuff":
				case "SpeedOfTheRealm":
				case "SpeedEnhancement":
				case "PetSpeedEnhancement":
				case "ArmorAbsorptionBuff":
				case "DexterityQuicknessDebuff":
				case "DexterityQuicknessBuff":
				case "StrengthConstitutionDebuff":
				case "StrengthConstitutionBuff":

				case "BodyResistDebuff":
				case "ColdResistDebuff":
				case "EnergyResistDebuff":
				case "HeatResistDebuff":
				case "MatterResistDebuff":
				case "SpiritResistDebuff":
				case "SlashResistDebuff":
				case "ThrustResistDebuff":
				case "CrushResistDebuff":

				case "BodyResistBuff":
				case "ColdResistBuff":
				case "EnergyResistBuff":
				case "HeatResistBuff":
				case "MatterResistBuff":
				case "SpiritResistBuff":
				case "SlashResistBuff":
				case "ThrustResistBuff":
				case "CrushResistBuff":
				case "BodySpiritEnergyBuff":
				case "HeatColdMatterBuff":

				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear":

				case "EssenceSear":
				case "DirectDamageWithDebuff":
				case "MesmerizeDurationBuff":
				case "PaladinArmorFactorBuff":
				case "Disease": // this is disease strength decrease
				case "ArmorFactorBuff": return (int)Value;

				case "StyleSpeedDecrease": return (int)(100 - Value);

				case "Bolt":
				case "SiegeArrow":
				case "ArrowDamageTypes":
				case "Archery": return 20;

				case "OffensiveProcPvE":
				case "DefensiveProc":
				case "OffensiveProc": return (int)Frequency / 100;

				case "AblativeArmor": return (int)Damage;
				case "Resurrect": return (int)ResurrectMana;
				case "ArmorAbsorptionDebuff":
				case "MeleeDamageDebuff": return (int)Value * (-1);
			}
			return 0;
		}


		public int GetDelveLink()
		{
			if (SubSpellId != 0)
			{
				return SubSpellId;
			}
			return 0;
		}

		public int GetDelveDamage()
		{
			switch (SpellType)
			{
				case "Bladeturn": return 51;
				case "DamageAdd":
				case "DamageSpeedDecrease":
				case "DirectDamage":
				case "DirectDamageWithDebuff":
				case "DamageShield":
				case "Bolt":
				case "Lifedrain": return (int)(Damage * 10);

				case "SummonAnimistFnF":
				case "SummonAnimistPet":
				case "SummonCommander":
				case "SummonMinion":
				case "SummonSimulacrum":
				case "SummonDruidPet":
				case "SummonHunterPet":
				case "SummonNecroPet":
				case "SummonTheurgistPet":
				case "SummonUnderhill":
				case "Disease": // this is disease speed decrease
				case "DamageOverTime": return (int)Damage;

				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear": return 2;

				case "CombatHeal": // guess
				case "SpreadHeal":
				case "SubSpellHeal":
				case "Heal":
				case "Charm":
				case "EnduranceRegenBuff":
				case "HealOverTime":
				case "AblativeArmor":
				case "HealthRegenBuff":
				case "PowerRegenBuff": return (int)Value;
				case "Resurrect": return ResurrectHealth;

				case "StyleBleeding": return (int)Damage;

				case "SiegeArrow":
				case "Archery": return (int)(Damage * 10);

				case "Taunt": return (int)Value;
			}
			return 0;
		}

		public int GetDelveParm(GameClient client)
		{
			switch (SpellType)
			{
				case "BodySpiritEnergyBuff": return Pulse > 0 ? 98 : 94;
				case "HeatColdMatterBuff": return Pulse > 0 ? 97 : 93;

				case "DamageAdd":
				case "ArmorAbsorptionDebuff":
				case "ArmorAbsorptionBuff":
				case "StyleSpeedDecrease":
				case "StyleStun":
				case "StrengthShear":
				case "StrengthDebuff":
				case "StrengthBuff":
				case "StrengthConstitutionShear":
				case "StrengthConstitutionDebuff":
				case "StrengthConstitutionBuff":
				case "Taunt":
				case "Stun":
				case "DamageOverTime":
				case "SpeedDecrease":
				case "DamageSpeedDecrease":
				case "DirectDamage":
				case "Bolt":
				case "HealOverTime":
				case "DamageShield":
				case "AblativeArmor":
				case "HealthRegenBuff":
				case "CombatHeal": // guess
				case "Lifedrain": return 1;

				case "CombatSpeedDebuff":
				case "StyleCombatSpeedDebuff":
				case "PowerRegenBuff":
				case "DexterityShear":
				case "DexterityDebuff":
				case "DexterityBuff":
				case "DexterityQuicknessShear":
				case "DexterityQuicknessDebuff":
				case "DexterityQuicknessBuff":
				case "ArmorFactorDebuff":
				case "MeleeDamageDebuff":
				case "ArmorFactorBuff": return 2;

				case "EnduranceRegenBuff":
				case "ConstitutionShear":
				case "ConstitutionBuff":
				case "ConstitutionDebuff":
				case "AcuityShear":
				case "AcuityDebuff":
				case "AcuityBuff": return 3;

				case "Confusion": return 5;

				case "CureMezz":
				case "Mesmerize": return 6;

				case "Bladeturn": return 9;

				case "DirectDamageWithDebuff":
				case "HeatResistDebuff":
				case "HeatResistBuff":
				case "SpeedOfTheRealm":
				case "PetSpeedEnhancement":
				case "SpeedEnhancement": return 10;

				case "CombatSpeedBuff": return 11;

				case "CureNearsight":
				case "Nearsight":
				case "ColdResistBuff":
				case "ColdResistDebuff": return 12;

				case "BodyResistDebuff":
				case "BodyResistBuff": return 16;

				case "EnergyResistDebuff":
				case "EnergyResistBuff": return 22;

				case "SpiritResistDebuff":
				case "SpiritResistBuff": return 17;

				case "MatterResistBuff":
				case "MatterResistDebuff": return 15;

				case "SummonAnimistFnF":
				case "SummonAnimistPet":
				case "SummonCommander":
				case "SummonMinion":
				case "SummonSimulacrum":
				case "SummonDruidPet":
				case "SummonHunterPet":
				case "SummonNecroPet":
				case "SummonUnderhill":
				case "SummonTheurgistPet": return 9915;

				case "DefensiveProc":
				case "OffensiveProc":
				case "OffensiveProcPvE":
					{
						if ((int)Value > 0)
						{
							client.Out.SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveAttachedSpell(client, (int)Value));
						}
						return (int)Value;
					}
				case "StyleBleeding": return 20;

				case "ArcheryDoT": return 8;
				case "ArrowDamageTypes": return 2;

				case "Charm": return GetCharmParm();
			}
			return 0;
		}

		/// <summary>
		/// Parm id specifically for charm type spells
		/// </summary>
		public int GetCharmParm()
		{
			switch (AmnesiaChance)
			{
				case 1: return 3; // humanoid
				case 2: return 5; // animal
				case 3: return 13; // insect
				case 4: return 50; // human animal
				case 5: return 51; // human animal insect
				case 6: return 52; // human animal insect magical
				case 7: return 53; // human animal insect magical undead
				case 8: return 6; // reptile
				default: return 54; // all
			}
		}

		public int GetDelveCastTimer()
		{
			switch (SpellType)
			{
				case "HereticDoTLostOnPulse":
				case "OffensiveProc": return 1;
			}
			if (CastTime == 2000)
			{
				return 1;
			}
			return CastTime - 2000;
		}

		public int GetDelveInstant()
		{
			switch (SpellType)
			{
				case "Heal":
				case "Charm":
				case "AblativeArmor":
				case "SubSpellHeal":
					if (IsInstantCast)
					{
						return 2;
					}
					return 0;

				case "StyleBleeding":
				case "StyleSpeedDecrease":
				case "StyleCombatSpeedDebuff": return 0;
			}
			return IsInstantCast ? 1 : 0;
		}

		public int GetDelveType1()
		{
			switch (SpellType)
			{
				case "DexterityDebuff": return 2;
				case "StyleBleeding":
				case "CurePoison":
				case "StrengthDebuff": return 1;

				case "CureDisease": return 18;
				case "Resurrect": return 65;

				case "StyleStun": return 22;

				case "CureNearsight":
				case "CureMezz":
				case "StyleCombatSpeedDebuff": return 8;
				case "StyleSpeedDecrease": return 39;
				case "AblativeArmor": return 43;

				case "Archery":
					if (Name.StartsWith("Critical Shot")) return 1752;
					else if (Name.StartsWith("Power Shot")) return 1032;
					else if (Name.StartsWith("Fire Shot") || Name.StartsWith("Cold Shot")) return 4;
					return 0;
			}
			return 0;

		}

		public int GetDelveFrequency()
		{
			if (Frequency != 0 || SpellType != "DamageOverTime")
			{
				return Frequency;
			}
			return 2490;
		}

		public string GetDelveNoCombat()
		{
			switch (SpellType)
			{
				case "SpeedOfTheRealm":
				case "SpeedEnhancement": return "\u0005";
				case "StyleStun": return " ";
			}
			return null;
		}

		public int GetDelveCostType()
		{
			switch (SpellType)
			{
				case "SiegeArrow":
				case "Archery": return 3;
			}
			return 0;
		}

		public int GetDelveIncreaseCap()
		{
			switch (SpellType)
			{
				case "HereticDoTLostOnPulse": return 150;
			}
			return 0;
		}

		#endregion
	}
	
}
