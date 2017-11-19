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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Buffs two stats at once, goes into specline bonus category
    /// </summary>	
    public abstract class DualStatBuff : SingleStatBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.SpecBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.SpecBuff; } }

		/// <summary>
		/// Default Constructor
		/// </summary>
		protected DualStatBuff(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}

	/// <summary>
	/// Str/Con stat specline buff
	/// </summary>
	[SpellHandler("StrengthConstitutionBuff")]
	public class StrengthConBuff : DualStatBuff
	{
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirStrength)
        	   || target.HasAbility(Abilities.VampiirConstitution))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
		public override eProperty Property1 { get { return eProperty.Strength; } }	
		public override eProperty Property2 { get { return eProperty.Constitution; } }	

		// constructor
		public StrengthConBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Dex/Qui stat specline buff
	/// </summary>
	[SpellHandler("DexterityQuicknessBuff")]
	public class DexterityQuiBuff : DualStatBuff
	{
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirDexterity)
        	   || target.HasAbility(Abilities.VampiirQuickness))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
		public override eProperty Property1 { get { return eProperty.Dexterity; } }	
		public override eProperty Property2 { get { return eProperty.Quickness; } }	

		// constructor
		public DexterityQuiBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
