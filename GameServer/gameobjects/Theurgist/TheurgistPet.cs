using System;
using System.Collections.Generic;
using System.Text;

namespace DOL.GS
{
	public class TheurgistPet : GamePet
	{
		public TheurgistPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

		public override void OnAttackedByEnemy(AttackData ad) { /* do nothing */ }
	}
}
