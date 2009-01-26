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
using DOL.Database;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a static Item in the gameworld
	/// </summary>
	public class GameStaticItem : GameObject
	{
		/// <summary>
		/// The emblem of the Object
		/// </summary>
		protected int m_Emblem;

		/// <summary>
		/// Constructs a new GameStaticItem
		/// </summary>
		public GameStaticItem() : base()
		{
			m_owners = new ArrayList(1);
		}

		#region Name/Model/GetName/GetExamineMessages
		/// <summary>
		/// gets or sets the model of this Item
		/// </summary>
		public override ushort Model
		{
			get { return base.Model; }
			set
			{
				base.Model = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}

		/// <summary>
		/// Gets or Sets the current Emblem of the Object
		/// </summary>
		public virtual int Emblem
		{
			get { return m_Emblem; }
			set
			{
				m_Emblem = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of this item
		/// </summary>
		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the Guild of this item
		/// </summary>
		public override string Guild
		{
			get { return base.Guild; }
			set
			{
				base.Guild = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}

		/// <summary>
		/// Returns name with article for nouns
		/// </summary>
		/// <param name="article">0=definite, 1=indefinite</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>name of this object (includes article if needed)</returns>
		public override string GetName(int article, bool firstLetterUppercase)
		{
			if (Name == "")
				return "";
			if(char.IsUpper(Name[0]))
			{
				// proper name
				if (firstLetterUppercase)
					return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStaticItem.GetName.Article1", Name);
				else
					return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStaticItem.GetName.Article2", Name);
			}
			else
			{
				// common noun
				return base.GetName(article, firstLetterUppercase);
			}
		}

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Insert(0, "You select "+ GetName(0, false) +".");
			return list;
		}
		#endregion

		public override void LoadFromDatabase(DataObject obj)
		{
			WorldObject item = obj as WorldObject;
			base.LoadFromDatabase(obj);
			CurrentRegionID = item.Region;
			Name = item.Name;
			Model = item.Model;
			Emblem = item.Emblem;
			Heading = item.Heading;
			X = item.X;
			Y = item.Y;
			Z = item.Z;
			Guild = item.Guild;
		}

		/// <summary>
		/// Gets or sets the heading of this item
		/// </summary>
		public override ushort Heading
		{
			get { return base.Heading; }
			set
			{
				base.Heading = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}

		/// <summary>
		/// Gets or sets the level of this item
		/// </summary>
		public override byte Level
		{
			get { return base.Level; }
			set
			{
				base.Level = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}

		/// <summary>
		/// Gets or sets the realm of this item
		/// </summary>
		public override eRealm Realm
		{
			get { return base.Realm; }
			set
			{
				base.Realm = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendObjectCreate(this);
				}
			}
		}

		/// <summary>
		/// Saves this Item in the WorldObject DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			WorldObject obj = null;
			if(InternalID != null)
				obj = (WorldObject) GameServer.Database.FindObjectByKey(typeof(WorldObject), InternalID);
			if(obj == null)
			  obj = new WorldObject();
			obj.Name = Name;
			obj.InternalID = obj.ObjectId;
			obj.Model = Model;
			obj.Emblem = Emblem;
			obj.Heading = Heading;
			obj.Region = CurrentRegionID;
			obj.Zone = this.CurrentZone.ID;
			obj.X = X;
			obj.Y = Y;
			obj.Z = Z;
			obj.Guild = Guild;
			obj.ClassType = this.GetType().ToString();

			if(InternalID == null)
			{
				GameServer.Database.AddNewObject(obj);
				InternalID = obj.ObjectId;
			}
			else
				GameServer.Database.SaveObject(obj);
		}

		/// <summary>
		/// Deletes this item from the WorldObject DB
		/// </summary>
		public override void DeleteFromDatabase()
		{
			if(InternalID != null)
			{
				WorldObject obj = (WorldObject) GameServer.Database.FindObjectByKey(typeof(WorldObject), InternalID);
				if(obj != null)
				  GameServer.Database.DeleteObject(obj);
			}
			InternalID = null;
		}

		/// <summary>
		/// Called to create an item in the world
		/// </summary>
		/// <returns>true when created</returns>
		public override bool AddToWorld()
		{
			if(!base.AddToWorld()) return false;
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendObjectCreate(this);
				
			SaveIntoDatabase();
			return true;
		}

		/// <summary>
		/// Called to remove the item from the world
		/// </summary>
		/// <returns>true if removed</returns>
		public override bool RemoveFromWorld()
		{
			if (ObjectState == eObjectState.Active)
			{
				foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE)) 
					player.Out.SendObjectRemove(this);
			}
			return base.RemoveFromWorld();
		}

		/// <summary>
		/// Holds the owners of this item, can be more than 1 person
		/// </summary>
		private readonly ArrayList	  m_owners;
		/// <summary>
		/// Adds an owner to this item
		/// </summary>
		/// <param name="player">the object that is an owner</param>
		public void AddOwner(GameObject player)
		{
			lock(m_owners)
			{
				foreach(WeakReference weak in m_owners)
					if(weak.Target==player) return;
				m_owners.Add(new WeakRef(player));
			}
		}
		/// <summary>
		/// Tests if a specific gameobject owns this item
		/// </summary>
		/// <param name="testOwner">the owner to test for</param>
		/// <returns>true if this object owns this item</returns>
		public bool IsOwner(GameObject testOwner)
		{
			lock(m_owners)
			{
				//No owner ... return true
				if(m_owners.Count==0) return true;

				foreach(WeakReference weak in m_owners)
					if(weak.Target==testOwner) return true;
				return false;
			}
		}

		/// <summary>
		/// Returns an array of owners
		/// </summary>
		public GameObject[] Owners
		{
			get
			{
				ArrayList activeOwners = new ArrayList();
				foreach(WeakReference weak in m_owners)
					if(weak.Target!=null)
						activeOwners.Add(weak.Target);
				return (GameObject[])activeOwners.ToArray(typeof(GameObject));
			}
		}
	}
}
