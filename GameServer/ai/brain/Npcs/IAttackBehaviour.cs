using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;

namespace DOL.AI.Brain
{
    public interface IAttackBehaviour
    {
        void Attack(GameObject target);
        void Retreat();
    }
}
