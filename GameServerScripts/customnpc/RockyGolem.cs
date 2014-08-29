namespace DOL.GS.Scripts
{
    public class RockyGolem : GameNPC
    {
        public override void Die(GameObject killer)
        {
            GamePlayer player = killer as GamePlayer;

            if (player.Group != null)
            {
                foreach (GamePlayer GroupPlayer in player.Group.GetPlayersInTheGroup())
                {
                    GroupPlayer.MoveTo(224, 35365,30884,14995,1077);
                }
            }
            else
            {
                player.MoveTo(224, 35365, 30884, 14995, 1077);
            }

            //The mob is killed by the killer
            base.Die(killer);

            //Begin the mob's respawn timer
            StartRespawn();

            return;
        }
    }
}