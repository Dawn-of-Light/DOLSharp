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
	public class SummonTheurgistPet : SummonSpellHandler
	{
		public SummonTheurgistPet(GameLiving caster, Spell spell, SpellLine line) 
			: base(caster, spell, line) { }

		/// <summary>
		/// Check whether it's possible to summon a pet.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Caster.PetCount >= ServerProperties.Properties.THEURGIST_PET_CAP)
			{
				MessageToCaster("You have too many controlled creatures!", eChatType.CT_SpellResisted);
				return false;
			}

			return base.CheckBeginCast(selectedTarget);
		}


		/// <summary>
		/// Summon the pet.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);

            pet.TempProperties.setProperty("target", target);
            (pet.Brain as IOldAggressiveBrain).AddToAggroList(target, 1);
			(pet.Brain as TheurgistPetBrain).Think();

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
			return new TheurgistPet(template);
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new TheurgistPetBrain(owner);
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
