using System;
using System.Reflection;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Events;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// A class for individual NPC inventories
	/// this bypasses shared inventory templates which we sometimes need
	/// </summary>
	public class GameNPCInventory : GameLivingInventory
	{
		/// <summary>
		/// Creates a Guard Inventory from an Inventory Template
		/// </summary>
		/// <param name="template"></param>
		public GameNPCInventory(GameNpcInventoryTemplate template)
		{
			foreach (InventoryItem item in template.AllItems)
			{
				AddItem((eInventorySlot)item.SlotPosition, new InventoryItem(item));
			}
		}
	}
}
