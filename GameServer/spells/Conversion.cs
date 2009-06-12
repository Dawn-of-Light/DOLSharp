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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Conversion")]
	public class ConversionSpellHandler : SpellHandler
	{
		public const string ConvertDamage = "Conversion";

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			effect.Owner.TempProperties.setProperty(ConvertDamage, 100000);
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

			eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
			eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_Spell;
			MessageToLiving(effect.Owner, Spell.Message1, toLiving);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			effect.Owner.TempProperties.removeProperty(ConvertDamage);
			return 1;
		}
		
		protected virtual void OndamageConverted(AttackData ad, int DamageAmount)
		{
		}

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackData ad = null;
			if (attackedByEnemy != null)
			{
				ad = attackedByEnemy.AttackData;
			}
			int reduceddmg = living.TempProperties.getIntProperty(ConvertDamage, 0);
			double absorbPercent = Spell.Damage;
			int damageConverted = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));

			if (damageConverted > reduceddmg)
			{
				damageConverted = reduceddmg;
				reduceddmg -= damageConverted;
				ad.Damage -= damageConverted;
				OndamageConverted(ad, damageConverted);
			}

			if (ad.Damage > 0)
				MessageToLiving(ad.Target, string.Format("You convert {0} damage into " + damageConverted + " Health.", damageConverted), eChatType.CT_Spell);
			MessageToLiving(ad.Attacker, string.Format("A magical spell absorbs {0} damage of your attack!", damageConverted), eChatType.CT_Spell);

			if (Caster.Health != Caster.MaxHealth)
			{
				MessageToCaster("You convert " + damageConverted + " damage into health.", eChatType.CT_Spell);
				Caster.Health = Caster.Health + damageConverted;
			}
			else
			{
				MessageToCaster("You cannot convert anymore health!", eChatType.CT_Spell);
			}

			if (Caster.Endurance != Caster.MaxEndurance)
			{
				MessageToCaster("You convert " + damageConverted + " damage into endurance", eChatType.CT_Spell);
				Caster.Endurance = Caster.Endurance + damageConverted;
			}
			else
			{
				MessageToCaster("You cannot convert anymore endurance!", eChatType.CT_Spell);
			}
			if (Caster.Mana != Caster.MaxMana)
			{
				MessageToCaster("You convert " + damageConverted + " damage into mana.", eChatType.CT_Spell);
				Caster.Mana = Caster.Mana + damageConverted;
			}
			else
			{
				MessageToCaster("You cannot convert anymore mana!", eChatType.CT_Spell);
			}

			if (reduceddmg <= 0)
			{
				GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
				if (effect != null)
					effect.Cancel(false);
			}
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
				{
					list.Add("Damage Absorb: " + Spell.Damage + "%");
					list.Add("Health Return: " + Spell.Damage + "%");
					list.Add("Power Return: " + Spell.Damage + "%");
					list.Add("Endurance Return: " + Spell.Damage + "%");
				}
				return list;
			}
		}
		public ConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	
	[SpellHandlerAttribute("MagicConversion")]
	public class MagicConversionSpellHandler : ConversionSpellHandler
	{
		//public const string ConvertDamage = "Conversion";

		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackData ad = null;
			if (attackedByEnemy != null)
			{
				ad = attackedByEnemy.AttackData;
			}


			if (ad.Damage > 0)
			{
				switch (attackedByEnemy.AttackData.AttackType)
				{
					case AttackData.eAttackType.Spell:
						{
							int reduceddmg = living.TempProperties.getIntProperty(ConvertDamage, 0);
							double absorbPercent = Spell.Damage;
							int damageConverted = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));
							if (damageConverted > reduceddmg)
							{
								damageConverted = reduceddmg;
								reduceddmg -= damageConverted;
								ad.Damage -= damageConverted;
								OndamageConverted(ad, damageConverted);
							}
							if (reduceddmg <= 0)
							{
								GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
								if (effect != null)
									effect.Cancel(false);
							}
							MessageToLiving(ad.Target, string.Format("You convert {0} damage into " + damageConverted + " Health.", damageConverted), eChatType.CT_Spell);
							MessageToLiving(ad.Attacker, string.Format("A magical spell absorbs {0} damage of your attack!", damageConverted), eChatType.CT_Spell);
							if (Caster.Health != Caster.MaxHealth)
							{
								MessageToCaster("You convert " + damageConverted + " damage into health.", eChatType.CT_Spell);
								Caster.Health = Caster.Health + damageConverted;
							}
							else
							{
								MessageToCaster("You cannot convert anymore health!", eChatType.CT_Spell);
							}

							if (Caster.Endurance != Caster.MaxEndurance)
							{
								MessageToCaster("You convert " + damageConverted + " damage into endurance", eChatType.CT_Spell);
								Caster.Endurance = Caster.Endurance + damageConverted;
							}
							else
							{
								MessageToCaster("You cannot convert anymore endurance!", eChatType.CT_Spell);
							}
							if (Caster.Mana != Caster.MaxMana)
							{
								MessageToCaster("You convert " + damageConverted + " damage into mana.", eChatType.CT_Spell);
								Caster.Mana = Caster.Mana + damageConverted;
							}
							else
							{
								MessageToCaster("You cannot convert anymore mana!", eChatType.CT_Spell);
							}
						}
						break;
				}
			}
		}

		public MagicConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
