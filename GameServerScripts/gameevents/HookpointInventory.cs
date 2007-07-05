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
using DOL.GS.Keeps;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// HookpointInventoryEvent
	/// </summary>
	public class HookpointInventoryEvent
	{
		/// <summary>
		/// Code to call when the server is started
		/// Here we add the items to the hookpoint inventory
		/// </summary>
		/// <param name="e">the event</param>
		/// <param name="sender">the object sender</param>
		/// <param name="args">the event arguments</param>
		[GameServerStartedEvent]
		public static void OnServerStarted(DOLEvent e, object sender, EventArgs args)
		{
			if (ServerProperties.Properties.LOAD_HOOKPOINTS)
			{
				/*string name,byte gold,byte silver,byte copper,ushort icon,string objectType,ushort flag*/
				HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Melee Guard", 67, 2624, "DOL.GS.Keeps.GuardFighter", 0));
				HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Ranged Guard", 67, 2623, "DOL.GS.Keeps.GuardStaticArcher", 0));
				HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Healer Guard", 67, 2628, "DOL.GS.Keeps.GuardHealer", 0));
				HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Caster Guard", 67, 2625, "DOL.GS.Keeps.GuardStaticCaster", 0));
				HookPointInventory.RedHPInventory.AddFirstFreeSlot(new HookPointItem("Stealther Guard", 67, 2627, "DOL.GS.Keeps.GuardStealther", 0));

				HookPointInventory.GreenHPInventory.AddFirstFreeSlot(new HookPointItem("Palintone", 50, 0x0A2C, "DOL.GS.GameSiegeCatapult", 0x4100));
				HookPointInventory.GreenHPInventory.AddFirstFreeSlot(new HookPointItem("Trebuchet", 20, 0x0A22, "DOL.GS.GameSiegeTrebuchet", 0x4B00));

				HookPointInventory.LightGreenHPInventory.AddFirstFreeSlot(new HookPointItem("Ballista", 20, 0x0A2C, "DOL.GS.GameSiegeBallista", 0x4b00));

				HookPointInventory.YellowHPInventory.AddFirstFreeSlot(new HookPointItem("Boiling Oil", 70, 0x0A40, "DOL.GS.GameSiegeCauldron", 0x2800));

				/*
				HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Healer", 10, 0x0A40, "DOL.GS.Scripts.GameHealer", 0));
				HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Hastener", 10, 0x0A40, "DOL.GS.Scripts.GameHastener", 0));
				HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("BlackSmith", 1, 0x0A40, "DOL.GS.Scripts.Blacksmith", 0));
				HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Dye Master", 1, 0x0A40, "DOL.GS.GameKeepGuard", 0));
				HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Arrow Merchant", 1, 0x0A40, "DOL.GS.GameKeepGuard", 0));
				HookPointInventory.BlueHPInventory.AddFirstFreeSlot(new HookPointItem("Poison Merchant", 1, 0x0A40, "DOL.GS.GameKeepGuard", 0));
				*/
			}
		}
	}
}
