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
	[SpellHandlerAttribute("Lifedrain")]
	public class LifedrainSpellHandler : DirectDamageSpellHandler
	{
		/// <summary>
		/// execute direct effect
		/// </summary>
		/// <param name="target">target that gets the damage</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;
			if (!target.Alive || target.ObjectState!=GameLiving.eObjectState.Active) return;

			// calc damage and healing
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			SendDamageMessages(ad);
			DamageTarget(ad, true);
			StealLife(ad);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
		}

		/// <summary>
		/// Uses percent of damage to heal the caster
		/// </summary>
		public virtual void StealLife(AttackData ad)
		{
			if(ad == null) return;
			if(!m_caster.Alive) return;

			int heal = (ad.Damage + ad.CriticalDamage) * m_spell.LifeDrainReturn/100;
			if (m_caster.IsDiseased)
			{
				MessageToCaster("You are diseased!", eChatType.CT_SpellResisted);
				heal >>= 1;
			}
			if(heal <= 0) return;
			heal = m_caster.ChangeHealth(m_caster, GameLiving.eHealthChangeType.Spell, heal);

			if(heal > 0) {
				MessageToCaster("You steal " + heal + " hit point" + (heal==1?".":"s."), eChatType.CT_Spell);
			}
			else {
				MessageToCaster("You cannot absorb any more life.", eChatType.CT_SpellResisted);
			}
		}

		/// <summary>
		/// Calculates the base 100% spell damage which is then modified by damage variance factors
		/// </summary>
		/// <returns></returns>
		public override double CalculateDamageBase()
		{
			double spellDamage = Spell.Damage;
			GamePlayer player = Caster as GamePlayer;
			if (player != null && player.CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				int manaStatValue = player.GetModified((eProperty) player.CharacterClass.ManaStat);
				spellDamage *= (manaStatValue + 180) / 250.0;
				if (spellDamage < 0)
					spellDamage = 0;
			}
			return spellDamage;
		}

		// constructor
		public LifedrainSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}		
	}
}
