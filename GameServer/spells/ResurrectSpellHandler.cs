/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Resurrect")]
	public class ResurrectSpellHandler : SpellHandler
	{
		private const string RESURRECT_CASTER_PROPERTY = "RESURRECT_CASTER";
		protected readonly ListDictionary m_resTimersByLiving = new ListDictionary();

		// constructor
		public ResurrectSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			if(target == null || target.IsAlive) return;

			SendEffectAnimation(target, 0, false, 1);
			GamePlayer targetPlayer = target as GamePlayer;
			if (targetPlayer == null)
			{
				//not a player
				ResurrectLiving(target);
			}
			else
			{
				targetPlayer.TempProperties.setProperty(RESURRECT_CASTER_PROPERTY, m_caster);
                RegionTimer resurrectExpiredTimer = new RegionTimer(targetPlayer);
				resurrectExpiredTimer.Callback = new RegionTimerCallback(ResurrectExpiredCallback);
				resurrectExpiredTimer.Properties.setProperty("targetPlayer", targetPlayer);
				resurrectExpiredTimer.Start(15000);
				lock (m_resTimersByLiving.SyncRoot)
				{
					m_resTimersByLiving.Add(target, resurrectExpiredTimer);
				}

				//send resurrect dialog
				targetPlayer.Out.SendCustomDialog("Do you allow " + m_caster.GetName(0, true) + " to resurrected you\nwith " + m_spell.ResurrectHealth + " percent hits?", new CustomDialogResponse(ResurrectResponceHandler));
			}
		}

		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int CalculateNeededPower(GameLiving target)
		{
			float factor = Math.Max (0.1f, 0.5f + (target.Level - m_caster.Level) / (float)m_caster.Level);

			//DOLConsole.WriteLine("res power needed: " + (int) (m_caster.MaxMana * factor) + "; factor="+factor);
			return (int) (m_caster.MaxMana * factor);
		}

		/// <summary>
		/// Resurrects target if it accepts
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		protected virtual void ResurrectResponceHandler(GamePlayer player, byte response)
		{
			//DOLConsole.WriteLine("resurrect responce: " + response);
			GameTimer resurrectExpiredTimer = null;
			lock (m_resTimersByLiving.SyncRoot)
			{
				resurrectExpiredTimer = (GameTimer)m_resTimersByLiving[player];
				m_resTimersByLiving.Remove(player);
			}
			if (resurrectExpiredTimer != null)
			{
				resurrectExpiredTimer.Stop();
			}

			GameLiving rezzer = (GameLiving)player.TempProperties.getObjectProperty(RESURRECT_CASTER_PROPERTY, null);
			if (!player.IsAlive)
			{
				if (rezzer == null)
				{
					player.Out.SendMessage("No one is currently trying to resurrect you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					if (response == 1)
					{
						ResurrectLiving(player); //accepted
						//VaNaTiC->
						#warning VaNaTiC: add this in GamePlayer.OnRevive with my RevivedEventArgs
						// Patch 1.56: Resurrection sickness now goes from 100% to 50% when doing a "full rez" on another player. 
						// We have do to this here, cause we dont have any other chance to get
						// an object-relation between the casted spell of the rezzer (here) and
						// the produced illness for the player (OnRevive()).
						// -> any better solution -> post :)
						if ( Spell.ResurrectHealth == 100 )
						{
							GameSpellEffect effect = SpellHandler.FindEffectOnTarget(player, GlobalSpells.PvERessurectionIllnessSpellType);
				            if ( effect != null )
				            	effect.Overwrite(new GameSpellEffect(effect.SpellHandler, effect.Duration / 2, effect.PulseFreq));
						}
						//VaNaTiC<-
					}
					else
					{
						player.Out.SendMessage("You decline to be resurrected.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						//refund mana
						m_caster.Mana += CalculateNeededPower(player);
					}
				}
			}
			player.TempProperties.removeProperty(RESURRECT_CASTER_PROPERTY);
		}

		/// <summary>
		/// Resurrects living
		/// </summary>
		/// <param name="living"></param>
		protected virtual void ResurrectLiving(GameLiving living)
		{
			if (m_caster.ObjectState != GameObject.eObjectState.Active) return;
			if (m_caster.CurrentRegionID != living.CurrentRegionID) return;

			living.Health = living.MaxHealth * m_spell.ResurrectHealth / 100;
			living.Mana = living.MaxMana * m_spell.ResurrectMana / 100;
			living.Endurance = 0; //no endurance after any rez
			living.MoveTo(m_caster.CurrentRegionID, m_caster.X, m_caster.Y, m_caster.Z, m_caster.Heading);

			GameTimer resurrectExpiredTimer = null;
			lock (m_resTimersByLiving.SyncRoot)
			{
				resurrectExpiredTimer = (GameTimer)m_resTimersByLiving[living];
				m_resTimersByLiving.Remove(living);
			}
			if (resurrectExpiredTimer != null)
			{
				resurrectExpiredTimer.Stop();
			}

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				player.StopReleaseTimer();
				player.Out.SendPlayerRevive(player);
				player.UpdatePlayerStatus();
				player.Out.SendMessage("You have been resurrected by " + m_caster.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Notify(GamePlayerEvent.Revive, player, new RevivedEventArgs(Caster, Spell));

				IList attackers;
				lock (player.Attackers.SyncRoot) { attackers = (IList)(player.Attackers as ArrayList).Clone(); }

				foreach (GameObject attacker in attackers)
				{
					if (attacker is GameLiving && attacker != living.TargetObject)
						((GameLiving)attacker).Notify(
							GameLivingEvent.EnemyHealed,
							(GameLiving)attacker,
							new EnemyHealedEventArgs(living, m_caster, GameLiving.eHealthChangeType.Spell, living.Health));
				}

				GamePlayer casterPlayer = Caster as GamePlayer;
				if (casterPlayer != null)
				{
					long rezRps = player.LastDeathRealmPoints * (Spell.ResurrectHealth + 50) / 1000;
					if (rezRps > 0)
					{
						casterPlayer.GainRealmPoints(rezRps);
					}
					else
					{
						casterPlayer.Out.SendMessage("The player you resurrected was not worth realm points on death.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						casterPlayer.Out.SendMessage("You thus get no realm points for the resurrect.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Cancels resurrection after some time
		/// </summary>
		/// <param name="callingTimer"></param>
		/// <returns></returns>
		protected virtual int ResurrectExpiredCallback(RegionTimer callingTimer)
		{
			GamePlayer player = (GamePlayer)callingTimer.Properties.getObjectProperty("targetPlayer", null);
			if (player == null) return 0;
			player.TempProperties.removeProperty(RESURRECT_CASTER_PROPERTY);
			player.Out.SendMessage("Your resurrection spell has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 0;
		}

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving target)
		{
			if (!base.CheckBeginCast(target))
				return false;

			GameLiving resurrectionCaster = target.TempProperties.getObjectProperty(RESURRECT_CASTER_PROPERTY, null) as GameLiving;
			if (resurrectionCaster != null)
			{
				//already considering resurrection - do nothing
				MessageToCaster("Your target is already considering a resurrection!", eChatType.CT_SpellResisted);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks after casting before spell is executed
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool CheckEndCast(GameLiving target)
		{
			GameLiving resurrectionCaster = target.TempProperties.getObjectProperty(RESURRECT_CASTER_PROPERTY, null) as GameLiving;
			if (resurrectionCaster != null)
			{
				//already considering resurrection - do nothing
				MessageToCaster("Your target is already considering a resurrection!", eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckEndCast(target);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Revive>
				Function: raise dead
 
				Brings target back to life, restores a portion of their health 
				and power and eliminates the experience penalty and con loss they 
				would have suffered were they to have /release.
 
				Health restored: 10
				Target: Dead
				Range: 1500
				Casting time: 4.0 sec
 
				<End Info>
				*/

				ArrayList list = new ArrayList();

				list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("Health restored: " + Spell.ResurrectHealth);
				if(Spell.ResurrectMana != 0) list.Add("Mana restored: " + Spell.ResurrectMana);
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
			
				return list;
			}
		}
	}
}
