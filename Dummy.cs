using DOL.GS;
namespace DOL.GS.Scripts
{
	public class Dummy : GameNPC
	{
		public override void OnAttackedByEnemy(AttackData ad){}
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount){}
		public override void StartAttack(GameObject attackTarget){}
		public override void AddAttacker(GameObject attacker){}
	}
}