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

using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	public class FrontiersPortalStone : GameStaticItem, IKeepItem
	{
		private string m_templateID;
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

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_component = component;
			PositionMgr.LoadKeepItemPosition(pos, this);
			this.m_component.Keep.TeleportStone = this;
			this.AddToWorld();
		}

		public void MoveToPosition(DBKeepPosition position)
		{ }

		public override byte Realm
		{
			get
			{
				if (m_component != null)
					return m_component.Keep.Realm;
				return base.Realm;
			}
		}

		public override ushort Model
		{
			get { return 2603; }
		}

		public override string Name
		{
			get { return "Frontiers Portal Stone"; }
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (player.Realm != this.Realm && player.Client.Account.PrivLevel == 1)
				return false;

			player.Out.SendDialogBox(eDialogCode.WarmapWindow, 0, 0, 0, 0, eDialogType.YesNo, false, "");

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

		#region Teleporter Effect

		protected GameMob sfx;

		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;
			GameMob mob = new GameMob();
			mob.Name = "teleport spell effect";
			mob.Flags = (uint)GameNPC.eFlags.PEACE + (uint)GameNPC.eFlags.DONTSHOWNAME;
			mob.Size = 255;
			mob.CurrentRegion = this.CurrentRegion;
			mob.X = this.X;
			mob.Y = this.Y;
			mob.Z = this.Z;
			mob.Model = 0x783;
			mob.Heading = this.Heading;
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
