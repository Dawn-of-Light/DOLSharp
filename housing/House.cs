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
using DOL.Language;
using log4net;

namespace DOL.GS.Housing
{

	public enum ePermsTypes
	{
		Player = 0x01,
		Guild = 0x02,
		GuildRank = 0x03,
		Account = 0x04,
		All = 0x05,
		Class = 0x06,
		Race = 0x07
	}

	public class House : Point3D
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string HOUSEFORHOUSERENT = "HouseForHouseRent";
		public const string MONEYFORHOUSERENT = "MoneyForHouseRent";
		public const string BPSFORHOUSERENT = "BPsForHouseRent";

		public const int MAX_HOOKPOINT_LOCATIONS = 25;


		#region Properties
		public int HouseNumber
		{
			get { return m_databaseItem.HouseNumber; }
			set { m_databaseItem.HouseNumber = value; }
		}

		public override int X
		{
			get { return m_databaseItem.X; }
			set { m_databaseItem.X = value; }
		}

        public override int Y
		{
			get { return m_databaseItem.Y; }
			set { m_databaseItem.Y = value; }
		}

        public override int Z
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

		public DateTime LastPaid
		{
			get { return m_databaseItem.LastPaid; }
			set { m_databaseItem.LastPaid = value; }
		}

		public long KeptMoney
		{
			get { return m_databaseItem.KeptMoney; }
			set { m_databaseItem.KeptMoney = value; }
		}

		public bool NoPurge
		{
			get { return m_databaseItem.NoPurge; }
			set { m_databaseItem.NoPurge = value; }
		}

		private int m_uniqueID;

		public int UniqueID
		{
			get { return m_uniqueID; }
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

		Hashtable m_housepointitems;

		public Hashtable HousepointItems
		{
			get { return m_housepointitems; }
			set { m_housepointitems = value; }
		}

		public DBHouse DatabaseItem
		{
			get { return m_databaseItem; }
		}

		ArrayList m_charspermissions;
		public ArrayList CharsPermissions
		{
			get { return m_charspermissions; }
			set { m_charspermissions = value; }
		}

		DBHouse m_databaseItem;

		DBHousePermissions[] m_houseAccess;

		public DBHousePermissions[] HouseAccess
		{
			get { return m_houseAccess; }
		}

        private Consignment m_consignment;

        public Consignment ConsignmentMerchant
        {
            get { return m_consignment; }
            set { m_consignment = value; }
        }

		#endregion

		/****************** AREDHEL ACCESS HANDLING START **********************/
		/// <summary>
		/// Get the access level of this player for this house.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public int GetPlayerAccessLevel(GamePlayer player)
		{
			foreach (DBHouseCharsXPerms permissions in CharsPermissions)
                if (permissions.Name == player.Name)
                    return permissions.PermLevel;

			//if the player is not in the permissions check for its guild
            if (player.Guild != null)
            {
                foreach (DBHouseCharsXPerms permissions in CharsPermissions)
                    if (permissions.Name == player.GuildName)
                        return permissions.PermLevel;

            }

            //at the end check if there are permissions for everybody
            foreach (DBHouseCharsXPerms permissions in CharsPermissions)
                if (permissions.Name == "All")
                    return permissions.PermLevel;

            return 0;
		}

		/// <summary>
		/// Get a set of permissions this player has in this house.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public DBHousePermissions GetPlayerPermissions(GamePlayer player)
		{
			if (player == null)
				return HouseAccess[0];

			int accessLevel = GetPlayerAccessLevel(player);
			player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "House.DBHousePermissions.AccessLevel", accessLevel)), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			if (accessLevel < 0 || accessLevel > 9)
				return HouseAccess[0];

			return HouseAccess[accessLevel];
		}

		/// <summary>
		/// Whether or not this player is the owner of this house.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool IsOwner(GamePlayer player)
		{
			return HouseMgr.IsOwner(m_databaseItem, player, false);
		}

		/// <summary>
		/// Whether or not this player is allowed to enter this house.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanEnter(GamePlayer player)
		{
			if (IsOwner(player) || player.Client.Account.PrivLevel > 1)
				return true;

			DBHousePermissions housePermissions = GetPlayerPermissions(player);
			return (housePermissions.Enter > 0);
		}


		/****************** AREDHEL ACCESS HANDLING END **********************/

		/// <summary>
		/// Sends a update of the house and the garden to all players in range
		/// </summary>
		public void SendUpdate()
		{
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort)this.RegionID, this.X, this.Y, this.Z, HouseMgr.HOUSE_DISTANCE))
			{
				player.Out.SendHouse(this);
				player.Out.SendGarden(this);
			}
		}

        public bool EditPorch(bool add_porch)
        {
            if (Porch == add_porch) //we cannot remove if removed, or add if added
                return false;
            if (add_porch == false)
                RemoveConsignment();
            Porch = add_porch;
            this.SendUpdate();
            this.SaveIntoDatabase();

            return true;
        }


        public bool AddConsignment(int startValue)
        {
            DataObject obj = GameServer.Database.SelectObject(typeof(Mob), "HouseNumber = '" + this.HouseNumber + "'");
            if (obj != null)
                return false;
            DBHouseMerchant merchant = (DBHouseMerchant)GameServer.Database.SelectObject(typeof(DBHouseMerchant), "HouseNumber = '" + this.HouseNumber + "'");
            if (merchant != null)
                return false;
            DBHouseMerchant newM = new DBHouseMerchant();
            newM.HouseNumber = this.HouseNumber;
            newM.Quantity = startValue;
            GameServer.Database.AddNewObject(newM);
            #region positioning
            double multi = 0;
            int range = 0;
            int zaddition = 0;
            int realm = 0;
            switch (Model)
            {
                case 1:
                    {
                        multi = 0.55;
                        range = 630;
                        zaddition = 40;
                        realm = 1;
                        break;
                    }
                case 2:
                    {
                        multi = 0.55;
                        range = 630;
                        zaddition = 40;
                        realm = 1;
                        break;
                    }
                case 3:
                    {
                        multi = -0.55;
                        range = 613;
                        zaddition = 100;
                        realm = 1;
                        break;
                    }
                case 4:
                    {
                        multi = 0.53;
                        range = 620;
                        zaddition = 100;
                        realm = 1;
                        break;
                    }
                case 5:
                    {
                        multi = -0.47;
                        range = 755;
                        zaddition = 40;
                        realm = 2;
                        break;
                    }
                case 6:
                    {

                        multi = -0.5;
                        range = 630;
                        zaddition = 40;
                        realm = 2;
                        break;
                    }
                case 7:
                    {
                        multi = 0.48;
                        range = 695;
                        zaddition = 100;
                        realm = 2;
                        break;
                    }
                case 8:
                    {
                        multi = -0.505;
                        range = 680;
                        zaddition = 100;
                        realm = 2;
                        break;
                    }
                case 9:
                    {
                        multi = 0.475;
                        range = 693;
                        zaddition = 40;
                        realm = 3;
                        break;
                    }
                case 10:
                    {
                        multi = 0.47;
                        range = 688;
                        zaddition = 40;
                        realm = 3;
                        break;
                    }
                case 11:
                    {
                        multi = -0.65;
                        range = 603;
                        zaddition = 100;
                        realm = 3;
                        break;
                    }
                case 12:
                    {
                        multi = -0.58;
                        range = 638;
                        zaddition = 100;
                        realm = 3;
                        break;
                    }
            }
            #endregion
            double angle = Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
            ushort heading = (ushort)((Heading < 180 ? Heading + 180 : Heading - 180) / 0.08789);
            int tX = (int)((X + (500 * Math.Sin(angle))) - Math.Sin(angle - multi) * range);
            int tY = (int)((Y - (500 * Math.Cos(angle))) + Math.Cos(angle - multi) * range);
            Consignment con = new Consignment();
            con.CurrentRegionID = RegionID;
            con.X = tX;
            con.Y = tY;
            if (this.DatabaseItem.GuildHouse)
                con.GuildName = this.DatabaseItem.GuildName;
            con.Z = Z + zaddition;
            con.Level = 50;
            con.Realm = (eRealm)realm;
            con.HouseNumber = HouseNumber;
            con.Name = "Consignment Merchant";
            con.Heading = heading;
            con.Model = 144;
            con.Flags |= (uint)GameNPC.eFlags.PEACE;
            con.LoadedFromScript = false;
            con.RoamingRange = 0;
            con.AddToWorld();
            con.SaveIntoDatabase();
            this.DatabaseItem.HasConsignment = true;
            this.SaveIntoDatabase();
            return true;
        }

        public void RemoveConsignment()
        {
            Mob npcmob = (Mob)GameServer.Database.SelectObject(typeof(Mob), "HouseNumber = '" + this.HouseNumber + "'");
            if (npcmob != null)
            {
                GameNPC[] npc = WorldMgr.GetNPCsByNameFromRegion(npcmob.Name, npcmob.Region, (eRealm)npcmob.Realm);
                foreach (GameNPC hnpc in npc)
                {
                    if (hnpc.HouseNumber == HouseNumber)
                    {
                        hnpc.DeleteFromDatabase();
                        hnpc.Delete();
                    }
                }
            }
            DBHouseMerchant merchant = (DBHouseMerchant)GameServer.Database.SelectObject(typeof(DBHouseMerchant), "HouseNumber = '" + this.HouseNumber + "'");
            if (merchant != null)
            {
                GameServer.Database.DeleteObject(merchant);
            }
            this.m_consignment = null;
            this.DatabaseItem.HasConsignment = false;
            this.SaveIntoDatabase();
        }  

		public bool IsInPerm(string name, ePermsTypes type, int lvl)
		{
			// todo modify when type is account, to check if name == one of charnames on the account
			foreach (DBHouseCharsXPerms perm in CharsPermissions)
			{
				if (type == ePermsTypes.All && perm.Type == (byte)type && perm.PermLevel == lvl)
					return true;
				if (perm.Name == name && perm.Type == (byte)type && perm.PermLevel == lvl)
					return true;
			}
			return false;
		}

		public bool CanPayRent(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if (perm.PayRent == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		//public bool CanEnter(GamePlayer p)
		//{
		//    if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
		//        return true;
		//    if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName)
		//        return true;
		//    foreach (DBHousePermissions perm in HouseAccess)
		//    {
		//        if (perm.Enter == 0)// optim
		//            continue;
		//        if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
		//            return true;
		//        if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
		//            return true;
		//        if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
		//            return true;
		//    }
		//    return false;
		//}

		public bool CanAddInterior(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if ((perm.Interior & 0x01) == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		public bool CanRemoveInterior(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if ((perm.Interior & 0x02) == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		public bool CanAddGarden(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if ((perm.Garden & 0x01) == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		public bool CanRemoveGarden(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if ((perm.Garden & 0x02) == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		public bool CanEditAppearance(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if (perm.Appearance == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		public bool CanViewHouseVault(GamePlayer p, int vaultIndex)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if (perm.Vault1 == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}

		public bool CanUseHouseVault(GamePlayer p)
		{
			if (HasOwnerPermissions(p) || p.Client.Account.PrivLevel > 1)
				return true;
			if (this.DatabaseItem.GuildHouse && this.DatabaseItem.GuildName == p.GuildName && p.Guild.GotAccess(p, eGuildRank.Leader))
				return true;
			foreach (DBHousePermissions perm in HouseAccess)
			{
				if (perm.Vault1 == 0)// optim
					continue;
				if (IsInPerm(null, ePermsTypes.All, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Player, perm.PermLevel))
					return true;
				if (IsInPerm(p.Name, ePermsTypes.Account, perm.PermLevel))
					return true;
			}
			return false;
		}



		public void RemoveFromPerm(int slot)
		{
			DBHouseCharsXPerms todel = null;
			foreach (DBHouseCharsXPerms perm in CharsPermissions)
				if (perm.Slot == slot)
				{
					todel = perm;
					break;
				}
			if (todel == null)
				return;
			CharsPermissions.Remove(todel);
			GameServer.Database.DeleteObject(todel);
		}

		public void ChangePerm(int slot, int nlvl)
		{
			foreach (DBHouseCharsXPerms perm in CharsPermissions)
				if (perm.Slot == slot)
				{
					perm.PermLevel = nlvl;
					GameServer.Database.SaveObject(perm);
					break;
				}
		}

		public bool AddToPerm(GamePlayer p, ePermsTypes type, int lvl)
		{
			if (IsInPerm(p.Name, type, lvl))
				return false;
			DBHouseCharsXPerms perm = new DBHouseCharsXPerms();
			perm.HouseNumber = HouseNumber;
			perm.Type = (byte)type;
			perm.Name = p.Name;
			perm.PermLevel = lvl;
			int slot = 0;
			bool ok = false;
			while (!ok)
			{
				ok = true;
				foreach (DBHouseCharsXPerms pe in CharsPermissions)
				{
					if (pe.Slot == slot)
					{
						ok = false;
						slot++;
						break;
					}
				}
			}
			if (!ok)
				return false;
			perm.Slot = slot;
			CharsPermissions.Add(perm);
			GameServer.Database.AddNewObject(perm);
			return true;
		}

		public bool AddGuildToPerm(Guild g, ePermsTypes type, int lvl)
        {
            if (IsInPerm(g.Name, type, lvl))
                return false;
            DBHouseCharsXPerms perm = new DBHouseCharsXPerms();
            perm.HouseNumber = HouseNumber;
            perm.Type = (byte)type;
            perm.Name = g.Name;
            perm.PermLevel = lvl;
            int slot = 0;
            bool ok = false;
            while (!ok)
            {
                ok = true;
                foreach (DBHouseCharsXPerms pe in CharsPermissions)
                {
                    if (pe.Slot == slot)
                    {
                        ok = false;
                        slot++;
                        break;
                    }
                }
            }
            if (!ok)
                return false;
            perm.Slot = slot;
            CharsPermissions.Add(perm);
            GameServer.Database.AddNewObject(perm);
            return true;
        }

        public bool AddAllToPerm(ePermsTypes type, int lvl)
        {
            string pname = "All";
            if (IsInPerm(pname, type, lvl))
                return false;
            DBHouseCharsXPerms perm = new DBHouseCharsXPerms();
            perm.HouseNumber = HouseNumber;
            perm.Type = (byte)type;
            perm.Name = pname;
            perm.PermLevel = lvl;
            int slot = 0;
            bool ok = false;
            while (!ok)
            {
                ok = true;
                foreach (DBHouseCharsXPerms pe in CharsPermissions)
                {
                    if (pe.Slot == slot)
                    {
                        ok = false;
                        slot++;
                        break;
                    }
                }
            }
            if (!ok)
                return false;
            perm.Slot = slot;
            CharsPermissions.Add(perm);
            GameServer.Database.AddNewObject(perm);
            return true;
        }

		/// <summary>
		/// Used to get into a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		public void Enter(GamePlayer player)
		{
            ArrayList list = this.GetAllPlayersInHouse();
            if (list.Count == 0)
            {
                foreach (GamePlayer pl in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    pl.Out.SendHouseOccuped(this, true);
            }
			GameClient client = player.Client;
			client.Out.SendMessage(string.Format("Entering house {0}.", this.HouseNumber), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendEnterHouse(this);
			client.Out.SendFurniture(this);
			client.Player.InHouse = true;
			client.Player.CurrentHouse = this;

			switch (this.Model)
			{
				//thx to sp4m
				default:
					client.Player.MoveTo((ushort)this.RegionID, this.X, this.Y, 25022, client.Player.Heading);
					break;

				case 1:
					client.Player.MoveTo((ushort)this.RegionID, this.X + 80, this.Y + 100, ((ushort)(25025)), client.Player.Heading);
					break;

				case 2:
					client.Player.MoveTo((ushort)this.RegionID, this.X - 260, this.Y + 100, ((ushort)(24910)), client.Player.Heading);
					break;

				case 3:
					client.Player.MoveTo((ushort)this.RegionID, this.X - 200, this.Y + 100, ((ushort)(24800)), client.Player.Heading);
					break;

				case 4:
					client.Player.MoveTo((ushort)this.RegionID, this.X - 350, this.Y - 30, ((ushort)(24660)), client.Player.Heading);
					break;

				case 5:
					client.Player.MoveTo((ushort)this.RegionID, this.X + 230, this.Y - 480, ((ushort)(25100)), client.Player.Heading);
					break;

				case 6:
					client.Player.MoveTo((ushort)this.RegionID, this.X - 80, this.Y - 660, ((ushort)(24700)), client.Player.Heading);
					break;

				case 7:
					client.Player.MoveTo((ushort)this.RegionID, this.X - 80, this.Y - 660, ((ushort)(24700)), client.Player.Heading);
					break;

				case 8:
					client.Player.MoveTo((ushort)this.RegionID, this.X - 90, this.Y - 625, ((ushort)(24670)), client.Player.Heading);
					break;

				case 9:
					client.Player.MoveTo((ushort)this.RegionID, this.X + 400, this.Y - 160, ((ushort)(25150)), client.Player.Heading);
					break;

				case 10:
					client.Player.MoveTo((ushort)this.RegionID, this.X + 400, this.Y - 80, ((ushort)(25060)), client.Player.Heading);
					break;

				case 11:
					client.Player.MoveTo((ushort)this.RegionID, this.X + 400, this.Y - 60, ((ushort)(24900)), client.Player.Heading);
					break;

				case 12:
					client.Player.MoveTo((ushort)this.RegionID, this.X, this.Y - 620, ((ushort)(24595)), client.Player.Heading);
					break;
			}

			client.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "House.Enter.EnteredHouse", this.HouseNumber), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// The spot you are teleported to when you exit this house.
		/// </summary>
		public IGameLocation OutdoorJumpPoint
		{
			get
			{
				double angle = Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
				int x = (int)(X + (0 * Math.Cos(angle) + 500 * Math.Sin(angle)));
				int y = (int)(Y - (500 * Math.Cos(angle) - 0 * Math.Sin(angle)));
				ushort heading = (ushort)((Heading < 180 ? Heading + 180 : Heading - 180) / 0.08789);

				return new GameLocation("Housing", RegionID, x, y, Z, heading);
			}
		}

		/// <summary>
		/// Used to leave a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		/// <param name="silent">text or not</param>
		public void Exit(GamePlayer player, bool silent)
		{
			double angle = Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
			int x = (int)(X + (0 * Math.Cos(angle) + 500 * Math.Sin(angle)));
			int y = (int)(Y - (500 * Math.Cos(angle) - 0 * Math.Sin(angle)));
			ushort heading = (ushort)((Heading < 180 ? Heading + 180 : Heading - 180) / 0.08789);
			player.MoveTo(RegionID, x, y, Z, heading);
			if (!silent)
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "House.Exit.LeftHouse", HouseNumber), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			player.Out.SendExitHouse(this);
            ArrayList list = this.GetAllPlayersInHouse();
            if (list.Count == 0)
            {
                foreach (GamePlayer pl in player.GetPlayersInRadius(HouseMgr.HOUSE_DISTANCE))
                    pl.Out.SendHouseOccuped(this, false);
            }
		}


		/// <summary>
		/// Sends the house info window to a player
		/// </summary>
		/// <param name="player">the player</param>
		public void SendHouseInfo(GamePlayer player)
		{
			ArrayList text = new ArrayList();
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Owner", this.Name));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Lotnum", HouseNumber));
			int level = Model - (int)((Model - 1) / 4) * 4;
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Level", level));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Porch"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.PorchEnabled", (Porch ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.PorchRoofColor", PorchRoofColor));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.ExteriorMaterials"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.RoofMaterial", RoofMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.WallMaterial", WallMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.DoorMaterial", DoorMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.TrussMaterial", TrussMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.PorchMaterial", PorchMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.WindowMaterial", WindowMaterial));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.ExteriorUpgrades"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.OutdoorGuildBanner", ((OutdoorGuildBanner) ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.OutdoorGuildShield", ((OutdoorGuildShield) ? "Y" : "N")));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.InteriorUpgrades"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.IndoorGuildBanner", ((IndoorGuildBanner) ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.IndoorGuildShield", ((IndoorGuildShield) ? "Y" : "N")));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.InteriorCarpets"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug1Color", Rug1Color));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug2Color", Rug2Color));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug3Color", Rug3Color));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug4Color", Rug4Color));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Lockbox", Money.GetString(KeptMoney)));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.RentalPrice", Money.GetString(HouseMgr.GetRentByModel(Model))));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.MaxLockbox", Money.GetString(HouseMgr.GetRentByModel(Model) * 4)));
			TimeSpan due = (LastPaid.AddDays(7).AddHours(1) - DateTime.Now);
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.RentDueIn", due.Days, due.Hours));
			
			player.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.HouseOwner", this.Name), text);

		}

		/// <summary>
		/// Returns true if the player has owner permissions for this house
		/// </summary>
		/// <param name="player">the player to check</param>
		/// <returns>true if the player has owner permissions for this house</returns>
		public bool HasOwnerPermissions(GamePlayer player)
		{
			return HouseMgr.IsOwner(m_databaseItem, player, false);
		}

		/// <summary>
		/// Returns true if the player is the real owner of this house
		/// </summary>
		/// <param name="player">the player to check</param>
		/// <returns>true if the player is the real owner of this house</returns>
		public bool IsRealOwner(GamePlayer player)
		{
			return HouseMgr.IsOwner(m_databaseItem, player, true);
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
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort)this.RegionID, this.X, this.Y, 25000, WorldMgr.VISIBILITY_DISTANCE))
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
			if (!CanEditAppearance(player))
				return;

			MerchantTradeItems items;

			if (player.InHouse)
			{
				items = HouseTemplateMgr.IndoorMenuItems;
			}
			else
			{
				items = HouseTemplateMgr.OutdoorMenuItems;
			}

			if (items == null)
				return;

			long price = 0;

			foreach (int slot in changes)
			{
				int page = slot / 30;
				int pslot = slot % 30;
				ItemTemplate item = items.GetItem(page, (eMerchantWindowSlot)pslot);
				if (item != null)
				{
					price += Money.GetMoney(0, 0, item.Gold, item.Silver, item.Copper);
				}
			}
			if (!player.RemoveMoney(price, LanguageMgr.GetTranslation(player.Client, "House.Edit.PayForChanges", Money.GetString(price))))
				{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "House.Edit.NotEnoughMoney"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}

			foreach (int slot in changes)
			{
				int page = slot / 30;
				int pslot = slot % 30;

				ItemTemplate item = items.GetItem(page, (eMerchantWindowSlot)pslot);

				if (item != null)
				{
					switch ((eObjectType)item.Object_Type)
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

			if (player.InHouse)
			{
				foreach (GamePlayer p in this.GetAllPlayersInHouse())
				{
					p.Out.SendEnterHouse(this); //update rugs.
				}
			}
			else
			{
				foreach (GamePlayer p in WorldMgr.GetPlayersCloseToSpot((ushort)this.RegionID, this.X, this.Y, this.Z, HouseMgr.HOUSE_DISTANCE))
				{
					p.Out.SendHouse(this); //update wall look
					p.Out.SendGarden(this);
				}
			}
		}

		/// <summary>
		/// Fill a hookpoint with an object, create it in the database.
		/// </summary>
		/// <param name="item">The itemtemplate of the item used to fill the hookpoint (can be null if templateid is filled)</param>
		/// <param name="position">The position of the hookpoint</param>
		/// <param name="templateID">The template id of the item (can be blank if item is filled)</param>
		public GameObject FillHookpoint(ItemTemplate item, uint position, string templateID)
		{
			if (item == null)
			{
				item = (ItemTemplate)GameServer.Database.SelectObject(typeof(ItemTemplate), "Id_nb = '" + GameServer.Database.Escape(templateID) + "'");
				if (item == null)
					return null;
			}


			//get location from slot
			IPoint3D location = GetHookpointLocation(position);
			if (location == null)
				return null;

			int x = location.X;
			int y = location.Y;
			int z = location.Z;
			ushort heading = GetHookpointHeading(position);
			GameStaticItem sItem = null;

			switch ((eObjectType)item.Object_Type)
			{
				case eObjectType.HouseNPC:
					{
						NpcTemplate npt = NpcTemplateMgr.GetTemplate(item.Bonus);
						if (npt == null)
							return null;

						GameNPC hNPC = (GameNPC)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(npt.ClassType, false);

						if (hNPC == null)
						{
							foreach (Assembly asm in ScriptMgr.Scripts)
							{
								hNPC = (GameNPC)asm.CreateInstance(npt.ClassType, false);
								if (hNPC != null) break;
							}
						}

						if (hNPC == null)
						{
							HouseMgr.Logger.Error("Can't create instance of type: " + npt.ClassType);
							return null;
						}

						hNPC.LoadTemplate(npt);

						hNPC.Name = item.Name;
						hNPC.CurrentHouse = this;
						hNPC.InHouse = true;
						hNPC.X = x;
						hNPC.Y = y;
						hNPC.Z = z;
						hNPC.Heading = heading;
						hNPC.CurrentRegionID = RegionID;
						hNPC.Realm = (eRealm)item.Realm;
						hNPC.Flags ^= (uint)GameNPC.eFlags.PEACE;
						hNPC.AddToWorld();
						break;
					}
				case eObjectType.HouseBindstone:
					{
						sItem = new GameStaticItem();
						sItem.CurrentHouse = this;
						sItem.InHouse = true;
						sItem.X = x;
						sItem.Y = y;
						sItem.Z = z;
						sItem.Heading = heading;
						sItem.CurrentRegionID = RegionID;
						sItem.Name = item.Name;
						sItem.Model = (ushort)item.Model;
						sItem.AddToWorld();
						//0:07:45.984 S=>C 0xD9 item/door create v171 (oid:0x0DDB emblem:0x0000 heading:0x0DE5 x:596203 y:530174 z:24723 model:0x05D2 health:  0% flags:0x04(realm:0) extraBytes:0 unk1_171:0x0096220C name:"Hibernia bindstone")
						//add bind point
						break;
					}
				case eObjectType.HouseInteriorObject:
					{
						sItem = new GameStaticItem();
						sItem.CurrentHouse = this;
						sItem.InHouse = true;
						sItem.X = x;
						sItem.Y = y;
						sItem.Z = z;
						sItem.Heading = heading;
						sItem.CurrentRegionID = RegionID;
						sItem.Name = item.Name;
						sItem.Model = (ushort)item.Model;
						sItem.AddToWorld();
						break;
					}
			}
			return sItem;
		}

        /// <summary>
        /// Is the player allowed to remove items from hookpoints?
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CanEmptyHook(GamePlayer player)
        {
            if (IsOwner(player) || player.Client.Account.PrivLevel > 1)
                return true;

            return false;
        }

		public void EmptyHookpoint(GamePlayer player, GameObject obj)
		{
            if (!CanEmptyHook(player))
            {
                player.Out.SendMessage("Only the Owner of a House can remove or place Items on Hookpoints!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            int posi = GetHookpointPosition(obj.X, obj.Y, obj.Z);
            if (posi == 16) //the standard return value if none is found
                return;

            //get the housepoint item
            String sqlWhere = String.Format("Position = '{0}' AND HouseID = '{1}'",
                posi, obj.CurrentHouse.HouseNumber);
            DBHousepointItem item = (DBHousepointItem)GameServer.Database.SelectObject(typeof(DBHousepointItem), sqlWhere);
            if (item == null)
                return;

			obj.Delete();
			GameServer.Database.DeleteObject(item);

			// Need to clear the current house points so we can replace items
			player.CurrentHouse.HousepointItems.Clear();
            /* what is this used for? it causes errors and there is no reason for it
			foreach (DBHousepointItem hpitem in GameServer.Database.SelectObjects(typeof(DBHousepointItem), "HouseID = '" + player.CurrentHouse.HouseNumber + "'"))
			{
				FillHookpoint(null, hpitem.Position, hpitem.ItemTemplateID);
				this.HousepointItems[hpitem.Position] = hpitem;
			} */

			player.CurrentHouse.SendUpdate();
            ItemTemplate template = (ItemTemplate)GameServer.Database.SelectObject(typeof(ItemTemplate), "Name = '" + GameServer.Database.Escape(obj.Name) + "'");
			if (template != null)
			player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, new InventoryItem(template));
		}

		/// <summary>
		/// Find a number that can be used for this vault.
		/// </summary>
		/// <returns></returns>
		public int GetFreeVaultNumber()
		{
			bool[] usedVaults = new bool[4] { false, false, false, false };
			String sqlWhere = String.Format("HouseID = '{0}' and ItemTemplateID like '%_vault'",
				HouseNumber);

			foreach (DBHousepointItem housePointItem in GameServer.Database.SelectObjects(typeof(DBHousepointItem), sqlWhere))
				if (housePointItem.Index >= 0 && housePointItem.Index <= 3)
					usedVaults[housePointItem.Index] = true;

			for (int freeVault = 0; freeVault <= 3; ++freeVault)
				if (!usedVaults[freeVault])
					return freeVault;

			return -1;
		}

		public House(DBHouse house)
		{
			m_databaseItem = house;
			m_houseAccess = new DBHousePermissions[10];
			m_indooritems = new Hashtable();
			m_outdooritems = new Hashtable();
			m_housepointitems = new Hashtable();
			m_charspermissions = new ArrayList();
		}

		#region Housepoint location

		public static void AddNewOffset(HouseHookpointOffset o)
		{
			RELATIVE_HOOKPOINTS_COORDS[o.Model][o.Hookpoint] = new int[] { o.OffX, o.OffY, o.OffZ, o.OffH };
		}

		public static void LoadHookpointOffsets()
		{
			//initialise array
			for (int i = 12; i > 0; i--)
			{
				for (int j = 1; j < House.RELATIVE_HOOKPOINTS_COORDS[i].Length; j++)
				{
					House.RELATIVE_HOOKPOINTS_COORDS[i][j] = null;
				}
			}
			HouseHookpointOffset[] objs = (HouseHookpointOffset[])GameServer.Database.SelectAllObjects(typeof(HouseHookpointOffset));
			foreach (HouseHookpointOffset o in objs)
			{
				AddNewOffset(o);
			}
		}

		/// <summary>
		/// Housing hookpoint coordinates offset relative to a house
		/// Note that this is using 1 as a base instead of 0
		/// </summary>
		private static readonly int[][][] RELATIVE_HOOKPOINTS_COORDS = new int[][][]
			{
				// NOTHING : Lot
				null,
				// ALB Cottage (model 1)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// ALB (model 2)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// ALB Villa(model 3)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// ALB Mansion(model 4)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID Cottage (model 5)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 6)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 7)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 8)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 9)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 10)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 11)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
				// MID (model 12)
				new int[MAX_HOOKPOINT_LOCATIONS + 1][],
			};

		public Point3D GetHookpointLocation(uint n)
		{
			try
			{
				Point3D p = new Point3D(X + RELATIVE_HOOKPOINTS_COORDS[Model][n][0], Y + RELATIVE_HOOKPOINTS_COORDS[Model][n][1], 25000 + RELATIVE_HOOKPOINTS_COORDS[Model][n][2]);
				return p;
			}
			catch
			{
				return null;
			}
		}

        public int GetHookpointPosition(int objX, int objY, int objZ)
        {
            int position = 16; //start with a position it can never be that can be checked for
            for (int i = 0; i < 15; i++)
            {
                if (RELATIVE_HOOKPOINTS_COORDS[Model][i] != null)
                {
                    if (RELATIVE_HOOKPOINTS_COORDS[Model][i][0] + X == objX && RELATIVE_HOOKPOINTS_COORDS[Model][i][1] + Y == objY)
                        position = i;
                }
            }
            return position;
        }

		public ushort GetHookpointHeading(uint n)
		{
			return (ushort)(Heading + RELATIVE_HOOKPOINTS_COORDS[Model][n][3]);
		}
		#endregion

		/// <summary>
		/// Load a house from the database
		/// </summary>
		public void LoadFromDatabase()
		{
			int i = 0;
			foreach (DBHouseIndoorItem dbiitem in GameServer.Database.SelectObjects(typeof(DBHouseIndoorItem), "HouseNumber = '" + this.HouseNumber + "'"))
			{
				IndoorItem iitem = new IndoorItem();
				iitem.CopyFrom(dbiitem);
				this.IndoorItems.Add(i++, iitem);
			}
			i = 0;
			foreach (DBHouseOutdoorItem dboitem in GameServer.Database.SelectObjects(typeof(DBHouseOutdoorItem), "HouseNumber = '" + this.HouseNumber + "'"))
			{
				OutdoorItem oitem = new OutdoorItem();
				oitem.CopyFrom(dboitem);
				this.OutdoorItems.Add(i++, oitem);
			}

			foreach (DBHouseCharsXPerms d in GameServer.Database.SelectObjects(typeof(DBHouseCharsXPerms), "HouseNumber = '" + this.HouseNumber + "'"))
			{
				this.CharsPermissions.Add(d);
			}

			foreach (DBHousePermissions dbperm in GameServer.Database.SelectObjects(typeof(DBHousePermissions), "HouseNumber = '" + this.HouseNumber + "'"))
			{
				this.HouseAccess[dbperm.PermLevel] = dbperm;
			}

			foreach (DBHousepointItem item in GameServer.Database.SelectObjects(typeof(DBHousepointItem), "HouseID = '" + this.HouseNumber + "'"))
			{
				if (item.ItemTemplateID.EndsWith("_vault"))
				{
					ItemTemplate template = (ItemTemplate)GameServer.Database.SelectObject(typeof(ItemTemplate),
						"Id_nb = '" + GameServer.Database.Escape(item.ItemTemplateID) + "'");
					if (template == null)
						continue;
					GameHouseVault houseVault = new GameHouseVault(template, item.Index);
					houseVault.Attach(this, item);
				}
				else
					FillHookpoint(null, item.Position, item.ItemTemplateID);
				this.HousepointItems[item.Position] = item;
			}
		}
	}
}