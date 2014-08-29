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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Based on HealSpellHandler.cs
	/// Spell calculates a percentage of the caster's health.
	/// Heals target for the full amount, Caster loses half that amount in health.
	/// </summary>
	[SpellHandlerAttribute("PetConversion")]
	public class PetConversionSpellHandler : SpellHandler
	{
		// constructor
		public PetConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}

		public override void FinishSpellCast(GameLiving target)
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
			base.FinishSpellCast(target);
		}
		
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			int mana = (int)(Spell.Value < 0 ? target.Health * Spell.Value * -0.01 : Spell.Value);
			int absorb = Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, mana);
			
			if (absorb > 0)
				MessageToCaster("You absorb " + absorb + " power points.", eChatType.CT_Spell);
			else
				MessageToCaster("Your mana is already full!", eChatType.CT_SpellResisted);
			
			target.Die(null);
		}
	}
}
