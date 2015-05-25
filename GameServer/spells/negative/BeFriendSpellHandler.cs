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
using System.Collections.Generic;

using DOL.AI.Brain;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("BeFriend")]
	public class BeFriendSpellHandler : SpellHandler 
	{
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast (target);
		}

		public override IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			var list = new List<GameLiving>();
			GameLiving target;
			
			target=Caster;
			foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Radius)) 
			{
				if(npc is GameNPC)
					list.Add(npc);
			}
			
			return list;
		}

		public virtual IList SelectRealmTargets(GameObject castTarget)
		{
			ArrayList list = new ArrayList();
			
			foreach (GamePlayer player in castTarget.GetPlayersInRadius((ushort)1000)) 
			{
				if(player.Realm == m_caster.Realm && player!=m_caster)
					list.Add(player);
			}

			list.Add(m_caster);

			return list;
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override bool StartSpell(GameLiving target)
		{
			if (target == null) return false;

			var targets = SelectTargets(target);
			IList realmtargets = SelectRealmTargets(target);

			foreach (GameLiving t in targets)
			{
				if(t.Level <= m_spell.Value)
				{
					GameNPC mob = (GameNPC)t;
					if(mob.Brain is StandardMobBrain)
					{
						StandardMobBrain sBrain = (StandardMobBrain) mob.Brain;
						//mob.StopAttack();

						foreach(GamePlayer player in realmtargets)
							sBrain.RemoveFromAggroList(player);

					}

					mob.AddBrain(new FriendBrain(this));
				}
			}

			return true;
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameNPC mob = (GameNPC)effect.Owner;
			mob.RemoveBrain(mob.Brain);

			if(mob.Brain==null)
				mob.AddBrain(new StandardMobBrain());

			return base.OnEffectExpires (effect, noMessages);
		}


		public BeFriendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
