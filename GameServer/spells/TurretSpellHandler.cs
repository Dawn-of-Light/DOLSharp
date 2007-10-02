using System;
using System.Collections;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;
using DOL.GS.ServerProperties;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("Turret")]
    public class TurretSpellHandler : SpellHandler
    {
        protected int x, y, z;
        public TurretSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            int nCount = 0;
            foreach (GameNPC npc in Caster.CurrentRegion.GetNPCsInRadius(Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Properties.TURRET_AREA_CAP_RADIUS, false))
                if (npc.Brain is TurretBrain)
                    nCount++;
            if (nCount >= ServerProperties.Properties.TURRET_AREA_CAP_COUNT)
            {
                MessageToCaster("You can't summon anymore Turrets in this Area!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster.PetCounter >= ServerProperties.Properties.TURRET_PLAYER_CAP_COUNT)
            {
                MessageToCaster("You cannot control anymore Turrets!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster.GroundTarget == null)
            {
                MessageToCaster("You have to set a Areatarget for this Spell.", eChatType.CT_SpellResisted);
                return false;
            }

            if (!Caster.GroundTargetInView)
            {
                MessageToCaster("Your Areatarget is not in view.", eChatType.CT_SpellResisted);
                return false;
            }
            if (!WorldMgr.CheckDistance(Caster, Caster.GroundTarget, CalculateSpellRange()))
            {
                MessageToCaster("You have to select a closer Areatarget.", eChatType.CT_SpellResisted);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }
        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect effect = CreateSpellEffect(target, effectiveness);
            x = Caster.GroundTarget.X;
            y = Caster.GroundTarget.Y;
            z = Caster.GroundTarget.Z;

            if (Spell.SubSpellID != 0)
            {
                Spell subspell = SkillBase.GetSpellByID(m_spell.SubSpellID);

                TurretBrain controlledBrain = new TurretBrain(Caster, subspell);
                controlledBrain.WalkState = eWalkState.Stay;
                controlledBrain.IsMainPet = false;
                GameNPC turret = new GameNPC();
                turret.HealthMultiplicator = true;
                turret.SetOwnBrain(controlledBrain);
                turret.X = x;
                turret.Y = y;
                turret.Z = z;
                turret.CurrentRegion = Caster.CurrentRegion;
                turret.Realm = Caster.Realm;
                turret.MaxSpeedBase = 0;
                turret.Model = 860;
                if (Spell.Damage < 0) turret.Level = (byte)(Caster.Level * Spell.Damage * -0.01);
                else turret.Level = (byte)Spell.Damage;
                if (turret.Level > Spell.Level) turret.Level = (byte)Spell.Value;
                if (turret.Level > 44) turret.Level = 44;
                turret.Name = Spell.Name;
                turret.AddToWorld();
                turret.Brain.Start();
                effect.Start(turret);
                Caster.PetCounter++;
            }
            else
                return;
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
            if (effect.Owner.IsAlive)
            {
                //Pet remove
                effect.Owner.Health = 0; // to send proper remove packet
                effect.Owner.Delete();
            }
            Caster.PetCounter--;

            return 0;
        }
    }
}