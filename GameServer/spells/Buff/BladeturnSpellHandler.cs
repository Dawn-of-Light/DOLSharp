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
using System.Collections.Concurrent;
using System.Reflection;

using DOL.GS.PacketHandler;
using DOL.GS.Effects;

using DOL.Events;
using DOL.Language;
 
namespace DOL.GS.Spells
{
	/// <summary>
	/// Disrupt a Physical Attack Landing on the Effect Owner.
	/// </summary>
	[SpellHandlerAttribute("Bladeturn")]
	public class BladeturnSpellHandler : AttackModifierBuffSpellHandler
	{
		/// <summary>
		/// Does this Bladeturn works on Range Attack only ?
		/// </summary>
		protected virtual bool RangeOnly
		{
			get { return false; }
		}
		
		/// <summary>
		/// Execute Spell and Consume Power.
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// Send Messages
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			
			eChatType toLiving = (!Spell.IsPulsing) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
			eChatType toOther = (!Spell.IsPulsing) ? eChatType.CT_System : eChatType.CT_SpellPulse;
			
			MessageToLiving(effect.Owner, Spell.Message1, toLiving);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
		}

		/// <summary>
		/// Send Messages
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (!noMessages && !Spell.IsPulsing) 
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			
			return base.OnEffectExpires(effect, noMessages);
		}

		/// <summary>
		/// On Player Attacked Change State of Attack
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnAttackedByEnemy(DOLEvent e, object sender, EventArgs args)
		{
			if (args == null || !(args is AttackedByEnemyEventArgs))
				return;
			
			AttackData ad = ((AttackedByEnemyEventArgs)args).AttackData;
			if (ad == null)
				return;
			
			
			GameSpellEffect bladeturn = SpellHelper.FindEffectOnTarget(ad.Target, this);
			if (bladeturn == null)
				return;
			
			bool isHit = ad.AttackResult == eAttackResult.HitStyle || ad.AttackResult == eAttackResult.HitUnstyled;
			
			// Critical Shot, no change, no BT break.
			if (ad.SpellHandler is ArcheryHandler && ((ArcheryHandler)ad.SpellHandler).ShotType == ArcheryHandler.eShotType.Critical && isHit)
			{
				HitPenetrateBladeturn(ad.Target);
				return;
			}
			
			// Power Shot, no change, BT break.
			if (ad.SpellHandler is ArcheryHandler && ((ArcheryHandler)ad.SpellHandler).ShotType == ArcheryHandler.eShotType.Power && isHit)
			{
				HitPenetrateBladeturn(ad.Target);
				bladeturn.Cancel(false);
				return;
			}
			
			// Critical Strike with Stealth Requirement Breaks BT
			if (ad.Style != null && ad.Style.StealthRequirement && ad.AttackResult == eAttackResult.HitStyle)
			{
				HitPenetrateBladeturn(ad.Target);
				bladeturn.Cancel(false);
				return;
			}
			
			// Old Archery
			if (ad.Attacker.RangedAttackType == GameLiving.eRangedAttackType.Critical && isHit)
			{
				HitPenetrateBladeturn(ad.Target);
				bladeturn.Cancel(false);
				return;
			}
			
			// Old Penetrating arrows
			if (ad.AttackType == AttackData.eAttackType.Ranged && isHit && ad.Target != bladeturn.SpellHandler.Caster && ad.Attacker.HasAbility(Abilities.PenetratingArrow))
			{
				int penetratingLevel = ad.Attacker.GetAbilityLevel(Abilities.PenetratingArrow);
				if (penetratingLevel > 0)
				{
					HitPenetrateBladeturn(ad.Target);
					bladeturn.Cancel(false);
					ad.Damage = (int)(ad.Damage * 0.25 * penetratingLevel);
					ad.CriticalDamage = (int)(ad.CriticalDamage * 0.25 * penetratingLevel);
					return;
				}
			}
			
			/*
			 * http://www.camelotherald.com/more/31.shtml
			 * - Bladeturns can now be penetrated by attacks from higher level monsters and
			 * players. The chance of the bladeturn deflecting a higher level attack is
			 * approximately the caster's level / the attacker's level.
			 * Please be aware that everything in the game is
			 * level/chance based - nothing works 100% of the time in all cases.
			 * It was a bug that caused it to work 100% of the time - now it takes the
			 * levels of the players involved into account.
			 */
			if ((((ad.IsMeleeAttack || ad.AttackType == AttackData.eAttackType.Ranged || ad.SpellHandler is ArcheryHandler) && !RangeOnly) 
			     || ((ad.AttackType == AttackData.eAttackType.Ranged || ad.SpellHandler is BoltSpellHandler) && RangeOnly))
			    && isHit)
			{
				if(!Util.ChanceDouble((double)(bladeturn.SpellHandler.Caster.Level + bladeturn.SpellHandler.Caster.GetModified(eProperty.BladeturnReinforcement)) / (double)ad.Attacker.Level))
				{
					HitPenetrateBladeturn(ad.Target);
				}
				else
				{
					if (ad.Target is GamePlayer)
						((GamePlayer)ad.Target).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					if (ad.Attacker is GamePlayer)
						((GamePlayer)ad.Attacker).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Attacker).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					ad.AttackResult = eAttackResult.Missed;
					ad.Damage = 0;
					ad.CriticalDamage = 0;
				}
				
				bladeturn.Cancel(false);
			}
		}
		
		/// <summary>
		/// Print Penetrate Bladeturn Messages
		/// </summary>
		private void HitPenetrateBladeturn(GameLiving target)
		{
			if (target is GamePlayer)
			{
				MessageToLiving(target, LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowPenetrated"), eChatType.CT_SpellResisted);
			}
		}
		
		// constructor
		public BladeturnSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}

	
	/// <summary>
	/// Disrupt a Physical Attack Landing on the Effect Owner with immunity timer.
	/// </summary>
	[SpellHandlerAttribute("PulsingBladeturn")]
	public class PulsingBladeturnSpellHandler : BladeturnSpellHandler
	{
		/// <summary>
		/// Dictionary containing startTime of PBT.
		/// </summary>
		private ConcurrentDictionary<GameSpellEffect, long> m_effectStartTime = new ConcurrentDictionary<GameSpellEffect, long>();
		
		private const int PULSINGBT_IMMUNITY = 3000;
		
		/// <summary>
		/// Overwrited by this type of SpellHandler
		/// </summary>
		protected virtual Type OverWriteBaseType
		{
			get { return typeof(BladeturnSpellHandler); }
		}
		
		/// <summary>
		/// Register PBT Start Time.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			if (m_effectStartTime.ContainsKey(effect))
			{
				long dummy;
				m_effectStartTime.TryRemove(effect, out dummy);
				m_effectStartTime.TryAdd(effect, effect.Owner.CurrentRegion.Time);
			}
			else
			{
				m_effectStartTime.TryAdd(effect, effect.Owner.CurrentRegion.Time);
			}
			
			base.OnEffectStart(effect);
		}
		
		/// <summary>
		/// Return remaining immunity time
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			
			if (m_effectStartTime.ContainsKey(effect))
			{
				long startTime;
				if (m_effectStartTime.TryRemove(effect, out startTime))
				{
					return (int)Math.Max(0, PULSINGBT_IMMUNITY - (effect.Owner.CurrentRegion.Time - startTime));
				}
			}
			
			return 0;
		}
		
		/// <summary>
		/// This is overwritable by Bladeturn
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (compare.SpellHandler.GetType() == OverWriteBaseType)
				return true;
			    
			return base.IsOverwritable(compare);
		}
		
		/// <summary>
		/// Check if we try to overwrite during immunity
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <param name="neweffect"></param>
		/// <returns></returns>
		public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			if (oldeffect.SpellHandler.GetType() == GetType())
			{
				// PBT is immune to PBT only
				if (oldeffect.ImmunityState)
					return false;
				
				if(oldeffect.SpellHandler is PulsingBladeturnSpellHandler && !((PulsingBladeturnSpellHandler)oldeffect.SpellHandler).IsApplyImmuneExpired(oldeffect))
					return false;
			}
			
			return base.IsNewEffectBetter(oldeffect, neweffect);
		}
		
		/// <summary>
		/// Check if the PBT is still "immune" while active
		/// </summary>
		/// <param name="effect"></param>
		/// <returns>true if expired</returns>
		private bool IsApplyImmuneExpired(GameSpellEffect effect)
		{
			if (m_effectStartTime.ContainsKey(effect))
			{
				long startTime = m_effectStartTime[effect];
				return PULSINGBT_IMMUNITY <= (effect.Owner.CurrentRegion.Time - startTime);
			}
			
			return true;
		}
		
		// constructor
		public PulsingBladeturnSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
