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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.Database;
using log4net;
using DOL.Events;
using DOL.GS.Effects;
using System.Collections;
using DOL.AI.Brain;

namespace DOL.GS
{
	/// <summary>
	/// The Bonedancer character class.
	/// </summary>
	public class CharacterClassBoneDancer : CharacterClassBase
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
	}
}
