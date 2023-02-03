using DOL.AI;
using DOL.AI.Brain;
using DOL.Events;

namespace DOL.GS
{
    public class BonedancerClassBehavior : DefaultClassBehavior
    {
        public BonedancerClassBehavior(GamePlayer player) : base(player) { }

        public override void OnSkillTrained(GamePlayer player, Specialization skill)
        {
            // BD subpet spells can be scaled with the BD's spec as a cap, so when a BD
            //	trains, we have to re-scale spells for subpets from that spec.
            if (DOL.GS.ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0
                && DOL.GS.ServerProperties.Properties.PET_CAP_BD_MINION_SPELL_SCALING_BY_SPEC
                && player.ControlledBrain != null && player.ControlledBrain.Body is GamePet pet
                && pet.ControlledNpcList != null)
                foreach (ABrain subBrain in pet.ControlledNpcList)
                    if (subBrain != null && subBrain.Body is BDSubPet subPet && subPet.PetSpecLine == skill.KeyName)
                        subPet.SortSpells();
        }
    }
}