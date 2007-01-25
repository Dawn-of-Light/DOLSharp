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
/*
 * Author:		Gandulf Kohlweiss
 * Date:
 * Directory: /scripts/quests/
 *
 * Description:
 *  Brief Walkthrough:
 */

using System;
using System.Collections;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using DOL.Events;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests
{

	/// <summary>
	/// BaseQuest provides some helper classes for writing quests and
	/// integrates a new QuestPart Based QuestSystem.
	/// </summary>
	/// <seealso cref="DOL.GS.Quests.BaseQuestPart"/>
	public abstract class BaseQuest : AbstractQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		/// <summary>
		/// Global Constant for all quests to define wether npcs and items should be saved in db or not.
		/// </summary>
		public static bool SAVE_INTO_DATABASE = false;

		public static Queue m_sayTimerQueue = new Queue();
		public static Queue m_sayObjectQueue = new Queue();
		public static Queue m_sayMessageQueue = new Queue();
		public static Queue m_sayChatTypeQueue = new Queue();
		public static Queue m_sayChatLocQueue = new Queue();

		public Queue m_animEmoteTeleportTimerQueue = new Queue();
		public Queue m_animEmoteObjectQueue = new Queue();

		public Queue m_animSpellTeleportTimerQueue = new Queue();
		public Queue m_animSpellObjectQueue = new Queue();

		public Queue m_portTeleportTimerQueue = new Queue();
		public Queue m_portObjectQueue = new Queue();
		public Queue m_portDestinationQueue = new Queue();

		// /// <summary>
		// /// List of all QuestParts that can be fired on interact Events.
		// /// </summary>
		//private static IDictionary interactQuestParts = new HybridDictionary();

		/// <summary>
		/// List of all QuestParts that can be fired on notify method of quest.
		/// </summary>
		private static IList questParts = null;

		/// <summary>
		/// Create an empty Quest
		/// </summary>
		public BaseQuest()
			: base()
		{
		}

		/// <summary>
		/// Constructs a new empty Quest
		/// </summary>
		public BaseQuest(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		/// <summary>
		/// Constructs a new Quest
		/// </summary>
		/// <param name="questingPlayer">The player doing this quest</param>
		/// <param name="step">The current step the player is on</param>
		public BaseQuest(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		/// <summary>
		/// Constructs a new Quest from a database Object
		/// </summary>
		/// <param name="questingPlayer">The player doing the quest</param>
		/// <param name="dbQuest">The database object</param>
		public BaseQuest(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
		}


		[ScriptUnloadedEvent]
		public static void ScriptUnloadedBase(DOLEvent e, object sender, EventArgs args)
		{
			if (questParts != null)
			{
				for (int i = questParts.Count - 1; i >= 0; i--)
				{
					RemoveQuestPart((BaseQuestPart)questParts[i]);
				}
			}
			questParts = null;
		}

		// Base QuestPart methods

		/// <summary>
		/// Registers all needed handlers for the given questPart,
		/// this will not add the questpart to the quest. For this case use AddQuestPart
		/// </summary>
		/// <param name="questPart">QuestPart to register handlers for</param>
		protected static void RegisterQuestPart(BaseQuestPart questPart)
		{
			if (questPart.Triggers == null)
				log.Warn("QuestPart without any triggers added, this questpart will never be notified.\n Details: " + questPart);

			foreach (IQuestTrigger trigger in questPart.Triggers)
			{
				trigger.Register();
			}
		}

		/// <summary>
		/// Remove all registered handlers for this quest,
		/// this will not remove the questPart from the quest.
		/// </summary>
		/// <param name="questPart">QuestPart to remove handlers from</param>
		protected static void UnRegisterQuestPart(BaseQuestPart questPart)
		{
			if (questPart.Triggers == null)
				return;

			foreach (IQuestTrigger trigger in questPart.Triggers)
			{
				trigger.Unregister();
			}
		}
		/// <summary>
		/// Adds the given questpart to the quest depending on the added triggers it will either
		/// be added as InteractQuestPart as NotifyQuestPart or both and also register the needed event handler.
		/// </summary>
		/// <param name="questPart">QuestPart to be added</param>
		public static void AddQuestPart(BaseQuestPart questPart)
		{
			if (questPart.QuestPartAdded)
				log.Error("QuestPart " + questPart + " was already added to Quest.");

			RegisterQuestPart(questPart);

			if (questParts == null)
				questParts = new ArrayList();

			if (!questParts.Contains(questPart))
				questParts.Add(questPart);

			questPart.QuestPartAdded = true;
		}

		/// <summary>
		/// Remove the given questpart from the quest and also unregister the handlers
		/// </summary>
		/// <param name="questPart">QuestPart to be removed</param>
		public static void RemoveQuestPart(BaseQuestPart questPart)
		{
			if (questParts == null)
				return;

			UnRegisterQuestPart(questPart);
			questParts.Remove(questPart);
			questPart.QuestPartAdded = false;
		}

		/// <summary>
		/// Quest internal Notify method only fires if player already has the quest assigned
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (questParts == null)
				return;

			foreach (BaseQuestPart questPart in questParts)
			{
				questPart.Notify(e, sender, args);
			}
		}

		#region Items

		protected static void RemoveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			RemoveItem(null, player, itemTemplate, true);
		}

		protected static void RemoveItem(GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			RemoveItem(null, player, itemTemplate, notify);
		}

		protected static void RemoveItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate)
		{
			RemoveItem(target, player, itemTemplate, true);
		}

		protected static void ReplaceItem(GamePlayer target, ItemTemplate itemTemplateOut, ItemTemplate itemTemplateIn)
		{
			target.Inventory.BeginChanges();
			RemoveItem(target, itemTemplateOut, false);
			GiveItem(target, itemTemplateIn);
			target.Inventory.CommitChanges();
		}

		protected static void RemoveItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			lock (player.Inventory)
			{
				InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				if (item != null)
				{
					player.Inventory.RemoveItem(item);
					if (target != null)
					{
						player.Out.SendMessage("You give the " + itemTemplate.Name + " to " + target.GetName(0, false), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				else if (notify)
				{
					player.Out.SendMessage("You cannot remove the \"" + itemTemplate.Name + "\" because you don't have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}
		#endregion

		protected static int MakeSaySequence(RegionTimer callingTimer)
		{
			m_sayTimerQueue.Dequeue();
			GamePlayer player = (GamePlayer)m_sayObjectQueue.Dequeue();
			String message = (String)m_sayMessageQueue.Dequeue();
			eChatType chatType = (eChatType)m_sayChatTypeQueue.Dequeue();
			eChatLoc chatLoc = (eChatLoc)m_sayChatLocQueue.Dequeue();

			player.Out.SendMessage(message, chatType, chatLoc);

			return 0;
		}


		protected void SendSystemMessage(String msg)
		{
			SendSystemMessage(m_questPlayer, msg);
		}

		protected void SendEmoteMessage(String msg)
		{
			SendEmoteMessage(m_questPlayer, msg, 0);
		}

		protected static void SendSystemMessage(GamePlayer player, String msg)
		{
			SendEmoteMessage(player, msg, 0);
		}

		protected static void SendSystemMessage(GamePlayer player, String msg, uint delay)
		{
			SendMessage(player, msg, delay, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		protected static void SendEmoteMessage(GamePlayer player, String msg)
		{
			SendEmoteMessage(player, msg, 0);
		}

		protected static void SendEmoteMessage(GamePlayer player, String msg, uint delay)
		{
			SendMessage(player, msg, delay, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
		}

		protected static void SendReply(GamePlayer player, String msg)
		{
			SendMessage(player, msg, 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}

		protected static void SendMessage(GamePlayer player, String msg, uint delay, eChatType chatType, eChatLoc chatLoc)
		{
			if (delay == 0)
				player.Out.SendMessage(msg, chatType, chatLoc);
			else
			{
				m_sayMessageQueue.Enqueue(msg);
				m_sayObjectQueue.Enqueue(player);
				m_sayChatLocQueue.Enqueue(chatLoc);
				m_sayChatTypeQueue.Enqueue(chatType);
				m_sayTimerQueue.Enqueue(new RegionTimer(player, new RegionTimerCallback(MakeSaySequence), (int)delay * 100));
			}
		}

		protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			GiveItem(null, player, itemTemplate);
		}

		protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			InventoryItem item = new InventoryItem(itemTemplate);
			if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
			{
				if (source == null)
				{
					player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				player.CreateItemOnTheGround(item);
				player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
			}
		}

		protected static ItemTemplate CreateTicketTo(String location)
		{
			ItemTemplate ticket = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "ticket_to_" + GameServer.Database.Escape(location.ToLower()));
			if (ticket == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ticket to " + location + ", creating it ...");
				ticket = new ItemTemplate();
				ticket.Name = "ticket to " + location;

				ticket.Weight = 0;
				ticket.Model = 498;

				ticket.Object_Type = (int)eObjectType.GenericItem;
				ticket.Item_Type = 40;

				ticket.Id_nb = "ticket_to_" + location.ToLower();
				;
				ticket.IsPickable = true;
				ticket.IsDropable = true;

				ticket.Gold = 0;
				ticket.Silver = 5;
				ticket.Copper = 3;
				ticket.PackSize = 1;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				GameServer.Database.AddNewObject(ticket);
			}
			return ticket;
		}

		//timer callbacks
		protected virtual int MakeAnimSpellSequence(RegionTimer callingTimer)
		{
			if (m_animSpellTeleportTimerQueue.Count > 0)
			{
				m_animSpellTeleportTimerQueue.Dequeue();
				GameLiving animObject = (GameLiving)m_animSpellObjectQueue.Dequeue();
				foreach (GamePlayer player in animObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendSpellCastAnimation(animObject, 1, 20);
				}
			}
			return 0;
		}

		protected virtual int MakeAnimEmoteSequence(RegionTimer callingTimer)
		{
			if (m_animEmoteTeleportTimerQueue.Count > 0)
			{
				m_animEmoteTeleportTimerQueue.Dequeue();
				GameLiving animObject = (GameLiving)m_animEmoteObjectQueue.Dequeue();
				foreach (GamePlayer player in animObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendEmoteAnimation(animObject, eEmote.Bind);
				}
			}
			return 0;
		}

		protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location)
		{
			TeleportTo(target, caster, location, 0, 0);
		}

		protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location, uint delay)
		{
			TeleportTo(target, caster, location, delay, 0);
		}

		protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location, uint delay, int fuzzyLocation)
		{
			delay *= 100; // 1/10sec to milliseconds
			if (delay <= 0)
				delay = 1;
			m_animSpellObjectQueue.Enqueue(caster);
			m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(caster, new RegionTimerCallback(MakeAnimSpellSequence), (int)delay));

			m_animEmoteObjectQueue.Enqueue(target);
			m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(target, new RegionTimerCallback(MakeAnimEmoteSequence), (int)delay + 2000));

			m_portObjectQueue.Enqueue(target);

			location.X += Util.Random(0 - fuzzyLocation, fuzzyLocation);
			location.Y += Util.Random(0 - fuzzyLocation, fuzzyLocation);

			m_portDestinationQueue.Enqueue(location);
			m_portTeleportTimerQueue.Enqueue(new RegionTimer(target, new RegionTimerCallback(MakePortSequence), (int)delay + 3000));

			if (location.Name != null)
			{
				m_questPlayer.Out.SendMessage(target.Name + " is being teleported to " + location.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

		}

		protected virtual int MakePortSequence(RegionTimer callingTimer)
		{
			if (m_portTeleportTimerQueue.Count > 0)
			{
				m_portTeleportTimerQueue.Dequeue();
				GameObject gameObject = (GameObject)m_portObjectQueue.Dequeue();
				GameLocation location = (GameLocation)m_portDestinationQueue.Dequeue();
				gameObject.MoveTo(location.RegionID, location.X, location.Y, location.Z, location.Heading);
			}
			return 0;
		}
	}
}
