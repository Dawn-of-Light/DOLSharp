using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Adrenaline Rush
	/// </summary>
	public class NaturesWombEffect : TimedEffect
	{


		public NaturesWombEffect()
			: base(5000)
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

			GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			owner.IsStunned = true;
			owner.StopAttack();
			owner.StopCurrentSpellcast();
			owner.DisableTurning(true);
			if (player != null)
			{
				player.Out.SendUpdateMaxSpeed();
			}
			else
			{
				owner.CurrentSpeed = owner.MaxSpeed;
			}
		}

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackData ad = null;
			if (attackedByEnemy != null)
				ad = attackedByEnemy.AttackData;

			if (ad.Damage + ad.CriticalDamage < 1)
				return;

			int heal = ad.Damage + ad.CriticalDamage;
			ad.Damage = 0;
			ad.CriticalDamage = 0;
			GamePlayer player = living as GamePlayer;
			GamePlayer attackplayer = ad.Attacker as GamePlayer;
			if (attackplayer != null)
				attackplayer.Out.SendMessage(living.Name + "'s druidic powers absorb your attack!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			int modheal = living.MaxHealth - living.Health;
			if (modheal > heal)
				modheal = heal;
			living.Health += modheal;
			if (player != null)
				player.Out.SendMessage("Your druidic powers convert your enemies attack and heal you for " + modheal + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

		}

		public override string Name { get { return "Nature's Womb"; } }

		public override ushort Icon { get { return 3052; } }

		public override void Stop()
		{
			GameEventMgr.RemoveHandler(owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			owner.IsStunned = false;
			owner.DisableTurning(false);
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

		public int SpellEffectiveness
		{
			get { return 100; }
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Stuns you for 5 seconds but absorbs all damage taken");
				return list;
			}
		}
	}
}