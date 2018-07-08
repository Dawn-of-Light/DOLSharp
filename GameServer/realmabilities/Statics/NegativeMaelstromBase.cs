using System;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities.Statics
{
    public class NegativeMaelstromBase : GenericBase
    {
        protected override string GetStaticName() { return "Negative Maelstrom"; }

        protected override ushort GetStaticModel() { return 1293; }

        protected override ushort GetStaticEffect() { return 7027; }

        private readonly DBSpell _dbSpell;
        private readonly SpellLine _spellLine;
        private readonly int _damage;
        private Spell _spell;

        public NegativeMaelstromBase(int damage)
        {
            _damage = damage;
            _dbSpell = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = damage,
                DamageType = (int) eDamageType.Cold,
                Target = "Enemy",
                Radius = 0,
                Type = "DirectDamageNoVariance",
                Value = 0,
                Duration = 0,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };

            _spellLine = new SpellLine("RAs","RealmAbilitys","RealmAbilitys",true);
        }

        protected override void CastSpell(GameLiving target)
        {
            if (!target.IsAlive)
            {
                return;
            }

            if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
            {
                int dealDamage = _damage;
                if (CurrentPulse<= 6)
                {
                    dealDamage = (int)Math.Round((double)CurrentPulse/ 6 * _damage);
                }

                _dbSpell.Damage = dealDamage;
                _spell = new Spell(_dbSpell, 1);
                ISpellHandler dd = ScriptMgr.CreateSpellHandler(Caster, _spell, _spellLine);
                dd.StartSpell(target);
            }
        }
    }
}
