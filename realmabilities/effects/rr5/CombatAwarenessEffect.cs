using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class CombatAwarenessEffect : TimedEffect
	{


		public CombatAwarenessEffect()
			: base(30000)
		{
			;
		}

		private GameLiving owner;

		public override void Start(GameLiving target)
		{
			base.Start(target);
			owner = target;
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
				}
			}
			target.DebuffCategory[(int)eProperty.MeleeDamage] -= 50;
			target.BuffBonusCategory4[(int)eProperty.EvadeChance] += 50;
			target.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 0.5);

			if (player != null)
			{
				player.Out.SendUpdateMaxSpeed();
			}

		}

		public override string Name { get { return "Combat Awareness"; } }

		public override ushort Icon { get { return 3090; } }

		public override void Stop()
		{
			owner.DebuffCategory[(int)eProperty.MeleeDamage] += 50;
			owner.BuffBonusCategory4[(int)eProperty.EvadeChance] -= 50;
			owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);

			GamePlayer player = owner as GamePlayer;
			if (player != null)
			{
				player.Out.SendUpdateMaxSpeed();
			}
			base.Stop();
		}

		public int SpellEffectiveness
		{
			get { return 100; }
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Reduces Melee damage and movement by 50% and grants 50% 360° evade");
				return list;
			}
		}
	}
}