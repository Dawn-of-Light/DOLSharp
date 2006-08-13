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
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

using log4net;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&repair",
		(uint) ePrivLevel.Player,
		"You can repair an item when you are a crafter",
		"/repair")]
	public class RepairCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			GameInventoryItem item = client.Player.TargetObject as GameInventoryItem;
			if (item != null)
			{
				client.Player.RepairItem(item.Item);
				return 1;
			}
			GameKeepDoor door = client.Player.TargetObject as GameKeepDoor;
			if (door != null)
			{
				if (!PreFireChecks(client.Player, door)) return 1;
				StartRepair(client.Player, door);
			}
			GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
			if (component != null)
			{
				if (!PreFireChecks(client.Player, component)) return 1;
				StartRepair(client.Player, component);
			}
			return 1;
		}

		public static bool PreFireChecks(GamePlayer player, GameLiving obj)
		{
			if (obj == null)
				return false;
			if (player.Realm != obj.Realm)
				return false;

			if ((obj as GameLiving).InCombat)
			{
				player.Out.SendMessage("The can't repair object while it under attack!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if ((obj as GameLiving).HealthPercent == 100)
			{
				player.Out.SendMessage("The component is already at full health!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.IsCrafting)
			{
				player.Out.SendMessage("You must end your current action before you repair anything!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.IsMoving || player.Strafing)
			{
				player.Out.SendMessage("You move and stop repairing.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (!player.Alive)
			{
				player.Out.SendMessage("You can't repair while dead.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.Sitting)
			{
				player.Out.SendMessage("You can't repair while sit.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (!WorldMgr.CheckDistance(player, obj, WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to repair this component.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			int repairamount = (GetTotalWoodForLevel(obj.Level) / 100) * 15;
			int playerswood = CalculatePlayersWood(player, 0);

			if (playerswood < repairamount)
			{
				player.Out.SendMessage("You need another " + (playerswood - repairamount) + " units of wood!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < 1)
			{
				player.Out.SendMessage("You need woodworking skill to repair.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			player.Stealth(false);

			return true;
		}

		static int workDuration = 20;
		public static void StartRepair(GamePlayer player, GameLiving obj)
		{
			player.Out.SendTimerWindow("Repairing: " + obj.Name, workDuration);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(Proceed);
			player.CraftTimer.Properties.setProperty("repair_player", player);
			player.CraftTimer.Properties.setProperty("repair_target", obj);
			player.CraftTimer.Start(workDuration * 1000);
		}

		protected static int Proceed(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getObjectProperty("repair_player", null);
			GameLiving obj = (GameLiving)timer.Properties.getObjectProperty("repair_target", null);

			if (player == null || obj == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("There was a problem getting back the target or player in door/component repair!");
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();

			if (Util.ChanceDouble(CalculateRepairChance(player,obj)))
			{
				if (obj is GameKeepDoor)
				{
					GameKeepDoor door = obj as GameKeepDoor;
					door.Repair((int)(door.MaxHealth * 0.15));
				}
				if (obj is GameKeepComponent)
				{
					GameKeepComponent component = obj as GameKeepComponent;
					component.Repair((int)(component.MaxHealth * 0.15));
				}
				CalculatePlayersWood(player, (GetTotalWoodForLevel(obj.Level + 1)));
				player.Out.SendMessage("You successfully repair the component by 15%!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				player.Out.SendMessage("You fail to repair the component!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			return 0;
		}
		public static double CalculateRepairChance(GamePlayer player, GameObject obj)
		{
			double skill = player.GetCraftingSkillValue(eCraftingSkill.WoodWorking);
			int skillneeded = (obj.Level + 1) * 50;
			double chance = skill / skillneeded;
			return chance;
		}

		public static int GetTotalWoodForLevel(int level)
		{
			switch (level)
			{
				case 1: return 2;
				case 2: return 44;
				case 3: return 192;
				case 4: return 840;
				case 5: return 3576;
				case 6: return 8640;
				case 7: return 14400;
				case 8: return 27200;
				case 9: return 42432;
				case 10: return 68100;
				default: return 0;
			}
		}
		static string[] WoodNames = { "rowan", "elm", "oak", "ironwood", "heartwood", "runewood", "stonewood", "ebonwood", "dyrwood", "duskwood" };

		public static int CalculatePlayersWood(GamePlayer player, int removeamount)
		{
			int amount = 0;
			foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				foreach (string name in WoodNames)
				{
					if (item.Name.Replace(" wooden boards", "").ToLower() == name)
					{
						int woodvalue = GetWoodValue(item.Name.ToLower());
						amount += item.Count * woodvalue;
						if (removeamount > 0)
						{
							if (item.Count * woodvalue < removeamount)
							{
								int removecount = Math.Min(1, removeamount / woodvalue);
								removeamount -= removecount * woodvalue;
								player.Inventory.RemoveCountFromStack(item, removecount);
							}
							else
							{
								removeamount -= item.Count * woodvalue;
								player.Inventory.RemoveItem(item);
							}
						}
						break;
					}
				}
			}
			return amount;
		}

		public static int GetWoodValue(string name)
		{
			switch (name.Replace(" wooden boards", ""))
			{
				case "rowan": return 1;
				case "elm": return 4;
				case "oak": return 8;
				case "ironwood": return 16;
				case "heartwood": return 32;
				case "runewood": return 48;
				case "stonewood": return 60;
				case "ebonwood": return 80;
				case "dyrwood": return 104;
				case "duskwood": return 136;
				default: return 0;
			}
		}
	}
}