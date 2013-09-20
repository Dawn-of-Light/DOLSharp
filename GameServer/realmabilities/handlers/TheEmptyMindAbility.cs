using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class TheEmptyMindAbility : TimedRealmAbility
	{
		public TheEmptyMindAbility(DBAbility dba, int level) : base(dba, level) { }

		public const Int32 m_duration = 45000; //45 seconds

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			foreach (GamePlayer t_player in living.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (t_player == living && living is GamePlayer)
				{
					(living as GamePlayer).Out.SendMessage("You clear your mind and become more resistant to magic damage!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
				else
				{
					t_player.Out.SendMessage(living.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
			
			int effectiveness = 10;
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: effectiveness = 10; break;
					case 2: effectiveness = 15; break;
					case 3: effectiveness = 20; break;
					case 4: effectiveness = 25; break;
					case 5: effectiveness = 30; break;
					default: effectiveness = 0; break;
				}				
			}
			else
			{
				switch (Level)
				{
					case 1: effectiveness = 10; break;
					case 2: effectiveness = 20; break;
					case 3: effectiveness = 30; break;
					default: effectiveness = 0; break;
				}
			}
			
			
			new TheEmptyMindEffect(effectiveness).Start(living);
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
