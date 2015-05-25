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
	[SpellHandlerAttribute("Fear")]
	public class FearSpellHandler : SpellHandler 
	{
		//VaNaTiC->
		/*
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		*/
		//VaNaTiC<-

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast (target);
			
			GameNPC t = target as GameNPC;
			if(t!=null)
				t.WalkToSpawn();
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

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override bool StartSpell(GameLiving target)
		{
			if (target == null) return false;

			var targets = SelectTargets(target);

			foreach (GameLiving t in targets)
			{
				if(t is GameNPC && t.Level <= m_spell.Value)
				{
					((GameNPC)t).AddBrain(new FearBrain());
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


		public FearSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
