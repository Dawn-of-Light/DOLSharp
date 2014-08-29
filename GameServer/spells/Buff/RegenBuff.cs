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
using System.Collections.Specialized;
using DOL.GS.Effects;
using DOL.GS.PropertyCalc;
using System.Reflection;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Health regeneration rate buff
	/// </summary>
	[SpellHandler("HealthRegenBuff")]
	public class HealthRegenSpellHandler : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.HealthRegenerationRate; } }

		public HealthRegenSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}

	/// <summary>
	/// Power regeneration rate buff
	/// </summary>
	[SpellHandler("PowerRegenBuff")]
	public class PowerRegenSpellHandler : SingleStatBuff
	{
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
            if (target is GamePlayer && (((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.Vampiir
                || ((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.MaulerAlb
                || ((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.MaulerMid
                || ((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.MaulerHib))
			{
				MessageToCaster("This spell has no effect on this class!", eChatType.CT_Spell);
				return;
			}
			
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		public override eProperty Property1 { get { return eProperty.PowerRegenerationRate; } }

		public PowerRegenSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}
	}

	/// <summary>
	/// Endurance regeneration rate buff
	/// </summary>
	[SpellHandler("EnduranceRegenBuff")]
	public class EnduranceRegenSpellHandler : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.EnduranceRegenerationRate; } }

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		/// <param name="target">The current target object</param>
		public override bool StartSpell(GameLiving target)
		{
			// paladin chants seem special
			if (SpellLine.Spec == Specs.Chants)
				SendEffectAnimation(Caster, 0, true, 1);

			return base.StartSpell(target);
		}

		/// <summary>
		/// Constructs a new EnduranceRegenSpellHandler
		/// </summary>
		/// <param name="caster">The spell caster</param>
		/// <param name="spell">The spell used</param>
		/// <param name="line">The spell line used</param>
		public EnduranceRegenSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
