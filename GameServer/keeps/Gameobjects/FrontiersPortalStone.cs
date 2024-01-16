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
using DOL.GS.Geometry;
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
		public DBKeepPosition DbKeepPosition
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
				if (CurrentRegion.ID == 163)
					return CurrentZone.Realm;
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

			//For players in frontiers only
			if (GameServer.KeepManager.FrontierRegionsList.Contains(player.CurrentRegionID))
			{
				if (player.Client.Account.PrivLevel == (int)ePrivLevel.Player)
				{
					if (player.Realm != this.Realm)
						return false;

					if (Component != null && Component.Keep is GameKeep)
					{
						if ((Component.Keep as GameKeep).OwnsAllTowers == false || (Component.Keep as GameKeep).InCombat)
							return false;
					}

					if (GameRelic.IsPlayerCarryingRelic(player))
						return false;
				}

				// open up the warmap window

				eDialogCode code = eDialogCode.WarmapWindowAlbion;
				switch (player.Realm)
				{
					case eRealm.Albion: code = eDialogCode.WarmapWindowAlbion; break;
					case eRealm.Midgard: code = eDialogCode.WarmapWindowMidgard; break;
					case eRealm.Hibernia: code = eDialogCode.WarmapWindowHibernia; break;
				}

				player.Out.SendDialogBox(code, 0, 0, 0, 0, eDialogType.Warmap, false, "");
			}

			//if no component assigned, teleport to the border keep
			if (Component == null && GameServer.KeepManager.FrontierRegionsList.Contains(player.CurrentRegionID) == false)
			{
				GameServer.KeepManager.ExitBattleground(player);
			}

			return true;
		}

        [Obsolete("This is going to be removed.")]
        public void GetTeleportLocation(out int x, out int y)
        {
            var angle = Orientation + Angle.Heading(Util.Random(- 500, 500));
            var portPosition = Position + Vector.Create(angle, length: Util.Random(50, 150));
            x = portPosition.X;
            y = portPosition.Y;
        }

		public class TeleporterEffect : GameNPC
		{
			public TeleporterEffect()
				: base()
			{
				m_name = "teleport spell effect";
				m_flags = eFlags.PEACE | eFlags.DONTSHOWNAME;
				m_size = 255;
				m_model = 0x783;
				m_maxSpeedBase = 0;
			}
		}

		#region Teleporter Effect

		protected TeleporterEffect sfx;

		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;
			TeleporterEffect mob = new TeleporterEffect();
			mob.Position = Position;
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
