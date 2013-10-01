using System;
using DOL.Events;

namespace DOL.GS.Scripts.Examples
{
	/// <summary>
	/// This sample shows how you can override format of InventoryLogging without modifying the core of Dol.
	/// </summary>
	public class CustomInventoryLoggingExample
	{
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			// Change formats of lines:
			InventoryLogging.ActionXformat[eInventoryActionType.Trade] = "Trade: {0} gives {2} to {1}.";
			InventoryLogging.ActionXformat[eInventoryActionType.Loot] = "Loot: {0} gives {2} to {1}.";
			InventoryLogging.ActionXformat[eInventoryActionType.Quest] = "Quest: {0} gives {2} to {1}.";
			InventoryLogging.ActionXformat[eInventoryActionType.Merchant] = "Merchant: {0} gives {2} to {1}.";
			InventoryLogging.ActionXformat[eInventoryActionType.Craft] = "Craft: {0} gives {2} to {1}.";
			InventoryLogging.ActionXformat[eInventoryActionType.Other] = "Other: {0} gives {2} to {1}.";

			InventoryLogging.GetGameObjectString = obj => obj == null ? "(null)" : obj.Name;
			InventoryLogging.GetItemString = (item, amount) => item == null ? "(null)" : (amount + " " + item.Name);
			InventoryLogging.GetMoneyString = Money.GetString;
		}
	}
}
