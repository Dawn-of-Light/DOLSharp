using System;
using System.Linq;
using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS
{
    public class BainsheeMorphEffect
    {
        protected const int WRAITH_FORM_RESET_DELAY = 30000;
        protected RegionTimerAction<GamePlayer> m_WraithTimerAction;
        protected DOLEventHandler m_WraithTriggerEvent;
        private GamePlayer player;

        public BainsheeMorphEffect(GamePlayer player) 
        {
            this.player = player;
            Init();
        }

        //Bainshee Transform While Casting.
        public void Init()
        {
            // Add Cast Listener.
            m_WraithTimerAction = new RegionTimerAction<GamePlayer>(player, pl => { if (pl.CharacterClass.Equals(CharacterClass.Bainshee)) TurnOutOfWraith(); });
            m_WraithTriggerEvent = new DOLEventHandler(TriggerUnWraithForm);
            GameEventMgr.AddHandler(player, GamePlayerEvent.CastFinished, new DOLEventHandler(TriggerWraithForm));
        }

        protected virtual void TriggerWraithForm(DOLEvent e, object sender, EventArgs arguments)
        {
            var player = sender as GamePlayer;

            if (player != this.player)
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
            if (player != this.player)
                return;
            TurnOutOfWraith(true);
        }

        public virtual void TurnInWraith()
        {
            if (player == null) return;

            if (m_WraithTimerAction.IsAlive)
            {
                m_WraithTimerAction.Stop();
            }
            else
            {
                switch (player.Race)
                {
                    case 11: player.Model = 1885; break; //Elf
                    case 12: player.Model = 1884; break; //Lurikeen
                    case 9:
                    default: player.Model = 1883; break; //Celt
                }

                GameEventMgr.AddHandler(player, GamePlayerEvent.RemoveFromWorld, m_WraithTriggerEvent);
            }

            m_WraithTimerAction.Start(WRAITH_FORM_RESET_DELAY);
        }

        public void TurnOutOfWraith()
        {
            TurnOutOfWraith(false);
        }

        public virtual void TurnOutOfWraith(bool forced)
        {
            if (player == null)
                return;

            // Keep Wraith Form if Pulsing Offensive Spell Running
            if (!forced && player.ConcentrationEffects.OfType<PulsingSpellEffect>().Any(pfx => pfx.SpellHandler != null && !pfx.SpellHandler.HasPositiveEffect))
            {
                TurnInWraith();
                return;
            }

            if (m_WraithTimerAction.IsAlive)
                m_WraithTimerAction.Stop();

            GameEventMgr.RemoveHandler(player, GamePlayerEvent.RemoveFromWorld, m_WraithTriggerEvent);

            player.Model = (ushort)player.Client.Account.Characters[player.Client.ActiveCharIndex].CreationModel;

        }
    }
}