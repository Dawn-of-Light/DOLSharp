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
        protected readonly int m_range;
        protected readonly int m_power;
        protected readonly int m_casttime;
        protected readonly bool m_minotaurspell;

        // warlocks

        // tooltip
        protected ushort m_tooltipId = 0;

        // params

        public Dictionary<string, List<string>> CustomParamsDictionary { get; set; }

        public bool IsPrimary { get; }

        public bool IsSecondary { get; }

        public bool InChamber { get; set; }

        public bool CostPower { get; set; } = true;

        public bool AllowBolt { get; }

        public int OverrideRange { get; set; }

        public ushort ClientEffect { get; }

        public string Description { get; } = string.Empty;

        public int SharedTimerGroup { get; }

        public string Target { get; } = string.Empty;

        public int Range
        {
            get
            {
                if (OverrideRange != 0 && m_range != 0)
                {
                    return OverrideRange;
                }

                return m_range;
            }
        }

        public int Power
        {
            get
            {
                if (!CostPower)
                {
                    return 0;
                }

                return m_power;
            }
        }

        public int CastTime
        {
            get
            {
                if (InChamber)
                {
                    return 0;
                }

                return m_casttime;
            }
        }

        public double Damage { get; set; }

        public eDamageType DamageType { get; } = eDamageType.Natural;

        public string SpellType { get; } = "-";

        public int Duration { get; set; }

        public int Frequency { get; }

        public int Pulse { get; }

        public int PulsePower { get; }

        public int Radius { get; }

        public int RecastDelay { get; }

        public int ResurrectHealth { get; }

        public int ResurrectMana { get; }

        public double Value { get; set; }

        public byte Concentration { get; }

        public int LifeDrainReturn { get; }

        public int AmnesiaChance { get; }

        public string Message1 { get; } = string.Empty;

        public string Message2 { get; } = string.Empty;

        public string Message3 { get; } = string.Empty;

        public string Message4 { get; } = string.Empty;

        public override eSkillPage SkillType => NeedInstrument ? eSkillPage.Songs : eSkillPage.Spells;

        public int InstrumentRequirement { get; }

        public int Group { get; }

        public int EffectGroup { get; }

        public int SubSpellId { get; }

        public bool MoveCast { get; }

        public bool Uninterruptible { get; }

        public bool IsFocus { get; }

        /// <summary>
        /// This spell can be sheared even if cast by self
        /// </summary>
        public bool IsShearable { get; set; } = false;

        /// <summary>
        /// Whether or not this spell is harmful.
        /// </summary>
        public bool IsHarmful
        {
            get
            {
                var target = Target.ToLower();
                return target == "enemy" || target == "area" || target == "cone";
            }
        }

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
            Description = dbspell.Description;
            Target = dbspell.Target;
            SpellType = dbspell.Type;
            m_range = dbspell.Range;
            Radius = dbspell.Radius;
            Value = dbspell.Value;
            Damage = dbspell.Damage;
            DamageType = (eDamageType)dbspell.DamageType;
            Concentration = (byte)dbspell.Concentration;
            Duration = dbspell.Duration * 1000;
            Frequency = dbspell.Frequency * 100;
            Pulse = dbspell.Pulse;
            PulsePower = dbspell.PulsePower;
            m_power = dbspell.Power;
            m_casttime = (int)(dbspell.CastTime * 1000);
            RecastDelay = dbspell.RecastDelay * 1000;
            ResurrectHealth = dbspell.ResurrectHealth;
            ResurrectMana = dbspell.ResurrectMana;
            LifeDrainReturn = dbspell.LifeDrainReturn;
            AmnesiaChance = dbspell.AmnesiaChance;
            Message1 = dbspell.Message1;
            Message2 = dbspell.Message2;
            Message3 = dbspell.Message3;
            Message4 = dbspell.Message4;
            ClientEffect = (ushort)dbspell.ClientEffect;
            InstrumentRequirement = dbspell.InstrumentRequirement;
            Group = dbspell.SpellGroup;
            EffectGroup = dbspell.EffectGroup;
            SubSpellId = dbspell.SubSpellID;
            MoveCast = dbspell.MoveCast;
            Uninterruptible = dbspell.Uninterruptible;
            IsFocus = dbspell.IsFocus;

            // warlocks
            IsPrimary = dbspell.IsPrimary;
            IsSecondary = dbspell.IsSecondary;
            AllowBolt = dbspell.AllowBolt;
            SharedTimerGroup = dbspell.SharedTimerGroup;
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
        public Spell(Spell spell, string spellType) : base(spell.Name, spell.ID, (ushort)spell.Icon, spell.Level, spell.InternalID)
        {
            Description = spell.Description;
            Target = spell.Target;
            SpellType = spellType; // replace SpellType
            m_range = spell.Range;
            Radius = spell.Radius;
            Value = spell.Value;
            Damage = spell.Damage;
            DamageType = spell.DamageType;
            Concentration = spell.Concentration;
            Duration = spell.Duration;
            Frequency = spell.Frequency;
            Pulse = spell.Pulse;
            PulsePower = spell.PulsePower;
            m_power = spell.Power;
            m_casttime = spell.CastTime;
            RecastDelay = spell.RecastDelay;
            ResurrectHealth = spell.ResurrectHealth;
            ResurrectMana = spell.ResurrectMana;
            LifeDrainReturn = spell.LifeDrainReturn;
            AmnesiaChance = spell.AmnesiaChance;
            Message1 = spell.Message1;
            Message2 = spell.Message2;
            Message3 = spell.Message3;
            Message4 = spell.Message4;
            ClientEffect = spell.ClientEffect;
            m_icon = spell.Icon;
            InstrumentRequirement = spell.InstrumentRequirement;
            Group = spell.Group;
            EffectGroup = spell.EffectGroup;
            SubSpellId = spell.SubSpellId;
            MoveCast = spell.MoveCast;
            Uninterruptible = spell.Uninterruptible;
            IsFocus = spell.IsFocus;
            IsPrimary = spell.IsPrimary;
            IsSecondary = spell.IsSecondary;
            AllowBolt = spell.AllowBolt;
            SharedTimerGroup = spell.SharedTimerGroup;
            m_minotaurspell = spell.m_minotaurspell;

            // Params
            CustomParamsDictionary = new Dictionary<string, List<string>>(spell.CustomParamsDictionary);
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
        public virtual void Delve(List<string> delve)
        {
            delve.Add($"Function: {Name}");
            delve.Add(string.Empty);
            delve.Add(Description);
            delve.Add(string.Empty);
            DelveEffect(delve);
            DelveTarget(delve);

            if (Range > 0)
            {
                delve.Add($"Range: {Range}");
            }

            if (Duration > 0 && Duration < 65535)
            {
                delve.Add($"Duration: {(Duration >= 60000 ? $"{Duration / 60000}:{Duration % 60000} min" : $"{Duration / 1000} sec")}");
            }

            delve.Add($"Casting time: {(CastTime == 0 ? "instant" : $"{CastTime} sec")}");

            if (Target.ToLower() == "enemy" || Target.ToLower() == "area" || Target.ToLower() == "cone")
            {
                delve.Add($"Damage: {GlobalConstants.DamageTypeToName(DamageType)}");
            }

            delve.Add(string.Empty);
        }

        private void DelveEffect(List<string> delve)
        {
        }

        private void DelveTarget(List<string> delve)
        {
            string target;
            switch (Target)
            {
                case "Enemy":
                    target = "Targetted";
                    break;
                default:
                    target = Target;
                    break;
            }

            delve.Add($"Target: {target}");
        }

        /// <summary>
        /// Whether or not the spell is instant cast.
        /// </summary>
        public bool IsInstantCast => CastTime <= 0;

        /// <summary>
        /// Wether or not the spell is Point Blank Area of Effect
        /// </summary>
        public bool IsPBAoE => Range == 0 && IsAoE;

        /// <summary>
        /// Wether or not this spell need Instrument (and is a Song)
        /// </summary>
        public bool NeedInstrument => InstrumentRequirement != 0;

        /// <summary>
        /// Wether or not this spell is an Area of Effect Spell
        /// </summary>
        public bool IsAoE => Radius > 0;

        /// <summary>
        /// Wether this spell Has valid SubSpell
        /// </summary>
        public bool HasSubSpell => SubSpellId > 0 || MultipleSubSpells.Count > 0;

        /// <summary>
        /// Wether this spell has a recast delay (cooldown)
        /// </summary>
        public bool HasRecastDelay => RecastDelay > 0;

        /// <summary>
        /// Wether this spell is concentration based
        /// </summary>
        public bool IsConcentration => Concentration > 0;

        /// <summary>
        /// Wether this spell has power usage.
        /// </summary>
        public bool UsePower => Power != 0;

        /// <summary>
        /// Wether this spell has pulse power usage.
        /// </summary>
        public bool UsePulsePower => PulsePower != 0;

        /// <summary>
        /// Wether this Spell is a pulsing spell (Song/Chant)
        /// </summary>
        public bool IsPulsing => Pulse != 0;

        /// <summary>
        /// Wether this Spell is a Song/Chant
        /// </summary>
        public bool IsChant => Pulse != 0 && !IsFocus;

        /// <summary>
        /// Wether this Spell is a Pulsing Effect (Dot/Hot...)
        /// </summary>
        public bool IsPulsingEffect => Frequency > 0 && !IsPulsing;

        public ushort InternalIconID => this.GetParamValue<ushort>("InternalIconID");

        public IList<int> MultipleSubSpells
        {
            get
            {
                return this.GetParamValues<int>("MultipleSubSpellID").Where(id => id > 0).ToList();
            }
        }

        public bool AllowCoexisting => this.GetParamValue<bool>("AllowCoexisting");
    }
}
