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
	/// Spell calculates a percentage of the caster's power.
	/// Heals target for the full amount, Caster loses half that amount in power.
	/// </summary>
	[SpellHandlerAttribute("PowerTransferDisabled")]
	public class PowerTransferSpellHandler : SpellHandler
	{
		// constructor
		public PowerTransferSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		/// <summary>
		/// Execute lifetransfer spell
		/// </summary>
		public override void StartSpell(GameLiving target)
		{
			IList targets = SelectTargets(target);
			if (targets.Count <= 0) return;

			bool healed = false;
			int transferHeal;
			double spellValue = m_spell.Value;

			transferHeal = (int)(Caster.MaxMana / 100 * Math.Abs(spellValue));
			
			//Needed to prevent divide by zero error
			if (transferHeal <= 0)
				transferHeal = 0; 
			else
			{
				//Remaining health is used if caster does not have enough health, leaving caster at 1 hitpoint
				if ( (transferHeal >> 1) >= Caster.Mana )
					transferHeal = ( (Caster.Mana - 1) << 1);
			}



			foreach(GameLiving healTarget in targets)
			{
				healed |= HealTarget(healTarget, transferHeal);
			}
			if (!healed && Spell.Target == "Realm")
			{
				Caster.Mana -= CalculateNeededPower(target) >> 1;	// only 1/2 power if no heal
			}
			else
			{
				Caster.Mana -= CalculateNeededPower(target);
				Caster.Mana -= transferHeal >> 1;
			}

			// send animation for non pulsing spells only
			if (Spell.Pulse == 0)
			{
				if (healed)
				{
					// send animation on all targets if healed
					foreach(GameLiving healTarget in targets)
						SendEffectAnimation(healTarget, 0, false, 1);
				}
				else
				{
					// show resisted effect if not healed
					SendEffectAnimation(Caster, 0, false, 0);
				}
			}
		}

		/// <summary>
		/// Heals hit points of one target and sends needed messages, no spell effects
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>true if heal was done</returns>
		public virtual bool HealTarget(GameLiving target, int amount)
		{
			if (target==null || target.ObjectState!=GameLiving.eObjectState.Active) return false;

			if (!target.IsAlive) 
			{
				MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster == target)
			{
				MessageToCaster("You cannot transfer power to yourself.", eChatType.CT_SpellResisted);
				return false;
			}
			
			if (amount <= 0)
			{
				MessageToCaster("You do not have enough power to transfer.", eChatType.CT_SpellResisted);
				return false;  
			}


			int heal = target.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, amount);			

			if (heal == 0) 
			{
				if (Spell.Pulse == 0)
				{
					MessageToCaster(target.GetName(0, true)+" is fully powered.", eChatType.CT_SpellResisted);
				}

				return false;
			}

			
			MessageToCaster("You heal " + target.GetName(0, false) + " for " + heal + " power points!", eChatType.CT_Spell);
			MessageToLiving(target, "You are healed by " + m_caster.GetName(0, false) + " for " + heal + " power points.", eChatType.CT_Spell);
			if(heal < amount)
					MessageToCaster(target.GetName(0, true)+" is fully powered.", eChatType.CT_Spell);

			return true;
		}
	}
}
