namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Vampiir Bolt clicks
    /// </summary>
    [SkillHandler(Abilities.VampiirBolt)]
    public class VampiirBoltAbilityHandler : SpellCastingAbilityHandler
    {
        public override long Preconditions => DEAD | SITTING | MEZZED | STUNNED | TARGET;

        public override int SpellID => 13200 + Ability.Level;
    }
}
