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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Damage Over Time spell handler
	/// </summary>
	[SpellHandlerAttribute("DamageOverTime")]
	public class DoTSpellHandler : DirectDamageSpellHandler
	{
		/// <summary>
		/// No Area variance for DOT spells
		/// </summary>
		/// <param name="target"></param>
		/// <param name="distance"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		protected override double CalculateAreaVariance(GameLiving target, int distance, int radius)
		{
			return 0;
		}

		/// <summary>
		/// Determines wether this spell is compatible with given spell
		/// and therefore overwritable by better versions
		/// spells that are overwritable cannot stack
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			return GetType() == compare.SpellHandler.GetType() && Spell.DamageType == compare.Spell.DamageType && SpellLine.IsBaseLine == compare.SpellHandler.SpellLine.IsBaseLine;
		}

		/// <summary>
		/// Calculates damage to target with resist chance and stores it in ad
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="effectiveness">value from 0..1 to modify damage</param>
		/// <returns>attack data</returns>
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
			
			// Viper Ability.
            if (SpellLine.KeyName == GlobalSpellsLines.Mundane_Poisons)
            {
                RealmAbilities.L3RAPropertyEnhancer ra = Caster.GetAbility<RealmAbilities.ViperAbility>();
				if (ra != null)
				{
					int additional = (int)(ad.Damage * ra.Amount * 0.01);
					ad.Damage += additional;
				}
            }
            
			// TODO Change this to event based effect.            
			GameSpellEffect iWarLordEffect = SpellHelper.FindEffectOnTarget(target, "CleansingAura");
			if (iWarLordEffect != null)
				ad.Damage *= (int)(1.00 - (iWarLordEffect.Spell.Value * 0.01));
                       
			return ad;
		}

		/// <summary>
		/// Calculates min damage variance %
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="min">returns min variance</param>
		/// <param name="max">returns max variance</param>
		public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			// Poison special variance (no overspec)
			if (SpellLine.KeyName == GlobalSpellsLines.Mundane_Poisons)
			{
				int speclevel = Caster.GetModifiedSpecLevel(SpellLine.Spec);
				max = 1.25;
				min = Math.Min(max, 0.25 + Math.Max(0.0, (speclevel - 1)) / Math.Max(1.0, Spell.Level));
				return;
			}
			
			base.CalculateDamageVariance(target, out min, out max);
		}

		/// <summary>
		/// Sends damage text messages but makes no damage
		/// </summary>
		/// <param name="ad"></param>
		public override void SendDamageMessages(AttackData ad)
		{
			// Graveen: only GamePlayer should receive messages :p
			GamePlayer PlayerReceivingMessages = null;
			
			if (Caster is GamePlayer)
				PlayerReceivingMessages = Caster as GamePlayer;
			
            if (Caster is GameNPC)
                if ((Caster as GameNPC).Brain is IControlledBrain)
                    PlayerReceivingMessages = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
            
            if (PlayerReceivingMessages == null) 
                return;

			MessageToCaster(String.Format(LanguageMgr.GetTranslation(PlayerReceivingMessages.Client, "DoTSpellHandler.SendDamageMessages.YourHitsFor", Spell.Name, ad.Target.GetName(0, false), ad.Damage)), eChatType.CT_YouHit);
            
            if (ad.CriticalDamage > 0)
                MessageToCaster(String.Format(LanguageMgr.GetTranslation(PlayerReceivingMessages.Client, "DoTSpellHandler.SendDamageMessages.YourCriticallyHits",
                    Spell.Name, ad.Target.GetName(0, false), ad.CriticalDamage)), eChatType.CT_YouHit);
		}

		/// <summary>
		/// DoT are always lasting the same time !
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return Spell.Duration;
		}

		/// <summary>
		/// Interrupt on Start.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
		}

		/// <summary>
		/// On DoT Pulse launch Direct Effect.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectPulse(GameSpellEffect effect)
		{
			base.OnEffectPulse(effect);

			if (effect.Owner.IsAlive)
			{
				// An acidic cloud surrounds you!
				MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
				// {0} is surrounded by an acidic cloud!
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_YouHit, effect.Owner);
				
				OnDirectEffect(effect.Owner, effect.Effectiveness);
			}
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			
			if (!noMessages)
			{
				// The acidic mist around you dissipates.
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				// The acidic mist around {0} dissipates.
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			
			return 0;
		}

		/// <summary>
		/// Deal Damage with no interrupt
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			AttackData ad = DealDamage(target, effectiveness);
		}

		/// <summary>
		/// Don't Show Effect Animation on Damage.
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="showEffectAnimation"></param>
		/// <param name="attackResult"></param>
		public override void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			base.DamageTarget(ad, false, attackResult);
		}
		
		// constructor
		public DoTSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
