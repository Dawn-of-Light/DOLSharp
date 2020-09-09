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
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Spells
{
	[SpellHandler("RampingDamageFocus")]
	public class RampingDamageFocus : SpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private GameLiving Target;
		private int pulseCount = 0;

		public RampingDamageFocus(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, new FocusSpell(spell), spellLine) { }

		public override void FinishSpellCast(GameLiving target)
		{
			Target = target;
			Caster.Mana -= (PowerCost(target) + Spell.PulsePower);
			base.FinishSpellCast(target);
			OnDirectEffect(target, CurrentEffectiveness);
		}

		public override bool StartSpell(GameLiving target)
		{
			if (m_spellTarget == null)
				m_spellTarget = target;

			if (m_spellTarget == null) return false;

			ApplyEffectOnTarget(target, CurrentEffectiveness);
			return true;
		}

		private double CurrentEffectiveness
		{
			get
			{
				double effectiveness = Caster.Effectiveness;

				if (Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
				{
					MasteryofConcentrationAbility ra = Caster.GetAbility<MasteryofConcentrationAbility>();
					if (ra != null && ra.Level > 0)
					{
						effectiveness *= System.Math.Round((double)ra.GetAmountForLevel(ra.Level) / 100, 2);
					}
				}
				return effectiveness;
			}
		}

		public override void OnSpellPulse(PulsingSpellEffect effect)
		{
			if (Caster.ObjectState != GameObject.eObjectState.Active)
				return;
			if (Caster.IsStunned || Caster.IsMezzed)
				return;

			(Caster as GamePlayer).Out.SendCheckLOS(Caster, m_spellTarget, CheckLOSPlayerToTarget);

			if (Caster.Mana >= Spell.PulsePower)
			{
				Caster.Mana -= Spell.PulsePower;
				SendEffectAnimation(Caster, 0, true, 1); // pulsing auras or songs
				pulseCount += 1;
				OnDirectEffect(Target, CurrentEffectiveness);
			}
			else
			{
				MessageToCaster("You do not have enough mana and your spell was cancelled.", eChatType.CT_SpellExpires);
				FocusSpellAction(null, Caster, null);
				effect.Cancel(false);
			}
		}

		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			if (Spell.Pulse == 0)
				SendEffectAnimation(effect.Owner, 0, false, 1);
			if (Spell.IsFocus) // Add Event handlers for focus spell
			{
				Caster.TempProperties.setProperty(FOCUS_SPELL, effect);
				GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));
				GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			}
		}

		public override bool CasterIsAttacked(GameLiving attacker)
		{
			if (Spell.Uninterruptible && Caster.GetDistanceTo(attacker) > 200)
				return false;
			if (Caster.EffectList.CountOfType(typeof(QuickCastEffect), typeof(MasteryofConcentrationEffect), typeof(FacilitatePainworkingEffect)) > 0)
				return false;
			if (IsCasting && Stage < 2)
			{
				if (Caster.ChanceSpellInterrupt(attacker))
				{
					Caster.LastInterruptMessage = attacker.GetName(0, true) + " attacks you and your spell is interrupted!";
					MessageToLiving(Caster, Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
					InterruptCasting(); // always interrupt at the moment
					return true;
				}
			}
			return false;
		}

		public override void OnEffectRemove(GameSpellEffect effect, bool overwrite)
		{
			FocusSpellAction(null, Caster, null);
			base.OnEffectRemove(effect, overwrite);
		}

		protected override void FocusSpellAction(DOLEvent e, object sender, EventArgs args)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;

			GameSpellEffect currentEffect = (GameSpellEffect)living.TempProperties.getProperty<object>(FOCUS_SPELL, null);
			if (currentEffect == null)
				return;

			if (args is AttackedByEnemyEventArgs attackedByEnemy)
			{
				var attacker = attackedByEnemy.AttackData.Attacker;
				if (e == GameLivingEvent.AttackedByEnemy && Spell.Uninterruptible && Caster.GetDistanceTo(attacker) > 200) 
				{ 
					return; 
				}
			}
			
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));
			GameEventMgr.RemoveHandler(currentEffect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
			Caster.TempProperties.removeProperty(FOCUS_SPELL);
			foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendInterruptAnimation(Caster);
			}
			GameSpellEffect effect = FindEffectOnTarget(currentEffect.Owner, this);
			if (effect != null)
				effect.Cancel(false);

			CancelPulsingSpell(Caster, Spell.SpellType);
			currentEffect.Cancel(false);

			if (e == GameLivingEvent.Moving)
				MessageToCaster("You move and interrupt your focus!", eChatType.CT_Important);

			MessageToCaster(String.Format("You lose your focus on your {0} spell.", currentEffect.Spell.Name), eChatType.CT_SpellExpires);
		}

		public override void CheckLOSPlayerToTarget(GamePlayer player, ushort response, ushort targetOID)
		{
			if (player == null) // Hmm
				return;

			bool isInView = (response & 0x100) == 0x100;
			if (isInView)
				return;

			if (Properties.ENABLE_DEBUG)
			{
				MessageToCaster("LoS Interrupt in CheckLOSPlayerToTarget", eChatType.CT_System);
				log.Debug("LoS Interrupt in CheckLOSPlayerToTarget");
			}

			if (Caster is GamePlayer)
			{
				MessageToCaster("You can't see your target from here!", eChatType.CT_Important);
				FocusSpellAction(null, Caster, null);
			}

			InterruptCasting();
		}

		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;

			var targets = SelectTargets(target);

			foreach (GameLiving t in targets)
			{
				if (Util.Chance(CalculateSpellResistChance(t)))
				{
					OnSpellResisted(t);
					continue;
				}
				
				DealDamage(t, effectiveness);

				if (Spell.Value > 0)
				{
					DBSpell dbSpell = new DBSpell();
					dbSpell.ClientEffect = Spell.ClientEffect;
					dbSpell.Icon = Spell.Icon;
					dbSpell.Type = "SpeedDecrease";
					dbSpell.Duration = (Spell.Radius == 0) ? 10 : 3;
					dbSpell.Target = "Enemy";
					dbSpell.Range = 1500;
					dbSpell.Value = Spell.Value;
					dbSpell.Name = Spell.Name + " Snare";
					dbSpell.DamageType = (int)Spell.DamageType;
					Spell subSpell = new Spell(dbSpell, Spell.Level);
					ISpellHandler subSpellHandler = new SnareWithoutImmunity(Caster, subSpell, SpellLine);
					subSpellHandler.StartSpell(t);
				}
			}
		}

		protected virtual void DealDamage(GameLiving target, double effectiveness)
		{
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

			double growthPercent = Spell.LifeDrainReturn * 0.01;
			double growthCapPercent = Spell.AmnesiaChance * 0.01;
			double damageIncreaseInPercent = Math.Min(pulseCount * growthPercent, growthCapPercent);

			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			ad.Damage += (int)(ad.Damage * damageIncreaseInPercent);
			DamageTarget(ad, true);
			SendDamageMessages(ad);
			target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>(32);
				GamePlayer p = null;

				if (Spell.Damage != 0)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
				list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Target", Spell.Target) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));
				if (Spell.Range != 0)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Range", Spell.Range) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));
				if (Spell.Duration != 0)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Duration") + " " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'") : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " (Focus) " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
				if (Spell.Power != 0)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
				list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
				if (Spell.Radius != 0)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Radius", Spell.Radius) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Radius", Spell.Radius));
				if (Spell.DamageType != eDamageType.Natural)
					list.Add(p != null ? LanguageMgr.GetTranslation(p.Client, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)) : LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
				list.Add(" "); //empty line
				list.Add("Repeating direct damage spell that starts at " + Spell.Damage + " " + Spell.DamageType + " damage and increase by " + Spell.LifeDrainReturn + "% every tick up to a maximum of " + Spell.AmnesiaChance + "%.");
				list.Add(" "); //empty line
				if (Spell.Value > 0)
					list.Add("The target is slowed by " + Spell.Value + "%.");

				return list;
			}
		}
	}

	public class SnareWithoutImmunity : SpeedDecreaseSpellHandler
	{
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			return 0;
		}

		public SnareWithoutImmunity(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	public class FocusSpell : Spell
	{
		public FocusSpell(Spell spell) : base(spell, spell.SpellType) 
		{
			if (spell.Frequency == 0) Frequency = 5000;
			else Frequency = spell.Frequency;
		}

		public override bool IsFocus => true;
		public override int Pulse => 1;
		public override int Frequency { get; }
	}
}
