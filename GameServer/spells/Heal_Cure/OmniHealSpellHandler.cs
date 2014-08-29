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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Heal Power / Endurance / Health
	/// </summary>
	[SpellHandlerAttribute("OmniHeal")]
	public class OmniHealSpellHandler : HealSpellHandler
	{
		// constructor
		public OmniHealSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
		
		public override int HealTarget(GameLiving target, int amount)
		{
			int healed = base.HealTarget(target, amount);
			int enduHealed = 0;
			int powerHealed = 0;
			int minHeal, maxHeal;
			
			// don't heal mana of class who can't !
			if (target is GamePlayer 
				    && !(
				    ((GamePlayer)target).CharacterClass is PlayerClass.ClassVampiir
					|| ((GamePlayer)target).CharacterClass is PlayerClass.ClassMaulerAlb
					|| ((GamePlayer)target).CharacterClass is PlayerClass.ClassMaulerHib
					|| ((GamePlayer)target).CharacterClass is PlayerClass.ClassMaulerMid))
			{
				// based on ResurrectMana, percent or relative value to heal.
				CalculateHealVariance(out minHeal, out maxHeal, target, 1.0, Spell.ResurrectMana);
				int amountPower = Util.Random(minHeal, maxHeal);
					
				powerHealed = target.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, amountPower);
								
				if (powerHealed == 0 && !Spell.IsPulsing && !Spell.IsPulsingEffect)
				{
					if (target == Caster)
					{
						MessageToCaster("Your power is full.", eChatType.CT_SpellResisted);
					}
					else 
					{
						MessageToCaster(target.GetName(0, true) + " power is full.", eChatType.CT_SpellResisted);
					}
				}
				else if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
				{
					if(target == Caster)
					{
						MessageToCaster("You restore yourself " + powerHealed + " power points.", eChatType.CT_Spell);
						
						if (powerHealed < amount)
							MessageToCaster("Your power is full.", eChatType.CT_Spell);
					}
					else
					{
						MessageToCaster("You restore " + target.GetName(0, false) + " for " + powerHealed + " power points!", eChatType.CT_Spell);
						MessageToLiving(target, "Your power was restored by " + m_caster.GetName(0, false) + " for " + powerHealed + " points.", eChatType.CT_Spell);
						
						if (powerHealed < amount)
							MessageToCaster(target.GetName(0, true) + " mana is full.", eChatType.CT_Spell);
					}
				}
			}
			
			// heal endurance, based on resurect health
			CalculateHealVariance(out minHeal, out maxHeal, target, 1.0, Spell.ResurrectHealth);
			int amountEndu = Util.Random(minHeal, maxHeal);

			enduHealed = target.ChangeEndurance(Caster, GameLiving.eEnduranceChangeType.Spell, amountEndu);

			if (enduHealed == 0 && !Spell.IsPulsing && !Spell.IsPulsingEffect)
			{

				if (target == Caster)
				{
					MessageToCaster("Your endurance is full.", eChatType.CT_SpellResisted);
				}
				else
				{
					MessageToCaster(target.GetName(0, true) + " endurance is full.", eChatType.CT_SpellResisted);
				}
			}
			else if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
			{
				if(target == Caster)
				{
					MessageToCaster("You restore yourself " + enduHealed + " endurance points.", eChatType.CT_Spell);
					
					if (enduHealed < amount)
						MessageToCaster("Your endurance is full.", eChatType.CT_Spell);
				}
				else
				{
					MessageToCaster("You restore " + target.GetName(0, false) + " for " + enduHealed + " endurance points!", eChatType.CT_Spell);
					MessageToLiving(target, "Your endurance was restored by " + m_caster.GetName(0, false) + " for " + enduHealed + " points.", eChatType.CT_Spell);
					
					if (enduHealed < amount)
						MessageToCaster(target.GetName(0, true) + " endurance is full.", eChatType.CT_Spell);
				}

			}
			
			return healed+powerHealed+enduHealed;
		}
	}
}
