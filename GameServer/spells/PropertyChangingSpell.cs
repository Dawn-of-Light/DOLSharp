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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell to change up to 3 property bonuses at once
	/// in one their specific given bonus category
	/// </summary>
	public abstract class PropertyChangingSpell : SpellHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Execute property changing spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// start changing effect on target
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			IPropertyIndexer bonuscat = GetBonusCategory(effect.Owner, BonusCategory1);

			int amount = (int)(Spell.Value * effect.Effectiveness);

			bonuscat[(int)Property1] += amount;

			if (Property2 != eProperty.Undefined)
			{
				bonuscat = GetBonusCategory(effect.Owner, BonusCategory2);
				bonuscat[(int)Property2] += amount;
			}
			if (Property3 != eProperty.Undefined)
			{
				bonuscat = GetBonusCategory(effect.Owner, BonusCategory3);
				bonuscat[(int)Property3] += amount;
			}

			SendUpdates(effect.Owner);

			eChatType toLiving = eChatType.CT_SpellPulse;
			eChatType toOther = eChatType.CT_SpellPulse;
			if (Spell.Pulse == 0 || !HasPositiveEffect)
			{
				toLiving = eChatType.CT_Spell;
				toOther = eChatType.CT_System;
				SendEffectAnimation(effect.Owner, 0, false, 1);
			}

			//messages are after buff and after "Your xxx has increased." messages
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
			if (!noMessages && Spell.Pulse == 0)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}

			IPropertyIndexer bonuscat = GetBonusCategory(effect.Owner, BonusCategory1);
			bonuscat[(int) Property1] -= (int) (Spell.Value*effect.Effectiveness);

			if (Property2 != eProperty.Undefined)
			{
				bonuscat = GetBonusCategory(effect.Owner, BonusCategory2);
				bonuscat[(int) Property2] -= (int) (Spell.Value*effect.Effectiveness);
			}
			if (Property3 != eProperty.Undefined)
			{
				bonuscat = GetBonusCategory(effect.Owner, BonusCategory3);
				bonuscat[(int) Property3] -= (int) (Spell.Value*effect.Effectiveness);
			}

			SendUpdates(effect.Owner);

			return 0;
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
					bonuscat = target.BuffBonusCategory1;
					break;
				case 2:
					bonuscat = target.BuffBonusCategory2;
					break;
				case 3:
					bonuscat = target.BuffBonusCategory3;
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

		// constructor
		public PropertyChangingSpell(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}
	}
}