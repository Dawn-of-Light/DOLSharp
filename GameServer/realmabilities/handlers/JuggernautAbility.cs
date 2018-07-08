using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS.RealmAbilities
{
    public class JuggernautAbility : TimedRealmAbility
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JuggernautAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _range = 1500;
        private int _duration = 60;
        private byte _value;

        public override void Execute(GameLiving living)
        {
            if (!(living is GamePlayer player))
            {
                Log.Warn("Could not retrieve player in JuggernautAbilityHandler.");
                return;
            }

            if (!living.IsAlive)
            {
                player.Out.SendMessage("You cannot use this ability while dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            if (living.IsMezzed)
            {
                player.Out.SendMessage("You cannot use this ability while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            if (living.IsStunned)
            {
                player.Out.SendMessage("You cannot use this ability while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            if (living.IsSitting)
            {
                player.Out.SendMessage("You cannot use this ability while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            if (living.ControlledBrain == null)
            {
                player.Out.SendMessage("You must have a pet controlled to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            if (!living.IsWithinRadius(player.ControlledBrain.Body, _range))
            {
                player.Out.SendMessage("Your pet is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            GameSpellEffect ml9 = SpellHandler.FindEffectOnTarget(living.ControlledBrain.Body,"SummonMastery");
            if (ml9 != null)
            {
                player.Out.SendMessage("Your Pet already has an ability of this type active", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1:
                        _value = 10;
                        break;
                    case 2:
                        _value = 15;
                        break;
                    case 3:
                        _value = 20;
                        break;
                    case 4:
                        _value = 25;
                        break;
                    case 5:
                        _value = 30;
                        break;
                    default:
                        return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1:
                        _value = 10;
                        break;
                    case 2:
                        _value = 20;
                        break;
                    case 3:
                        _value = 30;
                        break;
                    default:
                        return;
                }
            }

            new JuggernautEffect().Start(living, _duration, _value);

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }
    }
}
