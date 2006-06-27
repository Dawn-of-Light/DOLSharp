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

using System.Collections;
using System.Reflection;

using DOL.GS.PacketHandler;
using DOL.GS.Database;

using log4net;

namespace DOL.GS.Housing
{
	public class House : IWorldPosition
	{
		public int HouseNumber
		{
			get { return m_databaseItem.HouseNumber; }
			set { m_databaseItem.HouseNumber = value; }
		}
		
		public Point Position
		{
			get { return new Point(m_databaseItem.X, m_databaseItem.Y, m_databaseItem.Z); }
			set
			{
				m_databaseItem.X = value.X;
				m_databaseItem.Y = value.Y;
				m_databaseItem.Z = value.Z;
			}
		}
		
		private Region m_region;

		public Region Region
		{
			get { return m_region; }
			set
			{
				m_region = value;
				m_databaseItem.RegionID = value.RegionID;
			}
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

		ArrayList m_indooritems;

		public ArrayList IndoorItems
		{
			get { return m_indooritems; }
			set { m_indooritems = value; }
		}

		ArrayList m_outdooritems;

		public ArrayList OutdoorItems
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
			foreach(GamePlayer player in m_region.GetPlayerInRadius(Position, HouseMgr.HOUSE_DISTANCE, false))
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
			
			Point pos = Position;
			Point target;

			switch (this.Model)
			{
				//thx to sp4m
				default:
					target = pos;
					break;

				case 1:
					target = new Point(pos.X + 80, pos.Y + 100, 25025);
					break;

				case 2:
					target = new Point(pos.X - 260, pos.Y + 100, 24910);
					break;

				case 3:
					target = new Point(pos.X - 200, pos.Y + 100, 24800);
					break;

				case 4:
					target = new Point(pos.X - 350, pos.Y - 30, 24660);
					break;

				case 5:
					target = new Point(pos.X + 230, pos.Y - 480, 25100);
					break;

				case 6:
				case 7:
					target = new Point(pos.X - 80, pos.Y - 660, 24700);
					break;

				case 8:
					target = new Point(pos.X - 90, pos.Y - 625, 24670);
					break;

				case 9:
					target = new Point(pos.X + 400, pos.Y - 160, 25150);
					break;

				case 10:
					target = new Point(pos.X + 400, pos.Y - 80, 25060);
					break;

				case 11:
					target = new Point(pos.X + 400, pos.Y - 60, 24900);
					break;

				case 12:
					target = new Point(pos.X, pos.Y - 620, 24595);
					break;
			}
			client.Player.MoveTo((ushort)Region.RegionID, target, (ushort)client.Player.Heading);

			client.Out.SendMessage("You have entered house number " + HouseNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Used to leave a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		/// <param name="silent">text or not</param>
		public void Exit(GamePlayer player, bool silent)
		{
			player.MoveTo((ushort)Region.RegionID, Position, (ushort)Heading);
				
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
			int level = Model;
			while (level > 4)
			{
				level -= 4;
			}
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
			foreach (GamePlayer character in HouseMgr.GetOwners(this.m_databaseItem))
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
			//TODO: do with << and >> ;p
			int flag = 0;

			if ((OutdoorGuildBanner) && (OutdoorGuildShield)) { flag = 6; }
			else if (OutdoorGuildBanner) { flag = 2; }
			else if (OutdoorGuildShield) { flag = 4; }
			
			if (Porch) { flag += 1; }

			return flag;
		}

		public int GetGuildEmblemFlags()
		{
			int flag = 0;

			if (IndoorGuildShield) { flag = 2; }
			if (IndoorGuildBanner) { flag += 1; }

			return flag;
		}

		/// <summary>
		/// Returns a ArrayList with all players in the house
		/// </summary>
		public ArrayList GetAllPlayersInHouse()
		{
			ArrayList ret = new ArrayList();
			Point pos = Position;
			pos.Z = 25000;
			foreach (GamePlayer player in Region.GetPlayerInRadius(pos, WorldMgr.VISIBILITY_DISTANCE, false))
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
				GenericItemTemplate item = items.GetItem(page,(eMerchantWindowSlot)pslot);
				if(item!=null)
				{
					price += item.Value;
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

				GenericItemTemplate item = items.GetItem(page,(eMerchantWindowSlot)pslot);
				
				if(item!=null)
				{
				/*	switch(item.ObjectType)
					{
						case 2: IndoorGuildBanner = (item.DPS_AF == 1 ? true : false); break;
						case 3: IndoorGuildShield = (item.DPS_AF == 1 ? true : false); break;

						case 52: Rug1Color = item.DPS_AF; break;
						case 5: Rug2Color = item.DPS_AF; break;
						case 6: Rug3Color = item.DPS_AF; break;
						case 7: Rug1Color = item.DPS_AF; break;

						case 56: PorchRoofColor = item.DPS_AF; break;
						case 57: OutdoorGuildBanner = (item.DPS_AF == 1 ? true : false); break;
						case 58: OutdoorGuildShield = (item.DPS_AF == 1 ? true : false); break;
						case 59: RoofMaterial = item.DPS_AF; break;
						case 60: WallMaterial = item.DPS_AF; break;
						case 61: DoorMaterial = item.DPS_AF; break;
						case 62: PorchMaterial = item.DPS_AF; break;
						case 63: TrussMaterial = item.DPS_AF; break;

						default: WindowMaterial = (item.Gold-1); break; //dirty work a round - dont know how mythic did it, hardcoded? but it works.
					}*/
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
				foreach(GamePlayer p in Region.GetPlayerInRadius(Position, HouseMgr.HOUSE_DISTANCE, false))
				{
					p.Out.SendHouse(this); //update wall look
				}
			}
		}


		public House(DBHouse house)
		{
			m_region = WorldMgr.GetRegion((ushort) house.RegionID);
			m_databaseItem = house;
			m_indooritems = new ArrayList();
			m_outdooritems = new ArrayList();
		}
	}
}