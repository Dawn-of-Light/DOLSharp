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
using DOL.Database;
using DOL.Events;
using log4net;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Equips new created Characters with standard equipment
	/// </summary>
	public class StartupEquipment
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			//We want to be notified whenever a new character is created
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
		}

		// Here we equip our new Characters
		public static void CharacterCreation(DOLEvent e, object sender, EventArgs args)
		{
			CharacterEventArgs charArgs = args as CharacterEventArgs;
			if (charArgs == null)
				return;

			Hashtable usedSlots = new Hashtable();
			ArrayList equipItems = new ArrayList();
			ArrayList inventoryItems = new ArrayList();
			switch (charArgs.Character.Class)
			{
					//Alb Classes
				case 14:
					equipItems.Add("practice_sword");
					equipItems.Add("small_training_shield");
					break; //Fighter
				case 15:
					equipItems.Add("trimmed_branch");
					break; //Elementalist
				case 16:
					equipItems.Add("training_mace");
					equipItems.Add("small_training_shield");
					break; //Acolyte
				case 17:
					equipItems.Add("practice_dirk");
					break; //Alb Rouge
				case 18:
					equipItems.Add("trimmed_branch");
					break; //Mage
				case 20:
					equipItems.Add("trimmed_branch");
					break; //Disciple
					//Mid Classes
				case 35:
					equipItems.Add("training_axe");
					inventoryItems.Add("small_training_shield");
					break; //Viking
				case 36:
					equipItems.Add("trimmed_branch");
					break; //Mystic
				case 37:
					equipItems.Add("training_hammer");
					equipItems.Add("small_training_shield");
					break; //Seer
				case 38:
					equipItems.Add("training_sword_mid");
					break; //Mid Rogue
					//Hib Classes
				case 51:
					equipItems.Add("training_staff");
					break; //Magician
				case 52:
					equipItems.Add("training_sword_hib");
					equipItems.Add("training_shield");
					break; //Guardian
				case 53:
					equipItems.Add("training_club");
					equipItems.Add("training_shield");
					break; //Naturalist
				case 54:
					equipItems.Add("training_dirk");
					break; //Stalker
				case 57:
					equipItems.Add("training_staff");
					break; //Forester?
				default:
					if (log.IsWarnEnabled)
						log.Warn("No standard equipment defined for Character Class " + charArgs.Character.Class + "!");
					return;
			}

			// Add items to inventory
			foreach (string itemkey in inventoryItems)
			{
				GameInventoryItem gameItem = GameInventoryItem.CreateFromTemplate(itemkey);
				if (gameItem == null || gameItem.Item == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Item '" + itemkey + "' not found!");
					continue;
				}

				InventoryItem item = gameItem.Item;
				item.OwnerID = charArgs.Character.ObjectId;

				// add it into default backpack
				for (int i = (int) eInventorySlot.FirstBackpack; i < (int) eInventorySlot.LastBackpack; i++)
				{
					if (usedSlots[i] == null)
					{
						item.SlotPosition = i;
						usedSlots[i] = 1;
						break;
					}
				}
				GameServer.Database.AddNewObject(item);
			}

			// Equip items
			foreach (string itemkey in equipItems)
			{
				GameInventoryItem gameItem = GameInventoryItem.CreateFromTemplate(itemkey);
				if (gameItem == null || gameItem.Item == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Item '" + itemkey + "' not found!");
					continue;
				}

				InventoryItem item = gameItem.Item;
				item.OwnerID = charArgs.Character.ObjectId;
				item.Realm = charArgs.Character.Realm;

				if ((item.Item_Type >= Slot.RIGHTHAND && item.Item_Type <= Slot.RANGED) ||
					(item.Item_Type >= Slot.HELM && item.Item_Type <= Slot.RIGHTRING))
				{
					int slot;
					//shield in left hand
					if (item.Object_Type == (int) eObjectType.Shield)
						slot = Slot.LEFTHAND;
						//lefthanded weapons in right hand
					else if (item.Item_Type == Slot.LEFTHAND)
						slot = Slot.RIGHTHAND;
					else
						slot = item.Item_Type;

					// equip item
					if (usedSlots[slot] == null)
					{
						item.SlotPosition = slot;
						usedSlots[slot] = 1;
					}
				}

				if (item.SlotPosition == 0)
				{
					// add it into default backpack
					for (int i = (int) eInventorySlot.FirstBackpack; i < (int) eInventorySlot.LastBackpack; i++)
					{
						if (usedSlots[i] == null)
						{
							item.SlotPosition = i;
							usedSlots[i] = 1;
							break;
						}
					}
				}

				if (charArgs.Character.ActiveWeaponSlot == 0)
				{
					switch (item.SlotPosition)
					{
						case Slot.RIGHTHAND:
							charArgs.Character.ActiveWeaponSlot = (byte) GamePlayer.eActiveWeaponSlot.Standard;
							break;
						case Slot.TWOHAND:
							charArgs.Character.ActiveWeaponSlot = (byte) GamePlayer.eActiveWeaponSlot.TwoHanded;
							break;
						case Slot.RANGED:
							charArgs.Character.ActiveWeaponSlot = (byte) GamePlayer.eActiveWeaponSlot.Distance;
							break;
					}
				}
				GameServer.Database.AddNewObject(item);
			}
		}
	}
}