using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Events;
namespace DOL.GS.Spells
{

	[SpellHandlerAttribute("HereticPiercingMagic")]
	public class HereticPiercingMagic : SpellHandler
	{
        protected GameLiving focustarget = null;
        protected ArrayList m_focusTargets = null;
        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);
            focustarget = target;
        }
		public override int CalculateToHitChance(GameLiving target)
		{
			GameSpellEffect effect = FindEffectOnTarget(m_caster, "HereticPiercingMagic");
			int spellLevel;
			spellLevel = Spell.Level;
			if(effect != null)
			{
				spellLevel += (int)effect.Spell.Value;
			}
		
			int hitchance = 85 + ((spellLevel - target.Level) >> 1);
			if (!(m_caster is GamePlayer && target is GamePlayer)) 
			{
				// level mod
				hitchance -= (int)(m_caster.GetConLevel(target)*10);
			}
			return hitchance;
		}
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (m_focusTargets == null)
                m_focusTargets = new ArrayList();
            GameLiving living = effect.Owner as GameLiving;
            lock (m_focusTargets.SyncRoot)
            {
                if (!m_focusTargets.Contains(effect.Owner))
                    m_focusTargets.Add(effect.Owner);

                MessageToCaster("You concentrated on the spell!", eChatType.CT_Spell);
            }
        }
        protected virtual void BeginEffect()
        {
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.CastSpell, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventAction));
        }
        public void EventAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null) return;
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
                        if (effect != null)
                            effect.Cancel(false);
                    }
                }
            }
            MessageToCaster("You lose your concentration!", eChatType.CT_Spell);
            if (Spell.Pulse != 0 && Spell.Frequency > 0)
                CancelPulsingSpell(Caster, Spell.SpellType);

            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.CastSpell, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventAction));
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendInterruptAnimation(m_caster);
            }
        }
	
		public HereticPiercingMagic(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
