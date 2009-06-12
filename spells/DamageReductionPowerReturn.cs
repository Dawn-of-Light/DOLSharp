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
*///made by Yemla, based off Alblative
using System;
using System.Reflection;

using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using log4net;
using System.Collections;
using DOL.GS;
using DOL.AI.Brain;
using DOL.GS.Keeps;


namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("DmgReductionAndPowerReturn")]
   public class DamageReductionAndPowerReturnSpellHandler : SpellHandler
   {
		public const string Damage_Reduction = "damage reduction";

	  public override void OnEffectStart(GameSpellEffect effect)
	  {
			effect.Owner.TempProperties.setProperty(Damage_Reduction, 100000);         
		 GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

		 eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
			eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_Spell;///Pulse;
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
			effect.Owner.TempProperties.removeProperty(Damage_Reduction);         
		 if (!noMessages && Spell.Pulse == 0)
		 {
			MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
		 }
		 return 0;
	  }
	  public override void FinishSpellCast(GameLiving target)
	  {
		 m_caster.Mana -= PowerCost(target);
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

//         log.DebugFormat("sender:{0} res:{1} IsMelee:{2} Type:{3}", living.Name, ad.AttackResult, ad.IsMeleeAttack, ad.AttackType);

			int damagereduction = living.TempProperties.getIntProperty(Damage_Reduction, 0);
		 double absorbPercent = Spell.Damage;
		 int damageAbsorbed = (int)(0.01 * absorbPercent * (ad.Damage+ad.CriticalDamage));
			if (damageAbsorbed > damagereduction)
				damageAbsorbed = damagereduction;
			damagereduction -= damageAbsorbed;
		 ad.Damage -= damageAbsorbed;
		 OnDamageAbsorbed(ad, damageAbsorbed);

		 //TODO correct messages
			if (ad.Damage > 0)
			MessageToLiving(ad.Target, string.Format("The damage reduction absorbs {0} damage!", damageAbsorbed), eChatType.CT_Spell);
			MessageToLiving(ad.Attacker, string.Format("A damage reduction absorbs {0} damage of your attack!", damageAbsorbed), eChatType.CT_Spell);
			if (damageAbsorbed > 0)
			MessageToCaster("The barrier returns " + damageAbsorbed + " mana back to you.", eChatType.CT_Spell);
			Caster.Mana = Caster.Mana + damageAbsorbed;
			if (Caster.Mana == Caster.MaxMana)
				MessageToCaster("You cannot absorb any more power.", eChatType.CT_SpellResisted);

			if (damagereduction <= 0)
		 {
			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
			if(effect != null)
			   effect.Cancel(false);
		 }
		 else
		 {
				living.TempProperties.setProperty(Damage_Reduction, damagereduction);
		 }
	  }

	  protected virtual void OnDamageAbsorbed(AttackData ad, int DamageAmount)
	  {
	  }
	  
	  public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
	  {
		 if ( //VaNaTiC-> this cannot work, cause PulsingSpellEffect is derived from object and only implements IConcEffect
			  //e is PulsingSpellEffect ||
			  //VaNaTiC<-
			 Spell.Pulse != 0 || Spell.Concentration != 0 || e.RemainingTime < 1)
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
			effect.Owner.TempProperties.setProperty(Damage_Reduction, (int)vars[1]);
		 GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
	  }

	  public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
	  {
		 GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			effect.Owner.TempProperties.removeProperty(Damage_Reduction);
		 if (!noMessages && Spell.Pulse == 0)
		 {
			MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
		 }
		 return 0;
	  }
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();

				list.Add("Name: " + Spell.Name);
				list.Add("Description: " + Spell.Description);
				list.Add("Target: " + Spell.Target);
				if (Spell.Damage != 0)
					list.Add("Damage Absorb: " + Spell.Damage + "%");
				if (Spell.Value != 0)
					list.Add("Power Return: " + Spell.Damage +"%");
				if (Spell.CastTime < 0.1)
					list.Add("Casting time: Instant");
				else if (Spell.CastTime > 0)
					list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.Duration >= ushort.MaxValue * 1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
				else if (Spell.Duration != 0)

				if (Spell.Range != 0)
					list.Add("Range: " + Spell.Range);
				if (Spell.Radius != 0)
					list.Add("Radius: " + Spell.Radius);

				list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));

				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));

				if (Spell.DamageType != 0)
					list.Add("Damage Type: " + Spell.DamageType);
				return list;
			}
		}
		public DamageReductionAndPowerReturnSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
   }
}
