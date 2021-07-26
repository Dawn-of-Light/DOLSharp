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
	[SpellHandlerAttribute("PowerDrain")]
	public class PowerDrain : DirectDamageSpellHandler
	{
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			SendDamageMessages(ad);
			DamageTarget(ad, true);
			DrainPower(ad);
			target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
		}

		public virtual void DrainPower(AttackData ad)
		{
			if (ad == null || !m_caster.IsAlive)
				return;

			GameLiving owner = Owner();
			if (owner == null)
				return;

			int powerGain = (ad.Damage + ad.CriticalDamage) * m_spell.LifeDrainReturn / 100;
			powerGain = owner.ChangeMana(m_caster, GameLiving.eManaChangeType.Spell, powerGain);

			if (powerGain > 0)
				MessageToOwner(String.Format("Your summon channels {0} power to you!", powerGain), eChatType.CT_Spell);
			else
				MessageToOwner("You cannot absorb any more power.", eChatType.CT_SpellResisted);
		}
		
		protected virtual GameLiving Owner()
		{
			return Caster;
		}
		
		protected virtual void MessageToOwner(String message, eChatType chatType)
		{
			base.MessageToCaster(message, chatType);
		}

		public PowerDrain(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }

        public override string ShortDescription 
			=> $"Damage the target for {Spell.Damage} Spirit damage and the attacker gains {Spell.LifeDrainReturn}% of that damage as power.";
    }
}
