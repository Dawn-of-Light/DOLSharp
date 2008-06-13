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
	///
	/// </summary>
	[SpellHandlerAttribute("EnduranceHeal")]
	public class EnduranceHealSpellHandler : SpellHandler
	{
		// constructor
		public EnduranceHealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		/// <summary>
		/// Execute heal spell
		/// </summary>
		/// <param name="target"></param>
		public override void StartSpell(GameLiving target)
		{
			IList targets = SelectTargets(target);
			if (targets.Count <= 0) return;

			bool healed = false;
			int minHeal;
			int maxHeal;
			CalculateHealVariance(out minHeal, out maxHeal);

			foreach (GameLiving healTarget in targets)
			{
				int heal = Util.Random(minHeal, maxHeal);
				healed |= HealTarget(healTarget, heal);
			}

			// group heals seem to use full power even if no heals
			if (!healed && Spell.Target == "Realm")
				RemoveFromStat(CalculateNeededPower(target) >> 1); // only 1/2 power if no heal
			else
				RemoveFromStat(CalculateNeededPower(target));

			// send animation for non pulsing spells only
			if (Spell.Pulse == 0)
			{
				if (healed)
				{
					// send animation on all targets if healed
					foreach (GameLiving healTarget in targets)
						SendEffectAnimation(healTarget, 0, false, 1);
				}
				else
				{
					// show resisted effect if not healed
					SendEffectAnimation(Caster, 0, false, 0);
				}
			}

			if (!healed && Spell.CastTime == 0) m_startReuseTimer = false;
		}

		protected virtual void RemoveFromStat(int value)
		{
			m_caster.Mana -= value;
		}

		/// <summary>
		/// Heals hit points of one target and sends needed messages, no spell effects
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>true if heal was done</returns>
		public virtual bool HealTarget(GameLiving target, int amount)
		{
			if (target == null || target.ObjectState != GameLiving.eObjectState.Active) return false;

			if (!target.IsAlive)
			{
				//"You cannot heal the dead!" sshot550.tga
				MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
				return false;
			}

			int heal = target.ChangeEndurance(Caster, GameLiving.eEnduranceChangeType.Spell, amount);

			if (heal == 0)
			{
				if (Spell.Pulse == 0)
				{
					if (target == m_caster) MessageToCaster("Your endurance is full.", eChatType.CT_SpellResisted);
					else MessageToCaster(target.GetName(0, true) + " endurance is full.", eChatType.CT_SpellResisted);
				}
				return false;
			}

			if (m_caster == target)
			{
				MessageToCaster("You restore " + heal + " endurance points.", eChatType.CT_Spell);
				if (heal < amount)
					MessageToCaster("Your endurance is full.", eChatType.CT_Spell);
			}
			else
			{
				MessageToCaster("You restore " + target.GetName(0, false) + " for " + heal + " ednurance points!", eChatType.CT_Spell);
				MessageToLiving(target, "Your endurance was restored by " + m_caster.GetName(0, false) + " for " + heal + " points.", eChatType.CT_Spell);
				if (heal < amount)
					MessageToCaster(target.GetName(0, true) + " endurance is full.", eChatType.CT_Spell);
			}
			return true;
		}

		/// <summary>
		/// Calculates heal variance based on spec
		/// </summary>
		/// <param name="min">store min variance here</param>
		/// <param name="max">store max variance here</param>
		public virtual void CalculateHealVariance(out int min, out int max)
		{
			double spellValue = m_spell.Value;
			GamePlayer casterPlayer = m_caster as GamePlayer;

			// percents if less than zero
			if (spellValue < 0)
			{
				spellValue = (spellValue * -0.01) * m_caster.MaxEndurance;
			}
			min = max = (int)(spellValue);
			return;
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (selectedTarget != null && selectedTarget.EndurancePercent >= 90)
			{
				MessageToCaster("You cannot cast an endurance heal the target has above 90% endurance!", eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckBeginCast(selectedTarget);
		}
	}
}