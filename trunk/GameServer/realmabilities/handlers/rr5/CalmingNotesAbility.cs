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

namespace DOL.GS.RealmAbilities
{
    public class CalmingNotesAbility : RR5RealmAbility
    {
        public CalmingNotesAbility(DBAbility dba, int level) : base(dba, level) { }
 
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            
			SpellLine spline = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
 			Spell abSpell = SkillBase.GetSpellByID(7045);
					 
 			if (spline != null && abSpell != null)
			{        
	            foreach (GameNPC enemy in living.GetNPCsInRadius(750))
	            {
	            	if (enemy.IsAlive && enemy.Brain!=null)
	            		if(enemy.Brain is IControlledBrain)
							living.CastSpell(abSpell, spline);
	            }
 			}
            DisableSkill(living);
        }
        public override int GetReUseDelay(int level)
        {
            return 300;
        }
        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("Insta-cast spell that mesmerizes all enemy pets within 750 radius for 30 seconds.");
            list.Add("");
            list.Add("Radius: 700");
            list.Add("Target: Pet");
            list.Add("Duration: 30 sec");
            list.Add("Casting time: instant");
        }


    }
}
