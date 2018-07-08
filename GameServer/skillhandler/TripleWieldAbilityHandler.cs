using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using log4net;
using DOL.Language;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Triple Wield clicks
    /// </summary>
    [SkillHandler(Abilities.Triple_Wield)]
    public class TripleWieldAbilityHandler : IAbilityActionHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The ability reuse time in seconds
        /// </summary>
        private const int ReuseTimer = 7 * 60; // 7 minutes

        /// <summary>
        /// The ability effect duration in seconds
        /// </summary>
        private const int Duration = 30;

        public void Execute(Ability ab, GamePlayer player)
        {
            if (player == null)
            {
                if (Log.IsWarnEnabled)
                {
                    Log.Warn("Could not retrieve player in TripleWieldAbilityHandler.");
                }

                return;
            }

            if (!player.IsAlive)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsMezzed)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsStunned)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsSitting)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            TripleWieldEffect tw = player.EffectList.GetOfType<TripleWieldEffect>();
            if (tw != null)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseAlreadyActive"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            TripleWieldEffect twe = new TripleWieldEffect(Duration * 1000);
            twe.Start(player);
            player.DisableSkill(ab, ReuseTimer * 1000);
        }
    }
}
