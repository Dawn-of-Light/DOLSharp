using System;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class DecimationTrapAbility : TimedRealmAbility
	{
		/*private ushort region;
		private int x;
		private int y;
		private int z;*/
		private Area.Circle traparea;
		private GameLiving owner;
		private RegionTimer ticktimer;
		private const int TICKS = 6;
		private int effectiveness;
		private ushort region;

		public DecimationTrapAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			ushort Icon = 7026;
			effectiveness = 0;
			owner = living;
			switch (Level)
			{
				case 1: effectiveness = 300; break;
				case 2: effectiveness = 600; break;
				case 3: effectiveness = 900; break;
			}

			if (living.GroundTarget == null)
				return;
			if (!living.IsWithinRadius( living.GroundTarget, 1500 ))
				return;
			GamePlayer player = living as GamePlayer;
			if (player == null)
				return;

			if (player.RealmAbilityCastTimer != null)
			{
				player.RealmAbilityCastTimer.Stop();
				player.RealmAbilityCastTimer = null;
				player.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}

			foreach (GamePlayer p in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				p.Out.SendSpellCastAnimation(living, Icon, 20);

			player.RealmAbilityCastTimer = new RegionTimer(player);
			player.RealmAbilityCastTimer.Callback = new RegionTimerCallback(startSpell);
			player.RealmAbilityCastTimer.Start(2000);
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.Level >= 40;
		}

		private int startSpell(RegionTimer timer)
		{
			if (!owner.IsAlive)
				return 0;

			traparea = new Area.Circle("decimation trap", owner.X, owner.Y, owner.Z, 50);

			owner.CurrentRegion.AddArea(traparea);
			region = owner.CurrentRegionID;

			GameEventMgr.AddHandler(traparea, AreaEvent.PlayerEnter, new DOLEventHandler(EventHandler));
			ticktimer = new RegionTimer(owner);
			ticktimer.Callback = new RegionTimerCallback(onTick);
			ticktimer.Start(600000);
			getTargets();
			DisableSkill(owner);

			return 0;
		}

		private int onTick(RegionTimer timer)
		{
			removeHandlers();
			return 0;
		}

		protected void EventHandler(DOLEvent e, Object sender, EventArgs arguments)
		{
			AreaEventArgs args = arguments as AreaEventArgs;
			if (args == null)
				return;
			GameLiving living = args.GameObject as GameLiving;
			if (living == null)
				return;
			if (!GameServer.ServerRules.IsAllowedToAttack(owner, living, true))
				return;
			getTargets();
		}


		private void removeHandlers()
		{
			owner.CurrentRegion.RemoveArea(traparea);
			GameEventMgr.RemoveHandler(traparea, AreaEvent.PlayerEnter, new DOLEventHandler(EventHandler));
		}


		private void getTargets()
		{
			foreach (GamePlayer target in WorldMgr.GetPlayersCloseToSpot(region, traparea.X, traparea.Y, traparea.Z, 350))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(owner, target, true))
				{
					DamageTarget(target);
				}
			}
			foreach (GameNPC target in WorldMgr.GetNPCsCloseToSpot(region, traparea.X, traparea.Y, traparea.Z, 350))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(owner, target, true))
				{
					DamageTarget(target);
				}
			}
		}

		private void DamageTarget(GameLiving target)
		{
			if (!GameServer.ServerRules.IsAllowedToAttack(owner, target, false))
				return;
			if (!target.IsAlive)
				return;
			if (ticktimer.IsAlive)
			{
				ticktimer.Stop();
				removeHandlers();
			}
			int dist = target.GetDistance( new Point3D( traparea.X, traparea.Y, traparea.Z ) );
			double mod = 1;
			if (dist > 0)
				mod = 1 - ((double)dist / 350);

			int basedamage = (int)(effectiveness * mod);
			int resist = (int)(basedamage * target.GetModified(eProperty.Resist_Energy) * -0.01);
			int damage = basedamage + resist;


			GamePlayer player = owner as GamePlayer;
			if (player != null)
			{
				player.Out.SendMessage("You hit " + target.Name + " for " + damage + "(" + resist + ") points of damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}

			GamePlayer targetPlayer = target as GamePlayer;
			if (targetPlayer != null)
			{
				if (targetPlayer.IsStealthed)
					targetPlayer.Stealth(false);
			}

			foreach (GamePlayer p in target.GetPlayersInRadius(false, WorldMgr.VISIBILITY_DISTANCE))
			{
				p.Out.SendSpellEffectAnimation(owner, target, 7026, 0, false, 1);
				p.Out.SendCombatAnimation(owner, target, 0, 0, 0, 0, 0x14, target.HealthPercent);
			}

			//target.TakeDamage(owner, eDamageType.Energy, damage, 0);
			AttackData ad = new AttackData();
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
			ad.Attacker = owner;
			ad.Target = target;
			ad.DamageType = eDamageType.Energy;
			ad.Damage = damage;
			target.OnAttackedByEnemy(ad);
			owner.DealDamage(ad);
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Trap that deals the following damage in an 350 radius");
			list.Add("Level 1: 300 Damage");
			list.Add("Level 2: 600 Damage");
			list.Add("Level 3: 900 Damage");
			list.Add("");
			list.Add("Range 1500");
			list.Add("Target: Ground Target");
			list.Add("Radius: 350");
			list.Add("Casting time: 2 seconds");
		}
	}
}
