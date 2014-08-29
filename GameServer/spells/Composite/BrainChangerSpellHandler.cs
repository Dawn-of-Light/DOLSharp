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
using System.Collections.Concurrent;

using DOL.AI;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Change the Brain behavior of a Targeted Game NPC
	/// </summary>
	public abstract class BrainChangerSpellHandler : SpellHandler
	{
		public BrainChangerSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
		
		private ConcurrentDictionary<GameLiving, ABrain> m_changedBrains = new ConcurrentDictionary<GameLiving, ABrain>();
		
		public abstract ABrain GetReplacementBrain();

		/// <summary>
		/// Consume mana finish spell cast
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
			base.FinishSpellCast (target);
		}
		
		/// <summary>
		/// Select only GameNPC that aren't controlled by friends
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public override IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			IList<GameLiving> targetList = base.SelectTargets(castTarget);
			
			List<GameLiving> copyList = new List<GameLiving>(targetList);
			
			foreach (GameLiving living in copyList)
			{
				GameLiving target = living;
				
				if (!(target is GameNPC))
				{
					targetList.Remove(target);
				}
				else if (((GameNPC)target).Brain is IControlledBrain)
				{
					GamePlayer owner =  ((IControlledBrain)((GameNPC)target).Brain).GetPlayerOwner();
					
					if (owner != null && !GameServer.ServerRules.IsAllowedToAttack(Caster, owner, true))
						targetList.Remove(target);
				}
			}
			
			return targetList;
		}
		
		public override void OnEffectStart(DOL.GS.Effects.GameSpellEffect effect)
		{
			if (effect.Owner is GameNPC)
			{
				// Check if the target already has his brain replaced
				if (!m_changedBrains.ContainsKey(effect.Owner))
				{
					ABrain newBrain = GetReplacementBrain();
					m_changedBrains.TryAdd(effect.Owner, newBrain);
					((GameNPC)effect.Owner).AddBrain(newBrain);
				}
			}
			
			base.OnEffectStart(effect);
		}
		
		public override int OnEffectExpires(DOL.GS.Effects.GameSpellEffect effect, bool noMessages)
		{
			if (effect.Owner is GameNPC)
			{
				// Get The saved Brain to remove it.
				if (m_changedBrains.ContainsKey(effect.Owner))
				{
					ABrain newBrain; 
					m_changedBrains.TryGetValue(effect.Owner, out newBrain);
					((GameNPC)effect.Owner).RemoveBrain(newBrain);
					
					// Safety in case there aren't previous brains...
					if(((GameNPC)effect.Owner).Brain == null)
						((GameNPC)effect.Owner).AddBrain(new StandardMobBrain());
				}
			}
			
			return base.OnEffectExpires(effect, noMessages);
		}
	}
}
