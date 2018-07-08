using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.SkillHandler
{
    public class SpellCastingAbilityHandler : IAbilityActionHandler, ISpellCastingAbilityHandler
    {
        public Ability Ability { get; protected set; }

        public virtual int SpellID => 0;

        public virtual long Preconditions => 0;

        private Spell _spell;

        public virtual Spell Spell => _spell ?? (_spell = SkillBase.GetSpellByID(SpellID));

        private SpellLine _spellLine;

        public virtual SpellLine SpellLine => _spellLine ?? (_spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities));

        public void Execute(Ability ab, GamePlayer player)
        {
            if (player == null)
            {
                return;
            }

            Ability = ab;

            if (CheckPreconditions(player, Preconditions))
            {
                return;
            }

            if (SpellLine != null && Spell != null)
            {
                player.CastSpell(this);
            }
        }

        /// <summary>
        /// Checks for any of the given conditions and returns true if there was any
        /// prints messages
        /// </summary>
        /// <param name="living"></param>
        /// <param name="bitmask"></param>
        /// <returns></returns>
        public virtual bool CheckPreconditions(GameLiving living, long bitmask)
        {
            GamePlayer player = living as GamePlayer;
            if ((bitmask & DEAD) != 0 && !living.IsAlive)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if ((bitmask & MEZZED) != 0 && living.IsMezzed)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if ((bitmask & STUNNED) != 0 && living.IsStunned)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if ((bitmask & SITTING) != 0 && living.IsSitting)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if ((bitmask & INCOMBAT) != 0 && living.InCombat)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if ((bitmask & NOTINCOMBAT) != 0 && !living.InCombat)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if ((bitmask & STEALTHED) != 0 && living.IsStealthed)
            {
                player?.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseWhileStealthed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return true;
            }

            if (player != null && (bitmask & NOTINGROUP) != 0 && player.Group == null)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseMustBeInGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return true;
            }

            if (player != null && (bitmask & TARGET) != 0 && player.TargetObject == null)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseTargetNull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return true;
            }

            return false;
        }

        /*
         * Stored in hex, different values in binary
         * e.g.
         * 16|8|4|2|1
         * ----------
         * 1
         * 0|0|0|0|1 stored as 0x00000001
         * 2
         * 0|0|0|1|0 stored as 0x00000002
         * 4
         * 0|0|1|0|0 stored as 0x00000004
         * 8
         * 0|1|0|0|0 stored as 0x00000008
         * 16
         * 1|0|0|0|0 stored as 0x00000010
         */
        public const long DEAD = 0x00000001;
        public const long SITTING = 0x00000002;
        public const long MEZZED = 0x00000004;
        public const long STUNNED = 0x00000008;
        public const long INCOMBAT = 0x00000010;
        public const long NOTINCOMBAT = 0x00000020;
        public const long NOTINGROUP = 0x00000040;
        public const long STEALTHED = 0x000000080;
        public const long TARGET = 0x000000100;
    }
}
