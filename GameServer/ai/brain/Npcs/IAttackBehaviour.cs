using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;

namespace DOL.AI.Brain
{
    public interface IAttackBehaviour
    {
        bool Attack(GameObject target);
        void Retreat();
    }
}
