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
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.Keeps;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Direct Damage Spell Handler, serves as base for most of Damaging Spells
	/// </summary>
	[SpellHandlerAttribute("DirectDamage")]
	public class DirectDamageSpellHandler : SpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string LOSEFFECTIVENESS = "LOS Effectivness";

		/// <summary>
		/// Source Object For LoS Checking. (Default to Caster)
		/// </summary>
		protected virtual GameObject SourceCheckLoS
		{
			get { return Caster; }
		}
		
		/// <summary>
		/// Execute direct damage spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));

			base.FinishSpellCast(target);
		}
		
		/// <summary>
		/// Direct Damage should run LoS checks Before Applying Effect !
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// don't check LoS for main target, it should be done during casting...
			if (target == SpellCastTarget)
			{
				CheckedApplyEffectOnTarget(target, effectiveness);
			}
			else
			{
				// Check if spell need LoS Check.
				bool spellOK = true;

				if (Spell.Target.ToLower() == "cone" || Spell.IsPBAoE)
				{
					spellOK = false;
				}
				
				// if spell is needing LoS Check or Area need LoS Check
				if (spellOK == false || MustCheckLOS(Caster))
				{
					GamePlayer checkPlayer = null;
					
					if (target is GamePlayer)
					{
						checkPlayer = (GamePlayer)target;
					}
					else
					{
						if (target is GameNPC && ((GameNPC)target).Brain is IControlledBrain)
						{
							checkPlayer = ((IControlledBrain)((GameNPC)target).Brain).GetPlayerOwner();
						}
						
						if (checkPlayer == null && Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
						{
							checkPlayer = ((IControlledBrain)((GameNPC)Caster).Brain).GetPlayerOwner();
						}
						
						if (checkPlayer == null && Caster is GamePlayer)
						{
							checkPlayer = (GamePlayer)Caster;
						}
					}
					
					if (checkPlayer != null)
					{
	                    target.TempProperties.setProperty(LOSEFFECTIVENESS + target.ObjectID, effectiveness);
						checkPlayer.Out.SendCheckLOS(SourceCheckLoS, target, new CheckLOSResponse(ApplyEffectCheckLOS));
					}
					else
					{
						CheckedApplyEffectOnTarget(target, effectiveness);
					}
				}
				else
				{
					CheckedApplyEffectOnTarget(target, effectiveness);
				}
			}
		}
		
		/// <summary>
		/// Get LoS Check Result and continue applying effect.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		/// <param name="targetOID"></param>
		protected virtual void ApplyEffectCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if (Caster == null || Caster.ObjectState != GameObject.eObjectState.Active)
				return;

			if ((response & 0x100) == 0x100)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
					{
                        double effectiveness = target.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);
                        target.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);
                        
                        CheckedApplyEffectOnTarget(target, effectiveness);
                        
						// Due to LOS check delay the actual cast happens after FinishSpellCast does a notify, so we notify again
						GameEventMgr.Notify(GameLivingEvent.CastFinished, Caster, new CastingEventArgs(this, target, GetLastAttackData(target)));
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e);
				}
			}
			else
			{
				if (Spell.Target.ToLower() == "enemy" && Spell.Radius == 0 && Spell.Range != 0)
				{
					MessageToCaster("You can't see your target!", eChatType.CT_SpellResisted);
				}
			}
		}

		/// <summary>
		/// Direct Damage Specific ApplyEffectOnTarget after LoS Check
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public virtual void CheckedApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
		}
		
		/// <summary>
		/// execute direct effect
		/// </summary>
		/// <param name="target">target that gets the damage</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			AttackData ad = DealDamage(target, effectiveness);
			
			// Interrupt only on Direct Effect with a successful attack
			if(ad != null && ad.IsHit)
				target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
		}


		/// <summary>
		/// Calculates the base 100% spell damage which is then modified by damage variance factors
		/// </summary>
		/// <returns></returns>
		public override double CalculateDamageBase(GameLiving target)
		{
			GamePlayer player = Caster as GamePlayer;

			// % damage procs
			if (Spell.Damage < 0)
			{
				double spellDamage = 0;

				if (player != null)
				{
					// This equation is used to simulate live values - Tolakram
					spellDamage = Math.Max(0, (target.MaxHealth * Spell.Damage * -0.01) * 0.4);
				}

				return spellDamage;
			}

			return base.CalculateDamageBase(target);
		}

		/// <summary>
		/// Damage Cap for percent based spells.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override double DamageCap(GameLiving target, double effectiveness)
		{
			if (Spell.Damage < 0)
			{
				return Math.Max(0, (target.MaxHealth * Spell.Damage * -0.01) * effectiveness);
			}

			return base.DamageCap(target, effectiveness);
		}

		/// <summary>
		/// Specific DealDamage for delayed damage.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		protected virtual AttackData DealDamage(GameLiving target, double effectiveness)
		{
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			
			if (target == null || !target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active)
			{
				ad.AttackResult = eAttackResult.TargetDead;
				return ad;
			}

			// calc damage
			DamageTarget(ad, true);
			return ad;
		}

		/// <summary>
		/// We need to send resist spell los check packets because spell resist is calculated first, and
		/// so you could be inside keep and resist the spell and be interupted when not in view
		/// </summary>
		/// <param name="target"></param>
		protected override void OnSpellResisted(GameLiving target)
		{
			GamePlayer losChecker = null;
			
			// If target is the cast target or spell don't need los check
			if(target != SpellCastTarget || (Spell.Target.ToLower() == "cone" || Spell.IsPBAoE))
			{
				if (target is GamePlayer)
				{
					losChecker = (GamePlayer)target;
					losChecker.Out.SendCheckLOS(SourceCheckLoS, target, new CheckLOSResponse(ResistSpellCheckLOS));
				}
				else
				{
					if (target is GameNPC && ((GameNPC)target).Brain is IControlledBrain)
					{
						losChecker = ((IControlledBrain)((GameNPC)target).Brain).GetPlayerOwner();
					}
					
					if (losChecker == null && Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
					{
						losChecker = ((IControlledBrain)((GameNPC)Caster).Brain).GetPlayerOwner();
					}
					
					if (losChecker == null && Caster is GamePlayer)
					{
						losChecker = (GamePlayer)Caster;
					}
					
					if (losChecker != null)
					{
						losChecker.Out.SendCheckLOS(SourceCheckLoS, target, new CheckLOSResponse(ResistSpellCheckLOS));
					}
					else
					{
						SpellResisted(target);
					}
				}
			}
			else
			{
				SpellResisted(target);
			}
		}

		/// <summary>
		/// Get resist LoS Check answer
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		/// <param name="targetOID"></param>
		private void ResistSpellCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			// If LoS OK continue onResist...
			if ((response & 0x100) == 0x100)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
						SpellResisted(target);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
		}

		/// <summary>
		/// Override to call the Base SpellResist Message and Events
		/// </summary>
		/// <param name="target"></param>
		protected void SpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);
		}

		/// <summary>
		/// Remove Damage Immune Targets from Target List.
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public override IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			IList<GameLiving> targets = base.SelectTargets(castTarget);
			
			List<GameLiving> browseTargets = new List<GameLiving>(targets);
			
			foreach (GameLiving trgt in browseTargets)
			{
				GameLiving target = trgt;
				
				if (target.HasAbility(Abilities.DamageImmunity))
				{
					targets.Remove(target);
				}
			}
			
			return targets;
		}

		/// <summary>
		/// Override Casting Check for GTAE DD that can be cast without LoS
		/// </summary>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <param name="needViewRealm"></param>
		/// <param name="needViewOffensive"></param>
		/// <param name="notify"></param>
		/// <returns></returns>
		public override bool CheckCastingTarget(ref GameLiving target, bool quiet = false, bool needViewRealm = false, bool needViewOffensive = true, bool notify = true)
		{
			// Check GT Area spell
			if (Spell.Target.ToLower().Equals("area"))
			{
				target = Caster;
				
				if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
				{
					if (!quiet)
						MessageToCaster("Your ground target is out of range. Select a closer target.", eChatType.CT_SpellResisted);
					
					if (notify)
						Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetTooFarAway));
					
					return false;
				}
				
				if(ServerProperties.Properties.SPELL_GTAE_NEED_LOS)
				{
					if (!Caster.GroundTargetInView)
					{
						if (!quiet)
							MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
						
						if (notify)
							Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
						
						return false;
					}
				}
				
				return true;
			}

			return base.CheckCastingTarget(ref target, quiet, needViewRealm, needViewOffensive, notify);
		}
		
		// constructor
		public DirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
