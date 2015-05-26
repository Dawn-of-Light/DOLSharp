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

using DOL.AI.Brain;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Fear")]
	public class FearSpellHandler : SpellHandler 
	{
		/// <summary>
		/// Dictionary to Keep Track of Fear Brains attached to NPCs
		/// </summary>
		private readonly ReaderWriterDictionary<GameNPC, FearBrain> m_NPCFearBrains = new ReaderWriterDictionary<GameNPC, FearBrain>();
		
		/// <summary>
		/// Consume Power on Spell Start
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast (target);
		}

		/// <summary>
		/// Select only GameNPC Targets
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public override IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			return base.SelectTargets(castTarget).Where(t => t is GameNPC).ToList();
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			var npcTarget = target as GameNPC;
			if (npcTarget == null) return;
			
			if (npcTarget.Level > Spell.Value)
			{
				// Resisted
				OnSpellResisted(target);
				return;
			}
			
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// On Effect Start Replace Brain with Fear Brain.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			var npcTarget = effect.Owner as GameNPC;
			
			var fearBrain = new FearBrain();
			m_NPCFearBrains.AddOrReplace(npcTarget, fearBrain);
			
			npcTarget.AddBrain(fearBrain);
			fearBrain.Think();
			
			base.OnEffectStart(effect);
		}
		
		/// <summary>
		/// Called when Effect Expires
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			var npcTarget = effect.Owner as GameNPC;

			FearBrain fearBrain;
			if (m_NPCFearBrains.TryRemove(npcTarget, out fearBrain))
			{
				npcTarget.RemoveBrain(fearBrain);
			}

			if(npcTarget.Brain == null)
				npcTarget.AddBrain(new StandardMobBrain());

			return base.OnEffectExpires(effect, noMessages);
		}
		
		/// <summary>
		/// Spell Resists don't trigger notification or interrupt
		/// </summary>
		/// <param name="target"></param>
		protected override void OnSpellResisted(GameLiving target)
		{
			SendSpellResistAnimation(target);
			SendSpellResistMessages(target);
			StartSpellResistLastAttackTimer(target);
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public FearSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
