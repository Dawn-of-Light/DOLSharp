using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.Database;
namespace DOL.GS.RealmAbilities
{
	public class IchorOfTheDeepAbility : TimedRealmAbility
	{
		public IchorOfTheDeepAbility(DBAbility dba, int level) : base(dba, level) { }
		RegionTimer m_expireTimerID;
		RegionTimer m_rootExpire;
		int dmgValue = 0;
		int duration = 0;
		GamePlayer caster;
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			caster = living as GamePlayer;
			if (caster == null)
				return;

			if (caster.TargetObject == null)
			{
				caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			//150 dam/10 sec || 400/20  || 600/30 
			switch (Level)
			{
				case 1: dmgValue = 150; duration = 10000; break;
				case 2: dmgValue = 400; duration = 20000; break;
				case 3: dmgValue = 600; duration = 30000; break;
				default: return;
			}

			if (caster.TargetObject != null)
			{
				foreach (GamePlayer i_player in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{
					if (i_player == caster) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					else i_player.Out.SendMessage(caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

					i_player.Out.SendSpellCastAnimation(caster, 7029, 20);
				}
				DisableSkill(living);
				m_expireTimerID = new RegionTimer(caster, new RegionTimerCallback(EndCast), 2000);
			}
			else
			{
				caster.DisableSkill(this, 3 * 1000);
				caster.Out.SendMessage("You don't have a target.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}
		}

		protected virtual int EndCast(RegionTimer timer)
		{
			if (GameServer.ServerRules.IsAllowedToAttack(caster, (GameLiving)caster.TargetObject, true))
			{
				if (caster.TargetInView)
				{
					if (WorldMgr.GetDistance(caster, caster.TargetObject) <= 1875)
					{
						GameLiving living = caster.TargetObject as GameLiving;
						living.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);
						if (living.EffectList.GetOfType(typeof(ChargeEffect)) == null && living.EffectList.GetOfType(typeof(SpeedOfSoundEffect)) != null)
						{
							living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
							m_rootExpire = new RegionTimer(living, new RegionTimerCallback(RootExpires), duration);
							GameEventMgr.AddHandler(living, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
							SendUpdates(living);
						}
						foreach (GamePlayer player in caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.Out.SendSpellEffectAnimation(caster, (caster.TargetObject as GameLiving), 7029, 0, false, 1);
						}
						foreach (GameNPC mob in caster.TargetObject.GetNPCsInRadius(500))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(caster, mob, true))
							{
								(mob as GameLiving).TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);
								if (mob.EffectList.GetOfType(typeof(ChargeEffect)) == null && mob.EffectList.GetOfType(typeof(SpeedOfSoundEffect)) == null)
								{
									(mob as GameLiving).BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
									m_rootExpire = new RegionTimer(mob, new RegionTimerCallback(RootExpires), duration);
									GameEventMgr.AddHandler(mob, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
									SendUpdates(mob);
								}
								caster.Out.SendMessage("You hit the " + mob.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
								foreach (GamePlayer player2 in caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
								{
									player2.Out.SendSpellEffectAnimation(caster, mob, 7029, 0, false, 1);
								}
							}
						}
						foreach (GamePlayer aeplayer in caster.TargetObject.GetPlayersInRadius(500))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(caster, aeplayer, true))
							{
								(aeplayer as GameLiving).TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);
								if (aeplayer.EffectList.GetOfType(typeof(ChargeEffect)) == null && aeplayer.EffectList.GetOfType(typeof(SpeedOfSoundEffect)) == null)
								{
									(aeplayer as GameLiving).BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
									m_rootExpire = new RegionTimer(aeplayer, new RegionTimerCallback(RootExpires), duration);
									GameEventMgr.AddHandler(aeplayer, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
									SendUpdates(aeplayer);
								}
								caster.Out.SendMessage("You hit " + aeplayer.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
								foreach (GamePlayer player3 in caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
								{
									player3.Out.SendSpellEffectAnimation(caster, aeplayer, 7029, 0, false, 1);
								}
							}
						}
					}
					else
					{
						caster.Out.SendMessage(caster.TargetObject + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
				else
				{
					caster.Out.SendMessage(caster.TargetObject + " is not in view.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				caster.Out.SendMessage("You cant attack a friendly NPC.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}
			timer.Stop();
			timer = null;
			return 0;
		}
		protected virtual int RootExpires(RegionTimer timer)
		{
			GameLiving living = timer.Owner as GameLiving;
			if (living != null)
			{
				living.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
				SendUpdates(living);
			}
			timer.Stop();
			timer = null;
			return 0;
		}
		/// <summary>
		/// Sends updates on effect start/stop
		/// </summary>
		/// <param name="owner"></param>
		protected static void SendUpdates(GameLiving owner)
		{
			if (owner.IsMezzed || owner.IsStunned)
				return;

			GamePlayer player = owner as GamePlayer;
			if (player != null)
				player.Out.SendUpdateMaxSpeed();

			GameNPC npc = owner as GameNPC;
			if (npc != null)
			{
				int maxSpeed = npc.MaxSpeed;
				if (npc.CurrentSpeed > maxSpeed)
					npc.CurrentSpeed = maxSpeed;
			}
		}
		/// <summary>
		/// Handles attack on buff owner
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
			GameLiving living = sender as GameLiving;
			if (attackArgs == null) return;
			if (living == null) return;

			switch (attackArgs.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
					living.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
					SendUpdates(living);
					break;
			}
		}
		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
