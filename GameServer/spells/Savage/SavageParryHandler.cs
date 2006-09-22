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
using DOL.GS.SkillHandler;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Summary description for SavageHasteHandler.
	/// </summary>
	[SpellHandlerAttribute("SavageParryBuff")]
	public class SavageParryHandler : ParryChanceBuff
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if(!SavageAbilities.SavagePreFireChecks((GamePlayer)m_caster))
				return false;

            if (!SavageAbilities.CanUseSavageAbility(((GamePlayer)m_caster), m_spell.Power))
			{
				MessageToCaster("You don't have enought health to cast this spell!", eChatType.CT_SpellResisted);
				return false;
			}

			return base.CheckBeginCast (selectedTarget);
		}

		public override int OnEffectExpires(DOL.GS.Effects.GameSpellEffect effect, bool noMessages)
		{
			SavageAbilities.ApplyHPPenalty((GamePlayer)m_caster, m_spell.Power);
			return base.OnEffectExpires (effect, noMessages);
		}
		
		// constructor
		public SavageParryHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		
	}
}
