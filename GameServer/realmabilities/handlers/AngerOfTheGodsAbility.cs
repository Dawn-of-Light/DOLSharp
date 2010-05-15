using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	public class AngerOfTheGodsAbility : TimedRealmAbility
	{
        private DBSpell m_dbspell;
        private Spell m_spell;
        private SpellLine m_spellline;
        private double m_damage = 0;
        private GamePlayer m_player;

        public AngerOfTheGodsAbility(DBAbility dba, int level) : base(dba, level) {}
        public virtual void NewSpell(double damage)
        {
            m_dbspell = new DBSpell();
            m_dbspell.Name = "Anger of the Gods";
            m_dbspell.Icon = 7023;
            m_dbspell.ClientEffect = 7023;
            m_dbspell.Damage = damage;
            m_dbspell.DamageType = 0;
            m_dbspell.Target = "Group";
            m_dbspell.Radius = 0;
            m_dbspell.Type = "DamageAdd";
            m_dbspell.Value = 0;
            m_dbspell.Duration = 30;
            m_dbspell.Pulse = 0;
            m_dbspell.PulsePower = 0;
            m_dbspell.Power = 0;
            m_dbspell.CastTime = 0;
			m_dbspell.EffectGroup = 99999; // stacks with other damage adds
            m_dbspell.Range = 1000;
            m_spell = new Spell(m_dbspell, 1);
            m_spellline = new SpellLine("RAs", "RealmAbilities", "RealmAbilities", true);
        }	

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			m_player = living as GamePlayer;
			switch (Level)
			{
				case 1: m_damage = 10.0; break;
				case 2: m_damage = 20.0; break;
				case 3: m_damage = 30.0; break;
				default: return;
			}
			NewSpell(m_damage);
			CastSpell(m_player);
			DisableSkill(living);
		}

        protected void CastSpell(GameLiving target)
        {
            if (!target.IsAlive) return;
            m_spell = new Spell(m_dbspell, 1);
            ISpellHandler dd = ScriptMgr.CreateSpellHandler(m_player, m_spell, m_spellline);
			dd.IgnoreDamageCap = true;
            dd.StartSpell(target);
        }	

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
