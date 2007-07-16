using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class WhirlingStaffEffect : TimedEffect
	{
		public WhirlingStaffEffect()
			: base(6000)
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
			target.IsDisarmed = true;
			target.StopAttack();

		}

		public override string Name { get { return "Whirling Staff"; } }

		public override ushort Icon { get { return 3042; } }

		public override void Stop()
		{
			owner.IsDisarmed = false;
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
				list.Add("Disarms you for 6 seconds!");
				return list;
			}
		}
	}
}