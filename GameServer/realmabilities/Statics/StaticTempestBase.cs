using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities.Statics
{
    public class StaticTempestBase : GenericBase
    {
        protected override string GetStaticName() { return "Static Tempest"; }

        protected override ushort GetStaticModel() { return 2654; }

        protected override ushort GetStaticEffect() { return 7032; }

        private readonly Spell _spell;
        private readonly SpellLine _spellLine;

        public StaticTempestBase(int stunDuration)
        {
            var dbSpell = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = 0,
                DamageType = (int)eDamageType.Energy,
                Target = "Enemy",
                Radius = 0,
                Type = "UnresistableStun",
                Value = 0,
                Duration = stunDuration,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };

            _spell = new Spell(dbSpell,1);
            _spellLine = new SpellLine("RAs","RealmAbilitys","RealmAbilitys",true);
        }

        protected override void CastSpell(GameLiving target) {
            if (!target.IsAlive)
            {
                return;
            }

            if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
            {
                ISpellHandler stun = ScriptMgr.CreateSpellHandler(Caster, _spell, _spellLine);
                stun.StartSpell(target);
            }
        }
    }
}

