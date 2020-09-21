using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.ServerRules;

namespace DOL.UnitTests.Gameserver
{
    public class FakePacketLib : PacketLib1124
    {
        public FakePacketLib() : base(null) { }

        public override void SendCheckLOS(GameObject Checker, GameObject Target, CheckLOSResponse callback) { }
        public override void SendMessage(string msg, eChatType type, eChatLoc loc) { }
        public override void SendUpdateIcons(System.Collections.IList changedEffects, ref int lastUpdateEffectsCount) { }
        public override void SendConcentrationList() { }
        public override void SendCharStatsUpdate() { }
        public override void SendUpdateWeaponAndArmorStats() { }
        public override void SendUpdateMaxSpeed() { }
        public override void SendEncumberance() { }
        public override void SendStatusUpdate() { }
    }

    public class FakeRegion : Region
    {
        public long fakeElapsedTime { get { return fakeTimeManager.fakeTime; } set { fakeTimeManager.fakeTime = value; } }
        public FakeTimeManager fakeTimeManager = new FakeTimeManager();

        public FakeRegion() : base(null, new RegionData()) { }

        public override long Time => TimeManager.CurrentTime;

        public override ushort ID => 0;

        public override GameTimer.TimeManager TimeManager => fakeTimeManager;
    }

    public class FakeTimeManager : GameTimer.TimeManager
    {
        public long fakeTime = -1;

        public FakeTimeManager() : base("FakeTimer") { }

        public override long CurrentTime => fakeTime;
        public override bool Start() => true;
        protected override void InsertTimer(GameTimer t, int offsetTick) { }
        protected override void RemoveTimer(GameTimer t) { }

        protected override void Init() { }
    }

    public class FakeServer : GameServer
    {
        public FakeServerRules FakeServerRules = new FakeServerRules();

        protected override IServerRules ServerRulesImpl => FakeServerRules;
        protected override void CheckAndInitDB() { }

        public static FakeServer LoadAndReturn()
        {
            var fakeServer = new FakeServer();
            LoadTestDouble(fakeServer);
            return fakeServer;
        }
    }

    public class FakeServerRules : NormalServerRules
    {
        public bool fakeIsAllowedToAttack = false;

        public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
        {
            return fakeIsAllowedToAttack;
        }
    }

    public class UtilChanceIsHundredPercent : Util
    {
        protected override int RandomImpl(int min, int max) => 100;

        public static void Enable()
        {
            Util.LoadTestDouble(new UtilChanceIsHundredPercent());
        }
    }
}
