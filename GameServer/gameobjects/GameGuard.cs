using DOL.AI.Brain;
using DOL.Language;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    public class GameGuard : GameNPC
    {
        public GameGuard()
            : base()
        {
            m_ownBrain = new GuardBrain();
            m_ownBrain.Body = this;
        }

        public override void DropLoot(GameObject killer)
        {
            //Guards dont drop loot when they die
        }

        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = new ArrayList(4);
            list.Add(LanguageMgr.GetTranslation(player.Client, "GameGuard.GetExamineMessages.Examine", GetName(0, true), GetPronoun(0, true), GetAggroLevelString(player, false)));
            return list;
        }

        public override void StartAttack(GameObject attackTarget)
        {
            base.StartAttack(attackTarget);

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
            {
                if (player != null)
                    switch (Realm)
                    {
                        case eRealm.Albion:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameGuard.Albion.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                        case eRealm.Midgard:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameGuard.Midgard.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                        case eRealm.Hibernia:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameGuard.Hibernia.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                    }
            }
        }
    }
}