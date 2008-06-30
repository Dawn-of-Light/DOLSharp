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
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("TurretsRelease")]
	public class TurretsReleaseSpellHandler : SpellHandler
	{
		public TurretsReleaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			foreach (GameNPC npc in Caster.CurrentRegion.GetNPCsInRadius(Caster.X, Caster.Y, Caster.Z, (ushort)m_spell.Radius, false))
				if ((npc.Brain is TurretFNFBrain) && (npc.Brain as IControlledBrain).Owner == Caster)
					npc.Die(Caster);

			if (Caster.ControlledNpc != null)
			{
				GameEventMgr.Notify(GameLivingEvent.PetReleased, Caster.ControlledNpc, null);
			}
			Caster.PetCounter = 0;
		}
	}
}