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
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Damages the target and lowers their resistance to the spell's type.
	/// </summary>
	[SpellHandler("DirectDamageWithDebuff")]
	public class DirectDamageDebuffSpellHandler : DirectDamageSpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		private AbstractResistDebuff m_debuffSpellHandler;

		public virtual eProperty Property { get { return Caster.GetResistTypeForDamage(Spell.DamageType); } }
		public virtual string DebuffTypeName { get { return GlobalConstants.DamageTypeToName(Spell.DamageType); } }

		/// <summary>
		/// DD with Debuff must do damage before debuff !
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void CheckedApplyEffectOnTarget(DOL.GS.GameLiving target, double effectiveness)
		{
			// Damage
			OnDirectEffect(target, effectiveness);
			
			// Base
			base.CheckedApplyEffectOnTarget(target, effectiveness);
		}
		
		/// <summary>
		/// Direct Damage with Debuff having duration will start with onEffectStart
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			// Debuff - do not apply debuff to keep components or doors
			if ((effect.Owner is Keeps.GameKeepComponent) == false && (effect.Owner is Keeps.GameKeepDoor) == false)
			{
				m_debuffSpellHandler.OnEffectStart(effect);
			}
			
			base.OnEffectStart(effect);
		}
		
		/// <summary>
		/// Direct Damage with Debuff, effect expires should remove debuff
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			// Debuff ends - do not apply debuff to keep components or doors
			if ((effect.Owner is Keeps.GameKeepComponent) == false && (effect.Owner is Keeps.GameKeepDoor) == false)
			{
				m_debuffSpellHandler.OnEffectExpires(effect, noMessages);
			}
			
			return base.OnEffectExpires(effect, noMessages);
		}
		
		/// <summary>
		/// Get Debuff Duration.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return m_debuffSpellHandler.CalculateEffectDuration(target, effectiveness);
		}

		/// <summary>
		/// Effectiveness should be based on debuff effectiveness.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			// Get Effectiveness from resist debuff...
			GameSpellEffect effect = m_debuffSpellHandler.CreateSpellEffect(target, effectiveness);
			
			return base.CreateSpellEffect(target, effect.Effectiveness);
		}

		/// <summary>
		/// Debuff Effect should be overwritable by resist debuff
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (!base.IsOverwritable(compare))
			{
				if (!compare.SpellHandler.GetType().IsInstanceOfType(m_debuffSpellHandler) && !m_debuffSpellHandler.GetType().IsInstanceOfType(compare.SpellHandler))
					return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Use Debuff Spell Handler Comparison
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <param name="neweffect"></param>
		/// <returns></returns>
		public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			return m_debuffSpellHandler.IsNewEffectBetter(oldeffect, neweffect);
		}
		
		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				/*
				<Begin Info: Lesser Raven Bolt>
				Function: dmg w/resist decrease
 
				Damages the target, and lowers the target's resistance to that spell type.
 
				Damage: 32
				Resist decrease (Cold): 10%
				Target: Targetted
				Range: 1500
				Duration: 1:0 min
				Power cost: 5
				Casting time:      3.0 sec
				Damage: Cold
 
				<End Info>
				*/

				var list = new List<string>();

                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DirectDamageDebuffSpellHandler.DelveInfo.Function"));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
                if (Spell.Damage != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
                if (Spell.Value != 0)
                    list.Add(String.Format(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DirectDamageDebuffSpellHandler.DelveInfo.Decrease", DebuffTypeName, Spell.Value)));
                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Target", Spell.Target));
                if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Range", Spell.Range));
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Duration") + " Permanent.");
                else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min"));
                else if (Spell.Duration != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Duration") + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                if (Spell.Frequency != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
                if (Spell.Power != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
				if(Spell.RecastDelay > 60000)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.RecastTime") + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if(Spell.RecastDelay > 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.RecastTime") + (Spell.RecastDelay/1000).ToString() + " sec");
   				if(Spell.Concentration != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.ConcentrationCost", Spell.Concentration));
				if(Spell.Radius != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Radius", Spell.Radius));
				if(Spell.DamageType != eDamageType.Natural)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));

				return list;
			}
		}

		// constructor
		public DirectDamageDebuffSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			// Get a resist Debuff spell for composite
			switch (Property)
			{
				case eProperty.Resist_Body:
					m_debuffSpellHandler = new BodyResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Cold:
					m_debuffSpellHandler = new ColdResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Crush:
					m_debuffSpellHandler = new CrushResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Energy:
					m_debuffSpellHandler = new EnergyResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Heat:
					m_debuffSpellHandler = new HeatResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Matter:
					m_debuffSpellHandler = new MatterResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Slash:
					m_debuffSpellHandler = new SlashResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Spirit:
					m_debuffSpellHandler = new SpiritResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Thrust:
					m_debuffSpellHandler = new ThrustResistDebuff(caster, spell, line);
					break;
				case eProperty.Resist_Natural:
				default:
					m_debuffSpellHandler = new EssenceResistDebuff(caster, spell, line);
					break;
			}
			
		}
	}
}
