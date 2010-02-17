using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class ShieldTripDisarmEffect : TimedEffect
	{


		public ShieldTripDisarmEffect()
			: base(15000)
		{
			;
		}

		private GameLiving owner;

		public override void Start(GameLiving target)
		{
			base.Start(target);
			owner = target;
			//target.IsDisarmed = true;
            target.DisarmedTime = target.CurrentRegion.Time + m_duration;
			target.StopAttack();

		}

		public override string Name { get { return "Shield Trip"; } }

		public override ushort Icon { get { return 3045; } }

		public override void Stop()
		{
			//owner.IsDisarmed = false;
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
				list.Add("Disarms you for 15 seconds!");
				return list;
			}
		}
	}

	public class ShieldTripRootEffect : TimedEffect
	{
		private GameLiving owner;


		public ShieldTripRootEffect()
			: base(10000)
		{
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
			target.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
			owner = target;
			GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
			GamePlayer player = owner as GamePlayer;
			if (player != null)
			{
				player.Out.SendUpdateMaxSpeed();
			}
			else
			{
				owner.CurrentSpeed = owner.MaxSpeed;
			}

		}

		public override void Stop()
		{
			owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
			GameEventMgr.RemoveHandler(owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
			GamePlayer player = owner as GamePlayer;
			if (player != null)
			{
				player.Out.SendUpdateMaxSpeed();
			}
			else
			{
				owner.CurrentSpeed = owner.MaxSpeed;
			}
			base.Stop();
		}

		protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
			if (attackArgs == null) return;
			switch (attackArgs.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
					Stop();
					break;
			}

		}

		public override string Name { get { return "Shield Trip"; } }

		public override ushort Icon { get { return 7046; } }

		public int SpellEffectiveness
		{
			get { return 0; }
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Root Effect");
				return list;
			}
		}
	}

}