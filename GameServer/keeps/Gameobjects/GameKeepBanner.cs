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
using DOL.Database;
using DOL.GS.Geometry;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	public class GameKeepBanner : GameStaticItem , IKeepItem
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public enum eBannerType : int 
		{
			Realm = 0,
			Guild = 1,
		}

		public eBannerType BannerType;


		/// <summary>
		/// No Realm banner model (PvE)
		/// </summary>
		public const ushort NoRealmModel = 555;
		/// <summary>
		/// Albion unclaimed banner model
		/// </summary>
		public const ushort AlbionModel = 464;
		/// <summary>
		/// Midgard unclaimed banner model
		/// </summary>
		public const ushort MidgardModel = 465;
		/// <summary>
		/// Hibernia unclaimed banner model
		/// </summary>
		public const ushort HiberniaModel = 466;
		/// <summary>
		/// Albion claimed banner model
		/// </summary>
		public const ushort AlbionGuildModel = 679;
		/// <summary>
		/// Midgard claimed banner model
		/// </summary>
		public const ushort MidgardGuildModel = 681;
		/// <summary>
		/// Hibernia claimed banner model
		/// </summary>
		public const ushort HiberniaGuildModel = 680;

		protected string m_templateID = "";
		public string TemplateID
		{
			get { return m_templateID; }
		}

		protected GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
			set { m_component = value; }
		}

		protected DBKeepPosition m_position;
		public DBKeepPosition DbKeepPosition
		{
			get { return m_position; }
			set { m_position = value; }
		}

		public void DeleteObject()
		{
			if (Component != null)
			{
				if (Component.Keep != null)
				{
					Component.Keep.Banners.Remove(ObjectID.ToString());
				}

				Component.Delete();
			}

			Component = null;
			DbKeepPosition = null;

			base.Delete();
			CurrentRegion = null;
		}

		public override void LoadFromDatabase(DataObject obj)
		{
			if (obj == null) return;
			
			base.LoadFromDatabase(obj);
			string sKey = this.InternalID; // InternalID is set to obj.ObjectID by base.LoadFromDatabase()

			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea keepArea && keepArea.Keep is AbstractGameKeep keep)
				{
					Component = new GameKeepComponent();
					Component.Keep = keep;

					if (keep.Banners.ContainsKey(sKey) == false)
					{
						Component.Keep.Banners.Add(sKey, this);
						if (this.Model == AlbionGuildModel || this.Model == MidgardGuildModel || this.Model == HiberniaGuildModel)
							BannerType = eBannerType.Guild;
						else BannerType = eBannerType.Realm;
						if (BannerType == eBannerType.Guild && Component.Keep.Guild != null)
							ChangeGuild();
						else ChangeRealm();
						break;
					}
					else if (log.IsWarnEnabled)
						log.Warn($"LoadFromDatabase(): KeepID {keep.KeepID} already a banner using ObjectID {sKey}");
				}
			}// foreach
		}

		public override void DeleteFromDatabase()
		{
			string sKey = this.InternalID;
			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea)
				{
					Component.Keep.Banners.Remove(sKey);
					// break; This is a bad idea.  If there are multiple KeepAreas, we could end up with a banner on left on one of them that has been deleted from the DB
				}
			}
			base.DeleteFromDatabase();
		}

		public virtual void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			if (pos == null || component == null) return;
			
			m_templateID = pos.TemplateID;
			m_component = component;
			BannerType = (eBannerType)pos.TemplateType;

			PositionMgr.LoadKeepItemPosition(pos, this);
			string sKey = this.TemplateID;
			if (component.Keep.Banners.ContainsKey(sKey) == false)
			{
				component.Keep.Banners.Add(sKey, this);
				if (BannerType == eBannerType.Guild)
				{
					if (component.Keep.Guild != null)
					{
						ChangeGuild();
						Position += Vector.Create(z: 1500);
						this.AddToWorld();
					}
				}
				else
				{
					ChangeRealm();
					Position += Vector.Create(z: 1000);	// this works around an issue where all banners are at keep level instead of on top
							// with a z value > height of the keep the banners show correctly - tolakram
					this.AddToWorld();
				}
			}
			else if (log.IsWarnEnabled)
				log.Warn($"LoadFromPosition(): There is already a Banner with TemplateID {this.TemplateID} on KeepID {component.Keep.KeepID}, not adding Banner for KeepPosition_ID {pos.ObjectId} on KeepComponent_ID {component.InternalID}");
		}

        [Obsolete("This is going to be removed!")]
		public void MoveToPosition(DBKeepPosition position)
		{
			PositionMgr.LoadKeepItemPosition(position, this);
			int zAdd = 1000;
			if (BannerType == eBannerType.Guild) zAdd = 1500;

			this.MoveTo(Position + Vector.Create(z: zAdd));
		}

		public void ChangeRealm()
		{
			this.Realm = Component.Keep.Realm;

			switch (Realm)
			{
				case eRealm.None:
					{
						Model = NoRealmModel;
						break;
					}
				case eRealm.Albion:
					{
						Model = AlbionModel;
						break;
					}
				case eRealm.Midgard:
					{
						Model = MidgardModel;
						break;
					}
				case eRealm.Hibernia:
					{
						Model = HiberniaModel;
						break;
					}
			}
			Name = GlobalConstants.RealmToName(Component.Keep.Realm) + " Banner";
		}

		/// <summary>
		/// This function when keep is claimed to change guild for banner
		/// </summary>
		public void ChangeGuild()
		{
			if (BannerType != eBannerType.Guild)
				return;
			var guild = Component.Keep.Guild;

			int emblem = 0;
			if (guild != null)
			{
				emblem = guild.Emblem;
				this.AddToWorld();
			}
			else this.RemoveFromWorld();

			ushort model = AlbionGuildModel;
			switch (Component.Keep.Realm)
			{
				case eRealm.None: model = AlbionGuildModel; break;
				case eRealm.Albion: model = AlbionGuildModel; break;
				case eRealm.Midgard: model = MidgardGuildModel; break;
				case eRealm.Hibernia: model = HiberniaGuildModel; break;
			}
			this.Model = model;
			this.Emblem = emblem;
			this.Name = GlobalConstants.RealmToName(Component.Keep.Realm) + " Guild Banner";
		}

	}
}
