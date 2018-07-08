// Andraste v2.0 - Vico

using DOL.GS.Effects;
using DOL.Database;

// using log4net;
namespace DOL.GS.RealmAbilities
{
    public class ResoluteMinionAbility : RR5RealmAbility
    {
        public ResoluteMinionAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (!(living is GamePlayer player))
            {
                return;
            }

            if (player.ControlledBrain?.Body == null)
            {
                return;
            }

            player.ControlledBrain.Body.AddAbility(SkillBase.GetAbility(Abilities.CCImmunity));
            new ResoluteMinionEffect().Start(player.ControlledBrain.Body);
            foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                visPlayer.Out.SendSpellEffectAnimation(player, player.ControlledBrain.Body, 7047, 0, false, 0x01);
            }

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level) { return 300; }
    }
}

namespace DOL.GS.Effects
{
    public class ResoluteMinionEffect : TimedEffect
    {
        private const int Duration = 60000;

        public ResoluteMinionEffect() : base(Duration) { }

        private GameNPC _pet;

        public void Start(GameNPC controllednpc) { base.Start(controllednpc); _pet = controllednpc; }

        public override void Stop()
        {
            if (_pet != null)
            {
                if (_pet.EffectList.GetOfType<ResoluteMinionEffect>() != null)
                {
                    _pet.EffectList.Remove(this);
                }

                if (_pet.HasAbility(Abilities.CCImmunity))
                {
                    _pet.RemoveAbility("CCImmunity");
                }
            }

            base.Stop();
        }

        public override ushort Icon => 7047;
    }
}
