using System;
using System.Collections.Generic;
using System.Text;
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

        #region IAttackBehaviour Members

        public bool Attack(GameObject target)
        {
            Body.StartAttack(target);
            return true;
        }

        public void Retreat()
        {
            Body.StopAttack();
        }

        #endregion
    }
}
