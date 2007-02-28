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
 *///made by DeMAN
using System;
using System.Reflection;

using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

using log4net;
 
 
namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("AblativeArmor")]
	public class AblativeArmorSpellHandler : SpellHandler
	{
		public const string ABLATIVE_HP = "ablative hp";

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			effect.Owner.TempProperties.setProperty(ABLATIVE_HP, (int)Spell.Value);			
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

			eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
			eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_SpellPulse;
			MessageToLiving(effect.Owner, Spell.Message1, toLiving);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
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
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			effect.Owner.TempProperties.removeProperty(ABLATIVE_HP);			
			if (!noMessages && Spell.Pulse == 0) 
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			return 0;
		}

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{			
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;			
			AttackData ad = null;
			if (attackedByEnemy != null)
				ad = attackedByEnemy.AttackData;

//			log.DebugFormat("sender:{0} res:{1} IsMelee:{2} Type:{3}", living.Name, ad.AttackResult, ad.IsMeleeAttack, ad.AttackType);

			if (ad == null || (ad.AttackResult != GameLiving.eAttackResult.HitStyle && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled))
				return;
			if (!ad.IsMeleeAttack && ad.AttackType != AttackData.eAttackType.Ranged)
				return;

			int ablativehp = living.TempProperties.getIntProperty(ABLATIVE_HP, 0);
			double absorbPercent = 25;
			if (Spell.Damage > 0)
				absorbPercent = Spell.Damage;
			if (absorbPercent > 99)
				absorbPercent = 99;
			int damageAbsorbed = (int)(0.01 * absorbPercent * (ad.Damage+ad.CriticalDamage));
			if (damageAbsorbed > ablativehp)
				damageAbsorbed = ablativehp;
			ablativehp -= damageAbsorbed;
			ad.Damage -= damageAbsorbed;

			//TODO correct messages
			MessageToLiving(ad.Target, string.Format("Your melee buffer absorbs {0} damage!", damageAbsorbed), eChatType.CT_Spell);
			MessageToLiving(ad.Attacker, string.Format("A barrier absorbs {0} damage of your attack!", damageAbsorbed), eChatType.CT_Spell);

			if(ablativehp <= 0)
			{
				GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
				if(effect != null)
					effect.Cancel(false);
			}
			else
			{
				living.TempProperties.setProperty(ABLATIVE_HP,ablativehp);
			}
		}

		public override PlayerXEffect getSavedEffect(GameSpellEffect e)
		{
			if (e is PulsingSpellEffect || Spell.Pulse != 0 || Spell.Concentration != 0 || e.RemainingTime < 1)
				return null;
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.Var2 = (int)Spell.Value;
			eff.SpellLine = SpellLine.KeyName;
			return eff;
		}

		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			effect.Owner.TempProperties.setProperty(ABLATIVE_HP, (int)vars[1]);
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			effect.Owner.TempProperties.removeProperty(ABLATIVE_HP);
			if (!noMessages && Spell.Pulse == 0)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			return 0;
		}

		public AblativeArmorSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
