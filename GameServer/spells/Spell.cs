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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DOL.Database;

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
		// tooltip
		protected ushort m_tooltipId = 0;
		
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

        public ushort TooltipId
        {
        	get
        	{
        		if (m_tooltipId == 0)
        		{
        			TryGetNextFreeTooltipID(this);
        		}
        		
        		return m_tooltipId;
        	}
        	set
        	{
        		TryRegisterTooltipID(this, value);
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
            // tooltip
            TooltipId = dbspell.TooltipId;
		}

		/// <summary>
		/// Make a copy of a spell but change the spell type
		/// Usefull for customization of spells by providing custom spell handelers
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="spellType"></param>
		public Spell(Spell spell, string spellType) :
			base(spell.Name, spell.ID, (ushort)spell.Icon, spell.Level, spell.TooltipId)
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
        
        #region tooltip handling
        
        /// <summary>
        /// Collection to handle existing tooltip !
        /// </summary>
        public static Dictionary<ushort, WeakReference<Spell>> m_assignedTooltipID = new Dictionary<ushort, WeakReference<Spell>>(ushort.MaxValue+1);
        
        /// <summary>
		/// Destructor to free ID.
		/// </summary>
		~Spell()
		{
			TryFreeTooltipID(this);
		}

		public static void TryRegisterTooltipID(Spell sp, ushort tooltipID)
		{
			// 0 = Autoset !
			if (tooltipID == 0)
			{
				TryGetNextFreeTooltipID(sp);
				return;
			}
			
			lock (((ICollection)m_assignedTooltipID).SyncRoot)
			{
				if (m_assignedTooltipID.ContainsKey(tooltipID))
				{
					WeakReference<Spell> spref = m_assignedTooltipID[tooltipID];
					Spell outsp;
					if (spref != null && spref.TryGetTarget(out outsp))
					{
						// Raise an Exception, this Spell can't go in there !
						throw new ArgumentException("Tooltip ID (" + tooltipID + ") is incompatible for Spell : " + sp.ToString() + "\nAlreadyContaining Spell : " + outsp.ToString());
					}
					else
					{
						// Assign this spell.
						m_assignedTooltipID[tooltipID] = new WeakReference<Spell>(sp);
					}
					
					sp.m_tooltipId = tooltipID;
				}
				else
				{
					// Just create this ID.
					m_assignedTooltipID.Add(tooltipID, new WeakReference<Spell>(sp));
					sp.m_tooltipId = tooltipID;
				}
			}	
		}
		
		/// <summary>
		/// Find a free usable tooltipID in ushort list.
		/// </summary>
		/// <returns></returns>
		public static ushort TryGetNextFreeTooltipID(Spell sp)
		{
			// counter.
			int previous = 1;
			
			// lock collection
			lock (((ICollection)m_assignedTooltipID).SyncRoot)
			{
				
				foreach (KeyValuePair<ushort, WeakReference<Spell>> refsEntry in m_assignedTooltipID.OrderBy(ent => ent.Key))
				{
					// previous can feat there ?
					if (previous < refsEntry.Key)
					{
						break;
					}
					
					WeakReference<Spell> spref = refsEntry.Value;
					Spell outsp;
					// Try getting the value of this entry to be sure it's occupied
					if (spref == null || (spref.TryGetTarget(out outsp) == false))
					{
						// This slot is free in fact...
						previous = refsEntry.Key;
						break;
					}
					
					// inc
					previous = refsEntry.Key + 1;	
				}
				
				// Insert if we have a valid position.
				if (previous <= ushort.MaxValue)
				{
					if (m_assignedTooltipID.ContainsKey((ushort)previous))
					{
						m_assignedTooltipID[(ushort)previous] = new WeakReference<Spell>(sp);
					}
					else
					{
						m_assignedTooltipID.Add((ushort)previous, new WeakReference<Spell>(sp));
					}
					
					sp.m_tooltipId = (ushort)previous;
				}
				else
				{
					// Exception...
					throw new IndexOutOfRangeException("No more available ushort values for Spell TooltipID");
				}
				
			}
			
			// return id for property assignment
			return (ushort)previous;
		}
		
		/// <summary>
		/// Try freeing an occupied ID slot
		/// </summary>
		/// <param name="id"></param>
		public static void TryFreeTooltipID(Spell sp)
		{
			if (sp.m_tooltipId == 0)
				return;
			
			// Lock collection
			lock (((ICollection)m_assignedTooltipID).SyncRoot)
			{
				// Find the key
				if (m_assignedTooltipID.ContainsKey(sp.m_tooltipId))
				{
					// Find the Object
					WeakReference<Spell> spref = m_assignedTooltipID[sp.m_tooltipId];
					Spell outsp;
					if (spref != null && spref.TryGetTarget(out outsp))
					{
						if (sp == outsp)
						{
							// if it's the same object remove it from dictionary
							m_assignedTooltipID.Remove(sp.m_tooltipId);
						}
					}
					
					// if object is not found at given position it will be treated as empty anyway...
				}
			}
		}
		
		/// <summary>
		/// Returns spell with id, level of spell is always 1
		/// </summary>
		/// <param name="spellID"></param>
		/// <returns></returns>
		public static Spell GetSpellByTooltipID(ushort tooltipID)
		{
			Spell spell = null;
			
			if (tooltipID > 0)
			{
				lock (((ICollection)m_assignedTooltipID).SyncRoot)
				{
					if (m_assignedTooltipID.ContainsKey(tooltipID))
					{
						WeakReference<Spell> spref = m_assignedTooltipID[tooltipID];
						Spell outspl;
						if (spref != null && spref.TryGetTarget(out outspl))
						{
							spell = outspl;
						}
					}
				}
			}
			
			return spell;
		}
        #endregion
	}
	
}
