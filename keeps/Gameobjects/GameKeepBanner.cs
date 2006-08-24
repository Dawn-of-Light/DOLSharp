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
		}

		private DBKeepPosition m_position;
		public DBKeepPosition Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		public override void LoadFromDatabase(DataObject obj)
		{
			
		}

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;

			PositionMgr.LoadKeepItemPosition(pos, this);
			component.Keep.Banners[m_templateID] = this;
			ChangeRealm();
			if (component.Keep.Guild != null)
				ChangeGuild();
			this.AddToWorld();
		}

		public void MoveToPosition(DBKeepPosition position)
		{
			PositionMgr.LoadKeepItemPosition(position, this);
			this.MoveTo(this.CurrentRegionID, this.X, this.Y, this.Z, this.Heading);
		}

		/// <summary>
		/// save the banner to DB
		/// </summary>
		public override void SaveIntoDatabase()
		{

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
			Guild guild = Component.Keep.Guild;
			this.Emblem = guild.theGuildDB.Emblem;
			ushort model = AlbionGuildModel;
			switch (this.Component.Keep.Realm)
			{
				case 0: model = AlbionGuildModel; break;
				case 1: model = AlbionGuildModel; break;
				case 2: model = MidgardGuildModel; break;
				case 3: model = HiberniaGuildModel; break;
			}
			this.Model = model;
			this.Name = GlobalConstants.RealmToName((eRealm)this.Component.Keep.Realm) + " Guild Banner";
		}

		/// <summary>
		/// get the emblem of of realm or of guild when keep is claimed
		/// </summary>
		/// <returns></returns>
		public int GetEmblem()
		{
			if (this.Component.Keep.Guild != null)
			{
				return this.Component.Keep.Guild.theGuildDB.Emblem;
			}
			switch (this.Realm)
			{
				case 0: return 0;
				case 1: return 464;
				case 2: return 465;
				case 3: return 466;
				default: return 0;
			}
		}
	}
}
