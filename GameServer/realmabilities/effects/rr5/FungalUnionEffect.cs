using System.Collections;
using System;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{

    public class FungalUnionEffect : TimedEffect
    {
        private GameLiving owner;

        public FungalUnionEffect() : base(60000) { }


        public override void Start(GameLiving target)
        {
            base.Start(target);
            owner = target;
            GamePlayer player = target as GamePlayer;
            if (player != null)
            {
                player.Model = 1648;
            }
        }

        public override void Stop()
        {
            base.Stop();
            GamePlayer player = owner as GamePlayer;
            if (player is GamePlayer)
            {
                player.Model = (ushort)player.PlayerCharacter.CreationModel;
            }
        }


        public override string Name { get { return "Fungal Union"; } }


        public override ushort Icon { get { return 3061; } }


        public override System.Collections.IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add("Turns the animist into a mushroom for 60 seconds. Does not break on attack. Grants the animist a 10% chance of not spending power for each spell cast during the duration.");
                return list;
            }
        }
    }
}