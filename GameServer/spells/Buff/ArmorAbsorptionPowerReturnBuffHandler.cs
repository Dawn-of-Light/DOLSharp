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
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Armor Absorption buff and part of Damage returned as Power to the Caster.
	/// </summary>
    [SpellHandlerAttribute("ArmorAbsorptionPowerReturnBuff")]
	public class ArmorAbsorptionPowerReturnBuffHandler : ArmorAbsorptionBuff
	{
		
		public override void OnEffectStart(DOL.GS.Effects.GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			
			if(effect.Owner != null)
				GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttackAbsorb));
		}
		
		public override int OnEffectExpires(DOL.GS.Effects.GameSpellEffect effect, bool noMessages)
		{
			if(effect.Owner != null)
				GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttackAbsorb));
			
			return base.OnEffectExpires(effect, noMessages);
		}

		protected virtual void OnAttackAbsorb(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) 
				return;
			
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackData ad = null;
			if (attackedByEnemy != null)
				ad = attackedByEnemy.AttackData;
			
			// Only Hits
			if (ad == null || (ad.AttackResult != eAttackResult.HitStyle && ad.AttackResult != eAttackResult.HitUnstyled))
				return;
			
			// Melee Or Archery
			if (!ad.IsMeleeAttack && ad.AttackType != AttackData.eAttackType.Ranged)
				return;
			
			int manaAbsorb = (int)(0.01 * Spell.Value * (ad.Damage+ad.CriticalDamage));
			
			if(Caster == null)
				return;
			
			int manaGain = Caster.ChangeMana(living, GameLiving.eManaChangeType.Spell, manaAbsorb);
			
			if (manaGain > 0)
			{
				MessageToCaster("Your barrier returns " + manaAbsorb + " power back to you.", eChatType.CT_Spell);
			}
			else
			{
				MessageToCaster("You cannot absorb any more power with your barrier.", eChatType.CT_SpellResisted);
			}
			
		}
		
        public ArmorAbsorptionPowerReturnBuffHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
		{
		}
	}
}
