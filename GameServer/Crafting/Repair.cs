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
using System.Reflection;
using DOL.Database2;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The class holding all repair functions
	/// </summary>
	public class Repair
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaration

		/// <summary>
		/// The player currently crafting
		/// </summary>
		protected const string PLAYER_CRAFTER = "PLAYER_CRAFTER";

		/// <summary>
		/// The player currently crafting
		/// </summary>
		protected const string PLAYER_PARTNER = "PLAYER_PARTNER";

		/// <summary>
		/// The item in construction
		/// </summary>
		protected const string ITEM_CRAFTER = "ITEM_CRAFTER";

		#endregion

		#region First call function and callback

		/// <summary>
		/// Called when player try to use a secondary crafting skill
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static int BeginWork(GamePlayer player, InventoryItem item)
		{
			if (!IsAllowedToBeginWork(player, item, 50))
			{
				return 0;
			}

			GamePlayer tradePartner = null;
			if (player.TradeWindow != null) tradePartner = player.TradeWindow.Partner;

			if (player.IsMoving || player.IsStrafing)
			{
				player.Out.SendMessage("You move and stop to repair the " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (tradePartner != null) tradePartner.Out.SendMessage(player.Name + " move and stop to repair the " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			player.Out.SendMessage("You begin repairing the " + item.Name + " (" + CalculateSuccessChances(player, item).ToString() + "%).", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			if (tradePartner != null) tradePartner.Out.SendMessage(player.Name + " begin repairing the " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			int workDuration = GetCraftingTime(player, item);
			player.Out.SendTimerWindow("Repairing: " + item.Name, workDuration);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(Proceed);
			player.CraftTimer.Properties.setProperty(PLAYER_CRAFTER, player);
			player.CraftTimer.Properties.setProperty(PLAYER_PARTNER, tradePartner);
			player.CraftTimer.Properties.setProperty(ITEM_CRAFTER, item);
			player.CraftTimer.Start(workDuration * 1000);
			return 1;
		}

		/// <summary>
		/// Called when craft time is finished 
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		protected static int Proceed(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getObjectProperty(PLAYER_CRAFTER, null);
			GamePlayer tradePartner = (GamePlayer)timer.Properties.getObjectProperty(PLAYER_PARTNER, null);
			InventoryItem item = (InventoryItem)timer.Properties.getObjectProperty(ITEM_CRAFTER, null);

			if (player == null || item == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("There was a problem getting back the item to the player in the secondary craft system.");
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();

			if (Util.Chance(CalculateSuccessChances(player, item)))
			{
				int toRecoverCond = (int)((item.MaxCondition - item.Condition) * 0.01 / item.MaxCondition) + 1;
				if (toRecoverCond >= item.Durability)
				{
					item.Condition += (int)(item.Durability * item.MaxCondition / 0.01);
					item.Durability = 0;
				}
				else
				{
					item.Condition = item.MaxCondition;
					item.Durability -= toRecoverCond;
				}

				player.Out.SendInventorySlotsUpdate(new int[] { item.SlotPosition });

				player.Out.SendMessage("You have fully repaired the " + item.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (tradePartner != null) tradePartner.Out.SendMessage(player.Name + " have fully repaired the " + item.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				player.Out.SendMessage("You fail to improve the " + item.Name + "'s condition.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (tradePartner != null) tradePartner.Out.SendMessage(player.Name + " fail to improve the " + item.Name + "'s condition.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			return 0;
		}

		#endregion

		#region Requirement check

		/// <summary>
		/// Check if the player own can enchant the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="percentNeeded">min 50 max 100</param>
		/// <returns></returns>
		public static bool IsAllowedToBeginWork(GamePlayer player, InventoryItem item, int percentNeeded)
		{
			if (item.SlotPosition < (int)eInventorySlot.FirstBackpack || item.SlotPosition > (int)eInventorySlot.LastBackpack)
			{
				player.Out.SendMessage("You can only repair items in your backpack!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			eCraftingSkill skill = CraftingMgr.GetSecondaryCraftingSkillToWorkOnItem(item);
			if (skill == eCraftingSkill.NoCrafting)
			{
				player.Out.SendMessage("You can't repair " + item.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.IsCrafting)
			{
				player.Out.SendMessage("You must end your current action before you repair anything!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (item.Condition >= item.MaxCondition)
			{
				player.Out.SendMessage("The " + item.Name + " is fully repaired!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.GetCraftingSkillValue(skill) < ((percentNeeded / 100) * CraftingMgr.GetItemCraftLevel(item)))
			{
				player.Out.SendMessage("You don't have enough skill to repair the " + item.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}

		#endregion

		#region Calcul functions

		/// <summary>
		/// Calculate crafting time
		/// </summary>
		protected static int GetCraftingTime(GamePlayer player, InventoryItem item)
		{
			return Math.Max(1, item.Level / 2); // wrong but don't know the correct formula
		}

		/// <summary>
		/// Calculate the chance of sucess
		/// </summary>
		protected static int CalculateSuccessChances(GamePlayer player, InventoryItem item)
		{
			eCraftingSkill skill = CraftingMgr.GetSecondaryCraftingSkillToWorkOnItem(item);
			if (skill == eCraftingSkill.NoCrafting) return 0;

			int chancePercent = (int)((90 / (CraftingMgr.GetItemCraftLevel(item) * 0.5)) * player.GetCraftingSkillValue(skill)) - 80; // 50% = 10% chance, 100% = 100% chance
			if (chancePercent > 100)
				chancePercent = 100;
			else if (chancePercent < 0)
				chancePercent = 0;

			return chancePercent;
		}

		#endregion

		#region SiegeWeapon

		#region First call function and callback

		/// <summary>
		/// Called when player try to use a secondary crafting skill
		/// </summary>
		/// <param name="siegeWeapon"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public static int BeginWork(GamePlayer player, GameSiegeWeapon siegeWeapon)
		{
			if (!IsAllowedToBeginWork(player, siegeWeapon, 50))
			{
				return 0;
			}
			//chance with Woodworking 
			if (player.IsMoving || player.IsStrafing)
			{
				player.Out.SendMessage("You move and stop to repair the " + siegeWeapon.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			player.Out.SendMessage("You begin repairing the " + siegeWeapon.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			int workDuration = GetCraftingTime(player, siegeWeapon);
			player.Out.SendTimerWindow("Repairing: " + siegeWeapon.Name, workDuration);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(ProceedSiegeWeapon);
			player.CraftTimer.Properties.setProperty(PLAYER_CRAFTER, player);
			player.CraftTimer.Properties.setProperty(ITEM_CRAFTER, siegeWeapon);
			player.CraftTimer.Start(workDuration * 1000);
			return 1;
		}

		/// <summary>
		/// Called when craft time is finished 
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		protected static int ProceedSiegeWeapon(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getObjectProperty(PLAYER_CRAFTER, null);
			GameSiegeWeapon siegeWeapon = (GameSiegeWeapon)timer.Properties.getObjectProperty(ITEM_CRAFTER, null);

			if (player == null || siegeWeapon == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("There was a problem getting back the item to the player in the secondary craft system.");
				return 0;
			}
			if (!Util.Chance(CalculateSuccessChances(player, siegeWeapon)))
			{
				player.Out.SendMessage("You fail to repair the " + siegeWeapon.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			siegeWeapon.Health = siegeWeapon.MaxHealth;
			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();
			player.Out.SendObjectUpdate(siegeWeapon);//not sure if good packet for update
			player.Out.SendMessage("You have fully repaired the " + siegeWeapon.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 0;
		}
		#endregion

		#region Requirement check

		/// <summary>
		/// Check if the player own can enchant the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="siegeWeapon"></param>
		/// <param name="percentNeeded">min 50 max 100</param>
		/// <returns></returns>
		public static bool IsAllowedToBeginWork(GamePlayer player, GameSiegeWeapon siegeWeapon, int percentNeeded)
		{
			if (player.GetCraftingSkillValue(eCraftingSkill.WeaponCrafting) < 301)
			{
				player.Out.SendMessage("You must be a weapon crafter to repair the siegeweapon.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.IsCrafting)
			{
				player.Out.SendMessage("You must end your current action before you repair anything!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (siegeWeapon.Health >= siegeWeapon.MaxHealth)
			{
				player.Out.SendMessage("The " + siegeWeapon.Name + " is fully repaired!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}

		#endregion

		#region Calcul functions

		/// <summary>
		/// Calculate crafting time
		/// </summary>
		protected static int GetCraftingTime(GamePlayer player, GameSiegeWeapon siegeWeapon)
		{
			return 15; // wrong but don't know the correct formula
		}

		/// <summary>
		/// Calculate the chance of sucess
		/// </summary>
		protected static int CalculateSuccessChances(GamePlayer player, GameSiegeWeapon siegeWeapon)
		{
			player.GetCraftingSkillValue(eCraftingSkill.WoodWorking);
			int chancePercent = 90 - 50 / player.GetCraftingSkillValue(eCraftingSkill.WoodWorking);

			if (chancePercent > 100)
				chancePercent = 100;
			else if (chancePercent < 0)
				chancePercent = 0;
			return chancePercent;
		}

		#endregion

		#endregion
	}
}
