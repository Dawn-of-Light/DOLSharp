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
using DOL.Database;
using DOL.GS.Keeps;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&repair",
		ePrivLevel.Player,
		"You can repair an item when you are a crafter",
		"/repair")]
	public class RepairCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "repair"))
				return;

			WorldInventoryItem item = client.Player.TargetObject as WorldInventoryItem;
			if (item != null)
			{
				client.Player.RepairItem(item.Item);
				return;
			}
			GameKeepDoor door = client.Player.TargetObject as GameKeepDoor;
			if (door != null)
			{
				if (!PreFireChecks(client.Player, door)) return;
				StartRepair(client.Player, door);
			}
			GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
			if (component != null)
			{
				if (!PreFireChecks(client.Player, component)) return;
				StartRepair(client.Player, component);
			}
			GameSiegeWeapon weapon = client.Player.TargetObject as GameSiegeWeapon;
			if (weapon != null)
			{
				if (!PreFireChecks(client.Player, weapon)) return;
				StartRepair(client.Player, weapon);
			}
		}

		public bool PreFireChecks(GamePlayer player, GameLiving obj)
		{
			if (obj == null)
				return false;
			if (player.Realm != obj.Realm)
				return false;

			if (player.Client.Account.PrivLevel > (int)ePrivLevel.Player)
				return true;

			if ((obj as GameLiving).InCombat)
			{
				DisplayMessage(player, "You can't repair object while it is under attack!");
				return false;
			}

			if (obj is IKeepItem)
			{
				if (obj.CurrentRegion.Time - obj.LastAttackedByEnemyTick <= 60 * 1000)
				{
					DisplayMessage(player, "You can't repair the keep component while it is under attack!");
					return false;
				}
			}

			if ((obj as GameLiving).HealthPercent == 100)
			{
				DisplayMessage(player, "The component is already at full health!");
				return false;
			}
			if (obj is GameKeepComponent)
			{
				GameKeepComponent component = obj as GameKeepComponent;
				if (component.IsRaized)
				{
					DisplayMessage(player, "You cannot repair a raized tower!");
					return false;
				}
			}

			if (player.IsCrafting)
			{
				DisplayMessage(player, "You must end your current action before you repair anything!");
				return false;
			}
			if (player.IsMoving)
			{
				DisplayMessage(player, "You can't repair while moving");
				return false;
			}

			if (!player.IsAlive)
			{
				DisplayMessage(player, "You can't repair while dead.");
				return false;
			}

			if (player.IsSitting)
			{
				DisplayMessage(player, "You can't repair while sit.");
				return false;
			}

			if (!player.IsWithinRadius(obj, WorldMgr.INTERACT_DISTANCE))
			{
				DisplayMessage(player, "You are too far away to repair this component.");
				return false;
			}

			int repairamount = (GetTotalWoodForLevel(obj.Level) / 100) * 15;
			int playerswood = CalculatePlayersWood(player, 0);

			if (playerswood < repairamount)
			{
				DisplayMessage(player, "You need another " + (repairamount - playerswood) + " units of wood!");
				return false;
			}

			if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < 1)
			{
				DisplayMessage(player, "You need woodworking skill to repair.");
				return false;
			}

			player.Stealth(false);

			return true;
		}

		static int workDuration = 20;
		public void StartRepair(GamePlayer player, GameLiving obj)
		{
			player.Out.SendTimerWindow("Repairing: " + obj.Name, workDuration);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(Proceed);
			player.CraftTimer.Properties.setProperty("repair_player", player);
			player.CraftTimer.Properties.setProperty("repair_target", obj);
			player.CraftTimer.Start(workDuration * 1000);
		}

		protected int Proceed(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getProperty<object>("repair_player", null);
			GameLiving obj = (GameLiving)timer.Properties.getProperty<object>("repair_target", null);

			if (player == null || obj == null)
			{
				if (Log.IsWarnEnabled)
					Log.Warn("There was a problem getting back the target or player in door/component repair!");
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();

			if (!PreFireChecks(player, obj))
				return 0;

			if (Util.ChanceDouble(CalculateRepairChance(player,obj)))
			{
				int start = obj.HealthPercent;
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
				if (obj is GameSiegeWeapon)
				{
					GameSiegeWeapon weapon = obj as GameSiegeWeapon;
					weapon.Repair();
				}
				int finish = obj.HealthPercent;
				CalculatePlayersWood(player, (GetTotalWoodForLevel(obj.Level + 1)));
				DisplayMessage(player, "You successfully repair the component by 15%!");
				/*
				 * - Realm points will now be awarded for successfully repairing a door or outpost piece.
				 * Players will receive approximately 10% of the amount repaired in realm points.
				 * (Note that realm points for repairing a door or outpost piece will not work in the battlegrounds.)
				 */
				// tolakram - we have no idea how many hit points a live door has so this code is not accurate
				int amount = (finish - start) * obj.Level;  // level of non claimed keep is 4
				player.GainRealmPoints(Math.Min(150, amount));
			}
			else
			{
				DisplayMessage(player, "You fail to repair the component!");
			}

			return 0;
		}
		public static double CalculateRepairChance(GamePlayer player, GameObject obj)
		{
			if (player.Client.Account.PrivLevel > (int)ePrivLevel.Player)
				return 100;

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