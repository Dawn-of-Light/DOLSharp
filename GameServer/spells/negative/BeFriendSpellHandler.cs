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
using System.Collections.Generic;
using System.Linq;

using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandler("BeFriend")]
	public class BeFriendSpellHandler : SpellHandler 
	{
		/// <summary>
		/// Dictionary to Keep track of Friend Brains Attached to NPC
		/// </summary>
		private readonly ReaderWriterDictionary<GameNPC, FriendBrain> m_NPCFriendBrain = new ReaderWriterDictionary<GameNPC, FriendBrain>();
		
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
		/// Select only uncontrolled GameNPC Targets
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
				SendSpellResistAnimation(target);
				this.MessageToCaster(eChatType.CT_SpellResisted, "{0} is too strong for you to charm!", target.GetName(0, true));
				return;
			}
			
			if (npcTarget.Brain is IControlledBrain)
			{
				SendSpellResistAnimation(target);
				this.MessageToCaster(eChatType.CT_SpellResisted, "{0} is already under control.",  target.GetName(0, true));
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
			
			var currentBrain = npcTarget.Brain as IOldAggressiveBrain;
			var friendBrain = new FriendBrain(this);
			m_NPCFriendBrain.AddOrReplace(npcTarget, friendBrain);
			
			npcTarget.AddBrain(friendBrain);
			friendBrain.Think();
			
			// Prevent Aggro on Effect Expires.
			if (currentBrain != null)
				currentBrain.ClearAggroList();
			
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

			FriendBrain fearBrain;
			if (m_NPCFriendBrain.TryRemove(npcTarget, out fearBrain))
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
		public BeFriendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
