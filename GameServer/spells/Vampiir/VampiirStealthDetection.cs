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

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirStealthDetection")]
	public class VampiirStealthDetection : SpellHandler
	{
		public VampiirStealthDetection(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{

			effect.Owner.BaseBuffBonusCategory[(int)eProperty.Skill_Stealth]+=(int)m_spell.Value;
			base.OnEffectStart(effect);
	//		effect.Owner.BuffBonusCategory1[(int)eProperty.StealthRange] += (int)m_spell.Value;
		}


		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
	//		effect.Owner.BuffBonusCategory1[(int)eProperty.StealthRange] -= (int)m_spell.Value;
			effect.Owner.BaseBuffBonusCategory[(int)eProperty.Skill_Stealth]-=(int)m_spell.Value;
			return base.OnEffectExpires(effect, noMessages);
		}


	}
}























