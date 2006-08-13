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

using DOL.GS.PacketHandler;
using DOL.Database;

using log4net;

namespace DOL.GS.Housing
{
	public class House : IPoint3D
	{
		public int HouseNumber
		{
			get { return m_databaseItem.HouseNumber; }
			set { m_databaseItem.HouseNumber = value; }
		}

		public int X
		{
			get { return m_databaseItem.X; }
			set { m_databaseItem.X = value; }
		}

		public int Y
		{
			get { return m_databaseItem.Y; }
			set { m_databaseItem.Y = value; }
		}

		public int Z
		{
			get { return m_databaseItem.Z; }
			set { m_databaseItem.Z = value; }
		}

		public ushort RegionID
		{
			get { return m_databaseItem.RegionID; }
			set { m_databaseItem.RegionID = value; }
		}

		public int Heading
		{
			get { return m_databaseItem.Heading; }
			set { m_databaseItem.Heading = value; }
		}

		public string Name
		{
			get { return m_databaseItem.Name; }
			set { m_databaseItem.Name = value; }
		}


		public string OwnerIDs
		{
			get { return m_databaseItem.OwnerIDs; }
			set { m_databaseItem.OwnerIDs = value; }
		}

		public int Model
		{
			get { return m_databaseItem.Model; }
			set { m_databaseItem.Model = value; }
		}

		public int Emblem
		{
			get { return m_databaseItem.Emblem; }
			set { m_databaseItem.Emblem = value; }
		}

		public int PorchRoofColor
		{
			get { return m_databaseItem.PorchRoofColor; }
			set { m_databaseItem.PorchRoofColor = value; }
		}

		public int PorchMaterial
		{
			get { return m_databaseItem.PorchMaterial; }
			set { m_databaseItem.PorchMaterial = value; }
		}

		public bool Porch
		{
			get { return m_databaseItem.Porch; }
			set { m_databaseItem.Porch = value; }
		}

		public bool IndoorGuildBanner
		{
			get { return m_databaseItem.IndoorGuildBanner; }
			set { m_databaseItem.IndoorGuildBanner = value; }
		}

		public bool IndoorGuildShield
		{
			get { return m_databaseItem.IndoorGuildShield; }
			set { m_databaseItem.IndoorGuildShield = value; }
		}

		public bool OutdoorGuildBanner
		{
			get { return m_databaseItem.OutdoorGuildBanner; }
			set { m_databaseItem.OutdoorGuildBanner = value; }
		}

		public bool OutdoorGuildShield
		{
			get { return m_databaseItem.OutdoorGuildShield; }
			set { m_databaseItem.OutdoorGuildShield = value; }
		}

		public int RoofMaterial
		{
			get { return m_databaseItem.RoofMaterial; }
			set { m_databaseItem.RoofMaterial = value; }
		}

		public int DoorMaterial
		{
			get { return m_databaseItem.DoorMaterial; }
			set { m_databaseItem.DoorMaterial = value; }
		}

		public int WallMaterial
		{
			get { return m_databaseItem.WallMaterial; }
			set { m_databaseItem.WallMaterial = value; }
		}

		public int TrussMaterial
		{
			get { return m_databaseItem.TrussMaterial; }
			set { m_databaseItem.TrussMaterial = value; }
		}

		public int WindowMaterial
		{
			get { return m_databaseItem.WindowMaterial; }
			set { m_databaseItem.WindowMaterial = value; }
		}

		public int Rug1Color
		{
			get { return m_databaseItem.Rug1Color; }
			set { m_databaseItem.Rug1Color = value; }
		}

		public int Rug2Color
		{
			get { return m_databaseItem.Rug2Color; }
			set { m_databaseItem.Rug2Color = value; }
		}

		public int Rug3Color
		{
			get { return m_databaseItem.Rug3Color; }
			set { m_databaseItem.Rug3Color = value; }
		}

		public int Rug4Color
		{
			get { return m_databaseItem.Rug4Color; }
			set { m_databaseItem.Rug4Color = value; }
		}

		private int m_uniqueID;

		public int UniqueID
		{
			get { return m_uniqueID;  }
			set { m_uniqueID = value; }
		}

		Hashtable m_indooritems;

		public Hashtable IndoorItems
		{
			get { return m_indooritems; }
			set { m_indooritems = value; }
		}

		Hashtable m_outdooritems;

		public Hashtable OutdoorItems
		{
			get { return m_outdooritems; }
			set { m_outdooritems = value; }
		}

		DBHouse m_databaseItem;

		/// <summary>
		/// Sends a update of the house and the garden to all players in range
		/// </summary>
		public void SendUpdate()
		{
			foreach(GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort)this.RegionID, this.X, this.Y, this.Z, HouseMgr.HOUSE_DISTANCE))
			{
				player.Out.SendHouse(this);
				player.Out.SendGarden(this);
			}
		}

		public bool EditPorch(bool add_porch)
		{
			if (Porch == add_porch) //we cannot remove if removed, or add if added
				return false;

			Porch = add_porch;
			this.SendUpdate();

			return true;
		}

		/// <summary>
		/// Used to get into a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		public void Enter(GamePlayer player)
		{

			GameClient client = player.Client;
			client.Out.SendEnterHouse(this);
			client.Out.SendFurniture(this);
			client.Out.SendRemoveGarden(this);
			client.Player.InHouse = true;
			client.Player.CurrentHouse = this;

			switch (this.Model)
			{
				//thx to sp4m
				default:
					client.Player.MoveTo((ushort) this.RegionID, this.X, this.Y, 25022, client.Player.Heading);
					break;

				case 1:
					client.Player.MoveTo((ushort) this.RegionID, this.X + 80, this.Y + 100, ((ushort) (25025)), client.Player.Heading);
					break;

				case 2:
					client.Player.MoveTo((ushort) this.RegionID, this.X - 260, this.Y + 100, ((ushort) (24910)), client.Player.Heading);
					break;

				case 3:
					client.Player.MoveTo((ushort) this.RegionID, this.X - 200, this.Y + 100, ((ushort) (24800)), client.Player.Heading);
					break;

				case 4:
					client.Player.MoveTo((ushort) this.RegionID, this.X - 350, this.Y - 30, ((ushort) (24660)), client.Player.Heading);
					break;

				case 5:
					client.Player.MoveTo((ushort) this.RegionID, this.X + 230, this.Y - 480, ((ushort) (25100)), client.Player.Heading);
					break;

				case 6:
					client.Player.MoveTo((ushort) this.RegionID, this.X - 80, this.Y - 660, ((ushort) (24700)), client.Player.Heading);
					break;

				case 7:
					client.Player.MoveTo((ushort) this.RegionID, this.X - 80, this.Y - 660, ((ushort) (24700)), client.Player.Heading);
					break;

				case 8:
					client.Player.MoveTo((ushort) this.RegionID, this.X - 90, this.Y - 625, ((ushort) (24670)), client.Player.Heading);
					break;

				case 9:
					client.Player.MoveTo((ushort) this.RegionID, this.X + 400, this.Y - 160, ((ushort) (25150)), client.Player.Heading);
					break;

				case 10:
					client.Player.MoveTo((ushort) this.RegionID, this.X + 400, this.Y - 80, ((ushort) (25060)), client.Player.Heading);
					break;

				case 11:
					client.Player.MoveTo((ushort) this.RegionID, this.X + 400, this.Y - 60, ((ushort) (24900)), client.Player.Heading);
					break;

				case 12:
					client.Player.MoveTo((ushort) this.RegionID, this.X, this.Y - 620, ((ushort) (24595)), client.Player.Heading);
					break;
			}

			client.Out.SendMessage("You have entered house number " + this.HouseNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Used to leave a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		/// <param name="silent">text or not</param>
		public void Exit(GamePlayer player, bool silent)
		{
			double angle = Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
			int x = (int) (X + (0 * Math.Cos(angle) + 500 * Math.Sin(angle)));
			int y = (int) (Y - (500 * Math.Cos(angle) - 0 * Math.Sin(angle)));
			ushort heading = (ushort)((Heading < 180 ? Heading + 180 : Heading - 180) / 0.08789);
			player.MoveTo(RegionID, x, y, Z, heading);
			if(!silent) player.Out.SendMessage("You have left house number " + HouseNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

		}


		/// <summary>
		/// Sends the house info window to a player
		/// </summary>
		/// <param name="player">the player</param>
		public void SendHouseInfo(GamePlayer player)
		{
			ArrayList text = new ArrayList();
			text.Add("Owner: " + this.Name);
			text.Add("Lotnum: " + HouseNumber);
			int level = Model - (int)((Model - 1) / 4) * 4;
			text.Add("Level: " + level);
			text.Add(" ");
			text.Add("Porch: ");
			text.Add("-Enabled: " + (Porch ? "Y" : "N"));
			text.Add("-Color: " + PorchRoofColor);
			text.Add(" ");
			text.Add("Exterior Materials:");
			text.Add("-Roof: " + RoofMaterial);
			text.Add("-Wall: " + WallMaterial);
			text.Add("-Door: " + DoorMaterial);
			text.Add("-Support: " + TrussMaterial);
			text.Add("-Porch: " + PorchMaterial);
			text.Add("-Shutter: " + WindowMaterial);
			text.Add(" ");
			text.Add("Exterior Upgrades:");
			text.Add("-Banner: " + ((OutdoorGuildBanner) ? "Y" : "N"));
			text.Add("-Shield: " + ((OutdoorGuildShield) ? "Y" : "N"));
			text.Add(" ");
			text.Add("Interior Upgrades:");
			text.Add("-Banner: " + ((IndoorGuildBanner) ? "Y" : "N"));
			text.Add("-Shield: " + ((IndoorGuildShield) ? "Y" : "N"));
			text.Add(" ");
			text.Add("Interior Carpets:");
			text.Add("-First Color: " + Rug1Color);
			text.Add("-Second Color: " + Rug2Color);
			text.Add("-Third Color: " + Rug3Color);
			text.Add("-Fourth Color: " + Rug4Color);
			text.Add(" ");
			text.Add("Lockbox: (todo)");
			text.Add("Rental Price: (todo)");
			text.Add("Max in Lockbox: (todo)");
			text.Add("Rent due in: (todo)");
			text.Add(" ");
			text.Add("Owners:");
			foreach (Character character in HouseMgr.GetOwners(this.m_databaseItem))
			{
				text.Add("-" + character.Name);
			}
			player.Out.SendCustomTextWindow(this.Name + " 's house", text);

		}

		/// <summary>
		/// Returns true if the player is a owner of the house
		/// </summary>
		/// <param name="player">the player to check</param>
		public bool IsOwner(GamePlayer player)
		{
			return HouseMgr.IsOwner(m_databaseItem, player);
		}

		/// <summary>
		/// Saves this house into the database
		/// </summary>
		public void SaveIntoDatabase()
		{
			GameServer.Database.SaveObject(m_databaseItem);
		}

		public int GetPorchAndGuildEmblemFlags()
		{
			int flag = 0;
			if (Porch)
				flag |= 1;
			if (OutdoorGuildBanner)
				flag |= 2;
			if (OutdoorGuildShield)
				flag |= 4;
			return flag;
		}

		public int GetGuildEmblemFlags()
		{
			int flag = 0;
			if (IndoorGuildBanner)
				flag |= 1;
			if (IndoorGuildShield)
				flag |= 2;
			return flag;
		}

		/// <summary>
		/// Returns a ArrayList with all players in the house
		/// </summary>
		public ArrayList GetAllPlayersInHouse()
		{
			ArrayList ret = new ArrayList();
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort) this.RegionID, this.X, this.Y, 25000, WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player.CurrentHouse == this && player.InHouse)
				{
					ret.Add(player);
				}
			}
			return ret;
		}

		public void Edit(GamePlayer player, ArrayList changes)
		{
			//TODO: access check here

			//TODO: remove this bloody access check :p
			if(!this.IsOwner(player)) return;

			MerchantTradeItems items;

			if(player.InHouse)
			{
				items = HouseTemplateMgr.IndoorMenuItems;
			}
			else
			{
				items = HouseTemplateMgr.OutdoorMenuItems;
			}

			if(items==null)
				return;

			long price = 0;

			foreach(int slot in changes)
			{
				int page = slot / 30;
				int pslot = slot % 30;
				ItemTemplate item = items.GetItem(page,(eMerchantWindowSlot)pslot);
				if(item!=null)
				{
					price += Money.GetMoney(0,0,item.Gold,item.Silver,item.Copper);
				}
			}

			if(!player.RemoveMoney(price,"You pay "+Money.GetString(price)+" for your changes."))
			{
				player.Out.SendMessage("You don't have enough money to do that!",eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
				return;
			}

			foreach(int slot in changes)
			{
				int page = slot / 30;
				int pslot = slot % 30;

				ItemTemplate item = items.GetItem(page,(eMerchantWindowSlot)pslot);

				if(item != null)
				{
					switch((eObjectType)item.Object_Type)
					{
						case eObjectType.HouseInteriorBanner:
							IndoorGuildBanner = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseInteriorShield:
							IndoorGuildShield = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseCarpetFirst:
							Rug1Color = item.DPS_AF;
							break;
						case eObjectType.HouseCarpetSecond:
							Rug2Color = item.DPS_AF;
							break;
						case eObjectType.HouseCarpetThird:
							Rug3Color = item.DPS_AF;
							break;
						case eObjectType.HouseCarpetFourth:
							Rug4Color = item.DPS_AF;
							break;
						case eObjectType.HouseTentColor:
							PorchRoofColor = item.DPS_AF;
							break;
						case eObjectType.HouseExteriorBanner:
							OutdoorGuildBanner = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseExteriorShield:
							OutdoorGuildShield = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseRoofMaterial:
							RoofMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseWallMaterial:
							WallMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseDoorMaterial:
							DoorMaterial = item.DPS_AF;
							break;
						case eObjectType.HousePorchMaterial:
							PorchMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseWoodMaterial:
							TrussMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseShutterMaterial:
							WindowMaterial = item.DPS_AF;
							break;
						default: 
							break; //dirty work a round - dont know how mythic did it, hardcoded? but it works
					}
				}
			}

			GameServer.Database.SaveObject(m_databaseItem);

			if(player.InHouse)
			{
				foreach(GamePlayer p in this.GetAllPlayersInHouse())
				{
					p.Out.SendEnterHouse(this); //update rugs.
				}
			}
			else
			{
				foreach(GamePlayer p in WorldMgr.GetPlayersCloseToSpot((ushort)this.RegionID, this.X, this.Y, this.Z, HouseMgr.HOUSE_DISTANCE))
				{
					p.Out.SendHouse(this); //update wall look
				}
			}
		}


		public House(DBHouse house)
		{
			m_databaseItem = house;
			m_indooritems = new Hashtable();
			m_outdooritems = new Hashtable();
		}
	}
}