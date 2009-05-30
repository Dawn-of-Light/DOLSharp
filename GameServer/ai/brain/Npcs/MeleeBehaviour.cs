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
            Body.TargetObject = null;
        }

        private GameLiving Body { get; set; }

        #region IAttackBehaviour Members

        public void Attack(GameObject target)
        {
            if (target != Body.TargetObject)
                Body.StartAttack(target);
        }

        public void Retreat()
        {
            Body.StopAttack();
        }

        #endregion
    }
}
