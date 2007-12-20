using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    #region Battlemaster-1
    [SpellHandlerAttribute("MLEndudrain")]
    public class MLEndudrain : MasterlevelHandling
    {
        public MLEndudrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }


        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }


        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            int end = (int)(Spell.Damage);
            target.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, (-end));

            if (target is GamePlayer)
            {
                ((GamePlayer)target).Out.SendMessage(m_caster.Name + " steal you for " + end + " endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }

            target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
        }
    }
    #endregion

    #region Battlemaster-2
    [SpellHandlerAttribute("KeepDamageBuff")]
    public class KeepDamageBuff : MasterlevelBuffHandling
    {
        public override eProperty Property1 { get { return eProperty.KeepDamage; } }

        public KeepDamageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Battlemaster-3
    [SpellHandlerAttribute("MLManadrain")]
    public class MLManadrain : MasterlevelHandling
    {
        public MLManadrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            int mana = (int)(Spell.Damage);
            target.ChangeMana(target, GameLiving.eManaChangeType.Spell, (-mana));

            target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
        }
    }
    #endregion

    #region Battlemaster-4
    [SpellHandlerAttribute("Grapple")]
    public class Grapple : MasterlevelHandling
    {
        private int check = 0;
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (selectedTarget is GameNPC == true)
            {
                MessageToCaster("This spell works only on realm enemys.", eChatType.CT_SpellResisted);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                if (player.EffectList.GetOfType(typeof(ChargeEffect)) == null && player != null)
                {
                    effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 0);
                    player.Client.Out.SendUpdateMaxSpeed();
                    check = 1;
                }
                effect.Owner.StopAttack();
                effect.Owner.StopCurrentSpellcast();
                effect.Owner.IsDisarmed = true;
            }
            base.OnEffectStart(effect);
        }

        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }

        public override void FinishSpellCast(GameLiving target)
        {
            if (m_spell.SubSpellID > 0)
            {
                Spell spell = SkillBase.GetSpellByID(m_spell.SubSpellID);
                if (spell != null && spell.SubSpellID == 0)
                {
                    ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_caster, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                    spellhandler.StartSpell(Caster);
                }
            }
            base.FinishSpellCast(target);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner == null) return 0;

            base.OnEffectExpires(effect, noMessages);

            GamePlayer player = effect.Owner as GamePlayer;

            if (check > 0 && player != null)
            {
                effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, effect);
                player.Client.Out.SendUpdateMaxSpeed();
            }

            effect.Owner.IsDisarmed = false;
            return 0;
        }

        public Grapple(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Battlemaster-8
    [SpellHandlerAttribute("BodyguardHandler")]
    public class BodyguardHandler : SpellHandler
    {
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }
        public BodyguardHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Stylhandler
    [SpellHandlerAttribute("StyleHandler")]
    public class StyleHandler : MasterlevelHandling
    {
        public StyleHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
}

#region KeepDamageCalc

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The melee damage bonus percent calculator
    ///
    /// BuffBonusCategory1 is used for buffs
    /// BuffBonusCategory2 unused
    /// BuffBonusCategory3 is used for debuff
    /// BuffBonusCategory4 unused
    /// BuffBonusMultCategory1 unused
    /// </summary>
    [PropertyCalculator(eProperty.KeepDamage)]
    public class KeepDamagePercentCalculator : PropertyCalculator
    {
        public override int CalcValue(GameLiving living, eProperty property)
        {
            int percent = 100
                +living.BuffBonusCategory1[(int)property];

            return percent;
        }
    }
}

#endregion