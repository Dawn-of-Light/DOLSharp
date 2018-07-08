using System.Collections;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.RealmAbilities
{
    public class DashingDefenseAbility : TimedRealmAbility
    {
        public DashingDefenseAbility(DBAbility dba, int level) : base(dba, level) { }

        public const string Dashing = "Dashing";

        // private RegionTimer m_expireTimer;
        private int _duration = 1;
        private int _range = 1000;

        // private GamePlayer m_player;
        public override void Execute(GameLiving living)
        {
            if (!(living is GamePlayer player))
            {
                return;
            }

            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (player.TempProperties.getProperty(Dashing, false))
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "DashingDefenseAbility.Execute.AlreadyEffect"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _duration = 10; break;
                    case 2: _duration = 20; break;
                    case 3: _duration = 30; break;
                    case 4: _duration = 45; break;
                    case 5: _duration = 60; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _duration = 10; break;
                    case 2: _duration = 30; break;
                    case 3: _duration = 60; break;
                    default: return;
                }
            }

            DisableSkill(living);

            ArrayList targets = new ArrayList();
            if (player.Group == null)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "DashingDefenseAbility.Execute.MustInGroup"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            foreach (GamePlayer grpMate in player.Group.GetPlayersInTheGroup())
            {
                if (player.IsWithinRadius(grpMate, _range) && grpMate.IsAlive)
                {
                    targets.Add(grpMate);
                }
            }

            foreach (GamePlayer target in targets)
            {
                // send spelleffect
                if (!target.IsAlive)
                {
                    continue;
                }

                var success = !target.TempProperties.getProperty(Dashing, false);
                if (success)
                {
                    if (target != player)
                    {
                        new DashingDefenseEffect().Start(player, target, _duration);
                    }
                }
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 420;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            // TODO Translate
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info1"));
                list.Add(string.Empty);
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info2"));
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info3"));
            }
            else
            {
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info1"));
                list.Add(string.Empty);
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info2"));
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DashingDefenseAbility.AddEffectsInfo.Info3"));
            }
        }
    }
}
