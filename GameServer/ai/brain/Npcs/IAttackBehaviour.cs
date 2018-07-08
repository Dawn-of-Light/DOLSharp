using DOL.GS;

namespace DOL.AI.Brain
{
    public interface IAttackBehaviour
    {
        void Attack(GameObject target);

        void Retreat();
    }
}
