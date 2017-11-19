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
using DOL.Database;

namespace DOL.GS.Keeps
{
    public class GameKeepBanner : GameStaticItem , IKeepItem
	{

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
		public DBKeepPosition Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		public void DeleteObject()
		{
			if (Component != null)
			{
				if (Component.AbstractKeep != null)
				{
					Component.AbstractKeep.Banners.Remove(ObjectID);
				}

				Component.Delete();
			}

			Component = null;
			Position = null;

			base.Delete();
			CurrentRegion = null;
		}

		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			foreach (AbstractArea area in CurrentAreas)
			{
				if (area is KeepArea)
				{
					AbstractGameKeep keep = (area as KeepArea).Keep;
					Component = new GameKeepComponent();
					Component.AbstractKeep = keep;
					Component.AbstractKeep.Banners.Add(obj.ObjectId, this);
					if (Model == AlbionGuildModel || Model == MidgardGuildModel || Model == HiberniaGuildModel)
						BannerType = eBannerType.Guild;
					else BannerType = eBannerType.Realm;
					if (BannerType == eBannerType.Guild && Component.AbstractKeep.Guild != null)
						ChangeGuild();
					else ChangeRealm();
					break;
				}
			}
		}

		public override void DeleteFromDatabase()
		{
			foreach (AbstractArea area in CurrentAreas)
			{
				if (area is KeepArea)
				{
					Component.AbstractKeep.Banners.Remove(InternalID);
					break;
				}
			}
			base.DeleteFromDatabase();
		}

		public virtual void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;
			BannerType = (eBannerType)pos.TemplateType;

			PositionMgr.LoadKeepItemPosition(pos, this);
			component.AbstractKeep.Banners[m_templateID] = this;
			if (BannerType == eBannerType.Guild)
			{
				if (component.AbstractKeep.Guild != null)
				{
					ChangeGuild();
					Z += 1500;
                    AddToWorld();
				}
			}
			else
			{
				ChangeRealm();
				Z += 1000;  // this works around an issue where all banners are at keep level instead of on top
                            // with a z value > height of the keep the banners show correctly - tolakram
                AddToWorld();
			}
		}

		public void MoveToPosition(DBKeepPosition position)
		{
			PositionMgr.LoadKeepItemPosition(position, this);
			int zAdd = 1000;
			if (BannerType == eBannerType.Guild)
				zAdd = 1500;

            MoveTo(CurrentRegionID, X, Y, Z + zAdd, Heading);
		}

		public void ChangeRealm()
		{
            Realm = Component.AbstractKeep.Realm;

			switch ((eRealm)Realm)
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
            Name = GlobalConstants.RealmToName((eRealm)Component.AbstractKeep.Realm) + " Banner";
		}

		/// <summary>
		/// This function when keep is claimed to change guild for banner
		/// </summary>
		public void ChangeGuild()
		{
			if (BannerType != eBannerType.Guild)
				return;
			Guild guild = Component.AbstractKeep.Guild;

			int emblem = 0;
			if (guild != null)
			{
				emblem = guild.Emblem;
                AddToWorld();
			}
			else RemoveFromWorld();

			ushort model = AlbionGuildModel;
			switch (Component.AbstractKeep.Realm)
			{
				case eRealm.None: model = AlbionGuildModel; break;
				case eRealm.Albion: model = AlbionGuildModel; break;
				case eRealm.Midgard: model = MidgardGuildModel; break;
				case eRealm.Hibernia: model = HiberniaGuildModel; break;
			}
            Model = model;
            Emblem = emblem;
            Name = GlobalConstants.RealmToName(Component.AbstractKeep.Realm) + " Guild Banner";
		}

	}
}
