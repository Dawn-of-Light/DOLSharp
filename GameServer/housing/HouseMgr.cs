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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using log4net;

namespace DOL.GS.Housing
{
	public class HouseMgr
	{
		public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Timer CheckRentTimer;
		private static Dictionary<ushort, Dictionary<int, House>> _houseList;
		private static Dictionary<ushort, int> _idList;

		public static bool Start()
		{
			// load hookpoint offsets
			House.LoadHookpointOffsets();

			// initialize the house template manager
			HouseTemplateMgr.Initialize();

			_houseList = new Dictionary<ushort, Dictionary<int, House>>();
			_idList = new Dictionary<ushort, int>();

			int regions = 0;
			foreach (RegionEntry entry in WorldMgr.GetRegionList())
			{
				Region reg = WorldMgr.GetRegion(entry.id);
				if (reg != null && reg.HousingEnabled)
				{
					if (!_houseList.ContainsKey(reg.ID))
						_houseList.Add(reg.ID, new Dictionary<int, House>());

					if (!_idList.ContainsKey(reg.ID))
						_idList.Add(reg.ID, 0);

					regions++;
				}
			}

			int houses = 0;
			int lotmarkers = 0;
			int id = 0;

			foreach (DBHouse house in GameServer.Database.SelectAllObjects<DBHouse>())
			{
				// try and grab the houses for the region of thise house
				Dictionary<int, House> housesForRegion;
				_houseList.TryGetValue(house.RegionID, out housesForRegion);

				// if we don't have the given region loaded as a housing zone, skip this house
				if (housesForRegion == null)
					continue;

				// if we already loaded this house, that's no bueno, but just skip
				if (housesForRegion.ContainsKey(house.HouseNumber))
					continue;

				// if the house actually exists (isn't a lot anymore) then we just load it
				// from the database as normal
				if (house.Model != 0)
				{
					// create new house object for the given house definition
					var newHouse = new House(house) { UniqueID = id++ };

					newHouse.LoadFromDatabase();

					// store the house
					housesForRegion.Add(newHouse.HouseNumber, newHouse);
					houses++;
				}
				else
				{
					// we have a lot - need to spawn a lot marker for this one
					GameLotMarker.SpawnLotMarker(house);
					lotmarkers++;
				}
			}

			if (Log.IsInfoEnabled)
				Log.Info("[Housing] Loaded " + houses + " houses and " + lotmarkers + " lotmarkers in " + regions + " regions!");

			// start the timer for checking rents
			CheckRentTimer = new Timer(CheckRents, null, HousingConstants.RentTimerInterval, HousingConstants.RentTimerInterval);

			return true;
		}

		public static void Stop()
		{
		}

		public static IDictionary<int, House> GetHouses(ushort regionID)
		{
			// try and get the houses for the given region
			Dictionary<int, House> housesByRegion;
			_houseList.TryGetValue(regionID, out housesByRegion);

			return housesByRegion;
		}

		public static House GetHouse(ushort regionID, int houseNumber)
		{
			// try and get the houses for the given region
			Dictionary<int, House> housesByRegion;
			_houseList.TryGetValue(regionID, out housesByRegion);

			// if we couldn't find houses for the region, return null
			if (housesByRegion == null)
				return null;

			// if the house number exists, return the house
			if (housesByRegion.ContainsKey(houseNumber))
				return housesByRegion[houseNumber];

			// couldn't find the house, return null
			return null;
		}

		public static House GetHouse(int houseNumber)
		{
			// search thru each housing region, and if a house is found with
			// the given house number, return it
			foreach (var housingRegion in _houseList.Values)
			{
				if (housingRegion.ContainsKey(houseNumber))
					return housingRegion[houseNumber];
			}

			// couldn't find the house, return null
			return null;
		}

		public static GameConsignmentMerchant GetConsignmentByHouseNumber(int houseNumber)
		{
			// search thru each housing region, and if a house is found with
			// the given house number, return the consignment merchant
			foreach (var housingRegion in _houseList.Values)
			{
				if (housingRegion.ContainsKey(houseNumber))
					return housingRegion[houseNumber].ConsignmentMerchant;
			}

			// couldn't find the house, return null
			return null;
		}

		public static void AddHouse(House house)
		{
			// try and get the houses for the given region
			Dictionary<int, House> housesByRegion;
			_houseList.TryGetValue(house.RegionID, out housesByRegion);

			if (housesByRegion == null)
				return;

			// if the house doesn't exist yet, add it
			if (!housesByRegion.ContainsKey(house.HouseNumber))
			{
				housesByRegion.Add(house.HouseNumber, house);
			}
			else
			{
				// replace the existing lot with our new house
				housesByRegion[house.HouseNumber] = house;
			}

			// create any missing permissions
			for (int i = HousingConstants.MinPermissionLevel; i < HousingConstants.MaxPermissionLevel + 1; i++)
			{
				if(house.PermissionLevels.ContainsKey(i))
				{
					var oldPermission = house.PermissionLevels[i];
					if (oldPermission != null)
					{
						GameServer.Database.DeleteObject(oldPermission);
					}
				}

				// create a new, blank permission
				var permission = new DBHousePermissions(house.HouseNumber, i);
				house.PermissionLevels.Add(i, permission);

				// add the permission to the database
				GameServer.Database.AddObject(permission);
			}

			// save the house, broadcast an update
			house.SaveIntoDatabase();
			house.SendUpdate();
		}

		public static void UpgradeHouse(House house, InventoryItem deed)
		{
			// remove all players from the home before we upgrade it
			foreach (GamePlayer player in house.GetAllPlayersInHouse())
			{
				player.LeaveHouse();
			}

			// remove all indoor items
			var iobjs = GameServer.Database.SelectObjects<DBHouseIndoorItem>("HouseNumber = " + house.HouseNumber);

			foreach (DBHouseIndoorItem item in iobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			// remove all outdoor items
			var oobjs = GameServer.Database.SelectObjects<DBHouseOutdoorItem>("HouseNumber = " + house.HouseNumber);

			foreach (DBHouseOutdoorItem item in oobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			// figure out the new model to set the house
			int newmodel = 1;
			switch (deed.Id_nb)
			{
				case "alb_cottage_deed":
					newmodel = 1;
					break;
				case "alb_house_deed":
					newmodel = 2;
					break;
				case "alb_villa_deed":
					newmodel = 3;
					break;
				case "alb_mansion_deed":
					newmodel = 4;
					break;
				case "mid_cottage_deed":
					newmodel = 5;
					break;
				case "mid_house_deed":
					newmodel = 6;
					break;
				case "mid_villa_deed":
					newmodel = 7;
					break;
				case "mid_mansion_deed":
					newmodel = 8;
					break;
				case "hib_cottage_deed":
					newmodel = 9;
					break;
				case "hib_house_deed":
					newmodel = 10;
					break;
				case "hib_villa_deed":
					newmodel = 11;
					break;
				case "hib_mansion_deed":
					newmodel = 12;
					break;
			}

			// change the model of the house
			house.Model = newmodel;

			// save the house, and broadcast an update
			house.SaveIntoDatabase();
			house.SendUpdate();

			// if there is a consignment merchant, we have to readd him since we changed the house
			var merchant = GameServer.Database.SelectObject<DBHouseMerchant>("HouseNumber = '" + house.HouseNumber + "'");
			if (merchant != null)
			{
				int oldValue = merchant.Quantity;
				house.RemoveConsignment();
				house.AddConsignment(oldValue);
			}
		}

		public static void RemoveHouse(House house)
		{
			// try and get the houses for the given region
			Dictionary<int, House> housesByRegion;
			_houseList.TryGetValue(house.RegionID, out housesByRegion);

			if (housesByRegion == null)
				return;
				
			// remove the consignment merchant
			house.RemoveConsignment();

			// remove all the outside items of the house
			house.OutdoorItems.Clear();

			// remove the house for all nearby players
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(house, WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				player.Out.SendRemoveHouse(house);
				player.Out.SendGarden(house);
			}

			// clear the house number for the guild if this is a guild house
			if (house.DatabaseItem.GuildHouse)
			{
				Guild guild = GuildMgr.GetGuildByName(house.DatabaseItem.GuildName);
				if (guild != null)
				{
					guild.GuildHouseNumber = 0;
				}
			}

			// remove all players from the house
			foreach (GamePlayer player in house.GetAllPlayersInHouse())
			{
				player.LeaveHouse();
			}

			// zero out key values for the house
			house.OwnerID = "";
			house.KeptMoney = 0;
			house.Name = ""; // not null !
			house.Emblem = 0;
			house.Model = 0;
			house.Porch = false;
			house.DatabaseItem.GuildName = null;
			house.DatabaseItem.CreationTime = DateTime.Now;
			house.DatabaseItem.LastPaid = DateTime.MinValue;
			house.DatabaseItem.GuildHouse = false;

			#region Remove indoor/outdoor items and clear permissions

			// Remove all indoor items
			IList<DBHouseIndoorItem> iobjs =
				GameServer.Database.SelectObjects<DBHouseIndoorItem>("HouseNumber = " + house.HouseNumber);

			foreach (DBHouseIndoorItem item in iobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			// Remove all outdoor items
			IList<DBHouseOutdoorItem> oobjs =
				GameServer.Database.SelectObjects<DBHouseOutdoorItem>("HouseNumber = " + house.HouseNumber);

			foreach (DBHouseOutdoorItem item in oobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			// Remove all housepoint items
			IList<DBHousepointItem> hpobjs = GameServer.Database.SelectObjects<DBHousepointItem>("HouseID = " + house.HouseNumber);

			foreach (DBHousepointItem item in hpobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			// Remove all permissions
			IList<DBHousePermissions> pobjs =
				GameServer.Database.SelectObjects<DBHousePermissions>("HouseNumber = " + house.HouseNumber);

			foreach (DBHousePermissions item in pobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			// Remove all char x permissions
			IList<DBHouseCharsXPerms> cpobjs =
				GameServer.Database.SelectObjects<DBHouseCharsXPerms>("HouseNumber = " + house.HouseNumber);

			foreach (DBHouseCharsXPerms item in cpobjs)
			{
				GameServer.Database.DeleteObject(item);
			}

			#endregion

			// saved the cleared house in the database
			house.SaveIntoDatabase();

			// remove the house from the list of houses in the region
			housesByRegion.Remove(house.HouseNumber);

			// spawn a lot marker for the now-empty lot
			GameLotMarker.SpawnLotMarker(house.DatabaseItem);
		}

		/// <summary>
		/// Checks if a player is the owner of the house, this checks all characters on the account
		/// </summary>
		/// <param name="house">The house object</param>
		/// <param name="player">The player to check</param>
		/// <returns>True if the player is the owner</returns>
		public static bool IsOwner(DBHouse house, GamePlayer player)
		{
			// house and player can't be null
			if (house == null || player == null)
				return false;

			// if owner id isn't set, there is no owner
			if (string.IsNullOrEmpty(house.OwnerID))
				return false;

			// check if this a guild house, and if the player
			// 1) belongs to the guild and is 2) a GM in the guild
			if (player.Guild != null && house.GuildHouse)
			{
				if (player.Guild.Name == house.GuildName && player.Guild.GotAccess(player, eGuildRank.Leader))
					return true;
			}
			else
			{
				foreach (DOLCharacters c in player.Client.Account.Characters)
				{
					if (house.OwnerID == c.ObjectId)
						return true;
				}
			}

			return false;
		}

		public static int GetHouseNumberByPlayer(GamePlayer p)
		{
			House house = GetHouseByPlayer(p);

			return house != null ? house.HouseNumber : 0;
		}

		/// <summary>
		/// Get the house object from the owner player
		/// </summary>
		/// <param name="p">The player owner</param>
		/// <returns>The house object</returns>
		public static House GetHouseByPlayer(GamePlayer p)
		{
			// check every house in every region until we find
			// a house that belongs to this player
			foreach (var regs in _houseList)
			{
				foreach (var entry in regs.Value)
				{
					var house = entry.Value;

					if (house.OwnerID == p.DBCharacter.ObjectId)
						return house;
				}
			}

			// didn't find a house that belonged to the player,
			// so return null
			return null;
		}

		/// <summary>
		/// Gets the guild house object by real owner
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static House GetGuildHouseByPlayer(GamePlayer p)
		{
			// make sure player is in a guild
			if (p.Guild == null)
				return null;

			// check every house in every region until we find
			// a house that belongs to the same guild as the player
			foreach (var regs in _houseList)
			{
				foreach (var entry in regs.Value)
				{
					var house = entry.Value;

					if (house.DatabaseItem.GuildName == p.Guild.GuildID)
						return house;
				}
			}

			// didn't find a house that belonged to the player's guild,
			// or they aren't in a guild, so return null
			return null;
		}

		public static void HouseTransferToGuild(GamePlayer plr)
		{
			// player must be in a guild
			if (plr.Guild == null)
				return;

			// player's guild can't already have a guild house
			if (plr.Guild.GuildOwnsHouse)
				return;

			// send house xfer prompt to player
			plr.Out.SendCustomDialog(LanguageMgr.GetTranslation(plr.Client, "Scripts.Player.Housing.TransferToGuild", plr.Guild.Name), MakeGuildLot);

			return;
		}

		private static void MakeGuildLot(GamePlayer player, byte response)
		{
			// user responded no/decline
			if (response != 0x01)
				return;

			var playerHouse = GetHouse(GetHouseNumberByPlayer(player));
			var playerGuild = player.Guild;

			// double check and make sure this guild isn't null
			if (playerGuild == null)
				return;

			// adjust the house to be under guild control
			playerHouse.DatabaseItem.OwnerID = playerGuild.GuildID;
			playerHouse.DatabaseItem.Name = playerGuild.Name;
			playerHouse.DatabaseItem.GuildHouse = true;
			playerHouse.DatabaseItem.GuildName = playerGuild.Name;

			// adjust guild to reflect their new guild house
			player.Guild.GuildHouseNumber = playerHouse.HouseNumber;

			// notify guild members of the guild house acquisition
			player.Guild.SendMessageToGuildMembers(
				LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Housing.GuildNowOwns", player.Guild.Name, player.Name),
				eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

			// save the guild and broadcast updates
			player.Guild.SaveIntoDatabase();
			player.Guild.UpdateGuildWindow();

			// save the house and broadcast updates
			playerHouse.SaveIntoDatabase();
			playerHouse.SendUpdate();
		}

		public static long GetRentByModel(int model)
		{
			switch (model % 4)
			{
				case 0:
					return Properties.HOUSING_RENT_MANSION;
				case 1:
					return Properties.HOUSING_RENT_COTTAGE;
				case 2:
					return Properties.HOUSING_RENT_HOUSE;
				case 3:
					return Properties.HOUSING_RENT_VILLA;
			}

			return 0;
		}

		public static void CheckRents(object state)
		{
			Log.Debug("[Housing] Starting timed rent check");

			TimeSpan diff;
			var houseRemovalList = new List<House>();

			foreach (var regs in _houseList)
			{
				foreach (var entry in regs.Value)
				{
					var house = entry.Value;

					// if the house has no owner or is set to not be purged, 
					// we just skip over it
					if (string.IsNullOrEmpty(house.OwnerID) || house.NoPurge)
						continue;

					// get the time that rent was last paid for the house
					diff = DateTime.Now - house.LastPaid;

					// get the amount of rent for the given house
					long rent = GetRentByModel(house.Model);

					// if the rent isn't free and it's been 7 days or more,
					// the house needs to pay rent!
					if (rent > 0L && diff.Days >= 7)
					{
						long lockboxAmount = house.KeptMoney;
						long consignmentAmount = 0;

						var consignmentMerchant = house.ConsignmentMerchant;
						if (consignmentMerchant != null)
						{
							consignmentAmount = consignmentMerchant.TotalMoney;
						}

						// try to pull from the lockbox first
						if (lockboxAmount >= rent)
						{
							house.KeptMoney -= rent;
							house.LastPaid = DateTime.Now;
							house.SaveIntoDatabase();
						}
						else
						{
							long remainingDifference = (rent - lockboxAmount);

							// not enough was in the lockbox.  see if we have the difference
							// on the consignment merchant
							if (remainingDifference <= consignmentAmount)
							{
								// we have the difference, phew!
								house.KeptMoney = 0;
								consignmentMerchant.TotalMoney -= remainingDifference;
							}

							// house can't afford rent, so we schedule house to be
							// repossessed.
							houseRemovalList.Add(house);
						}
					}
				}
			}

			foreach (House h in houseRemovalList) // here we remove houses
			{
				RemoveHouse(h);
			}
		}

		internal static void SpecialBuy(GamePlayer gamePlayer, ushort item_slot, byte item_count, byte menu_id)
		{
			MerchantTradeItems items = null;

			//--------------------------------------------------------------------------
			//Retrieve the item list, the list can be retrieved from any object. Usually
			//a merchant but could be anything eg. Housing Lot Markers etc.
			switch ((eMerchantWindowType)menu_id)
			{
				case eMerchantWindowType.HousingInsideShop:
					items = HouseTemplateMgr.IndoorShopItems;
					break;
				case eMerchantWindowType.HousingOutsideShop:
					items = HouseTemplateMgr.OutdoorShopItems;
					break;
				case eMerchantWindowType.HousingBindstone:
					items = HouseTemplateMgr.IndoorBindstoneMenuItems;
					break;
				case eMerchantWindowType.HousingCrafting:
					items = HouseTemplateMgr.IndoorCraftMenuItems;
					break;
				case eMerchantWindowType.HousingNPC:
					items = HouseTemplateMgr.IndoorNPCMenuItems;
					break;
				case eMerchantWindowType.HousingVault:
					items = HouseTemplateMgr.IndoorVaultMenuItems;
					break;
			}

			GameMerchant.OnPlayerBuy(gamePlayer, item_slot, item_count, items);
		}

		/// <summary>
		/// This function gets the house close to spot
		/// </summary>
		/// <returns>array of house</returns>
		public static IEnumerable getHousesCloseToSpot(ushort regionid, int x, int y, int radius)
		{
			var myhouses = new ArrayList();
			int radiussqrt = radius * radius;
			foreach (House house in GetHouses(regionid).Values)
			{
				int xdiff = house.X - x;
				int ydiff = house.Y - y;
				int range = xdiff * xdiff + ydiff * ydiff;
				if (range < 0)
					range *= -1;
				if (range > radiussqrt)
					continue;
				myhouses.Add(house);
			}
			return myhouses;
		}
	}
}