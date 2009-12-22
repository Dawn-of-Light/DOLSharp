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
using System.Reflection;

using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;

using log4net;
using DOL.AI.Brain;
using System;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell to change up to 3 property bonuses at once
	/// in one their specific given bonus category
	/// </summary>
	
	public abstract class PropertyChangingSpell : SpellHandler
	{
		/// <summary>
		/// Execute property changing spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// vampiir, they cannot be buffed except with resists/armor factor/ haste / power regen
			GamePlayer vampiir = target as GamePlayer;
			if (vampiir!=null)
			{
				if (HasPositiveEffect && vampiir.CharacterClass.ID == (int)eCharacterClass.Vampiir && m_caster != vampiir)
				{
					//restrictions
					//if (this is PropertyChangingSpell
					//    && this is ArmorFactorBuff == false
					//    && this is CombatSpeedBuff == false
					//    && this is AbstractResistBuff == false
					//    && this is EnduranceRegenSpellHandler == false
					//    && this is EvadeChanceBuff == false
					//    && this is ParryChanceBuff == false)
					//{
					if (this is StrengthBuff || this is DexterityBuff || this is ConstitutionBuff || this is QuicknessBuff || this is StrengthConBuff || this is DexterityQuiBuff || this is AcuityBuff)
					{
						GamePlayer caster = m_caster as GamePlayer;
						if (caster != null)
						{
							caster.Out.SendMessage("Your buff has no effect on the Vampiir!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						vampiir.Out.SendMessage("This buff has no effect on you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (this is ArmorFactorBuff)
					{
						if (SpellHandler.FindEffectOnTarget(target, "ArmorFactorBuff") != null && m_spellLine.IsBaseLine != true)
						{
							MessageToLiving(target, "You already have this effect!", eChatType.CT_SpellResisted);
							return;
						}
					}
				}
				
				if (this is HeatColdMatterBuff || this is AllMagicResistsBuff)
				{
					if (this.Spell.Frequency <= 0)
					{
						GameSpellEffect Matter = FindEffectOnTarget(vampiir, "MatterResistBuff");
						GameSpellEffect Cold = FindEffectOnTarget(vampiir, "ColdResistBuff");
						GameSpellEffect Heat = FindEffectOnTarget(vampiir, "HeatResistBuff");
						if (Matter != null || Cold != null || Heat != null)
						{
							MessageToCaster(target.Name + " already has this effect", eChatType.CT_SpellResisted);
							return;
						}
					}
				}
				
				if (this is BodySpiritEnergyBuff || this is AllMagicResistsBuff)
				{
					if (this.Spell.Frequency <= 0)
					{
						GameSpellEffect Body = FindEffectOnTarget(vampiir, "BodyResistBuff");
						GameSpellEffect Spirit = FindEffectOnTarget(vampiir, "SpiritResistBuff");
						GameSpellEffect Energy = FindEffectOnTarget(vampiir, "EnergyResistBuff");
						if (Body != null || Spirit != null || Energy != null)
						{
							MessageToCaster(target.Name + " already has this effect", eChatType.CT_SpellResisted);
							return;
						}
					}
				}
			}
			
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// start changing effect on target
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			IPropertyIndexer bonuscat = GetBonusCategory(effect.Owner, BonusCategory1);

			ApplyBonus(effect.Owner , BonusCategory1, Property1, (int) (Spell.Value*effect.Effectiveness), false);
			ApplyBonus(effect.Owner , BonusCategory2, Property2, (int) (Spell.Value*effect.Effectiveness), false);
			ApplyBonus(effect.Owner , BonusCategory3, Property3, (int) (Spell.Value*effect.Effectiveness), false);
			ApplyBonus(effect.Owner , BonusCategory4, Property4, (int) (Spell.Value*effect.Effectiveness), false);
			ApplyBonus(effect.Owner , BonusCategory5, Property5, (int) (Spell.Value*effect.Effectiveness), false);
			ApplyBonus(effect.Owner , BonusCategory6, Property6, (int) (Spell.Value*effect.Effectiveness), false);
			
			// Xali: buffs/debuffs are now efficient on pets
			#region Petbuffs
			if (effect.Owner is GameNPC)
			{
				if ((effect.Owner as GameNPC).Brain is ControlledNpcBrain)
				{
					//Increase Pet's ArmorAbsorb/MagicAbsorb with Buffs
					if (this is StrengthBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeDamage] += (int)(((Spell.Value / 100) * Spell.Level) / 2);
					}
					else if (this is ConstitutionBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Body] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Energy] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Cold] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Heat] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Matter] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Spirit] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
					}
					else if (this is ArmorFactorBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
					}
					else if (this is DexterityBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(((Spell.Value / 100) * Spell.Level) / 4);
					}
					else if (this is QuicknessBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeSpeed] += (int)(((Spell.Value / 100) * Spell.Level) / 6);
					}
					else if (this is StrengthConBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeDamage] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Body] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Energy] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Cold] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Heat] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Matter] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Spirit] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
					}
					else if (this is DexterityQuiBuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeSpeed] += (int)(((Spell.Value / 100) * Spell.Level) / 6);
					}
					//Decrease Pet's ArmorAbsorb/MagicAbsorb with DeBuffs
					else if (this is StrengthDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeDamage] -= (int)(((Spell.Value / 100) * Spell.Level) / 2);
					}
					else if (this is ConstitutionDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Body] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Energy] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Cold] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Heat] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Matter] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Spirit] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
					}
					else if (this is ArmorFactorDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
					}
					else if (this is DexterityDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(((Spell.Value / 100) * Spell.Level) / 4);
					}
					else if (this is QuicknessDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeSpeed] -= (int)(((Spell.Value / 100) * Spell.Level) / 6);
					}
					else if (this is StrengthConDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeDamage] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Body] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Energy] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Cold] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Heat] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Matter] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.Resist_Spirit] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
					}
					else if (this is DexterityQuiDebuff)
					{
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)(((Spell.Value / 100) * Spell.Level) / 8);
						(effect.Owner as GameNPC).AbilityBonus[(int)eProperty.MeleeSpeed] -= (int)(((Spell.Value / 100) * Spell.Level) / 6);
					}
				}
			}
			#endregion
			
			SendUpdates(effect.Owner);

			eChatType toLiving = eChatType.CT_SpellPulse;
			eChatType toOther = eChatType.CT_SpellPulse;
			if (Spell.Pulse == 0 || !HasPositiveEffect)
			{
				toLiving = eChatType.CT_Spell;
				toOther = eChatType.CT_System;
				SendEffectAnimation(effect.Owner, 0, false, 1);
			}

			GameLiving player = null;

			if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
				player = ((Caster as GameNPC).Brain as IControlledBrain).Owner;
			else if (effect.Owner is GameNPC && (effect.Owner as GameNPC).Brain is IControlledBrain)
				player = ((effect.Owner as GameNPC).Brain as IControlledBrain).Owner;

			if (player != null)
			{
				// Controlled NPC. Show message in blue writing to owner...

				MessageToLiving(player, String.Format(Spell.Message2,
				                                      effect.Owner.GetName(0, true)), toLiving);

				// ...and in white writing for everyone else.

				foreach (GamePlayer gamePlayer in effect.Owner.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
					if (gamePlayer != player)
						MessageToLiving(gamePlayer, String.Format(Spell.Message2,
						                                          effect.Owner.GetName(0, true)), toOther);
			}
			else
			{
				MessageToLiving(effect.Owner, Spell.Message1, toLiving);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
			}
			if (ServerProperties.Properties.BUFF_RANGE > 0 && effect.Spell.Concentration > 0 && effect.SpellHandler.HasPositiveEffect && effect.Owner != effect.SpellHandler.Caster)
			{
				m_buffCheckAction = new BuffCheckAction(effect.SpellHandler.Caster, effect.Owner, effect);
				m_buffCheckAction.Start(BuffCheckAction.BUFFCHECKINTERVAL);
			}
			
		}

		BuffCheckAction m_buffCheckAction = null;

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
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			
			ApplyBonus(effect.Owner , BonusCategory1, Property1, (int) (Spell.Value*effect.Effectiveness), true);
			ApplyBonus(effect.Owner , BonusCategory2, Property2, (int) (Spell.Value*effect.Effectiveness), true);
			ApplyBonus(effect.Owner , BonusCategory3, Property3, (int) (Spell.Value*effect.Effectiveness), true);
			ApplyBonus(effect.Owner , BonusCategory4, Property4, (int) (Spell.Value*effect.Effectiveness), true);
			ApplyBonus(effect.Owner , BonusCategory5, Property5, (int) (Spell.Value*effect.Effectiveness), true);
			ApplyBonus(effect.Owner , BonusCategory6, Property6, (int) (Spell.Value*effect.Effectiveness), true);

			SendUpdates(effect.Owner);

			if (m_buffCheckAction != null)
			{
				m_buffCheckAction.Stop();
				m_buffCheckAction = null;
			}

			return base.OnEffectExpires(effect, noMessages);
		}

		protected virtual void SendUpdates(GameLiving target)
		{
		}

		protected IPropertyIndexer GetBonusCategory(GameLiving target, int categoryid)
		{
			IPropertyIndexer bonuscat = null;
			switch (categoryid)
			{
				case 1:
					bonuscat = target.BaseBuffBonusCategory;
					break;
				case 2:
					bonuscat = target.SpecBuffBonusCategory;
					break;
				case 3:
					bonuscat = target.DebuffCategory;
					break;
				case 4:
					bonuscat = target.BuffBonusCategory4;
					break;
				default:
					if (log.IsErrorEnabled)
						log.Error("BonusCategory not found " + categoryid + "!");
					break;
			}
			return bonuscat;
		}

		/// <summary>
		/// Property 1 which bonus value has to be changed
		/// </summary>
		public abstract eProperty Property1 { get; }

		/// <summary>
		/// Property 2 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property2
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 3 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property3
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 4 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property4
		{
			get { return eProperty.Undefined; }
		}
		/// <summary>
		/// Property 5 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property5
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 6 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property6
		{
			get { return eProperty.Undefined; }
		}
		
		/// <summary>
		/// Bonus Category where to change the Property1
		/// </summary>
		public virtual int BonusCategory1
		{
			get { return 1; }
		}

		/// <summary>
		/// Bonus Category where to change the Property2
		/// </summary>
		public virtual int BonusCategory2
		{
			get { return 1; }
		}

		/// <summary>
		/// Bonus Category where to change the Property3
		/// </summary>
		public virtual int BonusCategory3
		{
			get { return 1; }
		}

		/// <summary>
		/// Bonus Category where to change the Property4
		/// </summary>
		public virtual int BonusCategory4
		{
			get { return 1; }
		}

		/// <summary>
		/// Bonus Category where to change the Property5
		/// </summary>
		public virtual int BonusCategory5
		{
			get { return 1; }
		}

		/// <summary>
		/// Bonus Category where to change the Property6
		/// </summary>
		public virtual int BonusCategory6
		{
			get { return 1; }
		}
		
		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			ApplyBonus (effect.Owner, BonusCategory1,Property1, vars[1], false);
			ApplyBonus (effect.Owner, BonusCategory2,Property2, vars[1], false);
			ApplyBonus (effect.Owner, BonusCategory3,Property3, vars[1], false);
			ApplyBonus (effect.Owner, BonusCategory4,Property4, vars[1], false);
			ApplyBonus (effect.Owner, BonusCategory5,Property5, vars[1], false);
			ApplyBonus (effect.Owner, BonusCategory6,Property6, vars[1], false);

			SendUpdates(effect.Owner);
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			if (!noMessages && Spell.Pulse == 0)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}

			ApplyBonus (effect.Owner, BonusCategory1,Property1, vars[1], true);
			ApplyBonus (effect.Owner, BonusCategory2,Property2, vars[1], true);
			ApplyBonus (effect.Owner, BonusCategory3,Property3, vars[1], true);
			ApplyBonus (effect.Owner, BonusCategory4,Property4, vars[1], true);
			ApplyBonus (effect.Owner, BonusCategory5,Property5, vars[1], true);
			ApplyBonus (effect.Owner, BonusCategory6,Property6, vars[1], true);

			SendUpdates(effect.Owner);
			return 0;
		}

		/// <summary>
		/// Method used to apply bonuses
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="BonusCat"></param>
		/// <param name="Property"></param>
		/// <param name="Value"></param>
		/// <param name="IsSubstracted"></param>
		private void ApplyBonus(GameLiving owner,  int BonusCat, eProperty Property, int Value, bool IsSubstracted)
		{
			IPropertyIndexer tblBonusCat;
			if (Property != eProperty.Undefined)
			{
				tblBonusCat = GetBonusCategory(owner, BonusCat);
				if (IsSubstracted)
					tblBonusCat[(int)Property] -= Value;
				else
					tblBonusCat[(int)Property] += Value;
			}
		}
		
		public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
		{
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.Var2 = (int)(Spell.Value * e.Effectiveness);
			eff.SpellLine = SpellLine.KeyName;
			return eff;

		}

		// constructor
		public PropertyChangingSpell(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}
	}

	public class BuffCheckAction : RegionAction
	{
		public const int BUFFCHECKINTERVAL = 60000;//60 seconds

		private GameLiving m_caster = null;
		private GameLiving m_owner = null;
		private GameSpellEffect m_effect = null;

		public BuffCheckAction(GameLiving caster, GameLiving owner, GameSpellEffect effect)
			: base(caster)
		{
			m_caster = caster;
			m_owner = owner;
			m_effect = effect;
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected override void OnTick()
		{
			if (m_caster == null ||
			    m_owner == null ||
			    m_effect == null)
				return;

			if ( !m_caster.IsWithinRadius( m_owner, ServerProperties.Properties.BUFF_RANGE ) )
				m_effect.Cancel(false);
			else
				Start(BUFFCHECKINTERVAL);
		}
	}
}
