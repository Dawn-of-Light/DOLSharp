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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.AI.Brain;

using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell to change up to 10 property bonuses at once
	/// each in one their specific given bonus category
	/// And with their own Value...
	/// </summary>
	public abstract class PropertyChangingSpell : SpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Property changing should always start effect if buff.
		/// </summary>
		public override bool ForceStartEffect {
			get { return HasPositiveEffect; }
		}
		
		/// <summary>
		/// Execute property changing spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
			base.FinishSpellCast(target);
		}
		
		
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				
				// TODO this should be in resist buff/debuff handling ! Overwrite/new effect is better handling !
				if (this is HeatColdMatterBuff || this is AllMagicResistsBuff)
				{
					if (this.Spell.Frequency <= 0)
					{
						GameSpellEffect Matter = SpellHelper.FindEffectOnTarget(player, "MatterResistBuff");
						GameSpellEffect Cold = SpellHelper.FindEffectOnTarget(player, "ColdResistBuff");
						GameSpellEffect Heat = SpellHelper.FindEffectOnTarget(player, "HeatResistBuff");
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
						GameSpellEffect Body = SpellHelper.FindEffectOnTarget(player, "BodyResistBuff");
						GameSpellEffect Spirit = SpellHelper.FindEffectOnTarget(player, "SpiritResistBuff");
						GameSpellEffect Energy = SpellHelper.FindEffectOnTarget(player, "EnergyResistBuff");
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
			ApplyBonus(effect.Owner, BonusCategory1, Property1, (int)(BonusAmount1 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory2, Property2, (int)(BonusAmount2 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory3, Property3, (int)(BonusAmount3 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory4, Property4, (int)(BonusAmount4 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory5, Property5, (int)(BonusAmount5 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory6, Property6, (int)(BonusAmount6 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(BonusAmount7 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(BonusAmount8 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(BonusAmount9 * effect.Effectiveness), false);
			ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(BonusAmount10 * effect.Effectiveness), false);
			
			SendUpdates(effect.Owner);

			eChatType toLiving = eChatType.CT_SpellPulse;
			eChatType toOther = eChatType.CT_SpellPulse;
			if (!Spell.IsPulsing || !HasPositiveEffect)
			{
				toLiving = eChatType.CT_Spell;
				toOther = eChatType.CT_System;
			}

			GameLiving player = null;

			if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
				player = ((Caster as GameNPC).Brain as IControlledBrain).Owner;
			else if (effect.Owner is GameNPC && (effect.Owner as GameNPC).Brain is IControlledBrain)
				player = ((effect.Owner as GameNPC).Brain as IControlledBrain).Owner;

			if (player != null)
			{
				// Controlled NPC. Show message in blue writing to owner...

				MessageToLiving(player, String.Format(Spell.Message2, effect.Owner.GetName(0, true)), toLiving);

				// ...and in white writing for everyone else.

				foreach (GamePlayer gamePlayer in effect.Owner.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
					if (gamePlayer != player)
						MessageToLiving(gamePlayer, String.Format(Spell.Message2, effect.Owner.GetName(0, true)), toOther);
			}
			else
			{
				MessageToLiving(effect.Owner, Spell.Message1, toLiving);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
			}

			base.OnEffectStart(effect);
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
			if (!noMessages && !Spell.IsPulsing)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			
			ApplyBonus(effect.Owner, BonusCategory1, Property1, (int)(BonusAmount1 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory2, Property2, (int)(BonusAmount2 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory3, Property3, (int)(BonusAmount3 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory4, Property4, (int)(BonusAmount4 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory5, Property5, (int)(BonusAmount5 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory6, Property6, (int)(BonusAmount6 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(BonusAmount7 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(BonusAmount8 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(BonusAmount9 * effect.Effectiveness), true);
			ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(BonusAmount10 * effect.Effectiveness), true);

			SendUpdates(effect.Owner);

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
				case (int)eBuffBonusCategory.BaseBuff:
					bonuscat = target.BaseBuffBonusCategory;
					break;
				case (int)eBuffBonusCategory.SpecBuff:
					bonuscat = target.SpecBuffBonusCategory;
					break;
				case (int)eBuffBonusCategory.Debuff:
					bonuscat = target.DebuffCategory;
					break;
				case (int)eBuffBonusCategory.Other:
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
		public abstract eProperty Property1
		{
			get;
		}

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
		/// Property 7 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property7
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 8 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property8
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 9 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property9
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 10 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property10
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Bonus Category where to change the Property1
		/// </summary>
		public abstract int BonusCategory1
		{
			get;
		}

		/// <summary>
		/// Bonus Category where to change the Property2
		/// </summary>
		public virtual int BonusCategory2
		{
			get { return BonusCategory1; }
		}

		/// <summary>
		/// Bonus Category where to change the Property3
		/// </summary>
		public virtual int BonusCategory3
		{
			get { return BonusCategory2; }
		}

		/// <summary>
		/// Bonus Category where to change the Property4
		/// </summary>
		public virtual int BonusCategory4
		{
			get { return BonusCategory3; }
		}

		/// <summary>
		/// Bonus Category where to change the Property5
		/// </summary>
		public virtual int BonusCategory5
		{
			get { return BonusCategory4; }
		}

		/// <summary>
		/// Bonus Category where to change the Property6
		/// </summary>
		public virtual int BonusCategory6
		{
			get { return BonusCategory5; }
		}

		/// <summary>
		/// Bonus Category where to change the Property7
		/// </summary>
		public virtual int BonusCategory7
		{
			get { return BonusCategory6; }
		}

		/// <summary>
		/// Bonus Category where to change the Property8
		/// </summary>
		public virtual int BonusCategory8
		{
			get { return BonusCategory7; }
		}

		/// <summary>
		/// Bonus Category where to change the Property9
		/// </summary>
		public virtual int BonusCategory9
		{
			get { return BonusCategory8; }
		}

		/// <summary>
		/// Bonus Category where to change the Property10
		/// </summary>
		public virtual int BonusCategory10
		{
			get { return BonusCategory9; }
		}

		/// <summary>
		/// Amount for Bonus 1 (Default to Spell.Value)
		/// </summary>
		public virtual int BonusAmount1
		{
			get { return (int)Spell.Value; }
		}
		
		/// <summary>
		/// Amount for Bonus 2
		/// </summary>
		public virtual int BonusAmount2
		{
			get { return BonusAmount1; }
		}
		
		/// <summary>
		/// Amount for Bonus 3
		/// </summary>
		public virtual int BonusAmount3
		{
			get { return BonusAmount2; }
		}
		
		/// <summary>
		/// Amount for Bonus 4
		/// </summary>
		public virtual int BonusAmount4
		{
			get { return BonusAmount3; }
		}
		
		/// <summary>
		/// Amount for Bonus 5
		/// </summary>
		public virtual int BonusAmount5
		{
			get { return BonusAmount4; }
		}
		
		/// <summary>
		/// Amount for Bonus 6
		/// </summary>
		public virtual int BonusAmount6
		{
			get { return BonusAmount5; }
		}
		
		/// <summary>
		/// Amount for Bonus 7
		/// </summary>
		public virtual int BonusAmount7
		{
			get { return BonusAmount6; }
		}
		
		/// <summary>
		/// Amount for Bonus 8
		/// </summary>
		public virtual int BonusAmount8
		{
			get { return BonusAmount7; }
		}
		
		/// <summary>
		/// Amount for Bonus 9
		/// </summary>
		public virtual int BonusAmount9
		{
			get { return BonusAmount8; }
		}
		
		/// <summary>
		/// Amount for Bonus 10
		/// </summary>
		public virtual int BonusAmount10
		{
			get { return BonusAmount9; }
		}
		
		/// <summary>
		/// Restore Effect
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="vars"></param>
		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			ApplyBonus(effect.Owner, BonusCategory1, Property1, (int)(vars[1]*0.01*BonusAmount1), false);
			ApplyBonus(effect.Owner, BonusCategory2, Property2, (int)(vars[1]*0.01*BonusAmount2), false);
			ApplyBonus(effect.Owner, BonusCategory3, Property3, (int)(vars[1]*0.01*BonusAmount3), false);
			ApplyBonus(effect.Owner, BonusCategory4, Property4, (int)(vars[1]*0.01*BonusAmount4), false);
			ApplyBonus(effect.Owner, BonusCategory5, Property5, (int)(vars[1]*0.01*BonusAmount5), false);
			ApplyBonus(effect.Owner, BonusCategory6, Property6, (int)(vars[1]*0.01*BonusAmount6), false);
			ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(vars[1]*0.01*BonusAmount7), false);
			ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(vars[1]*0.01*BonusAmount8), false);
			ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(vars[1]*0.01*BonusAmount9), false);
			ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(vars[1]*0.01*BonusAmount10), false);

			SendUpdates(effect.Owner);
		}

		/// <summary>
		/// Restored Effect Expires
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="vars"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			if (!noMessages && !Spell.IsPulsing)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}

			ApplyBonus(effect.Owner, BonusCategory1, Property1, (int)(vars[1]*0.01*BonusAmount1), true);
			ApplyBonus(effect.Owner, BonusCategory2, Property2, (int)(vars[1]*0.01*BonusAmount2), true);
			ApplyBonus(effect.Owner, BonusCategory3, Property3, (int)(vars[1]*0.01*BonusAmount3), true);
			ApplyBonus(effect.Owner, BonusCategory4, Property4, (int)(vars[1]*0.01*BonusAmount4), true);
			ApplyBonus(effect.Owner, BonusCategory5, Property5, (int)(vars[1]*0.01*BonusAmount5), true);
			ApplyBonus(effect.Owner, BonusCategory6, Property6, (int)(vars[1]*0.01*BonusAmount6), true);
			ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(vars[1]*0.01*BonusAmount7), true);
			ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(vars[1]*0.01*BonusAmount8), true);
			ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(vars[1]*0.01*BonusAmount9), true);
			ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(vars[1]*0.01*BonusAmount10), true);

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
		protected virtual void ApplyBonus(GameLiving owner,  int BonusCat, eProperty Property, int Value, bool IsSubstracted)
		{
			IPropertyIndexer tblBonusCat;
			if (Property != eProperty.Undefined)
			{
				tblBonusCat = GetBonusCategory(owner, BonusCat);
				if (IsSubstracted)
					tblBonusCat[Property] -= Value;
				else
					tblBonusCat[Property] += Value;
			}
		}
		
		/// <summary>
		/// Saved Effect
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
		{
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.Var2 = (int)(e.Effectiveness * 100);
			eff.SpellLine = SpellLine.KeyName;
			
			return eff;

		}

		// constructor
		public PropertyChangingSpell(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
