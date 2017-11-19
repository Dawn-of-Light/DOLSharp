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
			: base(keep.Name, keep.X, keep.Y, 0, keep.IsPortalKeep ? PK_RADIUS : (keep is GameKeepTower ? TOWER_RADIUS : KEEP_RADIUS)
)		{
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
			GameServer.KeepManager.Log.Debug("ChangeRadius called for " + Keep.Name + " currently is " + Radius + " changing to " + newRadius);

			//setting radius to default
			if (newRadius == 0 && Radius != 0)
			{
				if (m_dbArea != null)
					GameServer.Database.DeleteObject(m_dbArea);
				Radius = Keep is GameKeep ? (Keep.IsPortalKeep ? PK_RADIUS : KEEP_RADIUS) : TOWER_RADIUS;
				return;
			}

			//setting different radius when radius was already something
			if (newRadius > 0 && Radius >= 0)
			{
				Radius = newRadius;
				if (m_dbArea != null)
				{
					m_dbArea.Radius = Radius;
					GameServer.Database.SaveObject(m_dbArea);
				}
				else
				{
					m_dbArea = new DBArea();
					m_dbArea.CanBroadcast = CanBroadcast;
					m_dbArea.CheckLOS = CheckLOS;
					m_dbArea.ClassType = GetType().ToString();
					m_dbArea.Description = Description;
					m_dbArea.Radius = Radius;
					m_dbArea.Region = (ushort)Keep.Region;
					m_dbArea.Sound = Sound;
					m_dbArea.X = X;
					m_dbArea.Y = Y;
					m_dbArea.Z = Z;

					GameServer.Database.AddObject(m_dbArea);
				}
			}
		}

		public override void LoadFromDatabase(DBArea area)
		{
			base.LoadFromDatabase(area);
			GameServer.KeepManager.Log.Debug("KeepArea " + area.Description + " LoadFromDatabase called");
			GameServer.KeepManager.Log.Debug("X: " + area.X + "(" + X + ") Y: " + area.Y + "(" + Y + ") Region:" + area.Region + " Radius: " + Radius);
		}
	}
}
