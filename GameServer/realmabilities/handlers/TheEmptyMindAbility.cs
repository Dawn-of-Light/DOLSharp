using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Effects;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class TheEmptyMindAbility : TimedRealmAbility
	{
        public TheEmptyMindAbility(DBAbility dba, int level) : base(dba, level) { }

		public const string KEY = "The Empty Mind";
		public const Int32 m_duration = 45000; //45 seconds

        public void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;

            foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (t_player == player)
                {
                    player.Out.SendMessage("You clear your mind and become more resistant to magic damage!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    t_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
            }
            if (player != null)
            {
                new TheEmptyMindEffect().Start(player, this);
            }
            DisableSkill(living);
        }
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
