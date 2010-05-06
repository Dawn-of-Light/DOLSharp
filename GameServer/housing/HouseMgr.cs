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
	public static class HouseMgr
	{
		public const int MaxHouses = 2000;

		private const int RentTimerInterval = 1000*60*60*2; // check every 2 hours
		public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Timer CheckRentTimer;
		private static Dictionary<ushort, Dictionary<int, House>> m_houselists;
		private static Dictionary<ushort, int> m_idlist;

		public static bool Start()
		{
			// load hookpoints.
			LoadHookpointOffsets();

			m_houselists = new Dictionary<ushort, Dictionary<int, House>>();
			m_idlist = new Dictionary<ushort, int>();

			int regions = 0;
			foreach (var entry in WorldMgr.GetRegionList())
			{
				Region reg = WorldMgr.GetRegion(entry.id);

				if (reg != null && reg.HousingEnabled)
				{
					if (!m_houselists.ContainsKey(reg.ID))
					{
						m_houselists.Add(reg.ID, new Dictionary<int, House>());
					}

					if (!m_idlist.ContainsKey(reg.ID))
					{
						m_idlist.Add(reg.ID, 0);
					}

					regions++;
				}
			}

			HouseTemplateMgr.Initialize();

			int houses = 0;
			int lotmarkers = 0;
			foreach (DBHouse house in GameServer.Database.SelectAllObjects<DBHouse>())
			{
				if (!string.IsNullOrEmpty(house.OwnerID))
				{
					int id = -1;
					if ((id = GetUniqueID(house.RegionID)) >= 0)
					{
						var newHouse = new House(house) {UniqueID = id};

						var hash = m_houselists[house.RegionID];

						if (hash == null) 
							continue;

						if (hash.ContainsKey(newHouse.HouseNumber)) 
							continue;

						if (house.Model != 0) // we do not need to do this for lots without a real house
						{
							newHouse.LoadFromDatabase();
						}
						else //we need to spawn a lot for this house
						{
							GameLotMarker.SpawnLotMarker(house);
							lotmarkers++;
						}

						hash.Add(newHouse.HouseNumber, newHouse);

						houses++;
					}
					else
					{
						if (Log.IsWarnEnabled)
						{
							Log.Warn("Failed to get a unique id, cant load house! More than " + MaxHouses + " houses in region " +
							         house.RegionID + " or region not loaded // housing not enabled?");
						}
					}
				}
				else
				{
					if (!m_idlist.ContainsKey(house.RegionID)) continue;
					GameLotMarker.SpawnLotMarker(house);
					lotmarkers++;
				}
			}

			if (Log.IsInfoEnabled)
				Log.Info("loaded " + houses + " houses and " + lotmarkers + " lotmarkers in " + regions + " regions!");

			CheckRentTimer = new Timer(CheckRents, null, RentTimerInterval, RentTimerInterval);
			return true;
		}

		public static void Stop()
		{
		}

		public static int GetUniqueID(ushort regionid)
		{
			if (m_idlist.ContainsKey(regionid))
			{
				var id = (int) m_idlist[regionid];
				id += 1;
				m_idlist[regionid] = id;

				return id;
			}

			return -1;
		}

		public static IDictionary<int, House> GetHouses(ushort regionID)
		{
			Dictionary<int, House> houses;

			if(!m_houselists.TryGetValue(regionID, out houses))
			{
				m_houselists.Add(regionID, new Dictionary<int, House>());
			}

			return houses;
		}

		public static House GetHouse(ushort regionID, int houseNumber)
		{
			Dictionary<int, House> houses;
			House house;

			if (!m_houselists.TryGetValue(regionID, out houses))
			{
				m_houselists.Add(regionID, new Dictionary<int, House>());

				return null;
			}

			houses.TryGetValue(houseNumber, out house);

			return house;
		}

		public static House GetHouse(int housenumber)
		{
			foreach(var region in m_houselists)
			{
				if(region.Value.ContainsKey(housenumber))
				{
					return region.Value[housenumber];
				}
			}

			return null;
		}

		public static Consignment GetConsignmentByHouseNumber(int housenumber)
		{
			foreach (var region in m_houselists)
			{
				if (region.Value.ContainsKey(housenumber))
				{
					var house = region.Value[housenumber];

					return house.ConsignmentMerchant;
				}
			}

			return null;
		}

		public static void AddHouse(House house)
		{
			Dictionary<int, House> houses;

			if(!m_houselists.TryGetValue(house.RegionID, out houses))
				return;

			if (!houses.ContainsKey(house.HouseNumber))
			{
				houses.Add(house.HouseNumber, house);
			}
			else // we have an empty lot in the hash, we need to replace the house object
			{
				houses[house.HouseNumber] = house;
			}

			for (int i = HousingConstants.MinPermissionLevel; i < HousingConstants.MaxPermissionLevel; i++) // we add missing permissions
			{
				if (house.HouseAccess[i] == null)
				{
					var permission = new DBHousePermissions(house.HouseNumber, i);

					house.HouseAccess[i] = permission;

					GameServer.Database.AddObject(permission);
				}
			}

			house.SaveIntoDatabase();
			house.SendUpdate();
		}

		public static void UpgradeHouse(House house, InventoryItem deed)
		{
			foreach (GamePlayer player in house.GetAllPlayersInHouse())
				player.LeaveHouse();

			#region Remove indoor/outdoor items

			// Remove all indoor items
			IList<DBHouseIndoorItem> iobjs =
				GameServer.Database.SelectObjects<DBHouseIndoorItem>("HouseNumber = " + house.HouseNumber);
			if (iobjs.Count > 0)
				foreach (DBHouseIndoorItem item in iobjs)
					GameServer.Database.DeleteObject(item);

			// Remove all outdoor items
			IList<DBHouseOutdoorItem> oobjs =
				GameServer.Database.SelectObjects<DBHouseOutdoorItem>("HouseNumber = " + house.HouseNumber);
			if (oobjs.Count > 0)
				foreach (DBHouseOutdoorItem item in oobjs)
					GameServer.Database.DeleteObject(item);

			#endregion

			#region newmodel

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

			#endregion

			house.Model = newmodel;
			house.SaveIntoDatabase();
			house.SendUpdate();

			#region consignment merchant

			var merchant = GameServer.Database.SelectObject<DBHouseMerchant>("HouseNumber = '" + house.HouseNumber + "'");
			if (merchant != null)
			{
				int oldValue = merchant.Quantity;
				house.RemoveConsignment();
				house.AddConsignment(oldValue);
			}

			#endregion
		}

		public static void RemoveHouse(House house)
		{
			Log.Warn("House " + house.UniqueID + " removed");

			Dictionary<int, House> houses;

			if (!m_houselists.TryGetValue(house.RegionID, out houses))
				return;

			house.OutdoorItems.Clear();
			foreach(var player in WorldMgr.GetPlayersCloseToSpot(house.RegionID, house.X, house.Y, house.Z, WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				player.Out.SendRemoveHouse(house);
				player.Out.SendGarden(house);
//				player.Out.SendRemoveGarden(house);
			}

			if (house.DatabaseItem.GuildHouse)
			{
				Guild guild = GuildMgr.GetGuildByName(house.DatabaseItem.GuildName);
				if (guild != null)
				{
					guild.GuildHouseNumber = 0;
				}
			}

			foreach (GamePlayer player in house.GetAllPlayersInHouse())
			{
				player.LeaveHouse();
			}

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

			#region Remove indoor/outdoor items & permissions

			// Remove all indoor items
			IList<DBHouseIndoorItem> iobjs =
				GameServer.Database.SelectObjects<DBHouseIndoorItem>("HouseNumber = " + house.HouseNumber);
			if (iobjs.Count > 0)
				foreach (DBHouseIndoorItem item in iobjs)
					GameServer.Database.DeleteObject(item);

			// Remove all outdoor items
			IList<DBHouseOutdoorItem> oobjs =
				GameServer.Database.SelectObjects<DBHouseOutdoorItem>("HouseNumber = " + house.HouseNumber);
			if (oobjs.Count > 0)
				foreach (DBHouseOutdoorItem item in oobjs)
					GameServer.Database.DeleteObject(item);

			// Remove all housepoint items
			IList<DBHousepointItem> hpobjs = GameServer.Database.SelectObjects<DBHousepointItem>("HouseID = " + house.HouseNumber);
			if (hpobjs.Count > 0)
				foreach (DBHousepointItem item in hpobjs)
					GameServer.Database.DeleteObject(item);

			// Remove all permissions
			IList<DBHousePermissions> pobjs =
				GameServer.Database.SelectObjects<DBHousePermissions>("HouseNumber = " + house.HouseNumber);
			if (pobjs.Count > 0)
				foreach (DBHousePermissions item in pobjs)
					GameServer.Database.DeleteObject(item);

			// Remove all char x permissions
			IList<DBHouseCharsXPerms> cpobjs =
				GameServer.Database.SelectObjects<DBHouseCharsXPerms>("HouseNumber = " + house.HouseNumber);
			if (cpobjs.Count > 0)
				foreach (DBHouseCharsXPerms item in cpobjs)
					GameServer.Database.DeleteObject(item);

			#endregion

			house.RemoveConsignment();
			house.SaveIntoDatabase();
			houses.Remove(house.HouseNumber);
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
			if (house == null || player == null)
				return false;

			if (string.IsNullOrEmpty(house.OwnerID))
				return false;

			if (player.Guild != null && house.GuildHouse)
			{
				if (player.Guild.Name == house.GuildName && player.Guild.GotAccess(player, eGuildRank.Leader))
					return true;
			}
			else
			{
				foreach (Character c in player.Client.Account.Characters)
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
			foreach (var regs in m_houselists)
			{
				foreach (var entry in regs.Value)
				{
					var house = entry.Value;

					if (house.OwnerID == null)
						continue;

					if (house.OwnerID == p.PlayerCharacter.ObjectId)
						return house;
				}
			}

			return null; // no house
		}

		/// <summary>
		/// Gets the guild house object by real owner
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static House GetGuildHouseByPlayer(GamePlayer p)
		{
			foreach (var regs in m_houselists)
			{
				foreach (var entry in regs.Value)
				{
					var house = entry.Value;

					if (house.OwnerID == null || !house.DatabaseItem.GuildHouse)
						continue;

					if (house.HasOwnerPermissions(p))
						return house;
				}
			}

			return null; // no house
		}

		public static bool IsGuildHouse(House house)
		{
			if (!m_houselists.ContainsKey(house.RegionID))
				return false;

			return house.DatabaseItem.GuildHouse;
		}

		public static string GetOwningGuild(House house)
		{
			if (!m_houselists.ContainsKey(house.RegionID))
				return "";

			return house.DatabaseItem.GuildName;
		}

		public static void SetGuildHouse(House house, bool guildowns, string owningGuild)
		{
			if (!m_houselists.ContainsKey(house.RegionID))
				return;

			house.DatabaseItem.Name = owningGuild;
			house.DatabaseItem.GuildHouse = guildowns;
			house.DatabaseItem.GuildName = owningGuild;
		}

		public static void HouseTransferToGuild(GamePlayer plr)
		{
			if (plr.Guild != null && plr.Guild.GuildOwnsHouse)
				return;

			plr.Out.SendCustomDialog(
				LanguageMgr.GetTranslation(plr.Client, "Scripts.Player.Housing.TransferToGuild", plr.Guild.Name), MakeGuildLot);
			return;
		}

		private static void MakeGuildLot(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			House house = GetHouse((GetHouseNumberByPlayer(player)));
			house.DatabaseItem.OwnerID = player.Guild.GuildID;
			player.Guild.GuildHouseNumber = house.HouseNumber;

			player.Guild.SendMessageToGuildMembers(
				LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Housing.GuildNowOwns", player.Guild.Name, player.Name),
				eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

			SetGuildHouse(house, true, player.GuildName);

			player.Guild.SaveIntoDatabase();
			player.Guild.UpdateGuildWindow();

			house.SaveIntoDatabase();
			house.SendUpdate();
		}

		public static string GetOwner(DBHouse house)
		{
			return house == null ? null : house.OwnerID;
		}

		public static long GetRentByModel(int model)
		{
			switch (model%4)
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
			Log.Debug("Time to check Rents!");
			TimeSpan diff;
			var todel = new ArrayList();

			foreach (var regs in m_houselists)
			{
				foreach (var entry in regs.Value)
				{
					var house = entry.Value;

					if (string.IsNullOrEmpty(house.OwnerID) || house.NoPurge)
						// Replaced OR by AND to fix table problems due to old method bugs
						continue;

					diff = DateTime.Now - house.LastPaid;
					long rent = GetRentByModel(house.Model);
					if (rent > 0L && diff.Days >= 7)
					{
						Log.Debug("House " + house.UniqueID + " must pay !");
						if (house.KeptMoney >= rent)
						{
							house.KeptMoney -= rent;
							house.LastPaid = DateTime.Now;
							house.SaveIntoDatabase();
						}
						else
						{
							todel.Add(house); //  to permit to delete house and continue the foreach
						} 
					}
				}
			}

			foreach (House h in todel) // here we remove houses
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
			switch ((eMerchantWindowType) menu_id)
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
			int radiussqrt = radius*radius;
			foreach (House house in GetHouses(regionid).Values)
			{
				int xdiff = house.X - x;
				int ydiff = house.Y - y;
				int range = xdiff*xdiff + ydiff*ydiff;
				if (range < 0)
					range *= -1;
				if (range > radiussqrt)
					continue;
				myhouses.Add(house);
			}
			return myhouses;
		}

		public static bool AddNewOffset(HouseHookpointOffset o)
		{
			if (o.Hookpoint <= HousingConstants.MaxHookpointLocations)
			{
				HousingConstants.RelativeHookpointsCoords[o.Model][o.Hookpoint] = new[] { o.OffX, o.OffY, o.OffZ, o.OffH };
				return true;
			}

			Log.Error("HOUSING: HouseHookPointOffset exceeds array size.  Model " + o.Model + " hookpoint " + o.Hookpoint);

			return false;
		}

		private static void LoadHookpointOffsets()
		{
			//initialise array
			for (int i = 12; i > 0; i--)
			{
				for (int j = 1; j < HousingConstants.RelativeHookpointsCoords[i].Length; j++)
				{
					HousingConstants.RelativeHookpointsCoords[i][j] = null;
				}
			}

			var objs = GameServer.Database.SelectAllObjects<HouseHookpointOffset>();
			foreach (HouseHookpointOffset o in objs)
			{
				AddNewOffset(o);
			}
		}
	}
}