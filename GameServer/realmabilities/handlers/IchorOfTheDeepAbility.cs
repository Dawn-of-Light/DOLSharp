using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using DOL.Events;
using DOL.Database;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	public class IchorOfTheDeepAbility : TimedRealmAbility
	{
		public IchorOfTheDeepAbility(DBAbility dba, int level) : base(dba, level) { }

		private RegionTimer m_expireTimerID;
		private RegionTimer m_rootExpire;
		private int dmgValue = 0;
		private int duration = 0;
		private GamePlayer caster;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			caster = living as GamePlayer;
			if (caster == null)
				return;

			if (caster.TargetObject == null)
			{
				caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			if (!caster.TargetInView)
			{
				caster.Out.SendMessage(caster.TargetObject.Name + " is not in view.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			if (!caster.IsWithinRadius( caster.TargetObject, 1875 ))
			{
				caster.Out.SendMessage(caster.TargetObject.Name + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			if (m_expireTimerID != null && m_expireTimerID.IsAlive)
			{
				caster.Out.SendMessage("You are already casting this ability.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
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

			#region resist and det
			GameLiving m_target = caster.TargetObject as GameLiving;

			int primaryResistModifier = m_target.GetResist(eDamageType.Spirit);
			int secondaryResistModifier = m_target.SpecBuffBonusCategory[(int)eProperty.Resist_Spirit];
			int rootdet = ((m_target.GetModified(eProperty.SpeedDecreaseDurationReduction) - 100) * -1);

			int ResistModifier = 0;
			ResistModifier += (int)((dmgValue * (double)primaryResistModifier) * -0.01);
			ResistModifier += (int)((dmgValue + (double)ResistModifier) * (double)secondaryResistModifier * -0.01);


			if (m_target is GamePlayer)
			{
				dmgValue += ResistModifier;
			}
			if (m_target is GameNPC)
			{
				dmgValue += ResistModifier;
			}
			
			int rootmodifier = 0;
			rootmodifier += (int)((duration * (double)primaryResistModifier) * -0.01);
			rootmodifier += (int)((duration + (double)primaryResistModifier) * (double)secondaryResistModifier * -0.01);
			rootmodifier += (int)((duration + (double)rootmodifier) * (double)rootdet * -0.01);
			
			duration += rootmodifier;

			if (duration < 1)
				duration = 1;
			#endregion


			foreach (GamePlayer i_player in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == caster) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				else i_player.Out.SendMessage(caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

				i_player.Out.SendSpellCastAnimation(caster, 7029, 20);
			}

			m_expireTimerID = new RegionTimer(caster, new RegionTimerCallback(EndCast), 2000);
		}

		protected virtual int EndCast(RegionTimer timer)
		{
			if (caster.TargetObject == null)
			{
				caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return 0;
			}

			if (caster.IsMoving)
			{
                caster.Out.SendMessage(LanguageMgr.GetTranslation(caster.Client, "SpellHandler.CasterMove"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3000);
				return 0;
			}

			if ( !caster.IsWithinRadius( caster.TargetObject, 1875 ) )
			{
				caster.Out.SendMessage(caster.TargetObject.Name + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return 0;
			}

			GameLiving living = caster.TargetObject as GameLiving;
			
			if(living==null)
			{
				timer.Stop();
				timer = null;
				return 0;
			}

			if (living.EffectList.GetOfType<ChargeEffect>() == null && living.EffectList.GetOfType<SpeedOfSoundEffect>() != null)
			{
				living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
				m_rootExpire = new RegionTimer(living, new RegionTimerCallback(RootExpires), duration);
				GameEventMgr.AddHandler(living, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
				SendUpdates(living);
			}

			foreach (GamePlayer player in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(caster, (caster.TargetObject as GameLiving), 7029, 0, false, 1);
			}

			foreach (GameNPC mob in living.GetNPCsInRadius(500))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(caster, mob, true))
					continue;

				if (mob.HasAbility(Abilities.CCImmunity) || mob.HasAbility(Abilities.RootImmunity) || mob.HasAbility(Abilities.DamageImmunity))
					continue;
				
				GameSpellEffect mez = SpellHandler.FindEffectOnTarget(mob, "Mesmerize");
				if (mez != null)
					mez.Cancel(false);
				
				mob.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);

				if (mob.EffectList.GetOfType<ChargeEffect>() == null && mob.EffectList.GetOfType<SpeedOfSoundEffect>() == null)
				{
					mob.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
					m_rootExpire = new RegionTimer(mob, new RegionTimerCallback(RootExpires), duration);
					GameEventMgr.AddHandler(mob, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
					SendUpdates(mob);
				}

				caster.Out.SendMessage("You hit the " + mob.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

				foreach (GamePlayer player2 in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player2.Out.SendSpellEffectAnimation(caster, mob, 7029, 0, false, 1);
				}
			}

			foreach (GamePlayer aeplayer in living.GetPlayersInRadius(500))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(caster, aeplayer, true))
					continue;

				GameSpellEffect mez = SpellHandler.FindEffectOnTarget(aeplayer, "Mesmerize");
				if (mez != null)
					mez.Cancel(false);
				aeplayer.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);
				aeplayer.StartInterruptTimer(3000, AttackData.eAttackType.Spell, caster);

				if (aeplayer.EffectList.GetOfType<ChargeEffect>() == null && aeplayer.EffectList.GetOfType<SpeedOfSoundEffect>() == null)
				{
					(aeplayer as GameLiving).BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 99 * 0.01);
					m_rootExpire = new RegionTimer(aeplayer, new RegionTimerCallback(RootExpires), duration);
					GameEventMgr.AddHandler(aeplayer, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
					SendUpdates(aeplayer);
				}

				caster.Out.SendMessage("You hit " + aeplayer.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

				foreach (GamePlayer player3 in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player3.Out.SendSpellEffectAnimation(caster, aeplayer, 7029, 0, false, 1);
				}
			}

			DisableSkill(caster);
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
				short maxSpeed = npc.MaxSpeed;
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
