using System;
using System.Collections;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using log4net;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Vampiir Bolt clicks
	/// </summary>
	[SkillHandler(Abilities.VampiirBolt)]
	public class VampiirBoltAbilityHandler : SpellCastingAbilityHandler
	{
		public override long Preconditions
		{
			get
			{
				return DEAD | SITTING | MEZZED | STUNNED | TARGET;
			}
		}

		public override ushort SpellID
		{
			get
			{
				return (ushort)(13200 + m_ability.Level);
			}
		}
	}
}
