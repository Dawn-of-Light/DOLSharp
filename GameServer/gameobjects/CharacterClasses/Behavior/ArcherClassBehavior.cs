using System.Linq;

namespace DOL.GS
{
    public class ArcherClassBehavior : DefaultClassBehavior
    {
        public override void OnSkillTrained(GamePlayer player, Specialization skill)
        {
            base.OnSkillTrained(player, skill);

            var isArcherSkill = new[] { Specs.RecurveBow, Specs.CompositeBow, Specs.Longbow }.Contains(skill.KeyName);

            if (isArcherSkill && ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
            {
                if (skill.Level >= 3 && skill.Level <= 5) AddAbility(player, Abilities.Critical_Shot, 1);
                else if (skill.Level < 9) AddAbility(player, Abilities.Critical_Shot, 2);
                else if (skill.Level < 12) AddAbility(player, Abilities.Critical_Shot, 3);
                else if (skill.Level < 15) AddAbility(player, Abilities.Critical_Shot, 4);
                else if (skill.Level < 18) AddAbility(player, Abilities.Critical_Shot, 5);
                else if (skill.Level < 21) AddAbility(player, Abilities.Critical_Shot, 6);
                else if (skill.Level < 24) AddAbility(player, Abilities.Critical_Shot, 7);
                else if (skill.Level < 27) AddAbility(player, Abilities.Critical_Shot, 8);
                else if (skill.Level >= 27) AddAbility(player, Abilities.Critical_Shot, 9);

                if (skill.Level >= 45) AddAbility(player, Abilities.RapidFire, 2);
                else if (skill.Level >= 35) AddAbility(player, Abilities.RapidFire, 1);

                if (skill.Level >= 45) AddAbility(player, Abilities.SureShot);

                if (skill.Level >= 50) AddAbility(player, Abilities.PenetratingArrow, 3);
                else if (skill.Level >= 40) AddAbility(player, Abilities.PenetratingArrow, 2);
                else if (skill.Level >= 30) AddAbility(player, Abilities.PenetratingArrow, 1);
            }
        }

        private void AddAbility(GamePlayer player, string skillName, int level = 1)
            => player.AddAbility(SkillBase.GetAbility(skillName, level));
    }
}