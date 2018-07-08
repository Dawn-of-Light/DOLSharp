using DOL.GS;

namespace DOL.AI.Brain
{
    public class MeleeBehaviour : IAttackBehaviour
    {
        public MeleeBehaviour(GameLiving body)
        {
            Body = body;
        }

        private GameLiving Body { get; set; }

        public void Attack(GameObject target)
        {
            Body.StartAttack(target);
        }

        public void Retreat()
        {
            Body.StopAttack();
        }
    }
}
