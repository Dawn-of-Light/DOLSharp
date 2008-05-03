using System.Reflection;
using System.Collections;
using DOL.Database2;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    public class DashingDefenseAbility : TimedRealmAbility
    {
        public DashingDefenseAbility(DBAbility dba, int level) : base(dba, level) { }

        public const string Dashing = "Dashing";

        //private RegionTimer m_expireTimer;
        int m_duration = 1;
        int m_range = 1000;
        //private GamePlayer m_player;

        public override void Execute(GameLiving living)
        {
            GamePlayer player = living as GamePlayer;
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            if (player.TempProperties.getProperty(Dashing, false))
            {
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "DashingDefenseAbility.Execute.AlreadyEffect"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
            }

            switch (Level)
            {
                case 1: m_duration = 10; break;
                case 2: m_duration = 30; break;
                case 3: m_duration = 60; break;
                default: return;
            }

            DisableSkill(living);

            ArrayList targets = new ArrayList();
            if (player.Group == null)
                {
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "DashingDefenseAbility.Execute.MustInGroup"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					return;
                }
            else foreach (GamePlayer grpMate in player.Group.GetPlayersInTheGroup())
                    if (WorldMgr.CheckDistance(grpMate, player, m_range) && grpMate.IsAlive)
                        targets.Add(grpMate);

            bool success;
            foreach (GamePlayer target in targets)
            {
                //send spelleffect
                if (!target.IsAlive) continue;
                success = !target.TempProperties.getProperty(Dashing, false);
                if (success)
                    if (target != null && target != player)
                    {
                        new DashingDefenseEffect().Start(player, target, m_duration);
                    }
            }

        }

        public override int GetReUseDelay(int level)
        {
            return 420;
        }

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info1"));
			list.Add("");
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info2"));
			list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info3"));
		}
    }
}
