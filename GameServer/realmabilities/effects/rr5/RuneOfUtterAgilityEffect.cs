using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Mastery of Concentration
	/// </summary>
	public class RuneOfUtterAgilityEffect : TimedEffect
	{
		int m_effectiveness;
		private GameLiving owner;

		public RuneOfUtterAgilityEffect()
			: base(15000)
		{
		}

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
				player.BuffBonusCategory4[(int)eProperty.EvadeChance] += 90;
			}
		}

		public override void Stop()
		{
			GamePlayer player = owner as GamePlayer;
			if (player != null)
				player.BuffBonusCategory4[(int)eProperty.EvadeChance] -= 90;
			base.Stop();
		}

		public override string Name { get { return "Rune Of Utter Agility"; } }

		public override ushort Icon { get { return 7074; } }

		public int SpellEffectiveness
		{
			get { return m_effectiveness; }
		}

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Increases your evade chance up to " + SpellEffectiveness + "% for 30 seconds.");
				return list;
			}
		}
	}
}