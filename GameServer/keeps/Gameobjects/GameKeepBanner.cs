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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// GameKeep is the keep in game in RVR
	/// </summary>
	public class GameKeepBanner : GameStaticItem
	{
		/// <summary>
		/// Constructor of GameKeepBanner
		/// </summary>
		/// <param name="keep"></param>
		public GameKeepBanner(AbstractGameKeep keep) : base()
		{
			this.Keep = keep;
		}

		/// <summary>
		/// Constructor of GameKeepBanner without keep linked
		/// be careful with this. Try to add always keep.
		/// </summary>
		public GameKeepBanner() : base()
		{
			
		}

		#region Properties

		/// <summary>
		/// This hold the keep owner
		/// </summary>
		protected AbstractGameKeep m_keep;
		/// <summary>
		/// The keep owner
		/// </summary>
		public AbstractGameKeep Keep 
		{ 
			get{return m_keep;}
			set{m_keep=value;} 
		}
		#endregion

		/// <summary>
		/// load GameKeepBanner from db object
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(object obj)
		{
			//todo make command to make banner
			//keep must been add before loadfromDB
			DBKeepObject dbkeepobj = obj as DBKeepObject;
			if (dbkeepobj == null)return;
			InternalID = dbkeepobj.KeepObjectID.ToString();
			this.Name = dbkeepobj.Name;
			this.Realm = (byte)dbkeepobj.Realm;
			this.Model = (ushort)dbkeepobj.Model;
			this.Emblem = (ushort)GetEmblem();
			this.Position = new Point(dbkeepobj.X, dbkeepobj.Y, dbkeepobj.Z);
			this.Heading = (ushort)dbkeepobj.Heading;
			this.Region = this.Keep.Region;
			this.Level = (byte)(dbkeepobj.BaseLevel);
			this.Keep.Banners.Add(this);
			this.AddToWorld();
		}

		/// <summary>
		/// save the banner to DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			DBKeepObject obj = null;
			if(InternalID != null)
				obj = (DBKeepObject) GameServer.Database.FindObjectByKey(typeof(DBKeepObject), InternalID);
			if(obj == null)
				obj = new DBKeepObject();
			obj.Name = Name;
			obj.Model = Model;
			obj.BaseLevel = Level;
			obj.ClassType = this.GetType().ToString();
			obj.EquipmentID = "";
			obj.Heading = Heading;
			obj.KeepID = Keep.KeepID;
			obj.Realm = Realm;
			Point pos = Position;
			obj.X = pos.X;
			obj.Y = pos.Y;
			obj.Z = pos.Z;
			obj.ClassType = this.GetType().ToString();

			/*if(InternalID == null)
			{
				GameServer.Database.AddNewObject(obj);
				InternalID = obj.ObjectId;
			}
			else
			{
				obj.ObjectId = this.InternalID;
				GameServer.Database.SaveObject(obj);
			}*/
		}

		/// <summary>
		/// This function when keep is claimed to change guild for banner
		/// </summary>
		/// <param name="guild"></param>
		public void ChangeGuild(Guild guild)
		{
			this.Emblem = (ushort)guild.Emblem;
			ushort model = 679 ;
			switch (Keep.Realm)
			{
				case 0: model=679; break;
				case 1: model=679; break;
				case 2: model=680; break;
				case 3: model=681; break;
			}
			this.Model = model;
			this.Name = GlobalConstants.RealmToName((eRealm)Keep.Realm) + " Guild Banner";
		}

		/// <summary>
		/// get the emblem of of realm or of guild when keep is claimed
		/// </summary>
		/// <returns></returns>
		public int GetEmblem()
		{
			if (this.Keep.Guild != null)
			{
				return this.Keep.Guild.Emblem;
			}
			switch (this.Realm)
			{
				case 0: return 0; 
				case 1: return 464; 
				case 2: return 465; 
				case 3: return 466;
				default : return 0;
			}
		}
	}
}
