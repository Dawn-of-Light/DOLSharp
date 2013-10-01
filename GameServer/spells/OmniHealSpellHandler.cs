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
	[SpellHandlerAttribute("OmniHeal")]
	public class OmniHealSpellHandler : SpellHandler
	{
		// constructor
		public OmniHealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
		/// <summary>
		/// Execute heal spell
		/// </summary>
		/// <param name="target"></param>
		public override bool StartSpell(GameLiving target)
		{
			IList targets = SelectTargets(target);
			if (targets.Count <= 0) return false;

			bool healed = false;

			foreach (GameLiving healTarget in targets)
			{
				healed |= HealTarget(healTarget);
			}

			// group heals seem to use full power even if no heals
			if (!healed && Spell.Target == "Realm")
				m_caster.Mana -= PowerCost(target) >> 1; // only 1/2 power if no heal
			else
				m_caster.Mana -= PowerCost(target);

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

			return true;
		}

		/// <summary>
		/// Heals hit points/power/endurance of one target and sends needed messages, no spell effects
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>true if heal was done</returns>
		public virtual bool HealTarget(GameLiving target)
		{
			if (target == null || target.ObjectState != GameLiving.eObjectState.Active) return false;

			// we can't heal people we can attack
			if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
				return false;

			if (!target.IsAlive)
			{
				//"You cannot heal the dead!" sshot550.tga
				MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
				return false;
			}

			bool healed = false;			
			int amount;
			int minHeal;
			int maxHeal;
			int heal;

			// don't heal mana of class who can't !
			if (target is GamePlayer 
				    && !(
				    ((GamePlayer)target).CharacterClass is PlayerClass.ClassVampiir
					|| ((GamePlayer)target).CharacterClass is PlayerClass.ClassMaulerAlb
					|| ((GamePlayer)target).CharacterClass is PlayerClass.ClassMaulerHib
					|| ((GamePlayer)target).CharacterClass is PlayerClass.ClassMaulerMid))
			{
				CalculateHealPowerVariance(out minHeal, out maxHeal);
				amount = Util.Random(minHeal, maxHeal);

				heal = target.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, amount);
								
				if (heal == 0)
				{
					if (Spell.Pulse == 0)
					{
						if (target == m_caster) MessageToCaster("Your power is full.", eChatType.CT_SpellResisted);
						else MessageToCaster(target.GetName(0, true) + " power is full.", eChatType.CT_SpellResisted);
					}
					
					healed |= false;
				}
				else if (m_caster == target)
				{
					MessageToCaster("You restore " + heal + " power points.", eChatType.CT_Spell);
					if (heal < amount)
						MessageToCaster("Your power is full.", eChatType.CT_Spell);
					
					healed |= true;
				}
				else
				{
					MessageToCaster("You restore " + target.GetName(0, false) + " for " + heal + " power points!", eChatType.CT_Spell);
					MessageToLiving(target, "Your power was restored by " + m_caster.GetName(0, false) + " for " + heal + " points.", eChatType.CT_Spell);
					if (heal < amount)
						MessageToCaster(target.GetName(0, true) + " mana is full.", eChatType.CT_Spell);
					
					healed |= true;
				}
			}
			
			// heal endurance
			CalculateHealEnduranceVariance(out minHeal, out maxHeal);
			amount = Util.Random(minHeal, maxHeal);

			heal = target.ChangeEndurance(Caster, GameLiving.eEnduranceChangeType.Spell, amount);

			if (heal == 0)
			{
				if (Spell.Pulse == 0)
				{
					if (target == m_caster) MessageToCaster("Your endurance is full.", eChatType.CT_SpellResisted);
					else MessageToCaster(target.GetName(0, true) + " endurance is full.", eChatType.CT_SpellResisted);
				}
				
				healed |= false;
			}
			else if (m_caster == target)
			{
				MessageToCaster("You restore " + heal + " endurance points.", eChatType.CT_Spell);
				if (heal < amount)
					MessageToCaster("Your endurance is full.", eChatType.CT_Spell);
				
				healed |= true;
			}
			else
			{
				MessageToCaster("You restore " + target.GetName(0, false) + " for " + heal + " endurance points!", eChatType.CT_Spell);
				MessageToLiving(target, "Your endurance was restored by " + m_caster.GetName(0, false) + " for " + heal + " points.", eChatType.CT_Spell);
				if (heal < amount)
					MessageToCaster(target.GetName(0, true) + " endurance is full.", eChatType.CT_Spell);
				
				healed |= true;
			}

			// heal hits points
			CalculateHealVariance(out minHeal, out maxHeal);
			amount = Util.Random(minHeal, maxHeal);

			heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, amount);

			if (heal == 0)
			{
				if (Spell.Pulse == 0)
				{
					if (target == m_caster) MessageToCaster("Your hits points are full.", eChatType.CT_SpellResisted);
					else MessageToCaster(target.GetName(0, true) + " hits points are full.", eChatType.CT_SpellResisted);
				}
				
				healed |= false;
			}
			else if (m_caster == target)
			{
				MessageToCaster("You restore " + heal + " hits points.", eChatType.CT_Spell);
				if (heal < amount)
					MessageToCaster("Your hits points are full.", eChatType.CT_Spell);
				
				healed |= true;
			}
			else
			{
				MessageToCaster("You restore " + target.GetName(0, false) + " for " + heal + " hits points!", eChatType.CT_Spell);
				MessageToLiving(target, "Your hits points were restored by " + m_caster.GetName(0, false) + " for " + heal + " points.", eChatType.CT_Spell);
				if (heal < amount)
					MessageToCaster(target.GetName(0, true) + " hits points are full.", eChatType.CT_Spell);
				
				healed |= true;
			}
			
			return healed;
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
				spellValue = (spellValue * -0.01) * m_caster.MaxHealth;
			}
			min = max = (int)(spellValue);
			return;
		}

		/// <summary>
		/// Calculates heal variance based on spec
		/// </summary>
		/// <param name="min">store min variance here</param>
		/// <param name="max">store max variance here</param>
		public virtual void CalculateHealPowerVariance(out int min, out int max)
		{
			double spellValue = m_spell.Value;
			GamePlayer casterPlayer = m_caster as GamePlayer;

			// percents if less than zero
			if (spellValue < 0)
			{
				spellValue = (spellValue * -0.01) * m_caster.MaxMana;
			}
			min = max = (int)(spellValue);
			return;
		}
		
		/// <summary>
		/// Calculates heal variance based on spec
		/// </summary>
		/// <param name="min">store min variance here</param>
		/// <param name="max">store max variance here</param>
		public virtual void CalculateHealEnduranceVariance(out int min, out int max)
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
	}
}
