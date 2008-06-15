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
using System.Reflection;

using DOL.Database;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Effect that stays on target and does additional
	/// damage after each melee attack
	/// </summary>
	[SpellHandler("DamageAdd")]
	public class DamageAddSpellHandler : AbstractDamageAddSpellHandler
	{
		/// <summary>
		/// The event type to hook on
		/// </summary>
		protected override DOLEvent EventType { get { return GameLivingEvent.AttackFinished; } }

		public virtual double DPSCap(int Level)
		{
			return (1.2 + 0.3 * Level) * 0.7;
		}

		/// <summary>
		/// Handler fired on every melee attack by effect target
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackFinishedEventArgs atkArgs = arguments as AttackFinishedEventArgs;
			if (atkArgs == null) return;
			if (atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
				&& atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitStyle) return;
			GameLiving target = atkArgs.AttackData.Target;
			if (target == null) return;
			if (target.ObjectState != GameObject.eObjectState.Active) return;
			if (target.IsAlive == false) return;
			if (target is GameKeepComponent || target is GameKeepDoor) return;
			GameLiving attacker = sender as GameLiving;
			if (attacker == null) return;
			if (attacker.ObjectState != GameObject.eObjectState.Active) return;
			if (attacker.IsAlive == false) return;

			int spread = m_minDamageSpread;
			spread += Util.Random(50);
			double dpsCap = DPSCap(attacker.Level);
			double dps = Math.Min(Spell.Damage, dpsCap);
			double damage = dps * atkArgs.AttackData.WeaponSpeed * spread * 0.001; // attack speed is 10 times higher (2.5spd=25)
			double damageResisted = damage * target.GetResist(Spell.DamageType) * -0.01;

			if (Spell.Damage < 0)
			{
				damage = atkArgs.AttackData.Damage * Spell.Damage / -100.0;
				damageResisted = damage * target.GetResist(Spell.DamageType) * -0.01;
			}

			AttackData ad = new AttackData();
			ad.Attacker = attacker;
			ad.Target = target;
			ad.Damage = (int)(damage + damageResisted);
			ad.Modifier = (int)damageResisted;
			ad.DamageType = Spell.DamageType;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.SpellHandler = this;
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;

			if (ad.Attacker is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)ad.Attacker).Brain as IControlledBrain;
				if (brain != null)
				{
					GamePlayer owner = brain.GetPlayerOwner();
					if (owner != null)
					{
						MessageToLiving(owner, String.Format("Your {0} hit {1} for {2} damage!", ad.Attacker.Name, target.GetName(0, false), ad.Damage), eChatType.CT_Spell);
					}
				}
			}
			else
			{
				if (Spell.Pulse != 0)
					MessageToLiving(attacker, String.Format("You hit {0} for {1} extra damage!", target.GetName(0, false), ad.Damage), eChatType.CT_Spell);
				else
					MessageToLiving(attacker, String.Format("You hit {0} for {1} damage!", target.GetName(0, false), ad.Damage), eChatType.CT_Spell);
			}
			MessageToLiving(target, String.Format("{0} does {1} extra damage to you!", attacker.GetName(0, false), ad.Damage), eChatType.CT_Spell);
			target.OnAttackedByEnemy(ad);
			attacker.DealDamage(ad);
			foreach (GamePlayer player in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null) continue;
				player.Out.SendCombatAnimation(null, target, 0, 0, 0, 0, 0x0A, target.HealthPercent);
			}
			//			log.Debug(String.Format("spell damage: {0}; damage: {1}; resisted damage: {2}; damage type {3}; minSpread {4}.", Spell.Damage, ad.Damage, ad.Modifier, ad.DamageType, m_minDamageSpread));
			//			log.Debug(String.Format("dpsCap: {0}; dps: {1}; dmg {2}; spread: {6}; resDmg: {3}; atkSpeed: {4}; resist: {5}.", dpsCap, dps, damage, damageResisted, attacker.AttackSpeed(null), ad.Target.GetResist(Spell.DamageType), spread));
		}

		// constructor
		public DamageAddSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
	}

	/// <summary>
	/// Effect that stays on target and does addition
	/// damage on every attack against this target
	/// </summary>
	[SpellHandler("DamageShield")]
	public class DamageShieldSpellHandler : AbstractDamageAddSpellHandler
	{
		/// <summary>
		/// The event type to hook on
		/// </summary>
		protected override DOLEvent EventType { get { return GameLivingEvent.AttackedByEnemy; } }

		/// <summary>
		/// Handler fired whenever effect target is attacked
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
			if (args == null) return;
			if (args.AttackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
				&& args.AttackData.AttackResult != GameLiving.eAttackResult.HitStyle) return;
			if (!args.AttackData.IsMeleeAttack) return;
			GameLiving attacker = sender as GameLiving; //sender is target of attack, becomes attacker for damage shield
			if (attacker == null) return;
			if (attacker.ObjectState != GameObject.eObjectState.Active) return;
			if (attacker.IsAlive == false) return;
			GameLiving target = args.AttackData.Attacker; //attacker becomes target for damage shield
			if (target == null) return;
			if (target.ObjectState != GameObject.eObjectState.Active) return;
			if (target.IsAlive == false) return;

			int spread = m_minDamageSpread;
			spread += Util.Random(50);
			double damage = Spell.Damage * target.AttackSpeed(target.AttackWeapon) * spread * 0.00001;
			double damageResisted = damage * target.GetResist(Spell.DamageType) * -0.01;

			if (Spell.Damage < 0)
			{
				damage = args.AttackData.Damage * Spell.Damage / -100.0;
				damageResisted = damage * target.GetResist(Spell.DamageType) * -0.01;
			}

			AttackData ad = new AttackData();
			ad.Attacker = attacker;
			ad.Target = target;
			ad.Damage = (int)(damage + damageResisted);
			ad.Modifier = (int)damageResisted;
			ad.DamageType = Spell.DamageType;
			ad.SpellHandler = this;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;

			if (ad.Attacker is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)ad.Attacker).Brain as IControlledBrain;
				if (brain != null)
				{
					GamePlayer owner = brain.GetPlayerOwner();
					if (owner != null && owner.ControlledNpc != null && ad.Attacker == owner.ControlledNpc.Body)
					{
						MessageToLiving(owner, String.Format("Your {0} hit {1} for {2} damage!", ad.Attacker.Name, target.GetName(0, false), ad.Damage), eChatType.CT_Spell);
					}
				}
			}
			else
			{
				MessageToLiving(attacker, String.Format("You hit {0} for {1} damage!", target.GetName(0, false), ad.Damage), eChatType.CT_Spell);
			}

			MessageToLiving(target, String.Format("{0} does {1} extra damage to you!", attacker.GetName(0, false), ad.Damage), eChatType.CT_Spell);
			target.OnAttackedByEnemy(ad);
			attacker.DealDamage(ad);
			foreach (GamePlayer player in attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null)
					continue;
				player.Out.SendCombatAnimation(null, target, 0, 0, 0, 0, 0x14, target.HealthPercent);
			}
			//			log.Debug(String.Format("spell damage: {0}; damage: {1}; resisted damage: {2}; damage type {3}; minSpread {4}.", Spell.Damage, ad.Damage, ad.Modifier, ad.DamageType, m_minDamageSpread));
			//			log.Debug(String.Format("dmg {0}; spread: {4}; resDmg: {1}; atkSpeed: {2}; resist: {3}.", damage, damageResisted, target.AttackSpeed(null), ad.Target.GetResist(Spell.DamageType), spread));
		}

		// constructor
		public DamageShieldSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
	}

	/// <summary>
	/// Contains all common code for damage add and shield spell handlers
	/// </summary>
	public abstract class AbstractDamageAddSpellHandler : SpellHandler
	{
		/// <summary>
		/// The event type to hook on
		/// </summary>
		protected abstract DOLEvent EventType { get; }

		/// <summary>
		/// The event handler of given event type
		/// </summary>
		protected abstract void EventHandler(DOLEvent e, object sender, EventArgs arguments);

		/// <summary>
		/// Holds min damage spread based on spec level caster
		/// had the moment spell was casted
		/// </summary>
		protected int m_minDamageSpread = 50;

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override void StartSpell(GameLiving target)
		{
			// set min spread based on spec
			if (Caster is GamePlayer)
			{
				int lineSpec = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
				m_minDamageSpread = 50;
				if (Spell.Level > 0)
				{
					m_minDamageSpread += (lineSpec - 1) * 50 / Spell.Level;
					if (m_minDamageSpread > 100) m_minDamageSpread = 100;
					else if (m_minDamageSpread < 50) m_minDamageSpread = 50;
				}
			}
			base.StartSpell(target);
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		//public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		//{
		//    base.ApplyEffectOnTarget(target, m_minDamageSpread);
		//}
		// Either keep this commented out or don't let DamageShield inherit
		// from AbstractDamageAddSpellHandler, because it screws spell duration
		// up completely.

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			// "Your weapon is blessed by the gods!"
			// "{0}'s weapon glows with the power of the gods!"
			eChatType chatType = eChatType.CT_SpellPulse;
			if (Spell.Pulse == 0)
			{
				chatType = eChatType.CT_Spell;
			}
			bool upperCase = Spell.Message2.StartsWith("{0}");
			MessageToLiving(effect.Owner, Spell.Message1, chatType);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, 
				effect.Owner.GetName(0, upperCase)), chatType, effect.Owner);
			GameEventMgr.AddHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
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
			if (!noMessages && Spell.Pulse == 0)
			{
				// "Your weapon returns to normal."
				// "{0}'s weapon returns to normal."
				bool upperCase = Spell.Message4.StartsWith("{0}");
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, 
					effect.Owner.GetName(0, upperCase)), eChatType.CT_SpellExpires, effect.Owner);
			}
			GameEventMgr.RemoveHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
			return 0;
		}

		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			GameEventMgr.AddHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return OnEffectExpires(effect, noMessages);
		}

		public override PlayerXEffect getSavedEffect(GameSpellEffect e)
		{
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.SpellLine = SpellLine.KeyName;
			return eff;
		}

		// constructor
		public AbstractDamageAddSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
	}
}
