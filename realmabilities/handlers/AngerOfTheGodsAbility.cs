using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	public class AngerOfTheGodsAbility : TimedRealmAbility
	{
        private DBSpell m_dbspell;
        private Spell m_spell = null;
        private SpellLine m_spellline;
        private double m_damage = 0;
        private GamePlayer m_player;

        public AngerOfTheGodsAbility(DBAbility dba, int level) : base(dba, level) {}
        public virtual void CreateSpell(double damage)
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
            m_spell = new Spell(m_dbspell, 0); // make spell level 0 so it bypasses the spec level adjustment code
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

			CreateSpell(m_damage);
			CastSpell(m_player);
			DisableSkill(living);
		}

        protected void CastSpell(GameLiving target)
        {
			if (target.IsAlive && m_spell != null)
			{
				ISpellHandler dd = ScriptMgr.CreateSpellHandler(m_player, m_spell, m_spellline);
				dd.IgnoreDamageCap = true;
				dd.StartSpell(target);
			}
        }	

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add("Level 1: Adds 10 DPS");
			list.Add("Level 2: Adds 20 DPS");
			list.Add("Level 3: Adds 30 DPS");
			list.Add("");
			list.Add("Target: Group");
			list.Add("Duration: 30 sec");
			list.Add("Casting time: instant");
		}
	}
}
