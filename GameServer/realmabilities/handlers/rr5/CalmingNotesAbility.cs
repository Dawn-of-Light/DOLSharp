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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Calming Notes (RR5 Minstrel) Realm Ability.
	/// </summary>
    public class CalmingNotesAbility : RR5RealmAbility
    {
    	// Spell and Spell Line used
    	private SpellLine m_spellLine;
    	private Spell m_spell;
    	
    	private const int THIS_SPELL_ID = 7045;
    	private const int THIS_LEVEL = 50;
    	
        public CalmingNotesAbility(DBAbility dba, int level) : base(dba, level) 
        {
        	m_spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
        	m_spell = SkillBase.GetSpellByID(THIS_SPELL_ID);
        	m_spell.Level = THIS_LEVEL;
        }
 
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            
			SpellLine spline = m_spellLine;
 			Spell abSpell = m_spell;
					 
 			if (spline != null && abSpell != null && living.GetSkillDisabledDuration(this) <= 0)
			{
 				living.CastSpell(abSpell, spline);
 				SendCasterSpellEffectAndCastMessage(living, m_spell.ClientEffect, true);
 				DisableSkill(living);
 			}
 			else if(living is GamePlayer) {
 				((GamePlayer)living).Out.SendMessage("The ability "+m_spell.Name+" is not ready yet !", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
 			}
        }
        
        public override int GetReUseDelay(int level)
        {
        	return (int)m_spell.RecastDelay/1000;
        }
        public override void AddEffectsInfo(IList<string> list)
        {
        	list.Add("Insta-cast spell that mesmerizes all enemies within "+m_spell.Radius+" radius for "+Math.Floor((double)m_spell.Duration/1000)+" seconds.");
            list.Add("");
            list.Add("Radius: "+m_spell.Radius);
            list.Add("Target: "+m_spell.Target);
            list.Add("Duration: "+Math.Floor((double)m_spell.Duration/1000)+" sec");
            list.Add("Casting time: "+(m_spell.CastTime > 0 ? m_spell.CastTime+" sec" : "instant"));
        }


    }
}
