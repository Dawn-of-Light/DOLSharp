using System;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("Bomber")]
    public class BomberSpellHandler : SpellHandler
    {
        /// <summary>
        /// Holds the Target for the Spell
        /// </summary>
        private GameLiving m_bombertarget = null;

        public BomberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
            {
                MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
                return;
            }

            if (Spell.SubSpellID != 0)
            {
                ControlledNpc controlledBrain = new ControlledNpc(Caster);

                GameNPC bomber = new GameNPC(template);
                controlledBrain.IsMainPet = false;
                bomber.SetOwnBrain(controlledBrain);
                int x, y;
                Caster.GetSpotFromHeading(64, out x, out y);
                bomber.X = x;
                bomber.Y = y;
                bomber.Z = Caster.Z;
                bomber.CurrentRegion = Caster.CurrentRegion;
                bomber.Realm = Caster.Realm;
                //Temporaly fix
                bomber.Flags |= (uint)GameNPC.eFlags.PEACE;
                bomber.Level = Caster.Level;
                bomber.Name = Spell.Name;
                bomber.AddToWorld();

                if (m_spell.Duration > 0)
                {
                    GameSpellEffect effect = CreateSpellEffect(target, effectiveness);
                    effect.Start(bomber);
                }

                m_bombertarget = Caster.TargetObject as GameLiving;
                GameEventMgr.AddHandler(bomber, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(BomberArriveAtTarget));
                bomber.Follow(target, 10, Spell.Range);
            }
        }

        /// <summary>
        /// Called when the Bomber reaches his target
        /// </summary>
        private void BomberArriveAtTarget(DOLEvent e, object obj, EventArgs args)
        {
            GameNPC bomber = obj as GameNPC;
            GameEventMgr.RemoveHandler(bomber, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(BomberArriveAtTarget));
            if (bomber == null || m_bombertarget == null) return;

            GameLiving living = m_bombertarget as GameLiving;

            bomber.Health = 0;
            bomber.Delete();

            if (living != null)
            {
                Spell subspell = SkillBase.GetSpellByID(m_spell.SubSpellID);
                if (subspell != null)
                {
                    if (WorldMgr.CheckDistance(living, bomber, 500))
                    {
                        ISpellHandler spellhandler = Scripts.ScriptMgr.CreateSpellHandler(Caster, subspell, SkillBase.GetSpellLine(this.SpellLine.KeyName));
                        spellhandler.StartSpell(living);
                    }
                    else
                        return;
                }
                else
                {
                    if (log.IsErrorEnabled)
                        log.Error("Bomber Subspell: " + subspell.ID + " is not implemented yet");
                    return;
                }
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
            GameEventMgr.RemoveHandler(effect.Owner, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(BomberArriveAtTarget));
            effect.Owner.Health = 0;
            effect.Owner.Delete();
            return 0;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }
    }
}