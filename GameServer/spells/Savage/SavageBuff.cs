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
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Spells
{
	
    // Main class for savage buffs
	public abstract class AbstractSavageBuff : PropertyChangingSpell
	{
		public override int BonusCategory1 { get { return 1; } }
	
		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			SendUpdates(effect.Owner);
		}
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                //list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
                //list.Add(" "); //empty line
                list.Add(Spell.Description);
                list.Add(" "); //empty line
                if (Spell.InstrumentRequirement != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
                if (Spell.Damage != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
                if (Spell.LifeDrainReturn != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.HealthReturned", Spell.LifeDrainReturn));
                else if (Spell.Value != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")));
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));
                if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " Permanent.");
                else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min"));
                else if (Spell.Duration != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                if (Spell.Frequency != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
                if (Spell.Power != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.HealthCost", Spell.Power.ToString("0;0'%'")));
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
                if (Spell.RecastDelay > 60000)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + (Spell.RecastDelay / 1000).ToString() + " sec");
                if (Spell.Concentration != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.ConcentrationCost", Spell.Concentration));
                if (Spell.Radius != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Radius", Spell.Radius));
                if (Spell.DamageType != eDamageType.Natural)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
                if (Spell.IsFocus)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Focus"));

                return list;
            }
        }

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			
			if (m_spell.Power != 0)
			{
				int cost = 0;
				if (m_spell.Power < 0)
					cost = (int)(m_caster.MaxHealth * Math.Abs(m_spell.Power) * 0.01);
				else
					cost = m_spell.Power;
				if (effect.Owner.Health > cost)
					effect.Owner.ChangeHealth(effect.Owner, GameLiving.eHealthChangeType.Spell, -cost);
			}
			SendUpdates(effect.Owner);
			return 0;
		}

		// constructor
		public AbstractSavageBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}		
	}
	
	public abstract class AbstractSavageStatBuff : AbstractSavageBuff
	{
		/// <summary>
        /// Sends needed updates on start/stop
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				player.Out.SendCharStatsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
			}
		}
		// constructor
		public AbstractSavageStatBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}				
	}
	public abstract class AbstractSavageResistBuff : AbstractSavageBuff
	{
		/// <summary>
        /// Sends needed updates on start/stop
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				player.Out.SendCharResistsUpdate();
				player.UpdatePlayerStatus();
			}
		}
		// constructor
		public AbstractSavageResistBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}				
	}
	
	[SpellHandlerAttribute("SavageParryBuff")]
	public class SavageParryBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.ParryChance; } }

		// constructor
		public SavageParryBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageEvadeBuff")]
	public class SavageEvadeBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.EvadeChance; } }

		// constructor
		public SavageEvadeBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageCombatSpeedBuff")]
	public class SavageCombatSpeedBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeSpeed; } }

		// constructor
		public SavageCombatSpeedBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageDPSBuff")]
	public class SavageDPSBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeDamage; } }

		// constructor
		public SavageDPSBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}	
	[SpellHandlerAttribute("SavageSlashResistanceBuff")]
	public class SavageSlashResistanceBuff : AbstractSavageResistBuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Slash; } }

		// constructor
		public SavageSlashResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageThrustResistanceBuff")]
	public class SavageThrustResistanceBuff : AbstractSavageResistBuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Thrust; } }

		// constructor
		public SavageThrustResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageCrushResistanceBuff")]
	public class SavageCrushResistanceBuff : AbstractSavageResistBuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }

		// constructor
		public SavageCrushResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}


