using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.Language;


namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Shadow Strike Ability inherit of RR5RealmAbility (RealmLevel >= 40 and free)
    /// </summary>
    public class ShadowStrikeAbility : RR5RealmAbility
    {
        private DBSpell m_dbspell;
        private Spell m_spell;
        private SpellLine m_spellline;
        private ShadowStrikeSpellHandler dd;
        private GamePlayer m_player;

        public ShadowStrikeAbility(DBAbility dba, int level) : base(dba, level)
        {
            CreateSpell();
        }

        private void CreateSpell()
        {
            m_dbspell = new DBSpell
            {
                Name = "Shadow Strike",
                Icon = 7073,
                ClientEffect = 12011,
                IsFocus = true,
                Target = "Enemy",
                Type = "ShadowStrike",
                CastTime = 10,
                MoveCast = false,
                Range = 1000
            };

            m_spell = new Spell(m_dbspell, 0); // make spell level 0 so it bypasses the spec level adjustment code
            m_spellline = new SpellLine("RAs", "RealmAbilities", "RealmAbilities", true);
        }

        protected bool CastSpell(GameLiving target)
        {
            if (target.IsAlive && m_spell != null)
            {
                dd = ScriptMgr.CreateSpellHandler(m_player, m_spell, m_spellline) as ShadowStrikeSpellHandler;
                dd.IgnoreDamageCap = true;
                return dd.CastSpell(target);
            }
            return false;
        }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }
            m_player = living as GamePlayer;

            CreateSpell();
            if(m_player.TargetObject is GameLiving)
            {
                CastSpell(m_player.TargetObject as GameLiving);
            }
            else
            {
                m_player.Out.SendMessage(LanguageMgr.GetTranslation(m_player.Client.Account.Language, "ShadowStrikeAbility.Target"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}