using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    // http://www.camelotherald.com/masterlevels/ma.php?ml=Banelord
    // shared timer 1
    [SpellHandler("CastingSpeedDebuff")]
    public class CastingSpeedDebuff : MasterlevelDebuffHandling
    {
        public override eProperty Property1 => eProperty.CastingSpeed;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        // constructor
        public CastingSpeedDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 5 for ml2 - shared timer 3 for ml8
    [SpellHandler("PBAEDamage")]
    public class PBAEDamage : MasterlevelHandling
    {
        // constructor
        public PBAEDamage(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);

            // For Banelord ML 8, it drains Life from the Caster
            if (Spell.Damage > 0)
            {
                var chealth = Caster.Health * (int)Spell.Damage / 100;

                if (Caster.Health < chealth)
                {
                    chealth = 0;
                }

                Caster.Health -= chealth;
            }

            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            GamePlayer player = target as GamePlayer;
            if (target is GamePlayer)
            {
                int value = (int)Spell.Value;
                var mana = player.Mana * value / 100;
                var end = player.Endurance * value / 100;
                var health = player.Health * value / 100;

                // You don't gain RPs from this Spell
                if (player.Health < health)
                {
                    player.Health = 1;
                }
                else
                {
                    player.Health -= health;
                }

                if (player.Mana < mana)
                {
                    player.Mana = 1;
                }
                else
                {
                    player.Mana -= mana;
                }

                if (player.Endurance < end)
                {
                    player.Endurance = 1;
                }
                else
                {
                    player.Endurance -= end;
                }

                GameSpellEffect effect2 = FindEffectOnTarget(target, "Mesmerize");
                if (effect2 != null)
                {
                    effect2.Cancel(true);
                    return;
                }

                foreach (GamePlayer _ in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    SendEffectAnimation(player, 0, false, 1);
                }

                player.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            }
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 25;
        }
    }

    // shared timer 3
    [SpellHandler("Oppression")]
    public class OppressionSpellHandler : MasterlevelHandling
    {
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return true;
        }

        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner is GamePlayer player)
            {
                player.UpdateEncumberance();
            }

            effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect mezz = FindEffectOnTarget(target, "Mesmerize");
            mezz?.Cancel(false);

            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GamePlayer player)
            {
                player.UpdateEncumberance();
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        public OppressionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 1
    [SpellHandler("MLFatDebuff")]
    public class MLFatDebuffHandler : MasterlevelDebuffHandling
    {
        public override eProperty Property1 => eProperty.FatigueConsumption;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect effect2 = FindEffectOnTarget(target, "Mesmerize");
            if (effect2 != null)
            {
                effect2.Cancel(false);
                return;
            }

            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            base.OnEffectStart(effect);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public MLFatDebuffHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }

    // shared timer 5
    [SpellHandler("MissHit")]
    public class MissHit : MasterlevelBuffHandling
    {
        public override eProperty Property1 => eProperty.MissHit;

        // constructor
        public MissHit(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 1

    [SpellHandler("MLUnbreakableSnare")]
    public class MLUnbreakableSnare : BanelordSnare
    {
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            int duration = Spell.Duration;
            if (duration < 1)
            {
                duration = 1;
            }
            else if (duration > (Spell.Duration * 4))
            {
                duration = Spell.Duration * 4;
            }

            return duration;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public MLUnbreakableSnare(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }
    }

    [SpellHandler("UnrresistableNonImunityStun")]
    public class UnrresistableNonImunityStun : MasterlevelHandling
    {
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.CCImmunity) || target.HasAbility(Abilities.StunImmunity))
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }

            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.IsStunned = true;
            effect.Owner.StopAttack();
            effect.Owner.StopCurrentSpellcast();
            effect.Owner.DisableTurning(true);

            SendEffectAnimation(effect.Owner, 0, false, 1);

            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
            MessageToCaster(Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner, Caster);

            if (effect.Owner is GamePlayer player)
            {
                player.Client.Out.SendUpdateMaxSpeed();
                player.Group?.UpdateMember(player, false, false);
            }
            else
            {
                effect.Owner.StopAttack();
            }

            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.IsStunned = false;
            effect.Owner.DisableTurning(false);

            if (effect.Owner == null)
            {
                return 0;
            }

            if (effect.Owner is GamePlayer player)
            {
                player.Client.Out.SendUpdateMaxSpeed();
                player.Group?.UpdateMember(player, false, false);
            }
            else
            {
                if (effect.Owner is GameNPC npc && npc.Brain is IOldAggressiveBrain aggroBrain)
                {
                    aggroBrain.AddToAggroList(Caster, 1);
                }
            }

            return 0;
        }

        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0 || compare.Spell.EffectGroup != 0)
            {
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            }

            if (compare.Spell.SpellType == "UnrresistableNonImunityStun")
            {
                return true;
            }

            return base.IsOverwritable(compare);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override bool HasPositiveEffect => false;

        public UnrresistableNonImunityStun(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }
    }

    // shared timer 3
    [SpellHandler("BLToHit")]
    public class BLToHit : MasterlevelBuffHandling
    {
        public override eProperty Property1 => eProperty.ToHitBonus;

        // constructor
        public BLToHit(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 5
    [SpellHandler("EffectivenessDebuff")]
    public class EffectivenessDeBuff : MasterlevelHandling
    {
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            if (effect.Owner is GamePlayer player)
            {
                player.Effectiveness -= Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
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
            if (effect.Owner is GamePlayer player)
            {
                player.Effectiveness += Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }

            return 0;
        }

        public EffectivenessDeBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("Banespike")]
    public class BanespikeHandler : MasterlevelBuffHandling
    {
        // ReSharper disable once ArrangeAccessorOwnerBody
        public override eProperty Property1 => eProperty.MeleeDamage;

        public BanespikeHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}

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
    [PropertyCalculator(eProperty.MissHit)]
    public class MissHitPercentCalculator : PropertyCalculator
    {
        public override int CalcValue(GameLiving living, eProperty property)
        {
            return +living.BaseBuffBonusCategory[(int)property]
                   + living.SpecBuffBonusCategory[(int)property]
                   - living.DebuffCategory[(int)property]
                   + living.BuffBonusCategory4[(int)property];
        }
    }
}