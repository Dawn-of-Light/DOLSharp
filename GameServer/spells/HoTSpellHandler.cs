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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Heal Over Time spell handler
	/// </summary>
	[SpellHandlerAttribute("HealOverTime")]
	public class HoTSpellHandler : SpellHandler
	{
		/// <summary>
		/// Execute heal over time spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.ChangeMana(null, GameLiving.eManaChangeType.Spell, -CalculateNeededPower(target));
			base.FinishSpellCast(target);
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// TODO: correct formula
			double eff = 1.25;
			if(Caster is GamePlayer)
			{
				double lineSpec = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
				if (lineSpec < 1)
					lineSpec = 1;
				eff = 0.75;
				if (Spell.Level > 0)
				{
					eff += (lineSpec-1.0)/Spell.Level*0.5;
					if (eff > 1.25)
						eff = 1.25;
				}
			}
			base.ApplyEffectOnTarget(target, eff);
		}

		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, Spell.Duration, Spell.Frequency, effectiveness);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{			
			SendEffectAnimation(effect.Owner, 0, false, 1);
			//"{0} seems calm and healthy."
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_Spell, effect.Owner);
		}

		public override void OnEffectPulse(GameSpellEffect effect)
		{
			base.OnEffectPulse(effect);
			OnDirectEffect(effect.Owner, effect.Effectiveness);
		}

		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target.ObjectState != GameObject.eObjectState.Active) return;
			if (target.Alive == false) return;

			base.OnDirectEffect(target, effectiveness);
			double heal = Spell.Value * effectiveness;
			if (target.IsDiseased)
				heal /= 2;
			target.ChangeHealth(target, GameLiving.eHealthChangeType.Regenerate , (int)heal); // Regenerate don't generate aggro
			//"You feel calm and healthy."
			MessageToLiving(target, Spell.Message1, eChatType.CT_Spell);
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			if (!noMessages) {
				//"Your meditative state fades."
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				//"{0}'s meditative state fades."
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			return 0;
		}


		// constructor
		public HoTSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
