using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.ServerRules;

namespace DOL.UnitTests.Gameserver
{
    public class FakePacketLib : PacketLib1124
    {
        public FakePacketLib() : base(null) { }

        public override void SendCheckLOS(GameObject Checker, GameObject Target, CheckLOSResponse callback)
        {
            //nothing
        }

        public override void SendMessage(string msg, eChatType type, eChatLoc loc)
        {
            //nothing
        }
    }

    public class FakeRegion : Region
    {

        public FakeRegion() : base(null, new RegionData()) { }

        public override long Time => -1;

        public override ushort ID => 0;
    }

    public class FakeServer : GameServer
    {
        protected override IServerRules ServerRulesImpl => new FakeServerRules();
        protected override void CheckAndInitDB() { }

        private class FakeServerRules : NormalServerRules
        {

            public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
            {
                return true;
            }
        }
    }
}
