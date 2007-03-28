using System;
using System.Collections;

using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
	public class CaptureMission : AbstractMission
	{
		private AbstractGameKeep m_keep = null;

		public enum eCaptureType : int
		{ 
			Tower = 1,
			Keep = 2,
		}

		public CaptureMission(eCaptureType type, object owner)
			: base(owner)
		{
			int realm = 0;
			if (owner is PlayerGroup)
				realm = (owner as PlayerGroup).Leader.Realm;
			else if (owner is GamePlayer)
				realm = (owner as GamePlayer).Realm;

			ArrayList list = new ArrayList();

			switch (type)
			{
				case eCaptureType.Tower:
					{
						IList keeps;
						if (owner is PlayerGroup)
							keeps = KeepMgr.GetKeepsOfRegion((owner as PlayerGroup).Leader.CurrentRegionID);
						else if (owner is GamePlayer)
							keeps = KeepMgr.GetKeepsOfRegion((owner as GamePlayer).CurrentRegionID);
						else keeps = new ArrayList();

						foreach (AbstractGameKeep keep in keeps)
						{
							if (keep.IsPortalKeep)
								continue;
							if (keep is GameKeepTower && keep.Realm != realm)
								list.Add(keep);
						}
						break;
					}
				case eCaptureType.Keep:
					{
						IList keeps;
						if (owner is PlayerGroup)
							keeps = KeepMgr.GetKeepsOfRegion((owner as PlayerGroup).Leader.CurrentRegionID);
						else if (owner is GamePlayer)
							keeps = KeepMgr.GetKeepsOfRegion((owner as GamePlayer).CurrentRegionID);
						else keeps = new ArrayList();

						foreach (AbstractGameKeep keep in keeps)
						{
							if (keep.IsPortalKeep)
								continue;
							if (keep is GameKeep && keep.Realm != realm && keep.Level < 5)
								list.Add(keep);
						}
						break;
					}
			}

			if (list.Count > 0)
				m_keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;

			GameEventMgr.AddHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e != KeepEvent.KeepTaken)
				return;

			KeepEventArgs kargs = args as KeepEventArgs;

			if (kargs.Keep != m_keep)
				return;

			GamePlayer testPlayer = null;
			if (m_owner is GamePlayer)
				testPlayer = m_owner as GamePlayer;
			else if (m_owner is PlayerGroup)
				testPlayer = (m_owner as PlayerGroup).Leader;

			if (testPlayer != null)
			{
				foreach (AbstractArea area in testPlayer.CurrentAreas)
				{
					if (area is KeepArea && (area as KeepArea).Keep == m_keep)
						FinishMission();
				}
			}

			ExpireMission();
		}

		public override void FinishMission()
		{
			base.FinishMission();
			GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
		}

		public override void ExpireMission()
		{
			base.ExpireMission();
			GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
		}

		public override string Description
		{
			get
			{
				if (m_keep == null)
					return "Keep is null when trying to send the description";
				else return "Capture " + m_keep.Name;
			}
		}
	}
}