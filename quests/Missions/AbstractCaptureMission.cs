using System;
using System.Collections;

using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
	public class CaptureMission : AbstractMission
	{
		private AbstractGameKeep m_keep = null;

		public CaptureMission(Type m_targetType, object owner)
			: base(owner)
		{
			int realm = 0;
			if (owner is PlayerGroup)
				realm = (owner as PlayerGroup).Leader.Realm;

			ArrayList list = new ArrayList();

			if (m_targetType is GameKeepTower)
			{
				foreach (AbstractGameKeep keep in KeepMgr.getNFKeeps())
				{
					if (keep is GameKeepTower && keep.Realm != realm)
						list.Add(keep);
				}
			}
			else if (m_targetType is GameKeep)
			{
				foreach (AbstractGameKeep keep in KeepMgr.getNFKeeps())
				{
					if (keep is GameKeep && keep.Realm != realm)
						list.Add(keep);
				}
			}

			if (list.Count > 0)
				m_keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e != KeepEvent.KeepTaken)
				return;

			KeepEventArgs kargs = args as KeepEventArgs;

			if (kargs.Keep != m_keep)
				return;

			//we dont allow events triggered by non group leaders
			if (MissionType == eMissionType.Group && sender is GamePlayer)
			{
				GamePlayer player = sender as GamePlayer;

				if (player.PlayerGroup != m_owner)
					return;

				if (player.PlayerGroup.Leader != player)
					return;
			}

			FinishMission();

		}

		public override string Description
		{
			get
			{
				return "Capture " + m_keep.Name;
			}
		}
	}
}