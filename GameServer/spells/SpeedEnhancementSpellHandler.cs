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
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	[SpellHandler("SpeedEnhancement")]
	public class SpeedEnhancementSpellHandler : SpellHandler
	{
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}
		
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = Spell.Duration;
			duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
			if (Spell.InstrumentRequirement != 0)
			{
				InventoryItem instrument = Caster.AttackWeapon;
				if (instrument != null)
				{
					duration *= 1.0 + Math.Min(1.0, instrument.Level / (double)Caster.Level); // up to 200% duration for songs
					duration *= instrument.Condition / (double)instrument.MaxCondition * instrument.Quality / 100;
				}
			}
			
			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}
		
		public override void OnEffectAdd(GameSpellEffect effect)
		{
			GamePlayer player = effect.Owner as GamePlayer;
			
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttack));
			if (player != null)
				GameEventMgr.AddHandler(player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(OnStealthStateChanged));
			
			base.OnEffectAdd(effect);
		}

		public override void OnEffectRemove(GameSpellEffect effect, bool overwrite)
		{
			GamePlayer player = effect.Owner as GamePlayer;
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
			if (player != null)
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(OnStealthStateChanged));
			
			base.OnEffectRemove(effect, overwrite);
		}
		
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target.EffectList.GetOfType<ChargeEffect>() != null)
				return;

			if (target.TempProperties.getProperty("Charging", false))
				return;

			if (target.EffectList.GetOfType<ArmsLengthEffect>() != null)
				return;

			if (target.EffectList.GetOfType<SpeedOfSoundEffect>() != null)
				return;

			if (target is GamePlayer && (target as GamePlayer).IsRiding)
				return;

			if (target is Keeps.GameKeepGuard)
				return;

			// Graveen: archery speed shot
			if ((Spell.Pulse != 0 || Spell.CastTime != 0) && target.InCombat)
			{
				MessageToLiving(target, "You've been in combat recently, the spell has no effect on you!", eChatType.CT_SpellResisted);
				return;
			}
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			GamePlayer player = effect.Owner as GamePlayer;

			if (player == null || !player.IsStealthed)
			{
				effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, Spell.Value / 100.0);
				SendUpdates(effect.Owner);
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);

			if (!noMessages)
			{
				SendUpdates(effect.Owner);
			}

			return base.OnEffectExpires(effect, noMessages);
		}

		protected virtual void SendUpdates(GameLiving owner)
		{
			if (owner.IsMezzed || owner.IsStunned)
				return;

			if (owner is GamePlayer)
			{
				((GamePlayer)owner).Out.SendUpdateMaxSpeed();
			}
			else if (owner is GameNPC)
			{
				GameNPC npc = (GameNPC)owner;
				short maxSpeed = npc.MaxSpeed;
				if (npc.CurrentSpeed > maxSpeed)
					npc.CurrentSpeed = maxSpeed;
			}
		}

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
			CastingEventArgs castFinished = arguments as CastingEventArgs;
			AttackData ad = null;
			ISpellHandler sp = null;

			if (attackedByEnemy != null)
			{
				ad = attackedByEnemy.AttackData;
			}
			else if (attackFinished != null)
			{
				ad = attackFinished.AttackData;
			}
			else if (castFinished != null)
			{
				sp = castFinished.SpellHandler;
				ad = castFinished.LastAttackData;
			}

			// Speed should drop if the player casts an offensive spell
			if (sp == null && ad == null)
			{
				return;
			}
			else if (sp == null && (ad.AttackResult != GameLiving.eAttackResult.HitStyle && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled))
			{
				return;
			}
			else if (sp != null && (sp.HasPositiveEffect || ad == null))
			{
				return;
			}

			GameSpellEffect speed = SpellHandler.FindEffectOnTarget(living, this);
			if (speed != null)
				speed.Cancel(false);
		}

		private void OnStealthStateChanged(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer player = (GamePlayer)sender;
			if (player.IsStealthed)
				player.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
			else player.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, Spell.Value / 100.0);
			// max speed update is sent in setalth method
		}

		public override IList<string> DelveInfo
		{
			get
			{
				/*
				<Begin Info: Motivation Sng>
 
				The movement speed of the target is increased.
 
				Target: Group
				Range: 2000
				Duration: 30 sec
				Frequency: 6 sec
				Casting time:      3.0 sec
				
				This spell's effect will not take hold while the target is in combat.
				<End Info>
				*/
				IList<string> list = base.DelveInfo;

				list.Add(" "); //empty line
				list.Add("This spell's effect will not take hold while the target is in combat.");

				return list;
			}
		}

		public SpeedEnhancementSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override string ShortDescription => $"The target's speed is increased to {Spell.Value}% of normal.";
    }
}
