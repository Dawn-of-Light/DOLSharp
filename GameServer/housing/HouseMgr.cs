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
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Housing
{
	public class HouseMgr
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const int MAXHOUSES = 2000; 
		public const int HOUSE_DISTANCE = 5120; //guessed, but i'm sure its > vis dist.

		private static Hashtable m_houselists;
		private static Hashtable m_idlist;

		public static bool Start()
		{
			m_houselists = new Hashtable();
			m_idlist = new Hashtable();
			int regions = 0;
			foreach(RegionEntry entry in WorldMgr.GetRegionList())
			{
				Region reg = WorldMgr.GetRegion(entry.id);
				if(reg!=null && reg.HousingEnabled)
				{
					if(!m_houselists.ContainsKey(reg.ID))
						m_houselists.Add(reg.ID, new Hashtable());

					if(!m_idlist.ContainsKey(reg.ID))
						m_idlist.Add(reg.ID, 0);

					regions++;
				}
			}
			HouseTemplateMgr.Initialize();

			int houses = 0;
			int lotmarkers = 0;
			foreach(DBHouse house in GameServer.Database.SelectAllObjects(typeof(DBHouse)))
			{
				if (house.Model!=0)
				{
					int id = -1;
					if((id = GetUniqueID(house.RegionID))>=0)
					{
						House newHouse = new House(house);
						newHouse.UniqueID = id;
						Hashtable hash = (Hashtable)m_houselists[house.RegionID];
						if (hash==null) continue;
						if (hash.ContainsKey(newHouse.HouseNumber)) continue;
						int i = 0;
						foreach(DBHouseIndoorItem dbiitem in GameServer.Database.SelectObjects(typeof(DBHouseIndoorItem), "HouseNumber = '" + newHouse.HouseNumber + "'"))
						{
							IndoorItem iitem = new IndoorItem();
							iitem.CopyFrom(dbiitem);
							newHouse.IndoorItems.Add(i++, iitem);
						}
						i = 0;
						foreach(DBHouseOutdoorItem dboitem in GameServer.Database.SelectObjects(typeof(DBHouseOutdoorItem), "HouseNumber = '" + newHouse.HouseNumber + "'"))
						{
							OutdoorItem oitem = new OutdoorItem();
							oitem.CopyFrom(dboitem);
							newHouse.OutdoorItems.Add(i++, oitem);
						}
						hash.Add(newHouse.HouseNumber,newHouse);
						houses++;
					} 
					else
					{
						if(log.IsWarnEnabled)
							log.Warn("Failed to get a unique id, cant load house! More than "+HouseMgr.MAXHOUSES+" houses in region "+house.RegionID+" or region not loaded // housing not enabled?");
					}
				}
				else
				{
					if(!m_idlist.ContainsKey(house.RegionID)) continue;
					GameLotMarker.SpawnLotMarker(house);
					lotmarkers++;
				}
			}

			if(log.IsInfoEnabled)
				log.Info("loaded "+houses+" houses and "+lotmarkers+" lotmarkers in "+regions+" regions!");
			
			return true;
		}

		public static void Stop()
		{
		}


		public static int GetUniqueID(ushort regionid)
		{
			if(m_idlist.ContainsKey(regionid))
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
			return (Hashtable)m_houselists[regionid];
		}

		public static House GetHouse(ushort regionid, int housenumber)
		{
			Hashtable hash = (Hashtable)m_houselists[regionid];
			if(hash==null) return null;

			return (House)hash[housenumber];
		}

		public static House GetHouse(int housenumber)
		{
			foreach(Hashtable hash in m_houselists.Values)
			{
				if(hash.ContainsKey(housenumber))
				{
					return (House)hash[housenumber];
				}
			}
			return null;
		}


		public static void AddHouse(House house)
		{
			Hashtable hash = (Hashtable)m_houselists[house.RegionID];
			if (hash==null) return;
			if (hash.ContainsKey(house.HouseNumber)) return;
			hash.Add(house.HouseNumber,house);
			house.SaveIntoDatabase();
			house.SendUpdate();
		}

		public static void RemoveHouse(House house)
		{
			Hashtable hash = (Hashtable)m_houselists[house.RegionID];
			if (hash==null) return;
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort) house.RegionID, house.X, house.Y, house.Z, WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				//player.Out.SendRemoveHouse(house);
				player.Out.SendRemoveGarden(house);
			}
			hash.Remove(house);
		}


		public static bool IsOwner(DBHouse house, GamePlayer player)
		{
			if (house == null || player == null) return false;
			if (house.OwnerIDs == null || house.OwnerIDs == "") return false;

			return (house.OwnerIDs.IndexOf(player.PlayerCharacter.ObjectId)>=0);
		}

		public static void AddOwner(DBHouse house, GamePlayer player)
		{
			if (house == null || player == null) return;
			if (house.OwnerIDs != null && house.OwnerIDs != "")
			{
				if(house.OwnerIDs.IndexOf(player.InternalID)<0)
					return;
			}
			house.OwnerIDs += player.InternalID+";";
			GameServer.Database.SaveObject(house);
		}

		public static void DeleteOwner(DBHouse house, GamePlayer player)
		{
			if (house == null || player == null) return;
			if (house.OwnerIDs == null || house.OwnerIDs == "") return;

			house.OwnerIDs = house.OwnerIDs.Replace(player.InternalID+";","");
			GameServer.Database.SaveObject(house);
		}
		
		public static ArrayList GetOwners(DBHouse house)
		{
			if(house==null) return null;
			if(house.OwnerIDs == null || house.OwnerIDs == "") return null;

			ArrayList owners = new ArrayList();
			string[] ids = house.OwnerIDs.Split(';');

			foreach(string id in ids)
			{
				Character character = (Character)GameServer.Database.FindObjectByKey(typeof(Character),id);
				if(character==null) continue;
				owners.Add(character);
			}
			return owners;
		}

	}
}