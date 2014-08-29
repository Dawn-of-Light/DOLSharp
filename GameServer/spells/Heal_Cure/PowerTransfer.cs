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
using System.Collections.Generic;
using System.Text;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler for power trasnfer.
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandlerAttribute("PowerTransfer")]
	class PowerTransfer : SpellHandler
	{
		/// <summary>
		/// Check if player tries to transfer power to himself.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
		{
			GameLiving owner = Caster;
			if(Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
			{
				owner = ((IControlledBrain)((GameNPC)Caster).Brain).GetLivingOwner();
			}

			if (selectedTarget != null && (selectedTarget == Caster || selectedTarget == owner))
			{
				MessageToCaster("You cannot transfer power to yourself!", eChatType.CT_SpellResisted);
				return false;
			}

			return base.CheckBeginCast(selectedTarget, quiet);
		}

		/// <summary>
		/// Execute direct effect.
		/// </summary>
		/// <param name="target">Target power is transferred to.</param>
		/// <param name="effectiveness">Effectiveness of the spell (0..1, equalling 0-100%).</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null)
				return;
			
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active)
				return;

			// if Caster is controlled, get owner

			GameLiving owner = Caster;
			
			if(Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
			{
				owner = ((IControlledBrain)((GameNPC)Caster).Brain).GetLivingOwner();
			}
			
			if (owner == null)
				return;

			// Calculate the amount of power to transfer from the owner.
			int powerTransfer = Math.Min((int)(Spell.Value*effectiveness), owner.Mana);
			
			// Heal target power
			int powerHealed = target.ChangeMana(owner, GameLiving.eManaChangeType.Spell, powerTransfer);
			
			// Drain Power
			if (powerHealed > 0)
				owner.ChangeMana(owner, GameLiving.eManaChangeType.Spell, -powerHealed);

			if (powerHealed <= 0)
			{
				// Resist animation
				SendEffectAnimation(target, 0, false, 0);
				MessageToCaster(String.Format("{0} is at full power already!", target.Name), eChatType.CT_SpellResisted);
			}
			else
			{
				// Send animation
				SendEffectAnimation(target, 0, false, 1);
				MessageToCaster(String.Format("You transfer {0} power to {1}!", powerHealed, target.Name), eChatType.CT_Spell);
				MessageToLiving(target, String.Format("{0} transfers {1} power to you!", owner.Name, powerHealed), eChatType.CT_Spell);
			}
		}

		/// <summary>
        /// Create a new handler for the power transfer spell.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public PowerTransfer(GameLiving caster, Spell spell, SpellLine line) 
            : base(caster, spell, line) { }
	}
}
