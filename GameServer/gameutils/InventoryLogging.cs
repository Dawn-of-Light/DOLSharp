using System;
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Type of trade
	/// </summary>
	public enum eInventoryActionType
	{
		/// <summary>
		/// Trade between 2 players
		/// </summary>
		Trade,
		/// <summary>
		/// A player pick up a loot
		/// </summary>
		Loot,
		/// <summary>
		/// Gain of a quest or quest's items
		/// </summary>
		Quest,
		/// <summary>
		/// Buy/sell an item
		/// </summary>
		Merchant,
		/// <summary>
		/// Crafting an item
		/// </summary>
		Craft,
		/// <summary>
		/// Any other action
		/// </summary>
		Other,
	}

	public static class InventoryLogging
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Dictionary<eInventoryActionType, string> ActionXformat =
			new Dictionary<eInventoryActionType, string>
                {
                    {eInventoryActionType.Trade, "[TRADE] {0} > {1}: {2}"},
                    {eInventoryActionType.Loot, "[LOOT] {0} > {1}: {2}"},
                    {eInventoryActionType.Quest, "[QUEST] {0} > {1}: {2}"},
                    {eInventoryActionType.Merchant, "[MERCHANT] {0} > {1}: {2}"},
                    {eInventoryActionType.Craft, "[CRAFT] {0} > {1}: {2}"},
                    {eInventoryActionType.Other, "[OTHER] {0} > {1}: {2}"}
                };

		public static Func<GameObject, string> GetGameObjectString = obj =>
			obj == null ? "(null)" : ("(" + obj.Name + ";" + obj.GetType() + ";" + obj.Coordinate + ";" + obj.Position.RegionID + ")");

		public static Func<ItemTemplate, int, string> GetItemString = (item, count) =>
			item == null ? "(null)" : ("(" + count + ";" + item.Name + ";" + item.Id_nb + ")");

		public static Func<long, string> GetMoneyString = amount =>
			"(MONEY;" + amount + ")";

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, GameObject destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			LogInventoryAction(GetGameObjectString(source), GetGameObjectString(destination), type, item, count);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string source, GameObject destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			LogInventoryAction(source, GetGameObjectString(destination), type, item, count);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, string destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			LogInventoryAction(GetGameObjectString(source), destination, type, item, count);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string source, string destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			// Check if you can log this action
			if (!_IsLoggingEnabled(type))
				return;

			string format;
			if (!ActionXformat.TryGetValue(type, out format))
				return; // Error, this format does not exists ?!

			try
			{
				GameServer.Instance.LogInventoryAction(string.Format(format, source ?? "(null)", destination ?? "(null)", GetItemString(item, count)));
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Log inventory error", e);
			}
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, GameObject destination, eInventoryActionType type, long money)
		{
			LogInventoryAction(GetGameObjectString(source), GetGameObjectString(destination), type, money);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string source, GameObject destination, eInventoryActionType type, long money)
		{
			LogInventoryAction(source, GetGameObjectString(destination), type, money);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, string destination, eInventoryActionType type, long money)
		{
			LogInventoryAction(GetGameObjectString(source), destination, type, money);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string source, string destination, eInventoryActionType type, long money)
		{
			// Check if you can log this action
			if (!_IsLoggingEnabled(type))
				return;

			string format;
			if (!ActionXformat.TryGetValue(type, out format))
				return; // Error, this format does not exists ?!

			try
			{
				GameServer.Instance.LogInventoryAction(string.Format(format, source ?? "(null)", destination ?? "(null)", GetMoneyString(money)));
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Log inventory error", e);
			}
		}

		private static bool _IsLoggingEnabled(eInventoryActionType type)
		{
			if (!ServerProperties.Properties.LOG_INVENTORY)
				return false;

			switch (type)
			{
				case eInventoryActionType.Trade: return ServerProperties.Properties.LOG_INVENTORY_TRADE;
				case eInventoryActionType.Loot: return ServerProperties.Properties.LOG_INVENTORY_LOOT;
				case eInventoryActionType.Craft: return ServerProperties.Properties.LOG_INVENTORY_CRAFT;
				case eInventoryActionType.Merchant: return ServerProperties.Properties.LOG_INVENTORY_MERCHANT;
				case eInventoryActionType.Quest: return ServerProperties.Properties.LOG_INVENTORY_QUEST;
				case eInventoryActionType.Other: return ServerProperties.Properties.LOG_INVENTORY_OTHER;
			}
			return false;
		}
	}
}
