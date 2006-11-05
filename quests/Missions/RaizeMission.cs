using System;
using System.Collections;

using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
	public class RaizeMission : AbstractMission
	{
		private AbstractGameKeep m_keep = null;

		public RaizeMission(object owner)
			: base(owner)
		{
			int realm = 0;
			if (owner is PlayerGroup)
				realm = (owner as PlayerGroup).Leader.Realm;
			else if (owner is GamePlayer)
				realm = (owner as GamePlayer).Realm;

			ArrayList list = new ArrayList();

			foreach (AbstractGameKeep keep in KeepMgr.getNFKeeps())
			{
				if (keep is GameKeepTower && keep.Realm != realm)
					list.Add(keep);
			}

			if (list.Count > 0)
				m_keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;

			GameEventMgr.AddHandler(KeepEvent.TowerRaized, new DOLEventHandler(Notify));
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e != KeepEvent.TowerRaized)
				return;

			KeepEventArgs kargs = args as KeepEventArgs;

			if (kargs.Keep != m_keep)
				return;

			FinishMission();
		}

		public override void FinishMission()
		{
			base.FinishMission();
			GameEventMgr.RemoveHandler(KeepEvent.TowerRaized, new DOLEventHandler(Notify));
		}

		public override void ExpireMission()
		{
			base.ExpireMission();
			GameEventMgr.RemoveHandler(KeepEvent.TowerRaized, new DOLEventHandler(Notify));
		}

		public override string Description
		{
			get
			{
				if (m_keep == null)
					return "Keep is null when trying to send the description";
				else return "Raize " + m_keep.Name;
			}
		}
	}
}