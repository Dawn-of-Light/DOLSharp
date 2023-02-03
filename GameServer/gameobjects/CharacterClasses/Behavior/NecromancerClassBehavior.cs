namespace DOL.GS
{
    public class NecromancerClassBehavior : DefaultClassBehavior
    {
        public NecromancerClassBehavior(GamePlayer player) : base(player) { }

        public override byte HealthPercentGroupWindow
        {
            get
            {
                if (Player.ControlledBrain == null)
                    return Player.HealthPercent;

                return Player.ControlledBrain.Body.HealthPercent;
            }
        }
    }
}