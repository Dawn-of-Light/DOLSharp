using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Spells
{

    [SpellHandler("HereticPiercingMagic")]
    public class HereticPiercingMagic : SpellHandler
    {
        protected GameLiving focustarget;
        protected ArrayList m_focusTargets;

        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);
            focustarget = target;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (m_focusTargets == null)
            {
                m_focusTargets = new ArrayList();
            }
            
            lock (m_focusTargets.SyncRoot)
            {
                if (!m_focusTargets.Contains(effect.Owner))
                {
                    m_focusTargets.Add(effect.Owner);
                }

                MessageToCaster("You concentrated on the spell!", eChatType.CT_Spell);
            }
        }

        protected virtual void BeginEffect()
        {
            GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventAction));
        }

        public void EventAction(DOLEvent e, object sender, EventArgs args)
        {
            if (!(sender is GameLiving))
            {
                return;
            }

            MessageToCaster("You lose your concentration!", eChatType.CT_SpellExpires);
            RemoveEffect();
        }

        protected virtual void RemoveEffect()
        {
            if (m_focusTargets != null)
            {
                lock (m_focusTargets.SyncRoot)
                {
                    foreach (GameLiving living in m_focusTargets)
                    {
                        GameSpellEffect effect = FindEffectOnTarget(living, this);
                        effect?.Cancel(false);
                    }
                }
            }

            MessageToCaster("You lose your concentration!", eChatType.CT_Spell);
            if (Spell.Pulse != 0 && Spell.Frequency > 0)
            {
                CancelPulsingSpell(Caster, Spell.SpellType);
            }

            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventAction));
            foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendInterruptAnimation(Caster);
            }
        }

        public HereticPiercingMagic(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
