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
	public class PerfectRecoveryAbility : TimedRealmAbility
	{
		public PerfectRecoveryAbility(DBAbility dba, int level) : base(dba, level) { }
		private Int32 m_resurrectValue = 5;
		private const String RESURRECT_CASTER_PROPERTY = "RESURRECT_CASTER";
		private RegionTimer m_expireTimer;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;
			if (player.TargetObject == null || !(player.TargetObject is GamePlayer) || player.TargetObject.Realm != player.Realm || (player.TargetObject.Realm == player.Realm && (player.TargetObject as GameLiving).IsAlive))
			{
				player.Out.SendMessage("You have to target a dead member of your realm!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			GamePlayer targetPlayer = player.TargetObject as GamePlayer;

			switch (Level)
			{
				case 2: m_resurrectValue = 20; break;
				case 3: m_resurrectValue = 100; break;
			}
			GameLiving resurrectionCaster = targetPlayer.TempProperties.getObjectProperty(RESURRECT_CASTER_PROPERTY, null) as GameLiving;
			if (resurrectionCaster != null)
			{
				player.Out.SendMessage("Your target is already considering a resurrection!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			if (WorldMgr.GetDistance(player, targetPlayer) > 1500 * player.GetModified(eProperty.SpellRange) * 0.01)
			{
				player.Out.SendMessage("You are too far away from your target to use this ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			if (targetPlayer != null)
			{
				SendCasterSpellEffectAndCastMessage(living, 7019, true);
				DisableSkill(living);
				ResurrectLiving(targetPlayer, player);
			}
		}
		public void ResurrectLiving(GamePlayer resurrectedPlayer, GameLiving rezzer)
		{
			if (rezzer.ObjectState != GameObject.eObjectState.Active)
				return;
			if (rezzer.CurrentRegionID != resurrectedPlayer.CurrentRegionID)
				return;
			resurrectedPlayer.Health = (int)(resurrectedPlayer.MaxHealth * m_resurrectValue / 100);
			resurrectedPlayer.Mana = (int)(resurrectedPlayer.MaxMana * m_resurrectValue / 100);
			resurrectedPlayer.Endurance = (int)(resurrectedPlayer.MaxEndurance * m_resurrectValue / 100); //no endurance after any rez
			resurrectedPlayer.MoveTo(rezzer.CurrentRegionID, rezzer.X, rezzer.Y, rezzer.Z, rezzer.Heading);
			if (m_expireTimer != null)
			{
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
			resurrectedPlayer.StopReleaseTimer();
			resurrectedPlayer.Out.SendPlayerRevive(resurrectedPlayer);
			resurrectedPlayer.UpdatePlayerStatus();

			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(resurrectedPlayer, "PvEResurrectionIllness");
			if (effect != null)
				effect.Cancel(false);
			resurrectedPlayer.Out.SendMessage("You have been resurrected by " + rezzer.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		public override int GetReUseDelay(int level)
		{
			return 300;
		}
	}
}

