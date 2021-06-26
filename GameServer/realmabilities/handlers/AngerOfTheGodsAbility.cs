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
        private Spell m_spell;
        private SpellLine m_spellline;
        private double m_damage;
        private GamePlayer m_player;

        public AngerOfTheGodsAbility(DBAbility dba, int level, DBSpell dbspell) : base(dba, level)
        {
            m_dbspell = dbspell;
        }
        public AngerOfTheGodsAbility(DBAbility dba, int level) : base(dba, level)
        {
            CreateSpell(CalculDomage(level));
        }

        private double CalculDomage(int level)
        {
            double damage = 0;
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (level)
                {
                    case 1: damage = 10.0; break;
                    case 2: damage = 15.0; break;
                    case 3: damage = 20.0; break;
                    case 4: damage = 25.0; break;
                    case 5: damage = 30.0; break;
                    default: break;
                }
            }
            else
            {
                switch (level)
                {
                    case 1: damage = 10.0; break;
                    case 2: damage = 20.0; break;
                    case 3: damage = 30.0; break;
                    default: break;
                }
            }
            return damage;
        }

        private void CreateSpell(double damage)
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
			
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: m_damage = 10.0; break;
					case 2: m_damage = 15.0; break;
					case 3: m_damage = 20.0; break;
					case 4: m_damage = 25.0; break;
					case 5: m_damage = 30.0; break;
					default: return;
				}				
			}
			else
			{
				switch (Level)
				{
					case 1: m_damage = 10.0; break;
					case 2: m_damage = 20.0; break;
					case 3: m_damage = 30.0; break;
					default: return;
				}				
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
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				list.Add("Level 1: Adds 10 DPS");
				list.Add("Level 2: Adds 15 DPS");
				list.Add("Level 3: Adds 20 DPS");
				list.Add("Level 4: Adds 25 DPS");
				list.Add("Level 5: Adds 30 DPS");
				list.Add("");
				list.Add("Target: Group");
				list.Add("Duration: 30 sec");
				list.Add("Casting time: instant");				
			}
			else
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
}
