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
using DOL.GS.PacketHandler;

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

		private string m_templateID = "";
		public string TemplateID
		{
			get { return m_templateID; }
		}

		private GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
			set { m_component = value; }
		}

		private DBKeepPosition m_position;
		public DBKeepPosition Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea)
				{
					AbstractGameKeep keep = (area as KeepArea).Keep;
					Component = new GameKeepComponent();
					Component.Keep = keep;
					Component.Keep.Banners.Add(obj.ObjectId, this);
					if (BannerType == eBannerType.Guild && Component.Keep.Guild != null)
						ChangeGuild();
					else ChangeRealm();
					break;
				}
			}
		}

		public override void DeleteFromDatabase()
		{
			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea)
				{
					Component.Keep.Banners.Remove(this.InternalID);
					break;
				}
			}
			base.DeleteFromDatabase();
		}

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;
			BannerType = (eBannerType)pos.TemplateType;

			PositionMgr.LoadKeepItemPosition(pos, this);
			component.Keep.Banners[m_templateID] = this;
			if (BannerType == eBannerType.Guild)
			{
				if (component.Keep.Guild != null)
				{
					ChangeGuild();
					this.AddToWorld();
				}
			}
			else
			{
				ChangeRealm();
				this.AddToWorld();
			}
		}

		public void MoveToPosition(DBKeepPosition position)
		{
			PositionMgr.LoadKeepItemPosition(position, this);
			this.MoveTo(this.CurrentRegionID, this.X, this.Y, this.Z, this.Heading);
		}

		public void ChangeRealm()
		{
			this.Realm = this.Component.Keep.Realm;

			switch ((eRealm)this.Realm)
			{
				case eRealm.None:
				case eRealm.Albion:
					{
						this.Model = AlbionModel;
						break;
					}
				case eRealm.Midgard:
					{
						this.Model = MidgardModel;
						break;
					}
				case eRealm.Hibernia:
					{
						this.Model = HiberniaModel;
						break;
					}
			}
			this.Name = GlobalConstants.RealmToName((eRealm)this.Component.Keep.Realm) + " Banner";
		}

		/// <summary>
		/// This function when keep is claimed to change guild for banner
		/// </summary>
		public void ChangeGuild()
		{
			if (BannerType != eBannerType.Guild)
				return;
			Guild guild = this.Component.Keep.Guild;

			int emblem = 0;
			if (guild != null)
			{
				emblem = guild.theGuildDB.Emblem;
				this.AddToWorld();
			}
			else this.RemoveFromWorld();

			ushort model = AlbionGuildModel;
			switch (this.Component.Keep.Realm)
			{
				case 0: model = AlbionGuildModel; break;
				case 1: model = AlbionGuildModel; break;
				case 2: model = MidgardGuildModel; break;
				case 3: model = HiberniaGuildModel; break;
			}
			this.Model = model;
			this.Emblem = emblem;
			this.Name = GlobalConstants.RealmToName((eRealm)this.Component.Keep.Realm) + " Guild Banner";
		}
	}
}
