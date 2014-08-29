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
using System.Linq;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("SpreadHeal")]
	public class SpreadhealSpellHandler : HealSpellHandler
	{
		// constructor
		public SpreadhealSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}


		/// <summary>
		/// Heals targets group with spread heal.
		/// The amount should be the single target heal value derived from spread value
		/// Group Total amount cap is Amount * 7
		/// Single Target Cap is Amount * 2
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>amount of healed hit points</returns>
		public override int HealTarget (GameLiving target, int amount)
		{
			List<GameLiving> injuredTargets;

			// Select only group member injured and in range
			if(target.Group != null)
			{
				List<GameLiving> members = new List<GameLiving>(8);
				
				foreach(GameLiving groupMember in target.Group.GetMembersInTheGroup())
				{
					GameLiving member = groupMember;
					if(member.IsWithinRadius(target, Spell.Range) && member.HealthPercentGroupWindow < 100)
						members.Add(member);
				}
				
				// Sort By Health Percent (Group Window status needed for Necro)
				injuredTargets = members.OrderBy(mem => mem.HealthPercentGroupWindow).ToList();
			}
			else
			{
				// if no group select target
				injuredTargets = new List<GameLiving>(1);
				injuredTargets.Add(target);
			}
			
			int totalAmount = amount * 7;
			int targetAmount = amount * 2;
			int totalHealed = 0;
			
			foreach(GameLiving groupMember in injuredTargets)
			{
				GameLiving member = groupMember;
				// If it's a necro shade try to heal the pet
				if(member.ControlledBrain is CasterNotifiedPetBrain && member.ControlledBrain.Body != null && member.EffectList.GetOfType<NecromancerShadeEffect>() != null)
				{
					member = member.ControlledBrain.Body;
				}
				
				// Heal up to TargetAmount or remaining pool
				targetAmount = Math.Min(totalAmount, targetAmount);
				
				int healed = base.HealTarget(member, targetAmount);
				// If heal succeeded display some animation
				if(healed > 0)
					SendEffectAnimation(member, 0, false, 1);
				
				totalHealed += healed;
				
				// reduce total pool, don't take critical/bonuses into account
				totalAmount -= Math.Min(targetAmount, healed);
				if(totalAmount <= 0)
					break;
			}
			
			// no heals...
			if(totalHealed == 0)
			{
				MessageToCaster("Your group is already fully healed!", eChatType.CT_SpellResisted);
				return 0;
			}

			return totalHealed;
		}
	}
}
