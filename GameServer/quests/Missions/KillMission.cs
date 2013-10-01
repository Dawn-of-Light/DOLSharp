using System;

using DOL.Events;

namespace DOL.GS.Quests
{
	public class KillMission : AbstractMission
	{
		private Type m_targetType = null;
		private int m_total = 0;
		private int m_current = 0;
		private string m_desc = "";

		public KillMission(Type targetType, int total, string desc, object owner)
			: base(owner)
		{
			m_targetType = targetType;
			m_total = total;
			m_desc = desc;
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e != GameLivingEvent.EnemyKilled)
				return;

			EnemyKilledEventArgs eargs = args as EnemyKilledEventArgs;

			//we don't want mission masters to be considered realm guards because they are insta respawn
			//in addition do not count realm 0 guards
			if (eargs.Target is Keeps.MissionMaster || eargs.Target.Realm == eRealm.None)
				return;

			if (m_targetType.IsInstanceOfType(eargs.Target) == false)
				return;

			//we dont allow events triggered by non group leaders
			if (MissionType == eMissionType.Group && sender is GamePlayer)
			{
				GamePlayer player = sender as GamePlayer;

				if (player.Group == null)
					return;

				if (player.Group.Leader != player)
					return;
			}

			//we don't want group events to trigger personal mission updates
			if (MissionType == eMissionType.Personal && sender is GamePlayer)
			{
				GamePlayer player = sender as GamePlayer;

				if (player.Group != null)
					return;
			}

			m_current++;
			UpdateMission();
			if (m_current == m_total)
				FinishMission();

		}

		public override string Description
		{
			get
			{
				return "Kill " + m_total + " " + m_desc + ", you have killed " + m_current + ".";
			}
		}

		public override long RewardRealmPoints
		{
			get
			{
				if (m_targetType == typeof(Keeps.GameKeepGuard))
					return 500;
				else if (m_targetType == typeof(GamePlayer))
					return 750;
				return 0;
			}
		}
	}
}