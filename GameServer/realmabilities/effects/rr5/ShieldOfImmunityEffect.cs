using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Mastery of Concentration
	/// </summary>
	public class ShieldOfImmunityEffect : TimedEffect
	{
		private GameLiving owner;

		public ShieldOfImmunityEffect()
			: base(20000)
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
			}
			GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
		}

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackData ad = null;
			if (attackedByEnemy != null)
				ad = attackedByEnemy.AttackData;

			if (ad.Damage < 1)
				return;

			if (ad.IsMeleeAttack || ad.AttackType == AttackData.eAttackType.Ranged)
			{
				int absorb = (int)(ad.Damage * 0.9);
                int critic = (int)(ad.CriticalDamage * 0.9);
				ad.Damage -= absorb;
                ad.CriticalDamage -= critic;
				if (living is GamePlayer)
					((GamePlayer)living).Out.SendMessage("Your Shield of Immunity absorbs " + (absorb + critic) + " points of damage", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if (ad.Attacker is GamePlayer)
					((GamePlayer)ad.Attacker).Out.SendMessage(living.Name + "'s Shield of Immunity absorbs " + (absorb + critic) + " points of damage", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

			}
		}

		public override void Stop()
		{
			GameEventMgr.RemoveHandler(owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

			base.Stop();
		}

		public override string Name { get { return "Shield of Immunity"; } }

		public override ushort Icon { get { return 3047; } }


		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Shield that absorbs 90% melee/archer damage for 20 seconds.");
				return list;
			}
		}
	}
}