using System.Collections.Generic;

namespace DOL.GS.Effects
{

    public class FungalUnionEffect : TimedEffect
    {
        private GameLiving _owner;

        public FungalUnionEffect() : base(60000) { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            if (target is GamePlayer player)
            {
                player.Model = 1648;
            }
        }

        public override void Stop()
        {
            base.Stop();
            if (_owner is GamePlayer player)
            {
                player.Model = player.CreationModel;
            }
        }

        public override string Name => "Fungal Union";

        public override ushort Icon => 3061;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Turns the animist into a mushroom for 60 seconds. Does not break on attack. Grants the animist a 10% chance of not spending power for each spell cast during the duration."
                };

                return list;
            }
        }
    }
}