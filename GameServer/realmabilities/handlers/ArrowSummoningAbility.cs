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
using DOL.GS.Effects;
using DOL.Database;
using System.Collections;
using DOL.Events;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
    public class ArrowSummoningAbility : TimedRealmAbility
	{
        public ArrowSummoningAbility(DBAbility dba, int level) : base(dba, level) { }
        public override void Execute(GameLiving living)
		{
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;
            GameInventoryItem as1 = GameInventoryItem.CreateFromTemplate("arrow_summoning1");
            GameInventoryItem as2 = GameInventoryItem.CreateFromTemplate("arrow_summoning2");
            GameInventoryItem as3 = GameInventoryItem.CreateFromTemplate("arrow_summoning3");

			if(!player.Inventory.AddTemplate(as1.Item,10,eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				player.Out.SendMessage("You do not have enough inventory space to place this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else if(!player.Inventory.AddTemplate(as2.Item,10,eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
                player.Out.SendMessage("You do not have enough inventory space to place this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else if(!player.Inventory.AddTemplate(as3.Item,10,eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
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
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;		
			lock(player.Inventory)
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
			ItemTemplate arrow_summoning1 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arrow_summoning1");
			if (arrow_summoning1 == null)
			{
				arrow_summoning1 = new ItemTemplate();
				arrow_summoning1.Name = "mystical barbed footed flight broadhead arrows";
				arrow_summoning1.Level = 1;
				arrow_summoning1.Durability = 100;
				arrow_summoning1.MaxDurability = 100;
				arrow_summoning1.Condition = 50000;
				arrow_summoning1.MaxCondition = 50000;
				arrow_summoning1.Quality = 100;
				arrow_summoning1.MaxQuality = 100;
				arrow_summoning1.DPS_AF = 0;
				arrow_summoning1.SPD_ABS = 47;
				arrow_summoning1.Hand = 0;
				arrow_summoning1.Type_Damage = 3;
				arrow_summoning1.Object_Type = 43;
				arrow_summoning1.Item_Type = 40;
				arrow_summoning1.Weight = 0;
				arrow_summoning1.Model = 1635;
				arrow_summoning1.IsPickable = true;
				arrow_summoning1.IsDropable = false;
				arrow_summoning1.IsTradable = false;
				arrow_summoning1.MaxCount = 20;
				arrow_summoning1.Id_nb = "arrow_summoning1";
				GameServer.Database.AddNewObject(arrow_summoning1);
				if (log.IsDebugEnabled)
					log.Debug("Added " + arrow_summoning1.Id_nb);
			}
			ItemTemplate arrow_summoning2 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arrow_summoning2");
			if (arrow_summoning2 == null)
			{
				arrow_summoning2 = new ItemTemplate();
				arrow_summoning2.Name = "mystical keen footed flight broadhead arrows";
				arrow_summoning2.Level = 1;
				arrow_summoning2.Durability = 100;
				arrow_summoning2.MaxDurability = 100;
				arrow_summoning2.Condition = 50000;
				arrow_summoning2.MaxCondition = 50000;
				arrow_summoning2.Quality = 100;
				arrow_summoning2.MaxQuality = 100;
				arrow_summoning2.DPS_AF = 0;
				arrow_summoning2.SPD_ABS = 47;
				arrow_summoning2.Hand = 0;
				arrow_summoning2.Type_Damage = 3;
				arrow_summoning2.Object_Type = 43;
				arrow_summoning2.Item_Type = 40;
				arrow_summoning2.Weight = 0;
				arrow_summoning2.Model = 1635;
				arrow_summoning2.IsPickable = true;
				arrow_summoning2.IsDropable = false;
				arrow_summoning2.IsTradable = false;
				arrow_summoning2.MaxCount = 20;
				arrow_summoning2.Id_nb = "arrow_summoning2";
				GameServer.Database.AddNewObject(arrow_summoning2);
				if (log.IsDebugEnabled)
					log.Debug("Added " + arrow_summoning2.Id_nb);
			}
			ItemTemplate arrow_summoning3 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "arrow_summoning3");
			if (arrow_summoning3 == null)
			{
				arrow_summoning3 = new ItemTemplate();
				arrow_summoning3.Name = "mystical blunt footed flight broadhead arrows";
				arrow_summoning3.Level = 1;
				arrow_summoning3.Durability = 100;
				arrow_summoning3.MaxDurability = 100;
				arrow_summoning3.Condition = 50000;
				arrow_summoning3.MaxCondition = 50000;
				arrow_summoning3.Quality = 100;
				arrow_summoning3.MaxQuality = 100;
				arrow_summoning3.DPS_AF = 0;
				arrow_summoning3.SPD_ABS = 47;
				arrow_summoning3.Hand = 0;
				arrow_summoning3.Type_Damage = 3;
				arrow_summoning3.Object_Type = 43;
				arrow_summoning3.Item_Type = 40;
				arrow_summoning3.Weight = 0;
				arrow_summoning3.Model = 1635;
				arrow_summoning3.IsPickable = true;
				arrow_summoning3.IsDropable = false;
				arrow_summoning3.IsTradable = false;
				arrow_summoning3.MaxCount = 20;
				arrow_summoning3.Id_nb = "arrow_summoning3";
				GameServer.Database.AddNewObject(arrow_summoning3);
				if (log.IsDebugEnabled)
					log.Debug("Added " + arrow_summoning3.Id_nb);
			}
		}
	}
}
