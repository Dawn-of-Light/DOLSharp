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
	public class FuryOfNatureEffect : TimedEffect
	{
		public FuryOfNatureEffect()
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

			GameEventMgr.AddHandler(target, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));


		}

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
			if (args == null) return;
			if (args.AttackData == null) return;

			int extra = 0;
			if (args.AttackData.StyleDamage > 0)
			{
				extra = args.AttackData.StyleDamage;
				args.AttackData.Damage += args.AttackData.StyleDamage;
				args.AttackData.StyleDamage *= 2;
			}

			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;
			if (player.Group == null)
				return;
			if (extra > 0)
				player.Out.SendMessage("Your Fury enables you to strike " + args.AttackData.Target.Name + " for " + extra + " additional points of damage", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			Hashtable injuredTargets = new Hashtable();
			GamePlayer mostInjuredLiving = owner as GamePlayer;
			if (mostInjuredLiving == null) return;

			foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
			{
				if (p == player)
					continue;
				mostInjuredLiving = p;
				break;
			}

			if (mostInjuredLiving == owner) return;

			double mostInjuredPercent = mostInjuredLiving.Health / (float)mostInjuredLiving.MaxHealth;
			int groupHealCap = args.AttackData.Damage;
			int targetHealCap = args.AttackData.Damage;
			if (player.Group.MemberCount > 2)
			{
				groupHealCap *= (player.Group.MemberCount);
				targetHealCap *= 2;
				foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
				{
					if (!p.IsAlive) continue;
					if (p == player) continue;
					if (p.IsWithinRadius(player, 2000))
					{
						double playerHealthPercent = p.Health / (double)p.MaxHealth;
						if (playerHealthPercent < 1)
						{
							injuredTargets.Add(p, playerHealthPercent);
							if (playerHealthPercent < mostInjuredPercent)
							{
								mostInjuredLiving = p;
								mostInjuredPercent = playerHealthPercent;
							}
						}
					}
				}
			}
			else
			{
				if (mostInjuredPercent < 1)
					injuredTargets.Add(mostInjuredLiving, mostInjuredPercent);
			}

			if (mostInjuredPercent >= 1)
			{
				player.Out.SendMessage("Your group is fully healed!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				return;
			}

			double bestHealPercent = targetHealCap / (double)mostInjuredLiving.MaxHealth;
			double totalHealed = 0;
			Hashtable healAmount = new Hashtable();

			IDictionaryEnumerator iter = injuredTargets.GetEnumerator();
			//calculate heal for all targets
			while (iter.MoveNext())
			{
				GameLiving healTarget = iter.Key as GameLiving;
				double targetHealthPercent = (double)iter.Value;
				//targets hp percent after heal is same as mostInjuredLiving
				double targetHealPercent = bestHealPercent + mostInjuredPercent - targetHealthPercent;
				int targetHeal = (int)(healTarget.MaxHealth * targetHealPercent);
				//DOLConsole.WriteLine("SpreadHeal: targetHealPercent=" + targetHealPercent + "; uncapped targetHeal=" + targetHeal + "; bestHealPercent=" + bestHealPercent + "; mostInjuredPercent=" + mostInjuredPercent + "; targetHealthPercent=" + targetHealthPercent);

				if (targetHeal > 0)
				{
					totalHealed += targetHeal;
					healAmount.Add(healTarget, targetHeal);
				}
			}

			iter = healAmount.GetEnumerator();
			//reduce healed hp according to targetHealCap and heal targets
			while (iter.MoveNext())
			{
				GameLiving healTarget = iter.Key as GameLiving;
				if (!healTarget.IsAlive)
					continue;
				double uncappedHeal = (int)iter.Value;
				int reducedHeal = (int)Math.Min(targetHealCap, uncappedHeal * (groupHealCap / totalHealed));

				//heal target
				int baseheal = healTarget.MaxHealth - healTarget.Health;
				if (reducedHeal < baseheal)
					baseheal = reducedHeal;
				healTarget.ChangeHealth(player, GameLiving.eHealthChangeType.Spell, baseheal);
				player.Out.SendMessage("You heal " + healTarget.Name + " for " + baseheal + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if (healTarget is GamePlayer)
					((GamePlayer)healTarget).Out.SendMessage(player.Name + " heals you for " + baseheal + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}

			return;



		}

		public override string Name { get { return "Fury of Nature"; } }

		public override ushort Icon { get { return 991; } }

		public override void Stop()
		{
			GameEventMgr.RemoveHandler(owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
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
				list.Add("Doubles style damage and returns all damage dealt as spreadheal to all group members except the caster");
				return list;
			}
		}
	}
}