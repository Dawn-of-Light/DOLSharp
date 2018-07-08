using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities.Statics
{
    public class ThornweedFieldBase : GenericBase
    {
        protected override string GetStaticName() { return "Thornwood Field"; }

        protected override ushort GetStaticModel() { return 2653; }

        protected override ushort GetStaticEffect() { return 7028; }

        private readonly Spell _spell;
        private readonly SpellLine _spellLine;

        public ThornweedFieldBase(int damage)
        {
            var dbSpell = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = damage,
                DamageType = (int) eDamageType.Natural,
                Target = "Enemy",
                Radius = 0,
                Type = "DamageSpeedDecreaseNoVariance",
                Value = 50,
                Duration = 5,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };

            _spell = new Spell(dbSpell,1);
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
                ISpellHandler snare = ScriptMgr.CreateSpellHandler(Caster, _spell, _spellLine);
                snare.StartSpell(target);
            }
        }
    }
}
