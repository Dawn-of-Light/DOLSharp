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
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Events;
using log4net;

namespace DOL.GS.RealmAbilities
{
    public class ArrowSummoningAbility : TimedRealmAbility
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ArrowSummoningAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (!(living is GamePlayer player))
            {
                return;
            }

            ItemTemplate arrowSummoning1 = GameServer.Database.FindObjectByKey<ItemTemplate>("arrow_summoning1");
            ItemTemplate arrowSummoning2 = GameServer.Database.FindObjectByKey<ItemTemplate>("arrow_summoning2");
            ItemTemplate arrowSummoning3 = GameServer.Database.FindObjectByKey<ItemTemplate>("arrow_summoning3");

            if (!player.Inventory.AddTemplate(GameInventoryItem.Create(arrowSummoning1),10,eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                player.Out.SendMessage("You do not have enough inventory space to place this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else if (!player.Inventory.AddTemplate(GameInventoryItem.Create(arrowSummoning2), 10, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                player.Out.SendMessage("You do not have enough inventory space to place this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else if (!player.Inventory.AddTemplate(GameInventoryItem.Create(arrowSummoning3), 10, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                player.Out.SendMessage("You do not have enough inventory space to place this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            GameEventMgr.AddHandler(player,GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            switch (level)
            {
                case 1: return 900;
                case 2: return 300;
                case 3: return 5;
            }

            return 600;
        }

        public void PlayerQuit(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer player))
            {
                return;
            }

            lock (player.Inventory)
            {
                InventoryItem item = player.Inventory.GetFirstItemByID("arrow_summoning1", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                while (item != null)
                {
                    player.Inventory.RemoveItem(item);
                    item = player.Inventory.GetFirstItemByID("arrow_summoning1", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                }

                item = player.Inventory.GetFirstItemByID("arrow_summoning2", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                while (item != null)
                {
                    player.Inventory.RemoveItem(item);
                    item = player.Inventory.GetFirstItemByID("arrow_summoning2", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                }

                item = player.Inventory.GetFirstItemByID("arrow_summoning3", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                while (item != null)
                {
                    player.Inventory.RemoveItem(item);
                    item = player.Inventory.GetFirstItemByID("arrow_summoning3", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                }
            }
        }

        [ScriptLoadedEvent]
        public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_ARROW_SUMMONING)
            {
                return;
            }

            ItemTemplate arrowSummoning1 = GameServer.Database.FindObjectByKey<ItemTemplate>("arrow_summoning1");
            if (arrowSummoning1 == null)
            {
                arrowSummoning1 = new ItemTemplate
                {
                    Name = "mystical barbed footed flight broadhead arrows",
                    Level = 1,
                    MaxDurability = 100,
                    MaxCondition = 50000,
                    Quality = 100,
                    DPS_AF = 0,
                    SPD_ABS = 47,
                    Hand = 0,
                    Type_Damage = 3,
                    Object_Type = 43,
                    Item_Type = 40,
                    Weight = 0,
                    Model = 1635,
                    IsPickable = true,
                    IsDropable = false,
                    IsTradable = false,
                    MaxCount = 20,
                    Id_nb = "arrow_summoning1"
                };

                GameServer.Database.AddObject(arrowSummoning1);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"Added {arrowSummoning1.Id_nb}");
                }
            }

            ItemTemplate arrowSummoning2 = GameServer.Database.FindObjectByKey<ItemTemplate>("arrow_summoning2");
            if (arrowSummoning2 == null)
            {
                arrowSummoning2 = new ItemTemplate
                {
                    Name = "mystical keen footed flight broadhead arrows",
                    Level = 1,
                    MaxDurability = 100,
                    MaxCondition = 50000,
                    Quality = 100,
                    DPS_AF = 0,
                    SPD_ABS = 47,
                    Hand = 0,
                    Type_Damage = 3,
                    Object_Type = 43,
                    Item_Type = 40,
                    Weight = 0,
                    Model = 1635,
                    IsPickable = true,
                    IsDropable = false,
                    IsTradable = false,
                    MaxCount = 20,
                    Id_nb = "arrow_summoning2"
                };

                GameServer.Database.AddObject(arrowSummoning2);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"Added {arrowSummoning2.Id_nb}");
                }
            }

            ItemTemplate arrowSummoning3 = GameServer.Database.FindObjectByKey<ItemTemplate>("arrow_summoning3");
            if (arrowSummoning3 == null)
            {
                arrowSummoning3 = new ItemTemplate
                {
                    Name = "mystical blunt footed flight broadhead arrows",
                    Level = 1,
                    MaxDurability = 100,
                    MaxCondition = 50000,
                    Quality = 100,
                    DPS_AF = 0,
                    SPD_ABS = 47,
                    Hand = 0,
                    Type_Damage = 3,
                    Object_Type = 43,
                    Item_Type = 40,
                    Weight = 0,
                    Model = 1635,
                    IsPickable = true,
                    IsDropable = false,
                    IsTradable = false,
                    MaxCount = 20,
                    Id_nb = "arrow_summoning3"
                };

                GameServer.Database.AddObject(arrowSummoning3);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"Added {arrowSummoning3.Id_nb}");
                }
            }
        }
    }
}
