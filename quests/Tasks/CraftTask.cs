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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Declares a Craft task.
	/// craft Item for NPC
	/// </summary>
	public class CraftTask : AbstractTask
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string RECIEVER_ZONE = "recieverZone";

		//public string ItemName;
		public const double RewardMoneyRatio = 1.25;
		/// <summary>
		/// Constructs a new Task
		/// </summary>
		/// <param name="taskPlayer">The player doing this task</param>
		public CraftTask(GamePlayer taskPlayer)
			: base(taskPlayer)
		{
		}

		/// <summary>
		/// Constructs a new Task from a database Object
		/// </summary>
		/// <param name="taskPlayer">The player doing the task</param>
		/// <param name="dbTask">The database object</param>
		public CraftTask(GamePlayer taskPlayer, DBTask dbTask)
			: base(taskPlayer, dbTask)
		{
		}

		private long m_rewardmoney = 0;
		public override long RewardMoney
		{
			get
			{
				return m_rewardmoney;
			}
		}

		public void SetRewardMoney(long money)
		{
			m_rewardmoney = money;
		}

		public override IList RewardItems
		{
			get { return null; }
		}

		/// <summary>
		/// Retrieves the name of the task
		/// </summary>
		public override string Name
		{
			get { return "Craft Task"; }
		}

		/// <summary>
		/// Retrieves the description
		/// </summary>
		public override string Description
		{
			get { return "Craft the " + ItemName + " for " + RecieverName + " in " + RecieverZone; }
		}


		/// <summary>
		/// Zone related to task stored in dbTask
		/// </summary>
		public virtual String RecieverZone
		{
			get { return GetCustomProperty(RECIEVER_ZONE); }
			set { SetCustomProperty(RECIEVER_ZONE, value); }
		}

		/// <summary>
		/// Called to finish the task.
		/// Should be overridden and some rewards given etc.
		/// </summary>
		public override void FinishTask()
		{
			base.FinishTask();
		}

		/// <summary>
		/// This method needs to be implemented in each task.
		/// It is the core of the task. The global event hook of the GamePlayer.
		/// This method will be called whenever a GamePlayer with this task
		/// fires ANY event!
		/// </summary>
		/// <param name="e">The event type</param>
		/// <param name="sender">The sender of the event</param>
		/// <param name="args">The event arguments</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			// Filter only the events from task owner
			if (sender != m_taskPlayer)
				return;

			if (CheckTaskExpired())
			{
				return;
			}

			GamePlayer player = (GamePlayer)sender;

			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				GameLiving target = gArgs.Target as GameLiving;
				InventoryItem item = gArgs.Item;

				if (player.Task.RecieverName == target.Name && item.Name == player.Task.ItemName)
				{
					player.Inventory.RemoveItem(item);
					FinishTask();
				}
			}
		}

		/// <summary>
		/// Generate an Item random Named for NPC Drop
		/// </summary>
		/// <param name="player">Level of Generated Item</param>
		/// <returns>A Generated NPC Item</returns>
		public static ItemTemplate GenerateNPCItem(GamePlayer player)
		{
			int mediumCraftingLevel = player.GetCraftingSkillValue(player.CraftingPrimarySkill) + 20;
			int lowLevel = mediumCraftingLevel - 20;
			int highLevel = mediumCraftingLevel + 20;

			var craftitem = GameServer.Database.SelectObjects<DBCraftedItem>("CraftingSkillType=" + (int)player.CraftingPrimarySkill + " AND CraftingLevel>" + lowLevel + " AND CraftingLevel<" + highLevel);
			int craftrnd = Util.Random(craftitem.Count);

			ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(craftitem[craftrnd].Id_nb);
			return template;
		}

		/// <summary>
		/// Create an Item, Search for a NPC to consign the Item and give Item to the Player
		/// </summary>
		/// <param name="player">The GamePlayer Object</param>
		/// <param name="source">The source of the task</param>
		public static bool BuildTask(GamePlayer player, GameLiving source)
		{
			if (source == null)
				return false;

			GameNPC NPC = GetRandomNPC(player);
			if (NPC == null)
			{
				player.Out.SendMessage("I have no task for you, come back some time later.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return false;
			}

			ItemTemplate taskItem = GenerateNPCItem(player);

			if (taskItem == null)
			{
				player.Out.SendMessage("I can't think of anything for you to make, perhaps you should ask again.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				log.ErrorFormat("Craft task item is null for player {0} at level {1}.", player.Name, player.Level);
				return false;
			}

			var craftTask = new CraftTask(player)
								{
									TimeOut = DateTime.Now.AddHours(2),
									ItemName = taskItem.Name,
									RecieverName = NPC.Name,
									RecieverZone = NPC.CurrentZone.Description
								};

			craftTask.SetRewardMoney((long)(taskItem.Price * RewardMoneyRatio));

			player.Task = craftTask;

			player.Out.SendMessage("Craft " + taskItem.GetName(0, false) + " for " + NPC.Name + " in " + NPC.CurrentZone.Description, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return true;

		}

		/// <summary>
		/// Find a Random NPC
		/// </summary>
		/// <param name="Player">The GamePlayer Object</param>		
		/// <returns>The GameNPC Searched</returns>
		public static GameNPC GetRandomNPC(GamePlayer Player)
		{
			return Player.CurrentZone.GetRandomNPC(new eRealm[] { eRealm.Albion, eRealm.Hibernia, eRealm.Midgard });
		}

		public new static bool CheckAvailability(GamePlayer player, GameLiving target)
		{
			if (target == null)
				return false;

			if (target is CraftNPC)
			{
				if (((target as CraftNPC).TheCraftingSkill == player.CraftingPrimarySkill))
					return AbstractTask.CheckAvailability(player, target, CHANCE);
			}
			return false;//else return false
		}
	}
}
