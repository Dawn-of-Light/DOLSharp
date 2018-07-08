using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class WallOfFlameAbility : RR5RealmAbility
    {
        public WallOfFlameAbility(DBAbility dba, int level) : base(dba, level) { }

        private readonly int dmgValue = 400; // 400 Dmg
        private readonly uint duration = 15; // 15 Sec duration

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            base.Execute(living);

            if (!(living is GamePlayer caster))
            {
                return;
            }

            if (caster.IsMoving)
            {
                caster.Out.SendMessage("You must be standing still to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            foreach (GamePlayer iPlayer in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (iPlayer == caster)
                {
                    iPlayer.MessageToSelf($"You cast {Name}!", eChatType.CT_Spell);
                }
                else
                {
                    iPlayer.MessageFromArea(caster, $"{caster.Name} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }

                iPlayer.Out.SendSpellCastAnimation(caster, 7028, 20);
            }

            Statics.WallOfFlameBase wof = new Statics.WallOfFlameBase(dmgValue);
            Point3D targetSpot = new Point3D(caster.X, caster.Y, caster.Z);
            wof.CreateStatic(caster, targetSpot, duration, 3, 150);

            DisableSkill(living);
            caster.StopCurrentSpellcast();
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}