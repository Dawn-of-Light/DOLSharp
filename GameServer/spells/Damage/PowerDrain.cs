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
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Handles power drain (conversion of target health to caster power).
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandlerAttribute("PowerDrain")]
	public class PowerDrain : DirectDamageSpellHandler
	{
		/// <summary>
		/// Execute direct effect.
		/// </summary>
		/// <param name="target">Target that takes the damage.</param>
		/// <param name="effectiveness">Effectiveness of the spell (0..1, equalling 0-100%).</param>
		protected override AttackData DealDamage(GameLiving target, double effectiveness)
		{
			AttackData ad = base.DealDamage(target, effectiveness);
			DrainPower(ad);
			return ad;
		}

		/// <summary>
		/// Use a percentage of the damage to refill caster's power.
		/// </summary>
		/// <param name="ad">Attack data.</param>
		public virtual void DrainPower(AttackData ad)
		{
			if (ad == null || !Caster.IsAlive)
				return;

			int powerGain = (ad.Damage + ad.CriticalDamage) * Spell.LifeDrainReturn / 100;

			if (Caster is GamePlayer)
			{
				int powerHeal = Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, powerGain);
				
				if (powerHeal > 0)
					MessageToCaster(String.Format("You steal {0} power!", powerGain), eChatType.CT_Spell);
				else
					MessageToCaster("You cannot absorb any more power.", eChatType.CT_SpellResisted);
			}
			else
			{
				if (Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
				{
					GameLiving owner = ((IControlledBrain)((GameNPC)Caster).Brain).GetLivingOwner();
					
					int powerHeal = owner.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, powerGain);

					if (powerHeal > 0)
						MessageToCaster(String.Format("Your summon channels {0} power to you!", powerGain), eChatType.CT_Spell);
					else
						MessageToCaster("Your summon cannot channel any more power to you.", eChatType.CT_SpellResisted);
				}
			}
		}

		/// <summary>
		/// Create a new handler for the power drain spell.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public PowerDrain(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
