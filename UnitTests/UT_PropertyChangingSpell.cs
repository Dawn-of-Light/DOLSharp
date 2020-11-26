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
            GS.ServerProperties.Properties.MOB_BUFF_EFFECT_MULTIPLIER = 13; //default
            GameLiving.LoadCalculators();
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

            resiPierceBuff.ApplyEffectOnTarget(target, 1);

            var actualResiPiercing = target.BaseBuffBonusCategory[eProperty.ResistPierce];
            Assert.AreEqual(5, actualResiPiercing);
        }

        [Test]
        public void ApplyEffectOnTarget_50ConBuffOnL50NPC_6PercentSlashResists()
        {
            var caster = new FakeNPC();
            var target = new FakeNPC();
            target.Level = 50;
            var dbSpell = new DBSpell();
            dbSpell.Value = 50;
            dbSpell.Target = "Realm";
            dbSpell.Duration = 10;
            var spell = new Spell(dbSpell, 0);
            var spellLine = new SpellLine("", "", "", true);
            var constitutionBuff = new ConstitutionBuff(caster, spell, spellLine);

            constitutionBuff.ApplyEffectOnTarget(target, 1);

            var actualArmorAbsorption = target.GetModified(eProperty.Resist_Slash);
            Assert.AreEqual(6, actualArmorAbsorption);
        }

        [Test]
        public void ApplyEffectOnTarget_50ConBuffOnL50NPC_50Constitution()
        {
            var caster = new FakeNPC();
            var target = new FakeNPC();
            target.Level = 50;
            var dbSpell = new DBSpell();
            dbSpell.Value = 50;
            dbSpell.Target = "Realm";
            dbSpell.Duration = 10;
            var spell = new Spell(dbSpell, 0);
            var spellLine = new SpellLine("", "", "", true);
            var constitutionBuff = new ConstitutionBuff(caster, spell, spellLine);

            constitutionBuff.ApplyEffectOnTarget(target, 1);

            var actual = target.GetModified(eProperty.Constitution);
            Assert.AreEqual(1, actual);
        }
    }
}
