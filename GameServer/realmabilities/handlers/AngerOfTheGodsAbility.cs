using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
    public class AngerOfTheGodsAbility : TimedRealmAbility
    {
        private DBSpell _dbspell;
        private Spell _spell;
        private SpellLine _spellline;
        private double _damage;
        private GamePlayer _player;

        public AngerOfTheGodsAbility(DBAbility dba, int level, DBSpell dbspell) : base(dba, level)
        {
            _dbspell = dbspell;
        }

        public virtual void CreateSpell(double damage)
        {
            _dbspell = new DBSpell
            {
                Name = "Anger of the Gods",
                Icon = 7023,
                ClientEffect = 7023,
                Damage = damage,
                DamageType = 0,
                Target = "Group",
                Radius = 0,
                Type = "DamageAdd",
                Value = 0,
                Duration = 30,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                EffectGroup = 99999,
                Range = 1000
            };

            _spell = new Spell(_dbspell, 0); // make spell level 0 so it bypasses the spec level adjustment code
            _spellline = new SpellLine("RAs", "RealmAbilities", "RealmAbilities", true);
        }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            _player = living as GamePlayer;

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _damage = 10.0; break;
                    case 2: _damage = 15.0; break;
                    case 3: _damage = 20.0; break;
                    case 4: _damage = 25.0; break;
                    case 5: _damage = 30.0; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _damage = 10.0; break;
                    case 2: _damage = 20.0; break;
                    case 3: _damage = 30.0; break;
                    default: return;
                }
            }

            CreateSpell(_damage);
            CastSpell(_player);
            DisableSkill(living);
        }

        protected void CastSpell(GameLiving target)
        {
            if (target.IsAlive && _spell != null)
            {
                ISpellHandler dd = ScriptMgr.CreateSpellHandler(_player, _spell, _spellline);
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
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                list.Add("Level 1: Adds 10 DPS");
                list.Add("Level 2: Adds 15 DPS");
                list.Add("Level 3: Adds 20 DPS");
                list.Add("Level 4: Adds 25 DPS");
                list.Add("Level 5: Adds 30 DPS");
                list.Add(string.Empty);
                list.Add("Target: Group");
                list.Add("Duration: 30 sec");
                list.Add("Casting time: instant");
            }
            else
            {
                list.Add("Level 1: Adds 10 DPS");
                list.Add("Level 2: Adds 20 DPS");
                list.Add("Level 3: Adds 30 DPS");
                list.Add(string.Empty);
                list.Add("Target: Group");
                list.Add("Duration: 30 sec");
                list.Add("Casting time: instant");
            }
        }
    }
}
