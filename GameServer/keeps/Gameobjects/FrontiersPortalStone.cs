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

using DOL.Database2;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	public class FrontiersPortalStone : GameStaticItem, IKeepItem
	{
		private string m_templateID = string.Empty;
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

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			if (component.Keep.DBKeep.BaseLevel < 50)
				return;
			m_component = component;
			PositionMgr.LoadKeepItemPosition(pos, this);
			this.m_component.Keep.TeleportStone = this;
			this.AddToWorld();
		}

		public void MoveToPosition(DBKeepPosition position)
		{ }

		public override eRealm Realm
		{
			get
			{
				if (m_component != null)
					return m_component.Keep.Realm;
				if (m_CurrentRegion.ID == 163)
					return CurrentZone.GetRealm();
				return base.Realm;
			}
		}

		public override string Name
		{
			get { return "Frontiers Portal Stone"; }
		}

		public override ushort Model
		{
			get { return 2603; }
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (player.Client.Account.PrivLevel == 1)
			{
				if (player.Realm != this.Realm)
					return false;
				if (Component != null && Component.Keep is GameKeep)
				{
					if ((Component.Keep as GameKeep).OwnsAllTowers == false)
						return false;
				}
			}

			//if no component assigned, teleport to the border keep
			if (Component == null)
			{
				string location = "";
				switch (player.Realm)
				{
					case eRealm.Albion: location = "Castle Sauvage"; break;
					case eRealm.Midgard: location = "Svasudheim Faste"; break;
					case eRealm.Hibernia: location = "Druim Ligen"; break;
				}

				if (location != "")
				{
					Teleport t = (Teleport)GameServer.Database.SelectObject(typeof(Teleport), "`TeleportID` = '" + location + "'");
					if (t != null)
					{
						player.MoveTo((ushort)t.RegionID, t.X, t.Y, t.Z, (ushort)t.Heading);
						return true;
					}
				}
			}

			eDialogCode code = eDialogCode.SimpleWarning;
			switch (player.Realm)
			{
				case eRealm.Albion: code = eDialogCode.WarmapWindowAlbion; break;
				case eRealm.Midgard: code = eDialogCode.WarmapWindowMidgard; break;
				case eRealm.Hibernia: code = eDialogCode.WarmapWindowHibernia; break;
			}

			player.Out.SendDialogBox(code, 0, 0, 0, 0, eDialogType.YesNo, false, "");

			return true;
		}

		public void GetTeleportLocation(out int x, out int y)
		{
			ushort originalHeading = m_Heading;
			m_Heading = (ushort)Util.Random((m_Heading - 500), (m_Heading + 500));
			int distance = Util.Random(50, 150);
			GetSpotFromHeading(distance, out x, out y);
			m_Heading = originalHeading;
		}

		public class TeleporterEffect : GameNPC
		{
			public TeleporterEffect()
				: base()
			{
				m_Name = "teleport spell effect";
				m_flags = (uint)GameNPC.eFlags.PEACE + (uint)GameNPC.eFlags.DONTSHOWNAME;
				m_size = 255;
				m_Model = 0x783;
				m_maxSpeedBase = 0;
			}
		}

		#region Teleporter Effect

		protected TeleporterEffect sfx;

		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;
			TeleporterEffect mob = new TeleporterEffect();
			mob.CurrentRegion = this.CurrentRegion;
			mob.X = this.X;
			mob.Y = this.Y;
			mob.Z = this.Z;
			mob.Heading = this.Heading;
			mob.Health = mob.MaxHealth;
			mob.MaxSpeedBase = 0;
			if (mob.AddToWorld())
				sfx = mob;
			return true;
		}

		public override bool RemoveFromWorld()
		{
			if (!base.RemoveFromWorld()) return false;
			if (sfx != null)
				sfx.Delete();
			return true;
		}
		#endregion
	}
}
