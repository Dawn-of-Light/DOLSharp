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
using DOL.AI.Brain;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    /// <summary>
    /// All stats debuff spell handler
    /// </summary>
    [SpellHandler("AllStatsDebuff")]
    public class AllStatsDebuff : SpellHandler
    {
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            effect.Owner.DebuffCategory[(int)eProperty.Dexterity] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Strength] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Constitution] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Acuity] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Piety] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Empathy] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Quickness] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Intelligence] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Charisma] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.ArmorAbsorption] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.MagicAbsorption] += (int)Spell.Value;

            if (effect.Owner is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.DebuffCategory[(int)eProperty.Dexterity] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Strength] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Constitution] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Acuity] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Piety] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Empathy] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Quickness] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Intelligence] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Charisma] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.ArmorAbsorption] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.MagicAbsorption] -= (int)Spell.Value;

            if (effect.Owner is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }

            if (target is GameNPC npc)
            {
                if (npc.Brain is IOldAggressiveBrain aggroBrain)
                {
                    aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
                }
            }
        }

        public AllStatsDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Lore debuff spell handler (Magic resist debuff)
    /// </summary>
    [SpellHandler("LoreDebuff")]
    public class LoreDebuff : SpellHandler
    {
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            effect.Owner.DebuffCategory[(int)eProperty.SpellDamage] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Heat] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Cold] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Matter] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Spirit] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Energy] += (int)Spell.Value;

            if (effect.Owner is GamePlayer player)
            {
                player.Out.SendCharResistsUpdate();
                player.UpdatePlayerStatus();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.DebuffCategory[(int)eProperty.SpellDamage] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Heat] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Cold] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Matter] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Spirit] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Energy] -= (int)Spell.Value;

            if (effect.Owner is GamePlayer player)
            {
                player.Out.SendCharResistsUpdate();
                player.UpdatePlayerStatus();
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }

            if (target is GameNPC npc)
            {
                if (npc.Brain is IOldAggressiveBrain aggroBrain)
                {
                    aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
                }
            }
        }

        public LoreDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Strength/Constitution drain spell handler
    /// </summary>
    [SpellHandler("StrengthConstitutionDrain")]
    public class StrengthConstitutionDrain : StrengthConDebuff
    {
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            Caster.BaseBuffBonusCategory[(int)eProperty.Strength] += (int)Spell.Value;
            Caster.BaseBuffBonusCategory[(int)eProperty.Constitution] += (int)Spell.Value;

            if (Caster is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            Caster.BaseBuffBonusCategory[(int)eProperty.Strength] -= (int)Spell.Value;
            Caster.BaseBuffBonusCategory[(int)eProperty.Constitution] -= (int)Spell.Value;

            if (Caster is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            }

            return base.OnEffectExpires(effect,noMessages);
        }

        public StrengthConstitutionDrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// ABS Damage shield spell handler
    /// </summary>
    [SpellHandler("ABSDamageShield")]
    public class ABSDamageShield : AblativeArmorSpellHandler
    {
        protected override void OnDamageAbsorbed(AttackData ad, int DamageAmount)
        {
            AttackData newad = new AttackData
            {
                Attacker = ad.Target,
                Target = ad.Attacker,
                Damage = DamageAmount,
                DamageType = Spell.DamageType,
                AttackType = AttackData.eAttackType.Spell,
                AttackResult = GameLiving.eAttackResult.HitUnstyled,
                SpellHandler = this
            };

            newad.Target.OnAttackedByEnemy(newad);
            newad.Attacker.DealDamage(newad);
        }

        public ABSDamageShield(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Morph spell handler
    /// </summary>
    [SpellHandler("Morph")]
    public class Morph : SpellHandler
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
           if (effect.Owner is GamePlayer player)
            {
                player.Model = (ushort)Spell.LifeDrainReturn;
                player.Out.SendUpdatePlayer();
            }

            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
           if (effect.Owner is GamePlayer player)
            {
                GameClient client = player.Client;
                player.Model = (ushort)client.Account.Characters[client.ActiveCharIndex].CreationModel;
                player.Out.SendUpdatePlayer();
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        public Morph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Arcane leadership spell handler (range+resist pierce)
    /// </summary>
    [SpellHandler("ArcaneLeadership")]
    public class ArcaneLeadership : CloudsongAuraSpellHandler
    {
        public ArcaneLeadership(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
