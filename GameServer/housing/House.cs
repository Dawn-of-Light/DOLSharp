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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DOL.Database;
using DOL.GS.Finance;
using DOL.GS.Geometry;
using DOL.Language;
using log4net;

namespace DOL.GS.Housing
{
	public class House
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly DBHouse dbHouse;
		private readonly Dictionary<int, DBHouseCharsXPerms> _housePermissions;
		private readonly Dictionary<uint, DBHouseHookpointItem> _housepointItems;
		private readonly Dictionary<int, IndoorItem> _indoorItems;
		private readonly Dictionary<int, OutdoorItem> _outdoorItems;
		private readonly Dictionary<int, DBHousePermissions> _permissionLevels;
		private GameConsignmentMerchant _consignmentMerchant;

		#region Properties

		public int HouseNumber
		{
			get { return dbHouse.HouseNumber; }
			set { dbHouse.HouseNumber = value; }
		}

		public string OwnerID
		{
			get { return dbHouse.OwnerID; }
			set { dbHouse.OwnerID = value; }
		}

		public int Model
		{
			get { return dbHouse.Model; }
			set { dbHouse.Model = value; }
		}

		public eRealm Realm
		{
			get
			{
				if (Model < 5)
					return eRealm.Albion;

				if (Model < 9)
					return eRealm.Midgard;

				return eRealm.Hibernia;
			}
		}

		public int Emblem
		{
			get { return dbHouse.Emblem; }
			set { dbHouse.Emblem = value; }
		}

		public int PorchRoofColor
		{
			get { return dbHouse.PorchRoofColor; }
			set { dbHouse.PorchRoofColor = value; }
		}

		public int PorchMaterial
		{
			get { return dbHouse.PorchMaterial; }
			set { dbHouse.PorchMaterial = value; }
		}

		public bool Porch
		{
			get { return dbHouse.Porch; }
			set { dbHouse.Porch = value; }
		}

		public bool IndoorGuildBanner
		{
			get { return dbHouse.IndoorGuildBanner; }
			set { dbHouse.IndoorGuildBanner = value; }
		}

		public bool IndoorGuildShield
		{
			get { return dbHouse.IndoorGuildShield; }
			set { dbHouse.IndoorGuildShield = value; }
		}

		public bool OutdoorGuildBanner
		{
			get { return dbHouse.OutdoorGuildBanner; }
			set { dbHouse.OutdoorGuildBanner = value; }
		}

		public bool OutdoorGuildShield
		{
			get { return dbHouse.OutdoorGuildShield; }
			set { dbHouse.OutdoorGuildShield = value; }
		}

		public int RoofMaterial
		{
			get { return dbHouse.RoofMaterial; }
			set { dbHouse.RoofMaterial = value; }
		}

		public int DoorMaterial
		{
			get { return dbHouse.DoorMaterial; }
			set { dbHouse.DoorMaterial = value; }
		}

		public int WallMaterial
		{
			get { return dbHouse.WallMaterial; }
			set { dbHouse.WallMaterial = value; }
		}

		public int TrussMaterial
		{
			get { return dbHouse.TrussMaterial; }
			set { dbHouse.TrussMaterial = value; }
		}

		public int WindowMaterial
		{
			get { return dbHouse.WindowMaterial; }
			set { dbHouse.WindowMaterial = value; }
		}

		public int Rug1Color
		{
			get { return dbHouse.Rug1Color; }
			set { dbHouse.Rug1Color = value; }
		}

		public int Rug2Color
		{
			get { return dbHouse.Rug2Color; }
			set { dbHouse.Rug2Color = value; }
		}

		public int Rug3Color
		{
			get { return dbHouse.Rug3Color; }
			set { dbHouse.Rug3Color = value; }
		}

		public int Rug4Color
		{
			get { return dbHouse.Rug4Color; }
			set { dbHouse.Rug4Color = value; }
		}

		public DateTime LastPaid
		{
			get { return dbHouse.LastPaid; }
			set { dbHouse.LastPaid = value; }
		}

		public long KeptMoney
		{
			get { return dbHouse.KeptMoney; }
			set { dbHouse.KeptMoney = value; }
		}

		public bool NoPurge
		{
			get { return dbHouse.NoPurge; }
			set { dbHouse.NoPurge = value; }
		}

		public int UniqueID { get; set; }

		public IDictionary<int, IndoorItem> IndoorItems
		{
			get { return _indoorItems; }
		}

		public IDictionary<int, OutdoorItem> OutdoorItems
		{
			get { return _outdoorItems; }
		}

		public IDictionary<uint, DBHouseHookpointItem> HousepointItems
		{
			get { return _housepointItems; }
		}

		public DBHouse DatabaseItem
		{
			get { return dbHouse; }
		}

		public IEnumerable<KeyValuePair<int, DBHouseCharsXPerms>> HousePermissions
		{
			get { return _housePermissions.OrderBy(entry => entry.Value.CreationTime); }
		}

		public IDictionary<int, DBHouseCharsXPerms> CharXPermissions
		{
			get { return _housePermissions; }
		}

		public IDictionary<int, DBHousePermissions> PermissionLevels
		{
			get { return _permissionLevels; }
		}

		public GameConsignmentMerchant ConsignmentMerchant
		{
			get { return _consignmentMerchant; }
			set { _consignmentMerchant = value; }
		}

		public bool IsOccupied
		{
			get
			{
				foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Position.With(z: 25000), WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player.CurrentHouse == this && player.InHouse)
					{
						return true;
					}
				}

				return false;
			}
		}

        public Position Position
        {
            get => Position.Create(dbHouse.RegionID, dbHouse.X, dbHouse.Y, dbHouse.Z, (ushort)(dbHouse.Heading/180.0*2048));
            set
            {
                dbHouse.X = value.X;
                dbHouse.Y = value.Y;
                dbHouse.Z = value.Z;
                dbHouse.Heading = value.Orientation.InDegrees;
                dbHouse.RegionID = value.RegionID;
            }
        }

        [Obsolete("Use .Position instead!")]
        public int X
        {
            get => Position.X;
            set => Position = Position.With(x: value);
        }

        [Obsolete("Use .Position instead!")]
        public int Y
        {
            get => Position.Y;
            set => Position = Position.With(y: value);
        }

        [Obsolete("Use .Position instead!")]
        public int Z
        {
            get => Position.Z;
            set => Position = Position.With(z: value);
        }

        [Obsolete("Use either .Orientation (in degrees) or .Position.Heading instead!")]
        public ushort Heading
        {
            get => (ushort)dbHouse.Heading;
            set => dbHouse.Heading = value;
        }

        ///<remarks>House orientation in degrees.</remarks>
        public ushort Orientation
        {
            get => (ushort)dbHouse.Heading;
        }

        public ushort RegionID
        {
            get => Position.RegionID;
            set => Position = Position.With(regionID: value);
        }

		public string Name
		{
			get { return dbHouse.Name; }
			set { dbHouse.Name = value; }
		}

		#endregion

		public House(DBHouse house)
		{
			dbHouse = house;
			_permissionLevels = new Dictionary<int, DBHousePermissions>();
			_indoorItems = new Dictionary<int, IndoorItem>();
			_outdoorItems = new Dictionary<int, OutdoorItem>();
			_housepointItems = new Dictionary<uint, DBHouseHookpointItem>();
			_housePermissions = new Dictionary<int, DBHouseCharsXPerms>();
		}

		~House()
		{
			log.DebugFormat("House destructor called for House #{0} in region {1}", HouseNumber, RegionID);
		}

        [Obsolete("Use OutdoorJumpPosition instead!")]
        public GameLocation OutdoorJumpPoint
            => new GameLocation(OutdoorJumpPosition);

        public Position OutdoorJumpPosition
            => (Position + Vector.Create(Position.Orientation + Angle.Degrees(180), length: 500)).TurnedAround();

		/// <summary>
		/// Sends a update of the house and the garden to all players in range
		/// </summary>
		public void SendUpdate()
		{
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Position, HousingConstants.HouseViewingDistance))
			{
				player.Out.SendHouse(this);
				player.Out.SendGarden(this);
			}
			
			foreach (GamePlayer player in GetAllPlayersInHouse())
			{
				player.Out.SendEnterHouse(this);
			}
		}

		/// <summary>
		/// Used to get into a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		public void Enter(GamePlayer player)
		{
			IList<GamePlayer> list = GetAllPlayersInHouse();
			if (list.Count == 0)
			{
				foreach (GamePlayer pl in WorldMgr.GetPlayersCloseToSpot(Position, HousingConstants.HouseViewingDistance))
				{
					pl.Out.SendHouseOccupied(this, true);
				}
			}

			ChatUtil.SendSystemMessage(player, "House.Enter.EnteringHouse", HouseNumber);
			player.Out.SendEnterHouse(this);
			player.Out.SendFurniture(this);

			player.InHouse = true;
			player.CurrentHouse = this;

            switch (Model)
            {
                //thx to sp4m
                default:
                    player.MoveTo(Position.With(z: 25022, heading: 0));
                    break;

                case 1:
                    player.MoveTo(Position.With(z: 25025, heading: 0) + Vector.Create(x: 80, y: 100));
                    break;

                case 2:
                    player.MoveTo(Position.With(z: 24910, heading: 0) + Vector.Create(x: -260, y: 100));
                    break;

                case 3:
                    player.MoveTo(Position.With(z: 24800, heading: 0) + Vector.Create(x: -200, y: 100));
                    break;

                case 4:
                    player.MoveTo(Position.With(z: 24660, heading: 0) + Vector.Create(x: -350, y: -30));
                    break;

                case 5:
                    player.MoveTo(Position.With(z: 25100, heading: 0) + Vector.Create(x: 230, y: -480));
                    break;

                case 6:
                    player.MoveTo(Position.With(z: 24700, heading: 0) + Vector.Create(x: -80, y: -660));
                    break;

                case 7:
                    player.MoveTo(Position.With(z: 24700, heading: 0) + Vector.Create(x: -80, y: -660));
                    break;

                case 8:
                    player.MoveTo(Position.With(z: 24670, heading: 0) + Vector.Create(x: -90, y: -625));
                    break;

                case 9:
                    player.MoveTo(Position.With(z: 25150, heading: 0) + Vector.Create(x: 400, y: -160));
                    break;

                case 10:
                    player.MoveTo(Position.With(z: 25060, heading: 0) + Vector.Create(x: 400, y: -80));
                    break;

                case 11:
                    player.MoveTo(Position.With(z: 24900, heading: 0) + Vector.Create(x: 400, y: -60));
                    break;

                case 12:
                    player.MoveTo(Position.With(z: 24595, heading: 0) + Vector.Create(y: -620));
                    break;
            }

			ChatUtil.SendSystemMessage(player, "House.Enter.EnteredHouse", HouseNumber);
		}

		/// <summary>
		/// Used to leave a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		/// <param name="silent">text or not</param>
		public void Exit(GamePlayer player, bool silent)
		{
			player.MoveTo(OutdoorJumpPosition);

			if (!silent)
			{
				ChatUtil.SendSystemMessage(player, "House.Exit.LeftHouse", HouseNumber);
			}

			player.Out.SendExitHouse(this);

			IList<GamePlayer> list = GetAllPlayersInHouse();
			if (list.Count == 0)
			{
				foreach (GamePlayer pl in WorldMgr.GetPlayersCloseToSpot(Position, HousingConstants.HouseViewingDistance))
				{
					pl.Out.SendHouseOccupied(this, false);
				}
			}
		}

		/// <summary>
		/// Sends the house info window to a player
		/// </summary>
		/// <param name="player">the player</param>
		public void SendHouseInfo(GamePlayer player)
		{
			int level = Model - ((Model - 1) / 4) * 4;
			TimeSpan due = (LastPaid.AddDays(ServerProperties.Properties.RENT_DUE_DAYS).AddHours(1) - DateTime.Now);
			var text = new List<string>();

			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Owner", Name));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Lotnum", HouseNumber));

			if (level > 0)
				text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Level", level));
			else
				text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Level", "Lot"));

			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Lockbox", Money.GetString(KeptMoney)));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.RentalPrice", Money.GetString(HouseMgr.GetRentByModel(Model))));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.MaxLockbox", Money.GetString(HouseMgr.GetRentByModel(Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS)));
			if (ServerProperties.Properties.RENT_DUE_DAYS > 0)
				text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.RentDueIn", due.Days, due.Hours));
			else
				text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.RentDueIn", "No Rent! 0", "0"));

			text.Add(" ");

			if (player.Client.Account.PrivLevel > (int)ePrivLevel.Player)
			{
				text.Add("GM: Model: " + Model);
				text.Add("GM: Realm: " + GlobalConstants.RealmToName(Realm));
				text.Add("GM: Last Paid: " + LastPaid);
				text.Add(" ");
			}

			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Porch"));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.PorchEnabled", (Porch ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.PorchRoofColor", PorchRoofColor));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.ExteriorMaterials"));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.RoofMaterial", RoofMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.WallMaterial", WallMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.DoorMaterial", DoorMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.TrussMaterial", TrussMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.PorchMaterial", PorchMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.WindowMaterial", WindowMaterial));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.ExteriorUpgrades"));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.OutdoorGuildBanner", ((OutdoorGuildBanner) ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.OutdoorGuildShield", ((OutdoorGuildShield) ? "Y" : "N")));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.InteriorUpgrades"));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.IndoorGuildBanner", ((IndoorGuildBanner) ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.IndoorGuildShield", ((IndoorGuildShield) ? "Y" : "N")));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.InteriorCarpets"));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Rug1Color", Rug1Color));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Rug2Color", Rug2Color));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Rug3Color", Rug3Color));
			text.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.Rug4Color", Rug4Color));

			player.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(player.Client.Account.Language, "House.SendHouseInfo.HouseOwner", Name), text);
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
		public IList<GamePlayer> GetAllPlayersInHouse()
		{
			var ret = new List<GamePlayer>();
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Position.With(z: 25000), WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player.CurrentHouse == this && player.InHouse)
				{
					ret.Add(player);
				}
			}

			return ret;
		}

		/// <summary>
		/// Find a number that can be used for this vault.
		/// </summary>
		/// <returns></returns>
		public int GetFreeVaultNumber()
		{
			var usedVaults = new[] {false, false, false, false};

			foreach (var housePointItem in DOLDB<DBHouseHookpointItem>.SelectObjects(DB.Column(nameof(DBHouseHookpointItem.HouseNumber)).IsEqualTo(HouseNumber).And(DB.Column(nameof(DBHouseHookpointItem.ItemTemplateID)).IsLike("%_vault"))))
			{
				if (housePointItem.Index >= 0 && housePointItem.Index <= 3)
				{
					usedVaults[housePointItem.Index] = true;
				}
			}

			for (int freeVault = 0; freeVault <= 3; ++freeVault)
			{
				if (!usedVaults[freeVault])
				{
					return freeVault;
				}
			}

			return -1;
		}

		#region Hookpoints

		public static bool AddNewOffset(HouseHookpointOffset o)
		{
			if (o.HookpointID <= HousingConstants.MaxHookpointLocations)
			{
				HousingConstants.RelativeHookpointsCoords[o.HouseModel][o.HookpointID] = new[] {o.X, o.Y, o.Z, o.Heading};
				return true;
			}

			log.Error("[Housing]: HouseHookPointOffset exceeds array size.  Model " + o.HouseModel + ", hookpoint " + o.HookpointID);

			return false;
		}

		public static void LoadHookpointOffsets()
		{
			for (int i = HousingConstants.MaxHouseModel; i > 0; i--)
			{
				for (int j = 1; j < HousingConstants.RelativeHookpointsCoords[i].Length; j++)
				{
					HousingConstants.RelativeHookpointsCoords[i][j] = null;
				}
			}

			IList<HouseHookpointOffset> objs = GameServer.Database.SelectAllObjects<HouseHookpointOffset>();
			foreach (HouseHookpointOffset o in objs)
			{
				AddNewOffset(o);
			}
		}

        [Obsolete("Use GetHookPointCoordinate(uint) instead!")]
		public Point3D GetHookpointLocation(uint n)
        {
            var loc = GetHookPointCoordinate(n);
            if(loc == Coordinate.Nowhere) return null;
            else return loc.ToPoint3D();
        }

        public Coordinate GetHookPointCoordinate(uint number)
        {
            if (number > HousingConstants.MaxHookpointLocations) return Coordinate.Nowhere;

            int[] hookpointsCoords = HousingConstants.RelativeHookpointsCoords[Model][number];

            if (hookpointsCoords == null) return Coordinate.Nowhere;

            var hookPointOffset = Vector.Create(x: hookpointsCoords[0], y: hookpointsCoords[1], z: hookpointsCoords[2]);
            return Position.Coordinate.With(z: 25000) + hookPointOffset;
        }

        private int GetHookpointPosition(Coordinate loc)
		{
			int position = -1;

			for (int i = 0; i < HousingConstants.MaxHookpointLocations; i++)
			{
				if (HousingConstants.RelativeHookpointsCoords[Model][i] != null)
				{
					if (HousingConstants.RelativeHookpointsCoords[Model][i][0] + Position.X == loc.X &&
					    HousingConstants.RelativeHookpointsCoords[Model][i][1] + Position.Y == loc.Y)
					{
						position = i;
					}
				}
			}

			return position;
		}

		private ushort GetHookpointHeading(uint n)
		{
			if (n > HousingConstants.MaxHookpointLocations)
				return 0;

			int[] hookpointsCoords = HousingConstants.RelativeHookpointsCoords[Model][n];

			if (hookpointsCoords == null)
				return 0;

			return (ushort) (Orientation + hookpointsCoords[3]);
		}

		/// <summary>
		/// Fill a hookpoint with an object, create it in the database.
		/// </summary>
		/// <param name="item">The itemtemplate of the item used to fill the hookpoint (can be null if templateid is filled)</param>
		/// <param name="position">The position of the hookpoint</param>
		/// <param name="templateID">The template id of the item (can be blank if item is filled)</param>
		/// <param name="heading">The requested heading of this house item</param>
		public bool FillHookpoint(uint position, string templateID, ushort heading, int index)
		{
			ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(templateID);

			if (item == null)
				return false;

			//get location from slot
			var coordinate = GetHookPointCoordinate(position);
			if (coordinate == Coordinate.Nowhere) return false;

			GameObject hookpointObject = null;

			switch ((eObjectType)item.Object_Type)
			{
				case eObjectType.HouseVault:
					{
						var houseVault = new GameHouseVault(item, index);
						houseVault.Attach(this, position, heading);
						hookpointObject = houseVault;
						break;
					}
				case eObjectType.HouseNPC:
					{
						hookpointObject = GameServer.ServerRules.PlaceHousingNPC(this, item, coordinate, GetHookpointHeading(position));
						break;
					}
				case eObjectType.HouseBindstone:
					{
						hookpointObject = new GameStaticItem();
						hookpointObject.CurrentHouse = this;
						hookpointObject.InHouse = true;
						hookpointObject.OwnerID = templateID;
                        hookpointObject.Position = Position.Create(RegionID, coordinate, heading);
						hookpointObject.Name = item.Name;
						hookpointObject.Model = (ushort) item.Model;
						hookpointObject.AddToWorld();
						//0:07:45.984 S=>C 0xD9 item/door create v171 (oid:0x0DDB emblem:0x0000 heading:0x0DE5 x:596203 y:530174 z:24723 model:0x05D2 health:  0% flags:0x04(realm:0) extraBytes:0 unk1_171:0x0096220C name:"Hibernia bindstone")
						//add bind point
						break;
					}
				case eObjectType.HouseInteriorObject:
					{
						hookpointObject = GameServer.ServerRules.PlaceHousingInteriorItem(this, item, coordinate, heading);
						break;
					}
			}

			if (hookpointObject != null)
			{
				HousepointItems[position].GameObject = hookpointObject;
				return true;
			}

			return false;
		}

		public void EmptyHookpoint(GamePlayer player, GameObject obj, bool addToInventory = true)
		{
			if (player.CurrentHouse != this || CanEmptyHookpoint(player) == false)
			{
				ChatUtil.SendSystemMessage(player, "Only the Owner of a House can remove or place Items on Hookpoints!");
				return;
			}

			int position = GetHookpointPosition(obj.Coordinate);

			if (position < 0)
			{
				ChatUtil.SendSystemMessage(player, "Invalid hookpoint position " + position);
				return;
			}

			var items = DOLDB<DBHouseHookpointItem>.SelectObjects(DB.Column(nameof(DBHouseHookpointItem.HookpointID)).IsEqualTo(position).And(DB.Column(nameof(DBHouseHookpointItem.HouseNumber)).IsEqualTo(obj.CurrentHouse.HouseNumber)));
			if (items.Count == 0)
			{
				ChatUtil.SendSystemMessage(player, "No hookpoint item found at position " + position);
				return;
			}

			// clear every item from this hookpoint
			GameServer.Database.DeleteObject(items);

			obj.Delete();
			player.CurrentHouse.HousepointItems.Remove((uint)position);
			player.CurrentHouse.SendUpdate();

			if (addToInventory)
			{
				var template = GameServer.Database.FindObjectByKey<ItemTemplate>(obj.OwnerID);
				if (template != null)
				{
                    if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, GameInventoryItem.Create(template)))
                        InventoryLogging.LogInventoryAction("(HOUSE;" + HouseNumber + ")", player, eInventoryActionType.Loot, template);
				}
			}
		}

		#endregion

		#region Editing

		public bool AddPorch()
		{
			if (Porch) // we can't add an already added porch
				return false;

			// set porch to true, we now have one!
			Porch = true;

			// broadcast updates
			SendUpdate();
			SaveIntoDatabase();

			return true;
		}

		public bool RemovePorch()
		{
			if (!Porch) // we can't remove an already removed porch
				return false;

			// remove the consignment merchant
			RemoveConsignmentMerchant();

			// set porch to false, no mo porch!
			Porch = false;

			// broadcast updates
			SendUpdate();
			SaveIntoDatabase();

			return true;
		}

		public bool AddConsignment(long startValue)
		{
			// check to make sure a consignment merchant doesn't already exist for this house.
			if (ConsignmentMerchant != null)
			{
				log.DebugFormat("Add CM: House {0} already has a consignment merchant.", HouseNumber);
				return false;
			}

			var obj = DOLDB<Mob>.SelectObject(DB.Column(nameof(Mob.HouseNumber)).IsEqualTo(HouseNumber));
			if (obj != null)
			{
				log.DebugFormat("Add CM: Found consignment merchant in Mob table for house {0}.", HouseNumber);
				return false;
			}

			var houseCM = DOLDB<HouseConsignmentMerchant>.SelectObject(DB.Column(nameof(HouseConsignmentMerchant.HouseNumber)).IsEqualTo(HouseNumber));
			if (houseCM != null)
			{
				log.DebugFormat("Add CM: Found active consignment merchant in HousingConsignmentMerchant table for house {0}.", HouseNumber);
				return false;
			}

			if (DatabaseItem.HasConsignment == true)
			{
				log.ErrorFormat("Add CM: No Consignment Merchant found but House DB record HasConsignment for house {0}!  Creating a new merchant.", HouseNumber);
			}

			// now let's try to find a CM with this owner ID and no house and if we find it, attach
			houseCM = DOLDB<HouseConsignmentMerchant>.SelectObject(DB.Column(nameof(HouseConsignmentMerchant.OwnerID)).IsEqualTo(OwnerID));

			if (houseCM != null)
			{
				if (houseCM.HouseNumber > 0)
				{
					log.ErrorFormat("Add CM: Found active consignment merchant for this owner, can't add new one to house {0}!", HouseNumber);
					return false;
				}

				houseCM.HouseNumber = HouseNumber;
				GameServer.Database.SaveObject(houseCM);
				log.Warn("Re-adding an existing consignment merchant for house " + HouseNumber);
			}

			if (houseCM == null)
			{
				// create a new consignment merchant entry, and add it to the DB
				log.Warn("Adding a consignment merchant for house " + HouseNumber);
				houseCM = new HouseConsignmentMerchant { OwnerID = OwnerID, HouseNumber = HouseNumber, Money = startValue };
				GameServer.Database.AddObject(houseCM);
			}

			float[] consignmentCoords = HousingConstants.ConsignmentPositioning[Model];
			double multi = consignmentCoords[0];
			var range = (int) consignmentCoords[1];
			var zaddition = (int) consignmentCoords[2];
			var realm = (int) consignmentCoords[3];

            var merchantPosition = OutdoorJumpPosition + Vector.Create(Position.Orientation - Angle.Radians(multi), length: range, z: zaddition);

			GameConsignmentMerchant con = GameServer.ServerRules.CreateHousingConsignmentMerchant(this);

			con.Position = merchantPosition;
			con.Level = 50;
			con.Realm = (eRealm) realm;
			con.HouseNumber = (ushort)HouseNumber;
			con.Model = 144;

			con.Flags |= GameNPC.eFlags.PEACE;
			con.LoadedFromScript = false;
			con.RoamingRange = 0;

			if (DatabaseItem.GuildHouse)
				con.GuildName = DatabaseItem.GuildName;

			con.AddToWorld();
			con.SaveIntoDatabase();

			DatabaseItem.HasConsignment = true;
			SaveIntoDatabase();

			return true;
		}

		public void RemoveConsignmentMerchant()
		{
			if (ConsignmentMerchant != null)
			{
				log.Warn("HOUSING: Removing consignment merchant for house " + HouseNumber);

				// If this is a guild house and the house is removed the items still belong to the guild ID and will show up
				// again if guild purchases another house and CM

				int count = 0;
				foreach(InventoryItem item in ConsignmentMerchant.DBItems(null))
				{
					item.OwnerLot = 0;
					GameServer.Database.SaveObject(item);
					count++;
				}

				if (count > 0)
				{
					log.Warn("HOUSING: Cleared OwnerLot for " + count + " items on the consignment merchant!");
				}

				var houseCM = DOLDB<HouseConsignmentMerchant>.SelectObject(DB.Column(nameof(HouseConsignmentMerchant.HouseNumber)).IsEqualTo(HouseNumber));
				if (houseCM != null)
				{
					houseCM.HouseNumber = 0;
					GameServer.Database.SaveObject(houseCM);
				}

				ConsignmentMerchant.HouseNumber = 0;
				ConsignmentMerchant.DeleteFromDatabase();
				ConsignmentMerchant.Delete();

				ConsignmentMerchant = null;
				DatabaseItem.HasConsignment = false;

				SaveIntoDatabase();
			}
		}

		public void Edit(GamePlayer player, List<int> changes)
		{
			MerchantTradeItems items = player.InHouse ? HouseTemplateMgr.IndoorMenuItems : HouseTemplateMgr.OutdoorMenuItems;
			if (items == null)
				return;

			if (!player.InHouse)
			{
				if (!CanChangeExternalAppearance(player))
					return;
			}
			else
			{
				if (!CanChangeInterior(player, DecorationPermissions.Add))
					return;
			}

			long price = 0;

			// tally up the price for all the requested changes
			foreach (int slot in changes)
			{
				int page = slot/30;
				int pslot = slot%30;

				ItemTemplate item = items.GetItem(page, (eMerchantWindowSlot) pslot);
				if (item != null)
				{
					price += item.Price;
				}
			}

			// make sure player has enough money to cover the changes
			if (!player.RemoveMoney(Currency.Copper.Mint(price)))
			{
                InventoryLogging.LogInventoryAction(player, "(HOUSE;" + HouseNumber + ")", eInventoryActionType.Merchant, price);
				ChatUtil.SendMerchantMessage(player, "House.Edit.NotEnoughMoney", null);
				return;
			}

			ChatUtil.SendSystemMessage(player, "House.Edit.PayForChanges", Money.GetString(price));

			// make all the changes
			foreach (int slot in changes)
			{
				int page = slot/30;
				int pslot = slot%30;

				ItemTemplate item = items.GetItem(page, (eMerchantWindowSlot) pslot);

				if (item != null)
				{
					switch ((eObjectType) item.Object_Type)
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

			// save the house
			GameServer.Database.SaveObject(dbHouse);
			
			SendUpdate();
		}

		#endregion

		#region Permissions

		#region Add/Remove/Edit

		public bool AddPermission(GamePlayer player, PermissionType permType, int permLevel)
		{
			// make sure player is not null
			if (player == null)
				return false;

			// get the proper target name (acct name or player name)
			string targetName = permType == PermissionType.Account ? player.Client.Account.Name : player.Name;

			//  check to make sure an existing mapping doesn't exist.
			foreach (DBHouseCharsXPerms perm in _housePermissions.Values)
			{
				// fast expression to evaluate to match appropriate permissions
				if (perm.PermissionType == (int) permType)
				{
					// make sure it's not identical, which would mean we couldn't add!
					if (perm.TargetName == targetName)
						return false;
				}
			}

			// no matching permissions, create a new one and add it.
			var housePermission = new DBHouseCharsXPerms(HouseNumber, targetName, player.Name, permLevel, (int) permType);
			GameServer.Database.AddObject(housePermission);

			// add it to our list
			_housePermissions.Add(GetOpenPermissionSlot(), housePermission);

			return true;
		}

		public bool AddPermission(string targetName, PermissionType permType, int permLevel)
		{
			//  check to make sure an existing mapping doesn't exist.
			foreach (DBHouseCharsXPerms perm in _housePermissions.Values)
			{
				// fast expression to evaluate to match appropriate permissions
				if (perm.PermissionType == (int) permType)
				{
					// make sure it's not identical, which would mean we couldn't add!
					if (perm.TargetName == targetName)
						return false;
				}
			}

			// no matching permissions, create a new one and add it.
			var housePermission = new DBHouseCharsXPerms(HouseNumber, targetName, targetName, permLevel, (int) permType);
			GameServer.Database.AddObject(housePermission);

			// add it to our list
			_housePermissions.Add(GetOpenPermissionSlot(), housePermission);

			return true;
		}

		/// <summary>
		/// Grabs the first available house permission slot.
		/// </summary>
		/// <returns>returns an open house permission slot as an int</returns>
		private int GetOpenPermissionSlot()
		{
			int startingSlot = 0;

			while(_housePermissions.ContainsKey(startingSlot))
			{
				startingSlot++;
			}

			return startingSlot;
		}

		public void RemovePermission(int slot)
		{
			// make sure the permission exists
			if (!_housePermissions.ContainsKey(slot))
				return;

			var matchedPerm = _housePermissions[slot];

			// remove the permission and delete it from the database
			_housePermissions.Remove(slot);
			GameServer.Database.DeleteObject(matchedPerm);
		}

		public void AdjustPermissionSlot(int slot, int newPermLevel)
		{
			// make sure the permission exists
			if (!_housePermissions.ContainsKey(slot))
				return;

			var permission = _housePermissions[slot];

			// check for proper permission level range
			if (newPermLevel < HousingConstants.MinPermissionLevel || newPermLevel > HousingConstants.MaxPermissionLevel)
				return;

			// update the permission level
			permission.PermissionLevel = newPermLevel;

			// save the permission
			GameServer.Database.SaveObject(permission);
		}

		#endregion

		#region Get Permissions

		private DBHouseCharsXPerms GetPlayerPermissions(GamePlayer player)
		{
			// make sure player isn't null
			if (player == null)
				return null;

			// try character permissions first
			IEnumerable<DBHouseCharsXPerms> charPermissions = from cp in _housePermissions.Values
				where
				cp.TargetName == player.Name &&
				cp.PermissionType == (int) PermissionType.Player
				select cp;

			if (charPermissions.Count() > 0)
				return charPermissions.First();

			// try account permissions next
			IEnumerable<DBHouseCharsXPerms> acctPermissions = from cp in _housePermissions.Values
				where
				cp.TargetName == player.Client.Account.Name &&
				cp.PermissionType == (int) PermissionType.Account
				select cp;

			if (acctPermissions.Count() > 0)
				return acctPermissions.First();

			if (player.Guild != null)
			{
				// try guild permissions next
				IEnumerable<DBHouseCharsXPerms> guildPermissions = from cp in _housePermissions.Values
					where
					player.Guild.Name == cp.TargetName &&
					cp.PermissionType == (int) PermissionType.Guild
					select cp;

				if (guildPermissions.Count() > 0)
					return guildPermissions.First();
			}

			// look for the catch-all permissions last
			IEnumerable<DBHouseCharsXPerms> allPermissions = from cp in _housePermissions.Values
				where cp.TargetName == "All"
				select cp;

			if (allPermissions.Count() > 0)
				return allPermissions.First();

			// nothing found, return null
			return null;
		}

		private bool HasAccess(GamePlayer player, Func<DBHousePermissions, bool> accessExpression)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player))
				return true;

			// get house permissions for the given player
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			// get result of the permission check expression
			return accessExpression(housePermissions);
		}

		private DBHousePermissions GetPermissionLevel(GamePlayer player)
		{
			// get player permissions mapping
			DBHouseCharsXPerms permissions = GetPlayerPermissions(player);

			if (permissions == null)
				return null;

			// get house permissions for the given mapping
			DBHousePermissions housePermissions = GetPermissionLevel(permissions);

			return housePermissions;
		}

		private DBHousePermissions GetPermissionLevel(DBHouseCharsXPerms charPerms)
		{
			// make sure permissions aren't null
			if (charPerms == null)
				return null;

			return GetPermissionLevel(charPerms.PermissionLevel);
		}

		private DBHousePermissions GetPermissionLevel(int permissionLevel)
		{
			DBHousePermissions permissions;
			_permissionLevels.TryGetValue(permissionLevel, out permissions);

			return permissions;
		}

		public bool HasOwnerPermissions(GamePlayer player)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			if (player.Client.Account.PrivLevel == (int)ePrivLevel.Admin)
				return true;

			// check by character name/account if not guild house
			if (!dbHouse.GuildHouse)
			{
				// check if character is explicit owner
				if (dbHouse.OwnerID == player.ObjectId)
					return true;

				// check account-wide if not a guild house
				IEnumerable<DOLCharacters> charsOnAccount = from chr in player.Client.Account.Characters
					where chr.ObjectId == dbHouse.OwnerID
					select chr;

				if (charsOnAccount.Count() > 0)
					return true;
			}

			// check based on guild
			if (player.Guild != null)
			{
				return OwnerID == player.Guild.GuildID && player.Guild.HasRank(player, Guild.eRank.Leader);
			}

			// no character/account/guild match, not an owner
			return false;
		}

		#endregion

		#region Check Permissions

		public bool CanEmptyHookpoint(GamePlayer player)
		{
			return HasOwnerPermissions(player);
		}

		public bool CanEnterHome(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanEnterHouse);
		}

		public bool CanUseVault(GamePlayer player, GameHouseVault vault, VaultPermissions vaultPerms)
		{
			// make sure player isn't null
			if (player == null || player.CurrentHouse != this)
				return false;

			if (HasOwnerPermissions(player))
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			// get the vault permissions for the given vault
			VaultPermissions activeVaultPermissions = VaultPermissions.None;

			switch (vault.Index)
			{
				case 0:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault1;
					break;
				case 1:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault2;
					break;
				case 2:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault3;
					break;
				case 3:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault4;
					break;
			}

			ChatUtil.SendDebugMessage(player, string.Format("Vault permissions = {0} for vault index {1}", (activeVaultPermissions & vaultPerms), vault.Index));

			return (activeVaultPermissions & vaultPerms) > 0;
		}

		public bool CanUseConsignmentMerchant(GamePlayer player, ConsignmentPermissions consignPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and Admins can do everything
			if (HasOwnerPermissions(player))
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			return ((ConsignmentPermissions) housePermissions.ConsignmentMerchant & consignPerms) > 0;
		}

		public bool CanChangeInterior(GamePlayer player, DecorationPermissions interiorPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || player.Client.Account.PrivLevel > 1)
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			return ((DecorationPermissions) housePermissions.ChangeInterior & interiorPerms) > 0;
		}

		public bool CanChangeGarden(GamePlayer player, DecorationPermissions gardenPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || player.Client.Account.PrivLevel > 1)
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			return ((DecorationPermissions) housePermissions.ChangeGarden & gardenPerms) > 0;
		}

		public bool CanChangeExternalAppearance(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanChangeExternalAppearance);
		}

		public bool CanBanish(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanBanish);
		}

		public bool CanBindInHouse(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanBindInHouse);
		}

		public bool CanPayRent(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanPayRent);
		}

		public bool CanUseMerchants(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanUseMerchants);
		}

		public bool CanUseTools(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanUseTools);
		}

		#endregion

		#endregion

		#region Database

		/// <summary>
		/// Saves this house into the database
		/// </summary>
		public void SaveIntoDatabase()
		{
			GameServer.Database.SaveObject(dbHouse);
		}

		/// <summary>
		/// Load a house from the database
		/// </summary>
		public void LoadFromDatabase()
		{
			int i = 0;
			_indoorItems.Clear();
			foreach (DBHouseIndoorItem dbiitem in DOLDB<DBHouseIndoorItem>.SelectObjects(DB.Column(nameof(DBHouseIndoorItem.HouseNumber)).IsEqualTo(HouseNumber)))
			{
				_indoorItems.Add(i++, new IndoorItem(dbiitem));
			}

			i = 0;
			_outdoorItems.Clear();
			foreach (DBHouseOutdoorItem dboitem in DOLDB<DBHouseOutdoorItem>.SelectObjects(DB.Column(nameof(DBHouseOutdoorItem.HouseNumber)).IsEqualTo(HouseNumber)))
			{
				_outdoorItems.Add(i++, new OutdoorItem(dboitem));
			}

			_housePermissions.Clear();
			foreach (DBHouseCharsXPerms d in DOLDB<DBHouseCharsXPerms>.SelectObjects(DB.Column(nameof(DBHouseCharsXPerms.HouseNumber)).IsEqualTo(HouseNumber)))
			{
				_housePermissions.Add(GetOpenPermissionSlot(), d);
			}

			_permissionLevels.Clear();
			foreach (DBHousePermissions dbperm in DOLDB<DBHousePermissions>.SelectObjects(DB.Column(nameof(DBHousePermissions.HouseNumber)).IsEqualTo(HouseNumber)))
			{
				if (_permissionLevels.ContainsKey(dbperm.PermissionLevel) == false)
				{
					_permissionLevels.Add(dbperm.PermissionLevel, dbperm);
				}
				else
				{
					log.ErrorFormat("Duplicate permission level {0} for house {1}", dbperm.PermissionLevel, HouseNumber);
				}
			}

			HousepointItems.Clear();
			foreach (DBHouseHookpointItem item in DOLDB<DBHouseHookpointItem>.SelectObjects(DB.Column(nameof(DBHouseHookpointItem.HouseNumber)).IsEqualTo(HouseNumber)))
			{
				if (HousepointItems.ContainsKey(item.HookpointID) == false)
				{
					HousepointItems.Add(item.HookpointID, item);
					FillHookpoint(item.HookpointID, item.ItemTemplateID, item.Heading, item.Index);
				}
				else
				{
					log.ErrorFormat("Duplicate item {0} attached to hookpoint {1} for house {2}!", item.ItemTemplateID, item.HookpointID, HouseNumber);
				}
			}
		}

		#endregion
	}
}