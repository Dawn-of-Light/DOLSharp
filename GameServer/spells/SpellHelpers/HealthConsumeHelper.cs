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
using System.Collections.Generic;

using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	/// HealthConsumeHelper has specific function to consume Health instead of Power
	/// Used for Savage Spell Handler.
	/// </summary>
	public class HealthConsumeHelper
	{
		private ISpellHandler m_spellHandler;
		
		public HealthConsumeHelper(ISpellHandler handler)
		{
			m_spellHandler = handler;
		}
		
		public virtual void ConsumeHealth(GameLiving target)
		{
			int cost = HealthCost(target);
			
			if(cost > 0 && target.Health > cost)
			{
				target.ChangeHealth(target, GameLiving.eHealthChangeType.Spell, -1 * cost);
			}
		}
		
		public virtual int HealthCost(GameLiving target)
		{
			if (m_spellHandler.Spell.Power != 0)
			{
				int cost = 0;
				if (m_spellHandler.Spell.Power < 0)
				{
					cost = (int)(target.MaxHealth * m_spellHandler.Spell.Power * -0.01);
				}
				else
				{
					cost = m_spellHandler.Spell.Power;
				}
				
				return cost;
			}
			
			return 0;
		}
		
		public virtual IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>(16);
                //list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
                //list.Add(" "); //empty line
                list.Add(m_spellHandler.Spell.Description);
                list.Add(" "); //empty line
                if (m_spellHandler.Spell.InstrumentRequirement != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(m_spellHandler.Spell.InstrumentRequirement)));
                if (m_spellHandler.Spell.Damage != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Damage", m_spellHandler.Spell.Damage.ToString("0.###;0.###'%'")));
                if (m_spellHandler.Spell.LifeDrainReturn != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.HealthReturned", m_spellHandler.Spell.LifeDrainReturn));
                else if (m_spellHandler.Spell.Value != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Value", m_spellHandler.Spell.Value.ToString("0.###;0.###'%'")));
                list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Target", m_spellHandler.Spell.Target));
                if (m_spellHandler.Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Range", m_spellHandler.Spell.Range));
                if (m_spellHandler.Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Duration") + " Permanent.");
                else if (m_spellHandler.Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Duration") + m_spellHandler.Spell.Duration / 60000 + ":" + (m_spellHandler.Spell.Duration % 60000 / 1000).ToString("00") + " min"));
                else if (m_spellHandler.Spell.Duration != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Duration") + (m_spellHandler.Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                if (m_spellHandler.Spell.Frequency != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Frequency", (m_spellHandler.Spell.Frequency * 0.001).ToString("0.0")));
                if (m_spellHandler.Spell.Power != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.HealthCost", m_spellHandler.Spell.Power.ToString("0;0'%'")));
                list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.CastingTime", (m_spellHandler.Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
                if (m_spellHandler.Spell.RecastDelay > 60000)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.RecastTime") + (m_spellHandler.Spell.RecastDelay / 60000).ToString() + ":" + (m_spellHandler.Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (m_spellHandler.Spell.RecastDelay > 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.RecastTime") + (m_spellHandler.Spell.RecastDelay / 1000).ToString() + " sec");
                if (m_spellHandler.Spell.Concentration != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.ConcentrationCost", m_spellHandler.Spell.Concentration));
                if (m_spellHandler.Spell.Radius != 0)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Radius", m_spellHandler.Spell.Radius));
                if (m_spellHandler.Spell.DamageType != eDamageType.Natural)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(m_spellHandler.Spell.DamageType)));
                if (m_spellHandler.Spell.IsFocus)
                    list.Add(LanguageMgr.GetTranslation((m_spellHandler.Caster as GamePlayer).Client, "DelveInfo.Focus"));

                return list;
			}
		}
	}
}
