using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;

namespace DOL.AI.Brain
{
    public class StayPutBehaviour : IAttackBehaviour
    {
        #region IAttackBehaviour Members

        public bool Attack(GameObject target)
        {
            return true;
        }

        public void Retreat()
        {
        }

        #endregion
    }
}
