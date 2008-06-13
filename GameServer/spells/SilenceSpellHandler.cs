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
	/// Reduce range needed to cast the sepll
	/// </summary>
	[SpellHandler("Silence")]
	public class SilenceSpellHandler : SpellHandler
	{
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect effect;
            effect = SpellHandler.FindEffectOnTarget(target, "Silence");
			if(effect!=null)
            {
				MessageToCaster("Your target already have an effect of that type!", eChatType.CT_SpellResisted);
				return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
		
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if(effect.Owner is GamePlayer)
			{
				(effect.Owner as GamePlayer).IsSilenced=true;
				effect.Owner.StopCurrentSpellcast();
				effect.Owner.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			}
		}
		
		public override int OnEffectExpires(GameSpellEffect effect,bool noMessages)
		{
			if(effect.Owner is GamePlayer)
			{
				(effect.Owner as GamePlayer).IsSilenced=false;
			}	
			return base.OnEffectExpires(effect,noMessages);
		}

		// constructor
		public SilenceSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}
