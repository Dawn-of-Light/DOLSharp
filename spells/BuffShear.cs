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
            int RandSpell = Util.Random(1, 30);
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
                #region All The rippable buffs
                switch(RandSpell)
                {
                        #region Acuity
                    case 1:
                    if (effect.SpellHandler.GetType().Equals(Acuity))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                    #endregion

                        #region BaseStr
                    case 2:
                    if (effect.SpellHandler.GetType().Equals(BaseStr))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region BaseDex
                    case 3:
                    if (effect.SpellHandler.GetType().Equals(BaseDex))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        return;
                        #endregion
                        
                        #region BaseCon
                    case 4:
                    if (effect.SpellHandler.GetType().Equals(BaseCon))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region StrCon
                    case 5:
                    if (effect.SpellHandler.GetType().Equals(StrCon))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        return;
                        #endregion
                        
                        #region DexQui
                    case 6:
                        if (effect.SpellHandler.GetType().Equals(DexQui))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region AF
                    case 7:
                        if (effect.SpellHandler.GetType().Equals(AF))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region HPRegen
                    case 8:
                        if (effect.SpellHandler.GetType().Equals(HPRegen))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Haste
                    case 9:
                        if (effect.SpellHandler.GetType().Equals(Haste))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region PowRegen
                    case 10:
                        if (effect.SpellHandler.GetType().Equals(PowRegen))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Unrupt
                    case 11:
                    if (effect.SpellHandler.GetType().Equals(Unrupt))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region WeapSkill
                    case 12:
                    if (effect.SpellHandler.GetType().Equals(WeapSkill))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region DPS
                    case 13:
                    if (effect.SpellHandler.GetType().Equals(DPS))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Evade
                    case 14:
                    if (effect.SpellHandler.GetType().Equals(Evade))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Parry
                    case 15:
                    if (effect.SpellHandler.GetType().Equals(Parry))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Cold
                    case 16:
                    if (effect.SpellHandler.GetType().Equals(Cold))

                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Energy
                    case 17:
                    if (effect.SpellHandler.GetType().Equals(Energy))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Crush
                    case 18:
                    if (effect.SpellHandler.GetType().Equals(Crush))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Thrust
                    case 19:
                    if (effect.SpellHandler.GetType().Equals(Thrust))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Slash
                    case 20: 
                    if (effect.SpellHandler.GetType().Equals(Slash))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Matter
                    case 21:
                    if (effect.SpellHandler.GetType().Equals(Matter))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Body
                    case 22:
                    if (effect.SpellHandler.GetType().Equals(Body))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Heat
                    case 23:
                    if (effect.SpellHandler.GetType().Equals(Heat))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region Spirit
                    case 24:
                    if (effect.SpellHandler.GetType().Equals(Spirit))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Body-Spirit-Energy
                    case 25:
                    if (effect.SpellHandler.GetType().Equals(BodySpiritEnergy))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Heat-Cold-Matter
                    case 26:
                    if (effect.SpellHandler.GetType().Equals(HeatColdMatter))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion

                        #region Crush-Slash-Thrust
                    case 27:
                    if (effect.SpellHandler.GetType().Equals(CrushSlashThrust))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region EndRegen
                    case 28:
                    if (effect.SpellHandler.GetType().Equals(EndRegen))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region DmgAdd
                    case 29:
                    if (effect.SpellHandler.GetType().Equals(DmgAdd))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                        
                        #region DmgShield
                    case 30:
                    if (effect.SpellHandler.GetType().Equals(DmgShield))
                        SendEffectAnimation(target, 0, false, 1);
                        effect.Cancel(false);
                        MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
                        MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
                        break;
                        #endregion
                }
                    SendEffectAnimation(target, 0, false, 0);
                    MessageToCaster("No enhancement of that type found on the target.", eChatType.CT_SpellResisted);
                #endregion
            }


			/*
			if (!noMessages) 
			{
				MessageToLiving(effect.Owner, effect.Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(effect.Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			*/
        }
        #region All the Types
        private static Type[] Acuity = new Type[] {typeof(AcuityBuff)};
        private static Type[] BaseStr = new Type[] {typeof(StrengthBuff)};
        private static Type[] BaseDex = new Type[] {typeof(DexterityBuff)};
        private static Type[] BaseCon = new Type[] {typeof(ConstitutionBuff)};
        private static Type[] StrCon = new Type[] {typeof(StrengthConBuff)};
        private static Type[] DexQui = new Type[] { typeof(DexterityQuiBuff) };
        private static Type[] AF = new Type[] { typeof(ArmorFactorBuff)};
        private static Type[] HPRegen = new Type[] {typeof(HealthRegenSpellHandler)};
        private static Type[] Haste = new Type[] {typeof(CombatSpeedBuff)};
        private static Type[] PowRegen = new Type[] {typeof(PowerRegenSpellHandler)};
        private static Type[] Unrupt = new Type[] {typeof(UninterruptableSpellHandler)};
        private static Type[] WeapSkill = new Type[] {typeof(WeaponSkillBuff)};
        private static Type[] DPS = new Type[] {typeof(DPSBuff)};
        private static Type[] Evade = new Type[] {typeof(EvadeChanceBuff)};
        private static Type[] Parry = new Type[] {typeof(ParryChanceBuff)};
        private static Type[] Cold = new Type[] {typeof(ColdResistBuff)};
        private static Type[] Energy = new Type[] {typeof(EnergyResistBuff)};
        private static Type[] Crush = new Type[] {typeof(CrushResistBuff)};
        private static Type[] Thrust = new Type[] {typeof(ThrustResistBuff)};
        private static Type[] Slash = new Type[] {typeof(SlashResistBuff)};
        private static Type[] Matter = new Type[] {typeof(MatterResistBuff)};
        private static Type[] Body = new Type[] {typeof(BodyResistBuff)};
        private static Type[] Heat = new Type[] {typeof(HeatResistBuff)};
        private static Type[] Spirit = new Type[] {typeof(SpiritResistBuff)};
        private static Type[] BodySpiritEnergy = new Type[] {typeof(BodySpiritEnergyBuff)};
        private static Type[] HeatColdMatter = new Type[] {typeof(HeatColdMatterBuff)};
        private static Type[] CrushSlashThrust = new Type[] {typeof(CrushSlashThrustBuff)};
        private static Type[] EndRegen = new Type[] {typeof(EnduranceRegenSpellHandler)};
        private static Type[] DmgAdd = new Type[] {typeof(DamageAddSpellHandler)};
        private static Type[] DmgShield = new Type[] {typeof(DamageShieldSpellHandler)};
        #endregion
		public RandomBuffShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}