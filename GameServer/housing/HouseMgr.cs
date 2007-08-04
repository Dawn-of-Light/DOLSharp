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
using System.Threading;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.Housing
{
	public class HouseMgr
	{
		public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const int MAXHOUSES = 2000;
		public const int HOUSE_DISTANCE = 5120; //guessed, but i'm sure its > vis dist.

		private static Timer CheckRentTimer = null;
		private static Hashtable m_houselists;
		private static Hashtable m_idlist;

		public static bool Start()
		{
			m_houselists = new Hashtable();
			m_idlist = new Hashtable();
			int regions = 0;
			foreach (RegionEntry entry in WorldMgr.GetRegionList())
			{
				Region reg = WorldMgr.GetRegion(entry.id);
				if (reg != null && reg.HousingEnabled)
				{
					if (!m_houselists.ContainsKey(reg.ID))
						m_houselists.Add(reg.ID, new Hashtable());

					if (!m_idlist.ContainsKey(reg.ID))
						m_idlist.Add(reg.ID, 0);

					regions++;
				}
			}
			HouseTemplateMgr.Initialize();

			int houses = 0;
			int lotmarkers = 0;
			foreach (DBHouse house in GameServer.Database.SelectAllObjects(typeof(DBHouse)))
			{
				if (house.Model != 0)
				{
					int id = -1;
					if ((id = GetUniqueID(house.RegionID)) >= 0)
					{
						House newHouse = new House(house);
						newHouse.UniqueID = id;
						Hashtable hash = (Hashtable)m_houselists[house.RegionID];
						if (hash == null) continue;
						if (hash.ContainsKey(newHouse.HouseNumber)) continue;

						newHouse.LoadFromDatabase();

						hash.Add(newHouse.HouseNumber, newHouse);
						houses++;
					}
					else
					{
						if (Logger.IsWarnEnabled)
							Logger.Warn("Failed to get a unique id, cant load house! More than " + MAXHOUSES + " houses in region " + house.RegionID + " or region not loaded // housing not enabled?");
					}
				}
				else
				{
					if (!m_idlist.ContainsKey(house.RegionID)) continue;
					GameLotMarker.SpawnLotMarker(house);
					lotmarkers++;
				}
			}

			if (Logger.IsInfoEnabled)
				Logger.Info("loaded " + houses + " houses and " + lotmarkers + " lotmarkers in " + regions + " regions!");

			CheckRentTimer = new Timer(new TimerCallback(CheckRents), null, 10000, 1000 * 3600);
			return true;
		}
		public static void Stop()
		{
		}

		public static int GetUniqueID(ushort regionid)
		{
			if (m_idlist.ContainsKey(regionid))
			{
				int id = (int)m_idlist[regionid];
				id += 1;
				m_idlist[regionid] = id;
				return id;
			}

			return -1;
		}
		public static Hashtable GetHouses(ushort regionid)
		{
			Hashtable table = m_houselists[regionid] as Hashtable;
			if (table == null)
				table = new Hashtable();
			return table;
		}
		public static House GetHouse(ushort regionid, int housenumber)
		{
			Hashtable hash = (Hashtable)m_houselists[regionid];
			if (hash == null) return null;

			return (House)hash[housenumber];
		}
		public static House GetHouse(int housenumber)
		{
			foreach (Hashtable hash in m_houselists.Values)
				if (hash.ContainsKey(housenumber))
					return (House)hash[housenumber];
			return null;
		}

		public static void AddHouse(House house)
		{
			Hashtable hash = (Hashtable)m_houselists[house.RegionID];
			if (hash == null) return;
			if (hash.ContainsKey(house.HouseNumber))
			{
				Logger.Warn("House ID exists !");
				return;
			}
			hash.Add(house.HouseNumber, house);
			int i = 0;
			for (i = 0; i < 10; i++) // we add missing permissions
			{
				if (house.HouseAccess[i] == null)
				{
					house.HouseAccess[i] = new DBHousePermissions(house.HouseNumber, i);

					GameServer.Database.AddNewObject(house.HouseAccess[i]);
				}
			}
			house.SaveIntoDatabase();
			house.SendUpdate();
		}

		public static void RemoveHouse(House house)
		{
			Logger.Debug("House " + house.UniqueID + " removed");
			Hashtable hash = (Hashtable)m_houselists[house.RegionID];
			if (hash == null) return;
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort)house.RegionID, house.X, house.Y, house.Z, WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				player.Out.SendRemoveHouse(house);
				player.Out.SendRemoveGarden(house);

			}
			foreach (GamePlayer player in house.GetAllPlayersInHouse())
				player.LeaveHouse();

			house.OwnerIDs = 0;
			house.KeptMoney = 0;
			house.Name = ""; // not null ! 
			house.Emblem = 0;
			house.Model = 0;
            house.DatabaseItem.CreationTime = DateTime.MinValue;
            house.DatabaseItem.LastPaid = DateTime.MinValue;

            #region Remove indoor/outdoor items & permissions
            DataObject[] objs;

            // Remove all indoor items
            objs = GameServer.Database.SelectObjects(typeof(DBHouseIndoorItem), "HouseNumber = " + house.HouseNumber);
            if (objs.Length > 0)
                foreach (DataObject item in objs)
                    GameServer.Database.DeleteObject(item);

            // Remove all outdoor items
            objs = GameServer.Database.SelectObjects(typeof(DBHouseOutdoorItem), "HouseNumber = " + house.HouseNumber);
            if (objs.Length > 0)
                foreach (DataObject item in objs)
                    GameServer.Database.DeleteObject(item);

            // Remove all permissions
            objs = GameServer.Database.SelectObjects(typeof(DBHousePermissions), "HouseNumber = " + house.HouseNumber);
            if (objs.Length > 0)
                foreach (DataObject item in objs)
                    GameServer.Database.DeleteObject(item);

            // Remove all char x permissions
            objs = GameServer.Database.SelectObjects(typeof(DBHouseCharsXPerms), "HouseNumber = " + house.HouseNumber);
            if (objs.Length > 0)
                foreach (DataObject item in objs)
                    GameServer.Database.DeleteObject(item);
            #endregion

            house.SaveIntoDatabase();
			hash.Remove(house.HouseNumber);
			GameLotMarker.SpawnLotMarker(house.DatabaseItem);
		}

		public static bool IsOwner(DBHouse house, GamePlayer player)
		{
			if (house == null || player == null) return false;
			if (/*house.OwnerIDs == null ||*/ house.OwnerIDs == 0) return false;

			//return (house.OwnerIDs.IndexOf(player.PlayerCharacter.ObjectId) >= 0);
			return house.OwnerIDs == player.PlayerCharacter.ObjectId;
		}
		public static void AddOwner(DBHouse house, GamePlayer player)
		{
			if (house == null || player == null) return;
			if (/*house.OwnerIDs != null &&*/ house.OwnerIDs != 0)
			{
				//if (house.OwnerIDs.IndexOf(player.InternalID) < 0)
				if (house.OwnerIDs != player.InternalID)
					return;
			}
			//house.OwnerIDs += player.InternalID+";";
			house.OwnerIDs = player.InternalID; // unique owner

			GameServer.Database.SaveObject(house);
		}
		public static void DeleteOwner(DBHouse house, GamePlayer player)
		{
			if (house == null || player == null) return;
			if (/*house.OwnerIDs == null ||*/ house.OwnerIDs == 0) return;

			//house.OwnerIDs = house.OwnerIDs.Replace(player.InternalID + ";", "");
			house.OwnerIDs = 0;
			GameServer.Database.SaveObject(house);
		}

		public static int GetHouseNumberByPlayer(GamePlayer p)
		{
			foreach (DictionaryEntry regs in m_houselists)
			{
				foreach (DictionaryEntry Entry in (Hashtable)(regs.Value))
				{
					House house = (House)Entry.Value;
					if (house.OwnerIDs == 0/*null*/)
						continue;
					if (house.IsOwner(p))
						return house.HouseNumber;
				}
			}
			return 0; // no house
		}

		/// <summary>
		/// Get the house object from the owner player
		/// </summary>
		/// <param name="p">The player owner</param>
		/// <returns>The house object</returns>
		public static House GetHouseByPlayer(GamePlayer p)
		{
			foreach (DictionaryEntry regs in m_houselists)
			{
				foreach (DictionaryEntry Entry in (Hashtable)(regs.Value))
				{
					House house = (House)Entry.Value;
					if (house.OwnerIDs == 0)
						continue;
					if (house.IsOwner(p))
						return house;
				}
			}
			return null; // no house
		}

		public static ArrayList GetOwners(DBHouse house)
		{
			if (house == null) return null;
			if (/*house.OwnerIDs == null ||*/ house.OwnerIDs == 0) return null;

			ArrayList owners = new ArrayList();
			//string[] ids = house.OwnerIDs.Split(';');

			//foreach (string id in ids)
			//{
				Character character = (Character)GameServer.Database.FindObjectByKey(typeof(Character), house.OwnerIDs);
				if (character != null) /*== null) continue;*/
				owners.Add(character);
			//}
			return owners;
		}

		public static long GetRentByModel(int Model)
		{
			if ((Model == 1) || (Model == 5) || (Model == 9))
				return 20 * 10000;
			if ((Model == 2) || (Model == 6) || (Model == 10))
				return 200 * 10000;
			if ((Model == 3) || (Model == 7) || (Model == 11))
				return 800 * 10000;
			//if ((Model == 4) || (Model == 8) || (Model == 12))
			return 2000 * 10000;
		}

		public static void CheckRents(object state)
		{
			Logger.Debug("Time to check Rents !");
			TimeSpan Diff;
			ArrayList todel = new ArrayList();

			foreach (DictionaryEntry regs in m_houselists)
			{
				foreach (DictionaryEntry Entry in (Hashtable)(regs.Value))
				{
					House house = (House)Entry.Value;
					if ((/*house.OwnerIDs == null &&*/ house.OwnerIDs == 0) || house.NoPurge) // Replaced OR by AND to fix table problems due to old method bugs
						continue;
					Diff = DateTime.Now - house.LastPaid;
					if (Diff.Days >= 7)
					{
						Logger.Debug("House " + house.UniqueID + " must pay !");
						long Rent = GetRentByModel(house.Model);
						if (house.KeptMoney >= Rent)
						{
							house.KeptMoney -= Rent;
							house.LastPaid = DateTime.Now;
							house.SaveIntoDatabase();
						}
						else todel.Add(house); //  to permit to delete house and continue the foreach
					}
				}
			}

			foreach (House h in todel) // here we remove houses
				RemoveHouse(h);
		}

		internal static void SpecialBuy(GamePlayer gamePlayer, ushort item_slot, byte item_count, byte menu_id)
		{
			MerchantTradeItems items = null;

			//--------------------------------------------------------------------------
			//Retrieve the item list, the list can be retrieved from any object. Usually
			//a merchant but could be anything eg. Housing Lot Markers etc.
			switch ((eMerchantWindowType)menu_id)
			{
				case eMerchantWindowType.HousingInsideShop: items = HouseTemplateMgr.IndoorShopItems; break;
				case eMerchantWindowType.HousingOutsideShop: items = HouseTemplateMgr.OutdoorShopItems; break;
				case eMerchantWindowType.HousingBindstone: items = HouseTemplateMgr.IndoorBindstoneMenuItems; break;
				case eMerchantWindowType.HousingCrafting: items = HouseTemplateMgr.IndoorCraftMenuItems; break;
				case eMerchantWindowType.HousingNPC: items = HouseTemplateMgr.IndoorNPCMenuItems; break;
				case eMerchantWindowType.HousingVault: items = HouseTemplateMgr.IndoorVaultMenuItems; break;
			}

			GameMerchant.OnPlayerBuy(gamePlayer, item_slot, item_count, items);
		}

		/// <summary>
		/// This function gets the house close to spot
		/// </summary>
		/// <returns>array of house</returns>
		public static IEnumerable getHousesCloseToSpot(ushort regionid, int x, int y, int radius)
		{
			ArrayList myhouses = new ArrayList();
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