using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Helper for charge realm ability
    /// </summary>
    public class DreamweaverEffect : TimedEffect
    {
        private GameLiving _owner;

        /// <summary>
        /// </summary>
        public DreamweaverEffect() : base(300000) { }

        /// <summary>
        /// Start the effect on player
        /// </summary>
        /// <param>The effect target</param>
        public override void Start(GameLiving target)
        {
            base.Start(target);
            _owner = target;
            if (target is GamePlayer player)
            {
                player.Model = GetRandomMorph();
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

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name => "Dreamweaver";

        /// <summary>
        /// Icon to show on players, can be id
        /// </summary>
        public override ushort Icon => 3051;

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Transforms you into a random morph for 5 minutes."
                };

                return list;
            }
        }

        public static ushort GetRandomMorph()
        {
            // Returns random morph/gender (like live..)
            return (ushort)Util.Random(1649, 1668);
        }
    }
}