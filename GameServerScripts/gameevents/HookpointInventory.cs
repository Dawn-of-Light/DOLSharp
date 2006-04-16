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

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// HookpointInventoryEvent
	/// </summary>
	public class HookpointInventoryEvent
	{
		[GameServerStartedEvent]
		public static void OnserverStarted(DOLEvent e, object sender, EventArgs args)
		{
			//TODO : good type in code
			//todo make new class like guard with special brain for each class

			/*string name,byte gold,byte silver,byte copper,ushort icon,string objectType,ushort flag*/
	/*		HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Melee Guard",67,0x0A40,"DOL.GS.GameKeepGuard",0));

			HookPointInventory.GreenHPInventory.AddFirstFreeSlot(new HookPointItem("Palintone",50,0x0A2C,"DOL.GS.GameSiegeCatapult",0x4100));
			HookPointInventory.GreenHPInventory.AddFirstFreeSlot(new HookPointItem("Trebuchet",20,0x0A22,"DOL.GS.GameSiegeTrebuchet",0x4B00));

			HookPointInventory.LightGreenHPInventory.AddFirstFreeSlot(new HookPointItem("Ballista",20,0x0A2C,"DOL.GS.GameSiegeBallista",0x4b00));

			HookPointInventory.YellowHPInventory.AddFirstFreeSlot(new HookPointItem("Boiling Oil",70,0x0A40,"DOL.GS.GameSiegeCauldron",0x2800));*/

//			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Healer",10,0x0A40,"DOL.GS.Scripts.GameHealer",0));
//			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Hastener",10,0x0A40,"DOL.GS.Scripts.GameHastener",0));
//			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("BlackSmith",1,0x0A40,"DOL.GS.Scripts.Blacksmith",0));
			/*
			HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Caster Guard",67,0x0A40,"DOL.GS.GameKeepGuard",0));
			HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Tanged Guard",67,0x0A40,"DOL.GS.GameKeepGuard",0));
			HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Healer Guard",67,0x0A40,"DOL.GS.GameKeepGuard",0));
			HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Stealther Guard",67,0x0A40,"DOL.GS.GameKeepGuard",0));


			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Healer",10,0x0A40,"DOL.GS.GameHealer",0));
			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Hastener",10,0x0A40,"DOL.GS.GameHastener",0));

			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Dye Master",1,0x0A40,"DOL.GS.GameKeepGuard",0));
			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Marchand de Projectile",1,0x0A40,"DOL.GS.GameKeepGuard",0));
			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Poison Merchant",1,0x0A40,"DOL.GS.GameKeepGuard",0));

			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("BlackSmith",1,0x0A40,"DOL.GS.Blacksmith",0));

			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Marchand de Gemmes",1,0x0A40,"DOL.GS.GameKeepGuard",0));
			HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Preserveur d'Ether",1,0x0A40,"DOL.GS.GameKeepGuard",0));
			*/
		}
	}
}
