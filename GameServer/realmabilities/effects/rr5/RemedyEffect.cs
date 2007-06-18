using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class RemedyEffect : TimedEffect
	{
		public RemedyEffect()
			: base(60000)
		{
			;
		}

		private GameLiving owner;
		private int healthdrain;

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
			healthdrain = (int)(target.MaxHealth * 0.1);
			if (target.Health <= healthdrain)
				return;
			target.TakeDamage(target, eDamageType.Body, healthdrain, 0);

		}

		public override string Name { get { return "Remedy"; } }

		public override ushort Icon { get { return 7060; } }

		public override void Stop()
		{
			if (!owner.IsAlive)
			{
				base.Stop();
				return;
			}
			int heal = owner.MaxHealth - owner.Health;
			if (heal > healthdrain)
				heal = healthdrain;
			owner.Health += healthdrain;
			base.Stop();
		}

		public int SpellEffectiveness
		{
			get { return 100; }
		}

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("For 60 seconds you're immune to all weapon poisons");
				return list;
			}
		}
	}
}