using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities.Statics
{
    public class WallOfFlameBase : GenericBase
    {
        protected override string GetStaticName() { return "Wall Of Flame"; }

        protected override ushort GetStaticModel() { return 2651; }

        protected override ushort GetStaticEffect() { return 7050; }

        private readonly Spell _spell;
        private readonly SpellLine _spellLine;

        public WallOfFlameBase(int damage)
        {
            var dbSpell = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = damage,
                DamageType = (int) eDamageType.Heat,
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

            _spell = new Spell(dbSpell, 1);
            _spellLine = new SpellLine("RAs", "RealmAbilitys", "RealmAbilitys", true);
        }

        protected override void CastSpell(GameLiving target)
        {
            if (!target.IsAlive)
            {
                return;
            }

            if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
            {
                ISpellHandler damage = ScriptMgr.CreateSpellHandler(Caster, _spell, _spellLine);
                damage.StartSpell(target);
            }
        }
    }
}