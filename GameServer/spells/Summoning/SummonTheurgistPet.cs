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
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.GS.Effects;
using log4net;
using System.Reflection;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Summon a theurgist pet.
	/// </summary>
	[SpellHandler("SummonTheurgistPet")]
	public class SummonTheurgistPetHandler : UncontrolledTargetedSummonHandler
	{
		public SummonTheurgistPetHandler(GameLiving caster, Spell spell, SpellLine line) 
			: base(caster, spell, line) { }

		/// <summary>
		/// Check whether it's possible to summon a pet.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckEndCast(GameLiving selectedTarget, bool quiet)
		{
			if (Caster.PetCount >= ServerProperties.Properties.THEURGIST_PET_CAP)
			{
				if (!quiet)
					MessageToCaster("You have too many controlled creatures!", eChatType.CT_SpellResisted);
				
				return false;
			}

			return base.CheckBeginCast(selectedTarget);
		}

		protected override GamePet GetGamePet(INpcTemplate template)
		{
			return new TheurgistPet(template);
		}

	}
}
