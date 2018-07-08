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
using log4net;

namespace DOL.GS.Quests
{
    /// <summary>
    /// Declares a Craft task.
    /// craft Item for NPC
    /// </summary>
    public class CraftTask : AbstractTask
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string RECIEVER_ZONE = "recieverZone";

        // public string ItemName;
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

        private long _rewardmoney;

        public override long RewardMoney => _rewardmoney;

        public void SetRewardMoney(long money)
        {
            _rewardmoney = money;
        }

        public override IList RewardItems => null;

        /// <summary>
        /// Retrieves the name of the task
        /// </summary>
        public override string Name => "Craft Task";

        /// <summary>
        /// Retrieves the description
        /// </summary>
        public override string Description => $"Craft the {ItemName} for {RecieverName} in {RecieverZone}";

        /// <summary>
        /// Zone related to task stored in dbTask
        /// </summary>
        public virtual string RecieverZone
        {
            get => GetCustomProperty(RECIEVER_ZONE);
            set => SetCustomProperty(RECIEVER_ZONE, value);
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
            {
                return;
            }

            if (CheckTaskExpired())
            {
                return;
            }

            GamePlayer player = (GamePlayer)sender;

            if (e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                if (!(gArgs.Target is GameLiving target))
                {
                    return;
                }

                InventoryItem item = gArgs.Item;

                if (player.Task.RecieverName == target.Name && item.Name == player.Task.ItemName)
                {
                    player.Inventory.RemoveItem(item);
                    InventoryLogging.LogInventoryAction(player, target, eInventoryActionType.Quest, item.Template, item.Count);
                    FinishTask();
                }
            }
        }

        /// <summary>
        /// Generate an Item random Named for NPC Drop
        /// </summary>
        /// <param name="player">Level of Generated Item</param>
        /// <returns>A Generated NPC Item</returns>
        public static ItemTemplate GenerateNpcItem(GamePlayer player)
        {
            int mediumCraftingLevel = player.GetCraftingSkillValue(player.CraftingPrimarySkill) + 20;
            int lowLevel = mediumCraftingLevel - 20;
            int highLevel = mediumCraftingLevel + 20;

            var craftitem = GameServer.Database.SelectObjects<DBCraftedItem>("`CraftingSkillType` = @CraftingSkillType AND `CraftingLevel` > @CraftingLevelLow AND `CraftingLevel` < @CraftingLevelHigh", new[] { new QueryParameter("@CraftingSkillType", (int)player.CraftingPrimarySkill), new QueryParameter("@CraftingLevelLow", lowLevel), new QueryParameter("@CraftingLevelHigh", highLevel) });
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
            {
                return false;
            }

            GameNPC npc = GetRandomNpc(player);
            if (npc == null)
            {
                player.Out.SendMessage("I have no task for you, come back some time later.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }

            ItemTemplate taskItem = GenerateNpcItem(player);

            if (taskItem == null)
            {
                player.Out.SendMessage("I can't think of anything for you to make, perhaps you should ask again.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                Log.Error($"Craft task item is null for player {player.Name} at level {player.Level}.");
                return false;
            }

            var craftTask = new CraftTask(player)
                                {
                                    TimeOut = DateTime.Now.AddHours(2),
                                    ItemName = taskItem.Name,
                                    RecieverName = npc.Name,
                                    RecieverZone = npc.CurrentZone.Description
                                };

            craftTask.SetRewardMoney((long)(taskItem.Price * RewardMoneyRatio));

            player.Task = craftTask;

            player.Out.SendMessage($"Craft {taskItem.GetName(0, false)} for {npc.Name} in {npc.CurrentZone.Description}", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            return true;
        }

        /// <summary>
        /// Find a Random NPC
        /// </summary>
        /// <param name="player">The GamePlayer Object</param>
        /// <returns>The GameNPC Searched</returns>
        public static GameNPC GetRandomNpc(GamePlayer player)
        {
            return player.CurrentZone.GetRandomNPC(new[] { eRealm.Albion, eRealm.Hibernia, eRealm.Midgard });
        }

        public new static bool CheckAvailability(GamePlayer player, GameLiving target)
        {
            if ((target as CraftNPC)?.TheCraftingSkill == player.CraftingPrimarySkill)
            {
                return CheckAvailability(player, target, Chance);
            }

            return false;// else return false
        }
    }
}
