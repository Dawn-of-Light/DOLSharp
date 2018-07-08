using System.Collections.Generic;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Trueshot grants 50% more range for next archery attack
    /// </summary>
    public class TrueshotEffect : StaticEffect
    {
        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                player.Out.SendMessage("You prepare a Trueshot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        public override string Name => "Trueshot";

        public override ushort Icon => 3004;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Grants 50% bonus to the next arrow fired. The arrow will penetrate and pop bladeturn."
                };

                return list;
            }
        }
    }
}