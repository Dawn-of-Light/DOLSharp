using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using NUnit.Framework;
using System;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_PropertyChangingSpell
    {
        [SetUp]
        public void Init()
        {
            GS.ServerProperties.Properties.DISABLED_REGIONS = "";
            GS.ServerProperties.Properties.DISABLED_EXPANSIONS = "";
            GS.ServerProperties.Properties.DEBUG_LOAD_REGIONS = "";
        }

        [Test]
        public void ApplyBuffOnTarget_5ResiPierceOnAnotherPlayer_Has5ResiPierceBaseBuffBonus()
        {
            var caster = new FakePlayer();
            var target = new FakePlayer();
            var dbSpell = new DBSpell();
            dbSpell.Value = 5;
            dbSpell.Target = "Realm";
            dbSpell.Duration = 10;
            var spell = new Spell(dbSpell, 0);
            var spellLine = new SpellLine("", "", "", true);
            var resiPierceBuff = new ResiPierceBuff(caster, spell, spellLine);
            FakeServer.LoadAndReturn().FakeServerRules.fakeIsAllowedToAttack = false;

            resiPierceBuff.ApplyEffectOnTarget(target, 1);

            var actualResiPiercing = target.BaseBuffBonusCategory[eProperty.ResistPierce];
            Assert.AreEqual(5, actualResiPiercing);
        }
    }
}
