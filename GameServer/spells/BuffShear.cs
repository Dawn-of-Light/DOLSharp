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
/*
 * Made by Biceps
 * TODO: DD proc
 */
using System;
using System.Collections;
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Shears strength buff 
	/// </summary>
	[SpellHandlerAttribute("StrengthShear")]
	public class StrengthShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "StrengthBuff"; } }
		public override string DelveSpellType { get { return "Strength"; } }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameSpellEffect effect;
            effect = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (effect != null)
            {
                effect.Cancel(false);
                return;
            }
        }

		// constructor
		public StrengthShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears dexterity buff
	/// </summary>
	[SpellHandlerAttribute("DexterityShear")]
	public class DexterityShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "DexterityBuff"; } }
		public override string DelveSpellType { get { return "Dexterity"; } }
		// constructor
		public DexterityShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears constitution buff
	/// </summary>
	[SpellHandlerAttribute("ConstitutionShear")]
	public class ConstitutionShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "ConstitutionBuff"; } }
		public override string DelveSpellType { get { return "Constitution"; } }
		// constructor
		public ConstitutionShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears acuity buff
	/// </summary>
	[SpellHandlerAttribute("AcuityShear")]
	public class AcuityShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "AcuityBuff"; } }
		public override string DelveSpellType { get { return "Acuity"; } }
		// constructor
		public AcuityShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears str/con buff
	/// </summary>
	[SpellHandlerAttribute("StrengthConstitutionShear")]
	public class StrengthConstitutionShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "StrengthConstitutionBuff"; } }
		public override string DelveSpellType { get { return "Str/Con"; } }
		// constructor
		public StrengthConstitutionShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears dex/qui buff
	/// </summary>
	[SpellHandlerAttribute("DexterityQuicknessShear")]
	public class DexterityQuicknessShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "DexterityQuicknessBuff"; } }
		public override string DelveSpellType { get { return "Dex/Qui"; } }
		// constructor
		public DexterityQuicknessShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Base class for all buff shearing spells
	/// </summary>
	public abstract class AbstractBuffShear : SpellHandler
	{
		/// <summary>
		/// The spell type to shear
		/// </summary>
		public abstract string ShearSpellType { get; }

		/// <summary>
		/// The spell type shown in delve info
		/// </summary>
		public abstract string DelveSpellType { get; }

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;

			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			GameSpellEffect mez = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (mez != null)
            {
                mez.Cancel(false);
                return;
            }
			if (target is GameNPC)
			{
				GameNPC npc = (GameNPC)target;
				IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}

			//check for spell.
			foreach(GameSpellEffect effect in target.EffectList.GetAllOfType(typeof(GameSpellEffect)))
			{
				if (effect.Spell.SpellType == ShearSpellType)
				{
					if (effect.Owner == effect.SpellHandler.Caster || effect.Spell.Value > Spell.Value)
					{
						SendEffectAnimation(target, 0, false, 0);
						MessageToCaster("The target's connection to their enhancement is too strong for you to remove.", eChatType.CT_SpellResisted);
						return;
					}

					SendEffectAnimation(target, 0, false, 1);
					effect.Cancel(false);
					MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
					MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);

					return;
				}
			}

			SendEffectAnimation(target, 0, false, 0);
			MessageToCaster("No enhancement of that type found on the target.", eChatType.CT_SpellResisted);

			/*
			if (!noMessages) 
			{
				MessageToLiving(effect.Owner, effect.Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(effect.Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			*/
		}

		/// <summary>
		/// When spell was resisted
		/// </summary>
		/// <param name="target">the target that resisted the spell</param>
		protected override void OnSpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);
			if (Spell.Damage == 0 && Spell.CastTime == 0)
			{
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			}
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Potency Whack>
				Function: buff shear
 
				Destroys a positive enhancement on the target.
 
				Type: Str/Con
				Maximum strength of buffs removed: 150
				Target: Enemy realm players and controlled pets only
				Range: 1500
				Power cost: 12
				Casting time:      2.0 sec
				Damage: Body
 
				<End Info>
				*/

				ArrayList list = new ArrayList();

				list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("Type: " + DelveSpellType);
				list.Add("Maximum strength of buffs removed: " + Spell.Value);
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.DamageType != eDamageType.Natural) list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));

				return list;
			}
		}

		// constructor
		public AbstractBuffShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandlerAttribute("RandomBuffShear")]
	public class RandomBuffShear : SpellHandler
	{

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			if (target is GameNPC)
			{
				GameNPC npc = (GameNPC)target;
				IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}

			//check for spell.
			foreach (GameSpellEffect effect in target.EffectList.GetAllOfType(typeof(GameSpellEffect)))
			{
				foreach (Type buffType in buffs)
				{
					if (effect.SpellHandler.GetType().Equals(buffType))
					{
						SendEffectAnimation(target, 0, false, 1);
						effect.Cancel(false);
						MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
						MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
						return;
					}
				}
			}

			SendEffectAnimation(target, 0, false, 0);
			MessageToCaster("No enhancement of that type found on the target.", eChatType.CT_SpellResisted);

			/*
			if (!noMessages) 
			{
				MessageToLiving(effect.Owner, effect.Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(effect.Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			*/
		}

		private static Type[] buffs = new Type[] { typeof(AcuityBuff), typeof(StrengthBuff), typeof(DexterityBuff), typeof(ConstitutionBuff), typeof(StrengthConBuff), typeof(DexterityQuiBuff),
			typeof(ArmorFactorBuff),typeof(ArmorAbsorbtionBuff),typeof(HealthRegenSpellHandler),typeof(CombatSpeedBuff),typeof(PowerRegenSpellHandler),typeof(UninterruptableSpellHandler),typeof(WeaponSkillBuff),typeof(DPSBuff),typeof(EvadeChanceBuff),typeof(ParryChanceBuff),
			typeof(ColdResistBuff),typeof(EnergyResistBuff),typeof(CrushResistBuff),typeof(ThrustResistBuff),typeof(SlashResistBuff),typeof(MatterResistBuff),typeof(BodyResistBuff),typeof(HeatResistBuff),typeof(SpiritResistBuff),typeof(BodySpiritEnergyBuff),typeof(HeatColdMatterBuff),typeof(CrushSlashThrustBuff),
			typeof(EnduranceRegenSpellHandler),typeof(DamageAddSpellHandler),typeof(DamageShieldSpellHandler) };
		// constructor
		public RandomBuffShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}