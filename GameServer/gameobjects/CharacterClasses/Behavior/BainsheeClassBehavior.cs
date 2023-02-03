using System;
using System.Linq;
using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS
{
    public class BainsheeClassBehavior : DefaultClassBehavior
    {
        protected const int WRAITH_FORM_RESET_DELAY = 30000;
        protected RegionTimerAction<GamePlayer> m_WraithTimerAction;
        protected DOLEventHandler m_WraithTriggerEvent;

        // Bainshee Transform While Casting.
        public override void Init()
        {
            // Add Cast Listener.
            m_WraithTimerAction = new RegionTimerAction<GamePlayer>(Player, pl => { if (pl.CharacterClass.Equals(CharacterClass.Bainshee)) TurnOutOfWraith(); });
            m_WraithTriggerEvent = new DOLEventHandler(TriggerUnWraithForm);
            GameEventMgr.AddHandler(Player, GamePlayerEvent.CastFinished, new DOLEventHandler(TriggerWraithForm));
        }

        protected virtual void TriggerWraithForm(DOLEvent e, object sender, EventArgs arguments)
        {
            var player = sender as GamePlayer;

            if (player != Player)
                return;

            var args = arguments as CastingEventArgs;

            if (args == null || args.SpellHandler == null)
                return;


            if (!args.SpellHandler.HasPositiveEffect)
                TurnInWraith();
        }

        protected virtual void TriggerUnWraithForm(DOLEvent e, object sender, EventArgs arguments)
        {
            GamePlayer player = sender as GamePlayer;
            if (player != Player)
                return;
            TurnOutOfWraith(true);
        }

        public virtual void TurnInWraith()
        {
            if (Player == null) return;

            if (m_WraithTimerAction.IsAlive)
            {
                m_WraithTimerAction.Stop();
            }
            else
            {
                switch (Player.Race)
                {
                    case 11: Player.Model = 1885; break; //Elf
                    case 12: Player.Model = 1884; break; //Lurikeen
                    case 9:
                    default: Player.Model = 1883; break; //Celt
                }

                GameEventMgr.AddHandler(Player, GamePlayerEvent.RemoveFromWorld, m_WraithTriggerEvent);
            }

            m_WraithTimerAction.Start(WRAITH_FORM_RESET_DELAY);
        }

        public void TurnOutOfWraith()
        {
            TurnOutOfWraith(false);
        }

        public virtual void TurnOutOfWraith(bool forced)
        {
            if (Player == null)
                return;

            // Keep Wraith Form if Pulsing Offensive Spell Running
            if (!forced && Player.ConcentrationEffects.OfType<PulsingSpellEffect>().Any(pfx => pfx.SpellHandler != null && !pfx.SpellHandler.HasPositiveEffect))
            {
                TurnInWraith();
                return;
            }

            if (m_WraithTimerAction.IsAlive)
                m_WraithTimerAction.Stop();

            GameEventMgr.RemoveHandler(Player, GamePlayerEvent.RemoveFromWorld, m_WraithTriggerEvent);

            Player.Model = (ushort)Player.Client.Account.Characters[Player.Client.ActiveCharIndex].CreationModel;

        }
    }
}