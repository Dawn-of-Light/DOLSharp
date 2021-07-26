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
using System.Text;
using DOL.GS.Effects;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.PropertyCalc;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler to summon a necromancer pet.
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandler("SummonNecroPet")]
	public class SummonNecromancerPet : SummonSpellHandler
	{
		public SummonNecromancerPet(GameLiving caster, Spell spell, SpellLine line) 
			: base(caster, spell, line) { }

		private int m_summonConBonus;
		private int m_summonHitsBonus;

		/// <summary>
		/// Note bonus constitution and bonus hits from items, then 
		/// summon the pet.
		/// </summary>
		public override bool CastSpell()
		{
			// First check current item bonuses for constitution and hits
            // (including cap increases) of the caster, bonuses from
			// abilities such as Toughness will transfer as well.

			int hitsCap = MaxHealthCalculator.GetItemBonusCap(Caster) 
			    + MaxHealthCalculator.GetItemBonusCapIncrease(Caster);

			m_summonConBonus = Caster.GetModifiedFromItems(eProperty.Constitution);
			m_summonHitsBonus = Math.Min(Caster.ItemBonus[(int)(eProperty.MaxHealth)], hitsCap)
				+ Caster.AbilityBonus[(int)(eProperty.MaxHealth)]; ;

            // Now summon the pet.

			return base.CastSpell();
		}

        /// <summary>
        /// Check if caster is already in shade form.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (FindStaticEffectOnTarget(Caster, typeof(ShadeEffect)) != null)
            {
                MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonNecromancerPet.CheckBeginCast.ShadeEffectIsNotNull"), eChatType.CT_System);
                return false;
            }
			if (Caster is GamePlayer && Caster.ControlledBrain != null)
			{
                MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "Summon.CheckBeginCast.AlreadyHaveaPet"), eChatType.CT_SpellResisted);
                return false;
			}
            return base.CheckBeginCast(selectedTarget);
        }

		/// <summary>
		/// Necromancer RR5 ability: Call of Darkness
		/// When active, the necromancer can summon a pet with only a 3 second cast time. 
		/// The effect remains active for 15 minutes, or until a pet is summoned.
		/// </summary>
		/// <returns></returns>
		public override int CalculateCastingTime()
		{
			if (Caster.EffectList.GetOfType<CallOfDarknessEffect>() != null)
				return 3000;

			return base.CalculateCastingTime();
		}

		/// <summary>
		/// Create the pet and transfer stats.
		/// </summary>
		/// <param name="target">Target that gets the effect</param>
		/// <param name="effectiveness">Factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);

			if (Caster is GamePlayer)
				(Caster as GamePlayer).Shade(true);

			// Cancel RR5 Call of Darkness if on caster.

			IGameEffect callOfDarkness = FindStaticEffectOnTarget(Caster, typeof(CallOfDarknessEffect));
			if (callOfDarkness != null)
				callOfDarkness.Cancel(false);
		}

		/// <summary>
		/// Delve info string.
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var delve = new List<string>();
                delve.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonNecromancerPet.DelveInfo.Function"));
				delve.Add("");
                delve.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonNecromancerPet.DelveInfo.Description"));
				delve.Add("");
                delve.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonNecromancerPet.DelveInfo.Target", Spell.Target));
                delve.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonNecromancerPet.DelveInfo.Power", Math.Abs(Spell.Power)));
                delve.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonNecromancerPet.DelveInfo.CastingTime", (Spell.CastTime / 1000).ToString("0.0## " + LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SpellHandler.DelveInfo.Sec"))));
				return delve;
			}
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new NecromancerPetBrain(owner);
		}

		protected override GamePet GetGamePet(INpcTemplate template)
		{
			return new NecromancerPet(template, m_summonConBonus, m_summonHitsBonus);
		}
	}
}
