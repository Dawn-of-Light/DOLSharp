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

using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Disease always debuffs the target by 15% movement
	/// and 7.5% total hit points, and prevents health regeneration.
	/// </summary>
	[SpellHandlerAttribute("Disease")]
	public class DiseaseSpellHandler : DebuffSpellHandler
	{
		/// <summary>
		/// Snare Amount of this Disease
		/// </summary>
		protected virtual double SnareValue
		{
			get { return 1.0 - Spell.Value * 0.01; }
		}
		
		/// <summary>
		/// Str percent debuff amount of this Disease
		/// </summary>
		protected virtual double StrengthDebuffValue
		{
			get { return 1.0 - Spell.Value * 0.5 * 0.01; }
		}
		
		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			
			GameSpellEffect mezz = SpellHelper.FindEffectOnTarget(effect.Owner, typeof(MesmerizeSpellHandler));
 			// cancel mezz
			if(mezz != null)
 				mezz.Cancel(false);
			
			// start disease.
			effect.Owner.Disease(true);
			effect.Owner.BuffBonusMultCategory1.Set(eProperty.MaxSpeed, this, SnareValue);
			effect.Owner.BuffBonusMultCategory1.Set(eProperty.Strength, this, StrengthDebuffValue);

			SendUpdates(effect);

			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_System, effect.Owner);

			effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
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
			
			effect.Owner.Disease(false);
			effect.Owner.BuffBonusMultCategory1.Remove(eProperty.MaxSpeed, this);
			effect.Owner.BuffBonusMultCategory1.Remove(eProperty.Strength, this);

			if (!noMessages)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);
			}

			SendUpdates(effect);

			return 0;
		}

		/// <summary>
		/// Sends needed updates on start/stop
		/// </summary>
		/// <param name="effect"></param>
		protected virtual void SendUpdates(GameSpellEffect effect)
		{
			// Update Player Speed
			GamePlayer player = effect.Owner as GamePlayer;
			if (player != null)
			{
				
				if (!player.IsIncapacitated)
					player.Out.SendUpdateMaxSpeed();
				
				player.Out.SendCharStatsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
			}

			// Update NPC Speed
			GameNPC npc = effect.Owner as GameNPC;
			if (npc != null)
			{
				if (npc.CurrentSpeed > npc.MaxSpeed)
					npc.CurrentSpeed = npc.MaxSpeed;
			}
		}

		/// <summary>
		/// Get Saved Effect.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
		{
			if (Spell.IsPulsing || Spell.IsConcentration || e.RemainingTime < 1)
				return null;
			
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.SpellLine = SpellLine.KeyName;
			return eff;
		}

		/// <summary>
		/// Saved Effect Restored
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="vars"></param>
		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			effect.Owner.Disease(true);
			effect.Owner.BuffBonusMultCategory1.Set(eProperty.MaxSpeed, this, SnareValue);
			effect.Owner.BuffBonusMultCategory1.Set(eProperty.Strength, this, StrengthDebuffValue);
			SendUpdates(effect);
		}

		/// <summary>
		/// Saved Effect Expires
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="vars"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return this.OnEffectExpires(effect, noMessages);
		}

		// constructor
		public DiseaseSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}
	}
}
