using System;
using System.Collections.Generic;
using System.Text;

namespace DOL.GS
{
	public class TheurgistPet : GamePet
	{
		public TheurgistPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

		public override int MaxHealth
		{
			get { return Level * 20; }
		}
		public override void OnAttackedByEnemy(AttackData ad) { }
	}
}
