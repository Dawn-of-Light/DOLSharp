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
using DOL.Database;
using System.Collections.Generic;

namespace DOL.GS
{
	/// <summary>
	/// 
	/// </summary>
	public class Spell : Skill
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
			get { return eSkillPage.Spells; }
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
			: base(dbspell.Name, (ushort)dbspell.SpellID, (ushort)dbspell.Icon, requiredLevel)
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
			m_icon = (ushort)dbspell.Icon;
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
		}

		/// <summary>
		/// Make a copy of a spell but change the spell type
		/// Usefull for customization of spells by providing custom spell handelers
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="spellType"></param>
		public Spell(Spell spell, string spellType) :
			base(spell.Name, (ushort)spell.ID, (ushort)spell.Icon, spell.Level)
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



		#region delvespell methods

		public string GetDelveFunction()
		{
			switch (SpellType)
			{
				case "Charm": return "charm";
				case "CureMezz": return "remove_eff";
				case "Lifedrain": return "lifedrain";
				case "ArmorFactorBuff": return "shield";
				case "ArmorAbsorptionBuff": return "absorb";

				case "DamageSpeedDecrease":
				case "SpeedDecrease": return "snare";

				case "Amnesia": return "amnesia";

				case "QuicknessDebuff":
				case "ConstitutionDebuff":
				case "StrengthDebuff":
				case "DexterityDebuff": return "nstat";

				case "QuicknessBuff":
				case "ConstitutionBuff":
				case "StrengthBuff":
				case "DexterityBuff": return "stat";

				case "DamageOverTime": return "dot";

				case "Confusion":
				case "Mesmerize":
				case "SpeedEnhancement":
				case "SpeedOfTheRealm":
				case "CombatSpeedBuff":
				case "Bladeturn": return "combat";

				case "DirectDamage": return "direct";

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

				case "BodyResistBuff":
				case "ColdResistBuff":
				case "EnergyResistBuff":
				case "HeatResistBuff":
				case "MatterResistBuff":
				case "SpiritResistBuff":
				case "SlashResistBuff":
				case "ThrustResistBuff":
				case "CrushResistBuff": return "resistance";

				case "EnduranceRegenBuff":
				case "PowerRegenBuff": return "enhancement";

				case "MezDampening": return "mez_dampen";
				case "Heal": return "heal";
				case "Resurrect": return "raise_dead";
				case "DamageAdd": return "dmg_add";

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
				case "OffensiveProc": return "off_proc";

				case "Stun": return "paralyze";
				case "HealOverTime": return "regen";
				case "DamageShield": return "dmg_shield";
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
				case "MezDampening": return Target == "Self" ? 4 : 0;
				case "DamageAdd":
				case "PowerRegenBuff":
				case "ArmorAbsorptionBuff":
				case "BodyResistBuff":
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
				case "Archery":
					if (Name.StartsWith("Power Shot")) return 1088;
					return 1024;
				case "ArcheryDoT": return 1;
			}
			return 0;
		}

		public int GetDelvePowerLevel(int level)
		{
			switch (SpellType)
			{
				case "Confusion": return (int)Value;

				case "SummonAnimistFnF":
				case "SummonAnimistPet":
				case "SummonCommander":
				case "SummonMinion":
				case "SummonSimulacrum":
				case "SummonDruidPet":
				case "SummonHunterPet":
				case "SummonNecroPet":
				case "SummonTheurgistPet":
				case "DamageOverTime":
				case "Charm": return -(int)Damage;

				case "CombatSpeedBuff": return -(int)(Value * 2);

				case "StyleBleeding": return (int)Damage;
				case "StyleSpeedDecrease": return (int)(100 - Value);
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

				default: return 0;
			}
		}

		public int GetDelvePowerCost()
		{
			switch(SpellType)
			{
				case "SiegeArrow":
				case "ArrowDamageTypes":
				case "Archery": return -Power;
			}
			return Power;
		}

		public int GetDelveLinkEffect()
		{
			switch (SpellType)
			{
				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear": return 7312;
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
			if (Duration > 0) return 2;
			if (Concentration > 0) return 4;
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
				case eDamageType.Energy: return 22;
			}
			return 0;
		}

		public int GetDelveBonus()
		{
			switch (SpellType)
			{
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
				case "ArmorAbsorptionDebuff":
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

				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear":

				case "EssenceSear":

				case "MezDampening":
				case "ArmorFactorBuff": return (int)Value;

				case "StyleSpeedDecrease": return (int)(100 - Value);

				case "SiegeArrow":
				case "ArrowDamageTypes":
				case "Archery": return 20;
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
				case "DamageShield":
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
				case "DamageOverTime": return (int)Damage;

				case "StrengthShear":
				case "DexterityShear":
				case "ConstitutionShear":
				case "AcuityShear":
				case "StrengthConstitutionShear":
				case "DexterityQuicknessShear": return 2;

				case "SpreadHeal":
				case "Heal":
				case "Charm":
				case "EnduranceRegenBuff":
				case "HealOverTime":
				case "PowerRegenBuff": return (int)Value;
				case "Resurrect": return ResurrectHealth;

				case "StyleBleeding": return (int)Damage;

				case "SiegeArrow":
				case "Archery":
					return (int)(Damage * 10);
			}
			return 0;
		}

		public int GetDelveParm()
		{
			switch (SpellType)
			{
				case "DamageAdd":
				case "ArmorAbsorptionDebuff":
				case "ArmorAbsorptionBuff":

				case "StrengthShear":
				case "StrengthDebuff":
				case "StrengthBuff":
				case "StrengthConstitutionShear":
				case "StrengthConstitutionDebuff":
				case "StrengthConstitutionBuff":

				case "Stun":
				case "DamageOverTime":
				case "SpeedDecrease":
				case "DamageSpeedDecrease":
				case "DirectDamage":
				case "HealOverTime":
				case "DamageShield":
				case "Lifedrain": return 1;

				case "PowerRegenBuff":
				case "DexterityShear":
				case "DexterityDebuff":
				case "DexterityBuff":
				case "DexterityQuicknessShear":
				case "DexterityQuicknessDebuff":
				case "DexterityQuicknessBuff":
				case "ArmorFactorDebuff":
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

				case "HeatResistDebuff":
				case "SpeedOfTheRealm":
				case "SpeedEnhancement": return 10;

				case "CombatSpeedBuff": return 11;

				case "ColdResistDebuff": return 12;
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

				case "StyleStun": return 1;
				case "StyleBleeding": return 20;
				case "StyleSpeedDecrease": return 1;
				case "StyleCombatSpeedDebuff": return 2;

				case "ArcheryDoT": return 8;
				case "ArrowDamageTypes": return 2;
			}
			return 0;
		}

		public int GetDelveCastTimer()
		{
			switch (SpellType)
			{
				case "HereticDoTLostOnPulse":
				case "OffensiveProc": return 1;
			}
			if (CastTime == 2000) return 1;
			return CastTime - 2000;
		}

		public int GetDelveInstant()
		{
			switch (SpellType)
			{
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
				case "CurePoison":
				case "StrengthDebuff": return 1;
				case "CureMezz": return 8;
				case "CureDisease": return 18;
				case "Resurrect": return 65;

				case "StyleStun": return 22;
				case "StyleBleeding": return 1;
				case "StyleCombatSpeedDebuff": return 8;
				case "StyleSpeedDecrease": return 39;

				case "Archery":
					if (Name.StartsWith("Critical Shot")) return 1752;
					else if (Name.StartsWith("Power Shot")) return 1032;
					else if (Name.StartsWith("Fire Shot") || Name.StartsWith("Cold Shot")) return 4;
					return 0;
			}
			return 0;
		}

		//sorry i need this on Eden
		public int GetDelveFrequency()
		{
			if (Frequency != 0 || SpellType != "DamageOverTime") return Frequency;
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


		/*public string DelveTooltip
		{
			get
			{
				switch (SpellType)
				{
					case "Lifedrain":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(bonus \"{3}\")(cast_timer \"{4}\")(damage \"{5}\")(damage_type \"{6}\")(level \"{7}\")(parm \"1\")(power_cost \"{8}\")(power_level \"{9}\")(range \"{10}\")(target \"{11}\"))",
							"lifedrain", ID, Name, LifeDrainReturn / 10, CastTime - 2000, Damage * 10, GetDelveDamageType(), Level, Power, Level, Range, GetDelveTargetType());
					case "ArmorFactorBuff":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(ability \"4\")(bonus \"{3}\")(cast_timer \"{4}\")(damage_type \"{5}\")(dur_type \"{6}\")(duration \"{7}\")(level \"{8}\")(no_interrupt \"{9}\")(parm \"2\")(power_cost \"{10}\")(power_level \"{11}\"))",
							"shield", ID, Name, Value, CastTime - 2000, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, "\u0001", Power, Level);
					case "SpeedEnhancement":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(bonus \"{3}\")(cast_timer \"{4}\")(damage_type \"{5}\")(dur_type \"{6}\")(duration \"{7}\")(frequency \"{8}\")(level \"{9}\")(no_combat \"{10}\")(parm \"10\")(power_level \"{11}\")(range \"{12}\")(target \"{13}\"))",
							"combat", ID, Name, Value, CastTime - 2000, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Frequency, Level, "\u0005", Level, Range, GetDelveTargetType());
					case "ArmorAbsorptionBuff":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(ability \"4\")(bonus \"{3}\")(cast_timer \"{4}\")(damage_type \"{5}\")(dur_type \"{6}\")(duration \"{7}\")(level \"{8}\")(no_interrupt \"{9}\")(parm \"1\")(power_cost \"{10}\")(power_level \"{11}\"))",
							"absorb", ID, Name, Value, CastTime - 2000, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, "\u0001", Power, Level);
					case "Bladeturn":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(cast_timer \"{3}\")(damage \"{4}\")(damage_type \"{5}\")(dur_type \"{6}\")(duration \"{7}\")(level \"{8}\")(parm \"9\")(power_cost \"{9}\")(power_level \"{10}\"))",
							"combat", ID, Name, CastTime - 2000, "51", GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, Power, Level);
					case "DamageOverTime":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(cast_timer \"{3}\")(damage \"{4}\")(damage_type \"{5}\")(dur_type \"{6}\")(duration \"{7}\")(frequency \"{8}\")(level \"{9}\")(parm \"1\")(power_cost \"{10}\")(power_level \"{11}\")(range \"{12}\")(target \"{13}\"))",
							"dot", ID, Name, CastTime - 2000, Damage, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Frequency, Level, Power, Level, Range, GetDelveTargetType());
					case "Mesmerize":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(cast_timer \"{3}\")(damage_type \"{4}\")(dur_type \"{5}\")(duration \"{6}\")(level \"{7}\")(parm \"6\")(power_cost \"{8}\")(power_level \"{9}\")(range \"{10}\")(target \"{11}\"))",
							"combat", ID, Name, CastTime - 2000, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, Power, Level, Range, GetDelveTargetType());
					case "SpeedDecrease":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(bonus \"{3}\")(cast_timer \"{4}\")(damage_type \"{5}\")(dur_type \"{6}\")(duration \"{7}\")(level \"{8}\")(parm \"1\")(power_cost \"{9}\")(power_level \"{10}\")(range \"{11}\")(target \"{12}\"))",
							"snare", ID, Name, Value - 1, CastTime - 2000, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, Power, Level, Range, GetDelveTargetType());
					case "Amnesia":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(bonus \"{3}\")(cast_timer \"{4}\")(damage_type \"{5}\")(level \"{6}\")(power_cost \"{7}\")(power_level \"{8}\")(range \"{9}\")(target \"{10}\"))",
							"amnesia", ID, Name, AmnesiaChance, CastTime - 2000, GetDelveDamageType(), Level, Power, Level, Range, GetDelveTargetType());
					case "Confusion":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(damage_type \"{3}\")(dur_type \"{4}\")(duration \"{5}\")(level \"{6}\")(parm \"5\")(power_cost \"{7}\")(power_level \"{8}\")(range \"{9}\")(target \"{10}\"))",
							"combat", ID, Name, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, Power, Level, Range, GetDelveTargetType());
					case "StrengthDebuff":
					case "DexterityDebuff":
						return string.Format("(Spell (Function \"{0}\")(Index \"{1}\")(Name \"{2}\")(bonus \"{3}\")(damage_type \"{4}\")(dur_type \"{5}\")(duration \"{6}\")(instant \"{7}\")(level \"8\")(parm \"2\")(power_cost \"5\")(power_level \"8\")(range \"1500\")(target \"1\")(timer_value \"5\")(type1 \"2\")(use_timer \"1\"))",
							"nstat", ID, Name, GetDelveDamageType(), GetDelveDurationType(), Duration / 1000, Level, Power, Level, Range, GetDelveTargetType());
				}
				return string.Format("(Spell (Name \"(not found)\")(Index \"{0}\"))", ID);
			}
		}*/

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
	}
}
