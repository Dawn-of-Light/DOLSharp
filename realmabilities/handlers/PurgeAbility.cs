using System;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Purge Ability, removes negative effects
	/// </summary>
	public class PurgeAbility : TimedRealmAbility
	{
		public PurgeAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING)) return;

			if (Level < 2)
			{
				PurgeTimer timer = new PurgeTimer(living, this);
				timer.Interval = 1000;
				timer.Start(1);
				DisableSkill(living);
			}
			else
			{
				SendCastMessage(living);
				if (RemoveNegativeEffects(living, this))
				{
					DisableSkill(living);
				}
			}
		}

		protected static bool RemoveNegativeEffects(GameLiving living, PurgeAbility purge)
		{
			bool removed = false;
			ArrayList effects = new ArrayList();
			lock (living.EffectList)
			{
				foreach (IGameEffect effect in living.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					if (gsp == null)
						continue;
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue; // ignore immunity effects
					if (gsp.SpellHandler.HasPositiveEffect)//only enemy spells are affected
						continue;
					/*
					if (gsp.SpellHandler is RvRResurrectionIllness)
						continue;
					 */
					//if (gsp.Spell.SpellType == "DesperateBowman")//Can't be purged
					//continue;
					effects.Add(gsp);
					removed = true;
				}

				foreach (IGameEffect effect in effects)
				{
					effect.Cancel(false);
				}
			}

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				foreach (GamePlayer rangePlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					rangePlayer.Out.SendSpellEffectAnimation(player, player, 7011, 0, false, (byte)(removed ? 1 : 0));
				}
				if (removed)
				{
					player.Out.SendMessage("All negative effects fall from you.", eChatType.CT_Advise, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.DisableSkill(purge, 5);
				}
			}
			return removed;
		}

		protected class PurgeTimer : GameTimer
		{
			GameLiving m_caster;
			PurgeAbility m_purge;
			int counter;

			public PurgeTimer(GameLiving caster, PurgeAbility purge)
				: base(caster.CurrentRegion.TimeManager)
			{
				m_caster = caster;
				m_purge = purge;
				counter = 5;
			}
			protected override void OnTick()
			{
				if (!m_caster.IsAlive)
				{
					Stop();
					if (m_caster is GamePlayer)
					{
						((GamePlayer)m_caster).DisableSkill(m_purge, 0);
					}
					return;
				}
				if (counter > 0)
				{
					GamePlayer player = m_caster as GamePlayer;
					if (player != null)
					{
						player.Out.SendMessage("Purge activates in " + counter + " seconds!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					counter--;
					return;
				}
				m_purge.SendCastMessage(m_caster);
				RemoveNegativeEffects(m_caster, m_purge);
				Stop();
			}
		}

		public override int GetReUseDelay(int level)
		{
			return (level < 3) ? 900 : 300;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Level 1 of this ability has a five second delay when triggering this ability before it activates. Higher levels of Purge do not have this delay.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Casting time: instant");
		}
	}
}