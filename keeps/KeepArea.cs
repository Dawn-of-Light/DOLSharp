using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	public class KeepArea : Area.Circle
	{
		public AbstractGameKeep Keep = null;

		public KeepArea(string desc, int x, int y, int z, int radius)
			: base(desc, x, y, z, radius)
		{
			m_Description = desc;
			m_X = x;
			m_Y = y;
			m_Z = z;
			m_Radius = radius;

			m_RadiusRadius = radius * radius;
		}

		public override void OnPlayerEnter(GamePlayer player)
		{
			base.OnPlayerEnter(player);
			if (Keep.Guild != null)
				player.Out.SendMessage("Controlled by " + Keep.Guild.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}
