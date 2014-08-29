namespace DOL.GS.Scripts
{
        public class BefuddledGolem : GameNPC
        {
            #region Hidden Locations in Treibh Caillte
            public static int[,] HiddenCavesJumpPoint =
            {
                // X      Y      Z
                {30458, 32125, 15552},  //  Dreaded Ursine
                {29413, 32628, 14680},  //  Thorg
                {29334, 32670, 12759},  //  Hursk the Alchemist & Helminth
                {31381, 32109, 16375},  //  Hervelina the Hermit
                {34143, 29921, 16348},  //  Perioclias
                {34158, 29906, 15292}   //  Driff Tinel & Ceracor
            };
            #endregion
            
            private int HiddenCaveChoice;

            public override void Die(GameObject killer)
            {
                GamePlayer player = killer as GamePlayer;
                this.HiddenCaveChoice = Util.Random(0, 5);

                if (player.Group != null)
                {
                    foreach (GamePlayer GroupPlayer in player.Group.GetPlayersInTheGroup())
                    {
                        GroupPlayer.MoveTo(224, HiddenCavesJumpPoint[HiddenCaveChoice, 0], HiddenCavesJumpPoint[HiddenCaveChoice, 1], HiddenCavesJumpPoint[HiddenCaveChoice, 2], 0);
                    }
                }
                else
                {
                    player.MoveTo(224, HiddenCavesJumpPoint[HiddenCaveChoice, 0], HiddenCavesJumpPoint[HiddenCaveChoice, 1], HiddenCavesJumpPoint[HiddenCaveChoice, 2], 0);
                }

                //The mob is killed by the killer
                base.Die(killer);

                return;
            }
        }
}