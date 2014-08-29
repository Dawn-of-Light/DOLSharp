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
	/// Summon an uncontrolled targeted Pet
	/// </summary>
	[SpellHandler("UncontrolledTargetedSummon")]
	public class UncontrolledTargetedSummonHandler : SummonSpellHandler
	{
		public UncontrolledTargetedSummonHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) 
		{
		}

		/// <summary>
		/// Summon the pet.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);

            m_pet.TempProperties.setProperty("target", target);
            (m_pet.Brain as IOldAggressiveBrain).AddToAggroList(target, 1);
			(m_pet.Brain as UncontrolledTargetedPetBrain).Think();

			Caster.PetCount++;
		}

		/// <summary>
		/// Despawn pet.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns>Immunity timer (in milliseconds).</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (Caster.PetCount > 0)
				Caster.PetCount--;

			return base.OnEffectExpires(effect, noMessages);
		}

		protected override void AddHandlers()
		{
		}

		protected override GamePet GetGamePet(INpcTemplate template)
		{
			return new UncontrolledTargetedPet(template);
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new UncontrolledTargetedPetBrain(owner);
		}

		protected override void SetBrainToOwner(IControlledBrain brain)
		{
		}

		protected override void GetPetLocation(out int x, out int y, out int z, out ushort heading, out Region region)
		{
			base.GetPetLocation(out x, out y, out z, out heading, out region);
			heading = Caster.Heading;
		}
	}
}
