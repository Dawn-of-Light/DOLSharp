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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	


	/// <summary>
	/// Buffs multiple spell buff
	/// </summary>	
	[SpellHandlerAttribute("HeroismBuff")]
	public class HeroismBuff : SpellHandler
	{
		public const int POTION_STR_BUFF = 1002000;
		public const int POTION_CON_BUFF = 1002001;
		public const int POTION_DEX_BUFF = 1002002;
		public const int POTION_STRCON_BUFF = 1002003;
		public const int POTION_DEXQUI_BUFF = 1002004;
		public const int POTION_ACU_BUFF = 1002005;
		// constructor
		public HeroismBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		
		public override bool StartSpell(GameLiving target)
        {
			if(!base.StartSpell(target))
				return false;
			
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_STR_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_CON_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_DEX_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_STRCON_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_DEXQUI_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_ACU_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			return true;
		}
	}
	
	
	/// <summary>
	/// Buffs multiple spell buff
	/// </summary>	
	[SpellHandlerAttribute("CourageBuff")]
	public class CourageBuff : SpellHandler
	{
		public const int POTION_STR_BUFF = 1002000;
		public const int POTION_CON_BUFF = 1002001;
		public const int POTION_DEX_BUFF = 1002002;
		
		// constructor
		public CourageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		
		public override bool StartSpell(GameLiving target)
        {
			if(!base.StartSpell(target))
				return false;
			
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_STR_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_CON_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_DEX_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			return true;
		}
	}
	
	
	/// <summary>
	/// Buffs multiple spell buff
	/// </summary>	
	[SpellHandlerAttribute("SuperiorCourageBuff")]
	public class SuperiorCourageBuff : SpellHandler
	{
		public const int POTION_STRCON_BUFF = 1002003;
		public const int POTION_DEXQUI_BUFF = 1002004;
		public const int POTION_ACU_BUFF = 1002005;
		// constructor
		public SuperiorCourageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		
		public override bool StartSpell(GameLiving target)
         {
			if(!base.StartSpell(target))
				return false;
			
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_STRCON_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_DEXQUI_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
			
			spellhandler = ScriptMgr.CreateSpellHandler(Caster, SkillBase.GetSpellByID(POTION_ACU_BUFF), SkillBase.GetSpellLine("Potions"));
			spellhandler.StartSpell(target);
		
			return true;			
		}
	}
}
