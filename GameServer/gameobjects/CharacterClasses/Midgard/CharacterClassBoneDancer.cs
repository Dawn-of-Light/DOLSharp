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

using DOL.Events;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.PlayerClass;

namespace DOL.GS
{
	/// <summary>
	/// The Bonedancer character class.
	/// </summary>
	public class CharacterClassBoneDancer : ClassMystic
	{
		/// <summary>
		/// Releases controlled object
		/// </summary>
		public override void CommandNpcRelease()
		{
			BDPet subpet = Player.TargetObject as BDPet;
			if (subpet != null && subpet.Brain is BDPetBrain && Player.ControlledBrain is CommanderBrain && (Player.ControlledBrain as CommanderBrain).FindPet(subpet.Brain as IControlledBrain))
			{
				Player.Notify(GameLivingEvent.PetReleased, subpet);
				return;
			}

			base.CommandNpcRelease();
		}

		/// <summary>
		/// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player">player to modify</param>
		/// <param name="skill">The skill that is trained</param>
		public override void OnSkillTrained(GamePlayer player, Specialization skill)
		{
			base.OnSkillTrained(player, skill);

			// BD subpet spells can be scaled with the BD's spec as a cap, so when a BD
			//	trains, we have to re-scale spells for subpets from that spec.
			if (DOL.GS.ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0
				&& DOL.GS.ServerProperties.Properties.PET_CAP_BD_MINION_SPELL_SCALING_BY_SPEC
				&& player.ControlledBrain != null && player.ControlledBrain.Body is GamePet pet)
					foreach (ABrain subBrain in pet.ControlledNpcList)
						if (subBrain != null && subBrain.Body is BDSubPet subPet && subPet.PetSpecLine == skill.KeyName)
							subPet.SortSpells();
		}


	}
}
