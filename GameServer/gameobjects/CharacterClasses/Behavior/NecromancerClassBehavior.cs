using System;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Spells;

namespace DOL.GS
{
    public class NecromancerClassBehavior : DefaultClassBehavior
    {
        public NecromancerClassBehavior(GamePlayer player) : base(player) { }

        public override void Init()
        {
            Player.Model = (ushort)Player.Client.Account.Characters[Player.Client.ActiveCharIndex].CreationModel;
        }

        public override byte HealthPercentGroupWindow
        {
            get
            {
                if (Player.ControlledBrain == null)
                    return Player.HealthPercent;

                return Player.ControlledBrain.Body.HealthPercent;
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (Player.ControlledBrain != null)
            {
                GameNPC pet = Player.ControlledBrain.Body;

                if (pet != null && sender == pet && e == GameLivingEvent.CastStarting && args is CastingEventArgs)
                {
                    ISpellHandler spellHandler = (args as CastingEventArgs).SpellHandler;

                    if (spellHandler != null)
                    {
                        int powerCost = spellHandler.PowerCost(Player);

                        if (powerCost > 0)
                            Player.ChangeMana(Player, GameLiving.eManaChangeType.Spell, -powerCost);
                    }

                    return;
                }
            }

            base.Notify(e, sender, args);
        }
    }
}