using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class ChargeAbility : TimedRealmAbility
    {
        public ChargeAbility(DBAbility dba, int level) : base(dba, level) { }

        // no charge when snared
        public override bool CheckPreconditions(GameLiving living, long bitmask)
        {
            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellEffect oEffect)
                    {
                        if (oEffect.Spell.SpellType.ToLower().IndexOf("speeddecrease", StringComparison.Ordinal) != -1 && oEffect.Spell.Value != 99)
                        {
                            if (living is GamePlayer player)
                            {
                                player.Out.SendMessage("You may not use this ability while snared!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }

                            return true;
                        }
                    }
                }
            }

            return base.CheckPreconditions(living, bitmask);
        }

        public override void Execute(GameLiving living)
        {
            if (living == null)
            {
                return;
            }

            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (living.TempProperties.getProperty("Charging", false) || living.EffectList.CountOfType(typeof(SpeedOfSoundEffect), typeof(ArmsLengthEffect), typeof(ChargeEffect)) > 0)
            {
                if (living is GamePlayer gamePlayer)
                {
                    gamePlayer.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }

                return;
            }

            ChargeEffect charge = living.EffectList.GetOfType<ChargeEffect>();
            charge?.Cancel(false);

            if (living is GamePlayer player)
            {
                player.Out.SendUpdateMaxSpeed();
            }

            new ChargeEffect().Start(living);
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (level)
                {
                    case 1: return 900;
                    case 2: return 600;
                    case 3: return 300;
                    case 4: return 180;
                    case 5: return 90;
                    default: return 600;
                }
            }

            switch (level)
            {
                case 1: return 900;
                case 2: return 300;
                case 3: return 90;
            }

            return 600;
        }

        public override bool CheckRequirement(GamePlayer player)
        {
            return player.Level >= 45;
        }
    }
}
