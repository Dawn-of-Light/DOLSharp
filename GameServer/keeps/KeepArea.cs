using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.Keeps
{
	public class KeepArea : Area.Circle
	{
		public AbstractGameKeep Keep = null;
		private const int PK_RADIUS = 4000;
		private const int KEEP_RADIUS = 3000;
		private const int TOWER_RADIUS = 1500;

		public KeepArea()
			: base()
		{ }

		public KeepArea(AbstractGameKeep keep)
			: base(keep.Name, keep.X, keep.Y, 0, keep is GameKeep ? (keep.IsPortalKeep ? PK_RADIUS : KEEP_RADIUS) : TOWER_RADIUS)
		{
			Keep = keep;
		}

		public override void OnPlayerEnter(GamePlayer player)
		{
			//[Ganrod] Nidel: NPE
			if (player == null || Keep == null)
			{
				return;
			}
			base.OnPlayerEnter(player);
			if (Keep.Guild != null)
				player.Out.SendMessage("Controlled by " + Keep.Guild.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public void ChangeRadius(int newRadius)
		{
			KeepMgr.Logger.Debug("ChangeRadius called for " + Keep.Name + " currently is " + m_Radius + " changing to " + newRadius);

			//setting radius to default
			if (newRadius == 0 && m_Radius != 0)
			{
				if (m_dbArea != null)
					GameServer.Database.DeleteObject(m_dbArea);
				m_Radius = Keep is GameKeep ? (Keep.IsPortalKeep ? PK_RADIUS : KEEP_RADIUS) : TOWER_RADIUS;
				return;
			}

			//setting different radius when radius was already something
			if (newRadius > 0 && m_Radius >= 0)
			{
				m_Radius = newRadius;
				if (m_dbArea != null)
				{
					m_dbArea.Radius = m_Radius;
					GameServer.Database.SaveObject(m_dbArea);
				}
				else
				{
					m_dbArea = new DBArea();
					m_dbArea.CanBroadcast = this.CanBroadcast;
					m_dbArea.CheckLOS = this.CheckLOS;
					m_dbArea.ClassType = this.GetType().ToString();
					m_dbArea.Description = this.Description;
					m_dbArea.Radius = this.Radius;
					m_dbArea.Region = (ushort)this.Keep.Region;
					m_dbArea.Sound = this.Sound;
					m_dbArea.X = this.X;
					m_dbArea.Y = this.Y;
					m_dbArea.Z = this.Z;

					GameServer.Database.AddNewObject(m_dbArea);
				}
			}
		}

		public override void LoadFromDatabase(DBArea area)
		{
			base.LoadFromDatabase(area);
			KeepMgr.Logger.Debug("KeepArea " + area.Description + " LoadFromDatabase called");
			KeepMgr.Logger.Debug("X: " + area.X + "(" + m_X + ") Y: " + area.Y + "(" + m_Y + ") Region:" + area.Region + " Radius: " + m_Radius);
		}
	}
}
