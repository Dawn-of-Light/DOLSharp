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
using DOL.AI.Brain;
using DOL.Events;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Database2;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.Styles;

namespace DOL.GS
{
	public class BDSubPet : BDPet
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the different subpet ids
		/// </summary>
		public enum SubPetType : byte
		{
			Melee = 0,
			Healer = 1,
			Caster = 2,
			Debuffer = 3,
			Buffer = 4,
			Archer = 5
		}

		/// <summary>
		/// Create a commander.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public BDSubPet(INpcTemplate npcTemplate, GameNPC owner, SubPetType type)
			: base(npcTemplate)
		{
			ControlledNpc controlledBrain = null;

			switch (type)
			{
				//Melee
				case SubPetType.Melee:
					controlledBrain = new BDMeleeBrain(owner);
					break;
				//Healer
				case SubPetType.Healer:
					controlledBrain = new BDHealerBrain(owner);
					break;
				//Mage
				case SubPetType.Caster:
					controlledBrain = new BDCasterBrain(owner);
					break;
				//Debuffer
				case SubPetType.Debuffer:
					controlledBrain = new BDDebufferBrain(owner);
					break;
				//Buffer
				case SubPetType.Buffer:
					controlledBrain = new BDBufferBrain(owner);
					break;
				//Range
				case SubPetType.Archer:
					controlledBrain = new BDArcherBrain(owner);
					ItemTemplate temp = GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "BD_Archer_Distance_bow") as ItemTemplate;
					if (temp == null)
						log.Error("Unable to find Bonedancer Archer's Bow");
					else
					{
						Inventory.RemoveItem(Inventory.GetItem(eInventorySlot.DistanceWeapon));
						Inventory.AddItem(eInventorySlot.DistanceWeapon, new InventoryItem(temp));
						AddStatsToWeapon();
					}
					break;
				//Other 
				default:
					controlledBrain = new ControlledNpc(owner);
					break;
			}

			// Create a brain for the pet.
			SetOwnBrain(controlledBrain);
		}

		public override int MaxSpeed
		{
			get
			{
				return (Brain as IControlledBrain).Owner.MaxSpeed;
			}
		}
	}
}