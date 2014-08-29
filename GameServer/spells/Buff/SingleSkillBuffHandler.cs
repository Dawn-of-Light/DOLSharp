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

namespace DOL.GS.Spells
{
	/// <summary>
	/// Abstract class for Skill Buff with Skill Update.
	/// </summary>
	public abstract class SingleSkillBuffHandler : SingleStatBuff
	{
		/// <summary>
		/// Update The Skill values.
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			if(target is GamePlayer)
			{
				((GamePlayer)target).Out.SendUpdatePlayerSkills();
			}
		}
		
    	// constructor
        public SingleSkillBuffHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
	}
	
	
	/// <summary>
    /// Stealth Skill buff
    /// </summary>
    [SpellHandlerAttribute("StealthSkillBuff")]
    public class StealthSkillBuff : SingleSkillBuffHandler
    {
        public override eProperty Property1 { get { return eProperty.Skill_Stealth; } }

        // constructor
        public StealthSkillBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// Flexible skill buff
    /// </summary>
    [SpellHandlerAttribute("FelxibleSkillBuff")]
    public class FelxibleSkillBuff : SingleSkillBuffHandler
    {
        public override eProperty Property1 { get { return eProperty.Skill_Flexible_Weapon; } }
        
        // constructor
        public FelxibleSkillBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
}
