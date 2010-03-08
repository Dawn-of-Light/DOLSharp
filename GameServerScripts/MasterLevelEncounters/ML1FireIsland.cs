//TODO: make sunkaio do a small AOE fire attack when he's hit by a player with melee damage.

using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Events;
using log4net;
using System.Reflection;
using DOL.GS.Atlantis;

namespace DOL.GS
{
    public class Ianetor : GameNPC
    {
        public static int albregion = 73;
        public static int hibregion = 130;
        public static int midregion = 30;
        public static int playerregion;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool InProgress = false;

        public static List<GameNPC> WallofFireList = new List<GameNPC>();
        public static List<GameNPC> StairGuardList = new List<GameNPC>();
        public static List<GameNPC> BossList = new List<GameNPC>(); //holds Sunkaio, Zopureo, and Aithos.
        public static GameNPC Zop = null;
        public static GameNPC Aith = null;
        public static GameNPC Sun = null;
        public static List<GameNPC> PurrosList = new List<GameNPC>();

        public static int[,] FirePosition = {
            //  0       1      2
            {430071, 543930, 8014}, //0---Fire 1
            {431310, 545681, 7996}, //1---Fire 2
            {434042, 543672, 8017}, //2---Fire 3
            {433184, 542018, 8003}, //3---Fire 4
            {433571, 542116, 8012}, //4---Fire 5
            {431715, 541725, 8049}, //5---Fire 6
            {432352, 541795, 7999}, //6---Fire 7
            {432752, 541895, 8015}, //7---Fire 8
            {434002, 542787, 8018}, //8---Fire 9
            {434033, 543234, 8016}, //9---Fire 10
            {433945, 542373, 8015}, //10--Fire 11
            {431900, 541686, 8004}, //11--Fire 12
            {432810, 546048, 8003}, //12--Fire 13
            {434072, 544105, 8010}, //13--Fire 14
            {432407, 546082, 7996}, //14--Fire 15
            {433402, 545622, 8006}, //15--Fire 16
            {434000, 544544, 8014}, //16--Fire 17
            {433690, 545313, 7994}, //17--Fire 18
            {433908, 544952, 8004}, //18--Fire 19
            {431695, 545857, 8006}, //19--Fire 20
            {432063, 546003, 8003}, //20--Fire 21
            {430422, 544720, 8000}, //21--Fire 22
            {430701, 542342, 8018}, //22--Fire 23
            {430643, 545085, 8009}, //23--Fire 24
            {430456, 542668, 8014}, //24--Fire 25
            {430217, 544355, 8002}, //25--Fire 26
            {430971, 545380, 7996}, //26--Fire 27
            {430260, 543081, 8017}, //27--Fire 28
            {431078, 542017, 8011}, //28--Fire 29
            {430063, 543504, 8015}  //29--Fire 30
        };

        public static int[,] GuardPosition = {
            //  0       1      2
            {431968, 542676, 8254}, //0---Guard 1
            {431741, 542806, 8256}, //1---Guard 2 Heading 3720
            {431592, 542038, 8361}, //2---Guard 3
            {431374, 542159, 8362}, //3---Guard 4
            //-----------------------------------
            {432559, 545313, 8222 }, //4---Guard 5
            {432856, 545149, 8228 }, //5---Guard 6 Heading 1735
            {433077, 545623, 8361 }, //6---Guard 7
            {432867, 545749, 8361 }  //7---Guard 8
        };


        //Blank for mobs that don't respawn.
        public override void StartRespawn()
        {
        }
        //Don't save into database
        public override void SaveIntoDatabase()
        {
        }

        public void BeginEncounter(GamePlayer player)
        {
            if (InProgress)
            {
                player.Out.SendMessage("Ianetor says \"I'm conducting a trail already in progress (possibly in another realm) at the moment.  Please try again in a little while.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return;
            }
            else 
                SayTo(player, "Very well, proceed to the isle at the South end of this bridge.  Your fight shall begin in one minute.  Make sure that the area is clear!");
            foreach (GamePlayer say in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                say.Out.SendMessage("Ianetor says, \"Anyone who doesn't wish to face my challenge of fire must leave Firestorm Island immediately or face death!  The game will begin in one minute!  Those who accepted my challenge must now move onto Firestorm Island!\"", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
            }
            
                InProgress = true;
                playerregion = player.CurrentRegionID;
                new RegionTimer(this, new RegionTimerCallback(ActivateEncounter), (60 * 1000));
            return;
        }
        //60 seconds have passed since whispering 'begin' to Ianetor.
        public int ActivateEncounter(RegionTimer timer)
        {
            SpawnEncounter();
            new RegionTimer(this, new RegionTimerCallback(DeSpawnEncounter), (900 * 1000)); //Despawn after 15 minutes, no matter what happens
            return 0;
        }

        public void SpawnEncounter()
        {
            //Lets start spawning the encounter mobs!
            //First we spawn the 30 Wall of Fire mobs around the perimeter of the island.
            for (int i = 0; i < 30; i++)
            {
                SpawnAfire(FirePosition[i, 0], FirePosition[i, 1], FirePosition[i, 2]);
            }
            //Next we spawn the two sets of stair guards
            for (int i = 0; i < 4; i++)
            {
                SpawnAGuard(GuardPosition[i, 0], GuardPosition[i, 1], GuardPosition[i, 2], 3720);
            }
            for (int i = 4; i < 8; i++)
            {
                SpawnAGuard(GuardPosition[i, 0], GuardPosition[i, 1], GuardPosition[i, 2], 1735);
            }
            //Next we spawn Sunkaio

            GameSunkaio sunkaio = new GameSunkaio();
            sunkaio.Model = 1349;
            sunkaio.Size = 100;
            sunkaio.Level = 68; //level 65 on live
            sunkaio.Name = "Sunkaio";
            sunkaio.CurrentRegionID = (ushort)Ianetor.playerregion;
            sunkaio.Heading = 1690;
            sunkaio.Realm = 0;
            sunkaio.CurrentSpeed = 0;
            sunkaio.MaxSpeedBase = 191;
            sunkaio.GuildName = "";
            sunkaio.X = 431865;
            sunkaio.Y = 544121;
            sunkaio.Z = 8311;
            sunkaio.RoamingRange = 0;
            sunkaio.RespawnInterval = 0;
            sunkaio.BodyType = 0;

            SunBrain brain = new SunBrain();
            brain.AggroLevel = 100;
            brain.AggroRange = 350;
            sunkaio.SetOwnBrain(brain);

            sunkaio.AddToWorld();
            BossList.Add(sunkaio);
            Sun = sunkaio;
            GameEventMgr.AddHandler(sunkaio, GameNPCEvent.Dying, new DOLEventHandler(Ianetor.SunHasDied));

            //Next we spawn Zopureo

            GameZopureo zopureo = new GameZopureo();
            zopureo.Model = 1349;
            zopureo.Size = 100;
            zopureo.Level = 70;
            zopureo.Name = "Zopureo";
            zopureo.CurrentRegionID = (ushort)Ianetor.playerregion;
            zopureo.Heading = 1690;
            zopureo.Realm = 0;
            zopureo.CurrentSpeed = 0;
            zopureo.MaxSpeedBase = 0;
            zopureo.GuildName = "";
            zopureo.X = 432767;
            zopureo.Y = 543483;
            zopureo.Z = 8291;
            zopureo.RoamingRange = 0;
            zopureo.RespawnInterval = 0;
            zopureo.BodyType = 0;

            ZopureoBrain zbrain = new ZopureoBrain();
            zbrain.AggroLevel = 100;
            zbrain.AggroRange = 0;
            zopureo.SetOwnBrain(zbrain);

            zopureo.AddToWorld();
            BossList.Add(zopureo);
            Zop = zopureo;
            GameEventMgr.AddHandler(zopureo, GameNPCEvent.Dying, new DOLEventHandler(Ianetor.ZopHasDied));

            //Next we spawn Aithos

            GameAithos aithos = new GameAithos();
            aithos.Model = 1349;
            aithos.Size = 100;
            aithos.Level = 68; //level 65 on live
            aithos.Name = "Aithos";
            aithos.CurrentRegionID = (ushort)Ianetor.playerregion;
            aithos.Heading = 1690;
            aithos.Realm = 0;
            aithos.CurrentSpeed = 0;
            aithos.MaxSpeedBase = 191;
            aithos.GuildName = "";
            aithos.X = 432377;
            aithos.Y = 543728;
            aithos.Z = 8334;
            aithos.RoamingRange = 0;
            aithos.RespawnInterval = 0;
            aithos.BodyType = 0;

            AithosBrain abrain = new AithosBrain();
            abrain.AggroLevel = 100;
            abrain.AggroRange = 350;
            aithos.SetOwnBrain(abrain);

            aithos.AddToWorld();
            BossList.Add(aithos);
            Aith = aithos;
            GameEventMgr.AddHandler(aithos, GameNPCEvent.Dying, new DOLEventHandler(Ianetor.AithosHasDied));
            //Make sure there are not too many players on the island when the encounter starts, if there is too many players
            //sick the stairgards on them.
            List<GamePlayer> islandplayers = new List<GamePlayer>();
            foreach (GamePlayer foundplayer in (Aith.GetPlayersInRadius((ushort) 2300)))
            {
                islandplayers.Add(foundplayer);
            }
            if (islandplayers.Count > 8)
            {
                EncounterMgr.BroadcastMsg(Aith, "You can not have more then one full group of players on the island, you must all die for your mistake!", 2300, true);
                EncounterMgr.BroadcastMsg(Aith, "Guards!!!!!!!!!!!", 2300, true);
                foreach (GameNPC stairgard in StairGuardList)
                {
                    IOldAggressiveBrain aggroBrain = stairgard.Brain as IOldAggressiveBrain;
                    foreach (GamePlayer foundplayer in islandplayers)
                    {
                        if (aggroBrain != null && (GameServer.ServerRules.IsAllowedToAttack(stairgard, foundplayer, true)))
                            aggroBrain.AddToAggroList(foundplayer, Util.Random(50, 100));
                    }
                }
            }
            return;
        }

        public static void DeSpawnEncounter()
        {

            foreach (GameWallofFire fire in WallofFireList)
            {
                fire.RemoveFromWorld();
            }
            WallofFireList.Clear();
            foreach (GameStairGuard guard in StairGuardList)
            {
                guard.Health = 0;
                guard.Delete();
            }
            StairGuardList.Clear();
            foreach (GameNPC boss in BossList)
            {
                boss.Health = 0;
                boss.Delete();
                if (boss == Aith)
                {
                    GameEventMgr.RemoveHandler(boss, GameNPCEvent.Dying, new DOLEventHandler(AithosHasDied));
                }
                if (boss == Zop)
                {
                    GameEventMgr.RemoveHandler(boss, GameNPCEvent.Dying, new DOLEventHandler(ZopHasDied));
                }
                if (boss == Sun)
                {
                    GameEventMgr.RemoveHandler(boss, GameNPCEvent.Dying, new DOLEventHandler(SunHasDied));
                }
            }
            BossList.Clear();

            foreach (GameNPC purro in PurrosList)
            {
                purro.Health = 0;
                purro.Delete();
            }
            PurrosList.Clear();
            
            InProgress = false;

            return;
        }

        public int DeSpawnEncounter(RegionTimer timer) //this one gets called if the 15 min timer runs out.
        {
            foreach (GameWallofFire fire in WallofFireList)
            {
                fire.RemoveFromWorld();
            }
            WallofFireList.Clear();
            foreach (GameStairGuard guard in StairGuardList)
            {
                guard.Health = 0;
                guard.Delete();
            }
            StairGuardList.Clear();
            foreach (GameNPC boss in BossList)
            {
                boss.Health = 0;
                boss.Delete();
                if (boss == Aith)
                {
                    GameEventMgr.RemoveHandler(boss, GameNPCEvent.Dying, new DOLEventHandler(AithosHasDied));
                }
                if (boss == Zop)
                {
                    GameEventMgr.RemoveHandler(boss, GameNPCEvent.Dying, new DOLEventHandler(ZopHasDied));
                }
                if (boss == Sun)
                {
                    GameEventMgr.RemoveHandler(boss, GameNPCEvent.Dying, new DOLEventHandler(SunHasDied));
                }
            }
            BossList.Clear();

            foreach (GameNPC purro in PurrosList)
            {
                purro.Health = 0;
                purro.Delete();
            }
            PurrosList.Clear();

            InProgress = false;
            return 0;
        }

        public void SpawnAfire(int fireX, int fireY, int fireZ)         
        {
            GameWallofFire fire = new GameWallofFire();
            fire.Model = 1;
            fire.Size = 50;
            fire.Level = 99;
            fire.Name = "Wall of Fire";
            fire.CurrentRegionID = (ushort)Ianetor.playerregion;
            fire.Heading = 1690;
            fire.Realm = 0;
            fire.CurrentSpeed = 0;
            fire.MaxSpeedBase = 0;
            fire.GuildName = "";
            fire.X = fireX;
            fire.Y = fireY;
            fire.Z = fireZ;
            fire.RoamingRange = 0;
            fire.RespawnInterval = 0;
            fire.BodyType = 0;
            fire.Flags ^= (uint)GameNPC.eFlags.CANTTARGET;
            fire.Flags ^= (uint)GameNPC.eFlags.DONTSHOWNAME;

            FireBrain brain = new FireBrain();
            brain.AggroLevel = 100;
            brain.AggroRange = 0;
            fire.SetOwnBrain(brain);

            fire.AddToWorld();
            WallofFireList.Add(fire);

            return;
        }

        public void SpawnAGuard(int guardX, int guardY, int guardZ, ushort heading)
        {
            GameStairGuard guard = new GameStairGuard();
            guard.Model = 1349;
            guard.Size = 37;
            guard.Level = 80; //level 70 on live
            guard.Name = "daleros ephoros";
            guard.CurrentRegionID = (ushort)Ianetor.playerregion;
            guard.Heading = heading;
            guard.Realm = 0;
            guard.CurrentSpeed = 0;
            guard.MaxSpeedBase = 300;
            guard.GuildName = "";
            guard.X = guardX;
            guard.Y = guardY;
            guard.Z = guardZ;
            guard.RoamingRange = 0;
            guard.RespawnInterval = 0;
            guard.BodyType = 0;
            guard.Strength = 600;

            StandardMobBrain brain = new StandardMobBrain();
            brain.AggroLevel = 100;
            brain.AggroRange = 200;
            guard.SetOwnBrain(brain);

            guard.AddToWorld();
            StairGuardList.Add(guard);

            return;
        }

        public static void PurroHasDied(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC p = sender as GameNPC;
            GameZopureo.ZopureoChangeHealth(p.MaxHealth);
            Ianetor.PurrosList.Remove(p);
            GameEventMgr.RemoveHandler(sender, GameNPCEvent.Dying, new DOLEventHandler(PurroHasDied));
            return;
        }

        public static void AithosHasDied(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC npc = sender as GameNPC;
            Ianetor.BossList.Remove(npc);
            GameEventMgr.RemoveHandler(sender, GameNPCEvent.Dying, new DOLEventHandler(AithosHasDied));
            CheckEncounterState();
            return;
        }

        public static void ZopHasDied(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC npc = sender as GameNPC;
            Ianetor.BossList.Remove(npc);
            GameEventMgr.RemoveHandler(sender, GameNPCEvent.Dying, new DOLEventHandler(ZopHasDied));
            CheckEncounterState();
            return;
        }

        public static void SunHasDied(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC npc = sender as GameNPC;
            Ianetor.BossList.Remove(npc);
            GameEventMgr.RemoveHandler(sender, GameNPCEvent.Dying, new DOLEventHandler(SunHasDied));
            CheckEncounterState();
            return;
        }

        public static void CheckEncounterState()
        {
            if ((Ianetor.Zop.IsAlive) || (Ianetor.Sun.IsAlive) || (Ianetor.Aith.IsAlive))
            {
                return;
            }
            Ianetor.DeSpawnEncounter();
        }


        public override bool Interact(GamePlayer player)
        {
            if (base.Interact(player))
            {
                TurnTo(player, 1500);
                SayTo(player, "Have you seen it!  Have you seen it!  Fire!  Fire raining from the sky!  Not from the sun...  No!  No!  It is not the sun!  What it is doesn't really matter, but I know you must go there.  I can offer you [assistance] to you if you assist me!");
                return true;
            }
            return false;
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (source != null && source is GamePlayer)
            {
                GamePlayer player = source as GamePlayer;
                switch (str)
                {
                    case "assistance":
                        SayTo(player, "Yes!  Yes, I knew you'd be interested....yes.  I'm bored, HA!  Bored, can you believe it, with all my magic, I'm bored!  I've just run out of things to do really.  So I'd like to play a little game.  If you play this game I'll make sure that you are safe from the fire raining from the sky, I'll make sure it can't get to you!  Still [interested]?");
                        break;
                    case "interested":
                        SayTo(player,  "Yes!  Yes, I knew you would be... yes...  The rules are as follows.  If you break them, you will die!  They are simple, so pay attention, because I don't want to have to repeat myself!  The [first] rule is...");
                        break;
                    case "first":
                        SayTo(player, "Only a small group of you may be on the island and involved in the battle at a time!  Should you cheat, and bring more, the battle will become much more difficult for you.  The [second] rule is...");
                        break;
                    case "second":
                        SayTo(player, "Anyone that attempts to join the battle after it has already begun will be in violation of the second rule and will be slain!  I am going to surround the isle with fire traps whose fire is so intense that it can strike down the healthiest among your kind instantly!  Anyone that tries to run across the bridge after the battle has begun shall be slain by the daleros ephoros!  Would you like a few [hints] as to what you will face?");
                        break;
                    case "hints":
                        SayTo(player, "Well, I'll admit that I'm a rather diabolical sort.  Life would be so much duller if I wasn't!  Whomever Sunkaio is chasing should run!  That individual will be slain nearly instantly if it catches up to them!  But anyone else who follows it or attacks it will face far less risk.  Second, know that the best attack you can make on the Zopureo is not a direct one.  Finally, know that the Aithos burns far more intensely when its minions Sunkaio and the Zopureo still exist!  That is all I will say, it should be enough, you don't need any more [assistance] than that do you?  Are you ready to [begin]?");
                        Say("Ianetor provides some hints for the entire group, \"Whomever Sunkaio is chasing should run!  That individual will be slain nearly instantly if it catches up to them!  But anyone else who follows it or attacks it will face far less risk.  Second, know that the best attack you can make on the Zopureo is not a direct one.  Finally, know that the Aithos burns far more intensely when its minions Sunkaio and the Zopureo still exist!  That is all I will say, it should be enough, you don't need any more help than that do you?");
                        break;
                    case "begin":
                    BeginEncounter(player);
                        break;
                    case "stop":
                        DeSpawnEncounter();
                        break;
                    default:
                        break;
                }
                return true;
            }
            return base.WhisperReceive(source, str);
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            SpawnIanetor1(albregion);
            SpawnIanetor2(albregion);
            SpawnIanetor1(midregion);
            SpawnIanetor2(midregion);
            SpawnIanetor1(hibregion);
            SpawnIanetor2(hibregion);
            log.Warn("Loading Master Level 1: Fire Island.  Spawned Ianetor's.");
        }

        public static void SpawnIanetor1(int region)
        {
            Ianetor ianetor1 = new Ianetor();
            ianetor1.Name = "Ianetor";
            ianetor1.GuildName = "";
            ianetor1.Model = 1194;
            ianetor1.Realm = 0;
            ianetor1.CurrentRegionID = (ushort)region;
            ianetor1.Size = 35;
            ianetor1.Level = 0;
            ianetor1.X = 433401;
            ianetor1.Y = 546420;
            ianetor1.Z = 8481;
            ianetor1.Heading = 3720;
            ianetor1.RoamingRange = 0;
            ianetor1.Flags ^= (uint)GameNPC.eFlags.PEACE;
            ianetor1.CurrentSpeed = 0;
            ianetor1.MaxSpeedBase = 0;
            ianetor1.AddToWorld();
        }
        public static void SpawnIanetor2(int region)
        {
            Ianetor ianetor2 = new Ianetor();
            ianetor2.Name = "Ianetor";
            ianetor2.GuildName = "";
            ianetor2.Model = 1194;
            ianetor2.Realm = 0;
            ianetor2.CurrentRegionID = (ushort)region;
            ianetor2.Size = 35;
            ianetor2.Level = 0;
            ianetor2.X = 431100;
            ianetor2.Y = 541433;
            ianetor2.Z = 8481;
            ianetor2.Heading = 1735;
            ianetor2.RoamingRange = 0;
            ianetor2.Flags ^= (uint)GameNPC.eFlags.PEACE;
            ianetor2.CurrentSpeed = 0;
            ianetor2.MaxSpeedBase = 0;
            ianetor2.AddToWorld();
        }

    }

    class GameWallofFire : GameNPC
    {

        public override void StartRespawn()
        {
        }

        public override void SaveIntoDatabase()
        {
        }

        public override int AttackRange
        {
            get { return 0; }
            set { }
        }

    }

    class GameStairGuard : GameNPC
    {

        public override void StartRespawn()
        {
        }

        public override void SaveIntoDatabase()
        {
        }

        public override int AttackRange
        {
            get { return 300; }
            set { }
        }


    }

    class GameSunkaio : GameNPC
    {
        public override void StartRespawn()
        {
        }

        public override void SaveIntoDatabase()
        {
        }
    }

    class GameAithos : GameNPC
    {

        public override void StartRespawn()
        {
        }

        public override void SaveIntoDatabase()
        {
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            //While Sunkaio or Zopureo are live, Aithos will take half the damage you do to him.
            if (Ianetor.BossList.Contains(Ianetor.Zop) || Ianetor.BossList.Contains(Ianetor.Sun))
            {
                int dmg = (int)Math.Round((decimal)(damageAmount / 2));
                damageAmount = dmg;
            }
            base.TakeDamage(source, damageType, damageAmount, criticalAmount);
        }

    }

    class GameZopureo : GameNPC
    {

        public override void StartRespawn()
        {
        }

        public override void SaveIntoDatabase()
        {
        }

        public override int AttackRange
        {
            get { return 0; }
            set { }
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            //Zop dosn't take damage from direct attacks.
            //If Someone is trying to damage him directly anyway, he should sick his flames.
            bool purrosicked = false;
            foreach (GameNPC purro in Ianetor.PurrosList)
            {
                if (!purrosicked)
                {
                    if (!purro.AttackState)
                    {
                        purro.AddAttacker(source);
                        purro.StartAttack(source);
                        purrosicked = true;
                        //only sick one purro at a time on the attacker each time Zopureo is attacked by someone.
                    }
                }
            }
        }

        public void SpawnPurros()
        {
            SpawnAPurro(432802, 543389, 8291);
            SpawnAPurro(432709, 543571, 8302);
            SpawnAPurro(432664, 543446, 8298);
            SpawnAPurro(432867, 543538, 8285);
        }

        public void SpawnAPurro(int purroX, int purroY, int purroZ)
        {
            GamePurros purro = new GamePurros();
            purro.Model = 911;
            purro.Size = 50;
            purro.Level = 60;
            purro.Name = "purros";
            purro.CurrentRegionID = (ushort)Ianetor.playerregion;
            purro.Heading = 1690;
            purro.Realm = 0;
            purro.CurrentSpeed = 0;
            purro.MaxSpeedBase = 350;
            purro.GuildName = "";
            purro.X = purroX;
            purro.Y = purroY;
            purro.Z = purroZ;
            purro.RoamingRange = 0;
            purro.RespawnInterval = 0;
            purro.BodyType = 0;

            PurroBrain pbrain = new PurroBrain();
            pbrain.AggroLevel = 100;
            pbrain.AggroRange = 200;
            purro.SetOwnBrain(pbrain);

            purro.AddToWorld();
            Ianetor.PurrosList.Add(purro);
            GameEventMgr.AddHandler(purro, GameNPCEvent.Dying, new DOLEventHandler(Ianetor.PurroHasDied));
            return;
        
        }

        public static void ZopureoChangeHealth(int damage)
        {
            
            if (Ianetor.Zop.IsAlive)
            {
                Ianetor.Zop.Health -= (int) Math.Round((decimal)damage / 2);
                if (Ianetor.Zop.Health <= 0)
                {
                    //Zopureo just died, despawn all purro's
                    foreach (GameNPC purro in Ianetor.PurrosList)
                    {
                        purro.Health = 0;
                        purro.Delete();
                    }
                    Ianetor.PurrosList.Clear();
                    Ianetor.BossList.Remove(Ianetor.Zop);
                    Ianetor.Zop.Health = 0;
                    Ianetor.Zop.Die(Ianetor.Zop);
                    Ianetor.Zop.Delete();
                }

            }
        }

    }

    class GamePurros : GameNPC
    {

        public override void StartRespawn()
        {
        }

        public override void SaveIntoDatabase()
        {
        }

        public override int AttackRange
        {
            get { return 300; }
            set { }
        }
    }
}

namespace DOL.AI.Brain
{


    public class AithosBrain : StandardMobBrain
    {
        public AithosBrain()
            : base()
        {
            AggroLevel = 100;
            AggroRange = 150;
            ThinkInterval = 3000;
        }

        public override void Think()
        {
            GameAithos aithosbody = Body as GameAithos;
            ushort DDattackrange = 200;
            int damage = 0;
            //attack the player if he's too close, encouraging him to kite
            foreach (GamePlayer player in aithosbody.GetPlayersInRadius(DDattackrange)) //each player that is in attack distance of the mob
            {
                if (aithosbody.IsObjectInFront(player, 180) && aithosbody.AttackState)
                {
                    if (GameServer.ServerRules.IsAllowedToAttack(aithosbody, player, true))
                    {
                        //1 in 4 chance every think cycle if a player is in front of him and he is attacking to do a Direct Damage.
                        if (Util.Random(3) == 3)
                        {
                            //If Sunkaio or Zopureo are alive, damage is higher.
                            if (Ianetor.BossList.Contains(Ianetor.Zop) || Ianetor.BossList.Contains(Ianetor.Sun))
                            {
                                damage = 1000;
                            }
                            else
                                damage = 500;
                            //This probably should be redone as a spell.
                            player.Out.SendMessage("Aithos sets you alight for " + damage + " damage!", eChatType.CT_Damaged, eChatLoc.CL_ChatWindow);
                            player.Out.SendSpellEffectAnimation(aithosbody, player, 310, 0, false, 1);
                            player.TakeDamage(aithosbody, eDamageType.Heat, damage, 0);
                            GamePlayer target = player;
                            foreach (GamePlayer onlookers in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {
                                if (onlookers != target) //dont send it to the person it happened to, only the other onlookers.
                                {
                                    onlookers.Out.SendSpellEffectAnimation(aithosbody, target, 310, 0, false, 1);
                                    onlookers.Out.SendMessage("Aithos burns " + target.Name + "!", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);

                                }
                            }
                        }
                    }
                }
            }

            base.Think();
        }

    }


    public class FireBrain : StandardMobBrain
    {
        public FireBrain()
            : base()
        {
            AggroLevel = 100;
            AggroRange = 0;
            ThinkInterval = 3000;
            
        }
        public override void Think()
        {
            GameWallofFire fire = Body as GameWallofFire;
            ushort attackrange = 300;
            //This should probably be redone as a spell.
            foreach (GamePlayer player in fire.GetPlayersInRadius(attackrange)) //each player that is in attack distance of the mob
            {
                if (GameServer.ServerRules.IsAllowedToAttack(fire, player, true))
                {
                    player.Out.SendMessage("The Wall of Fire burns you for 10,000 damage!", eChatType.CT_Damaged, eChatLoc.CL_ChatWindow);
                    player.Out.SendSpellEffectAnimation(fire, player, 310, 0, false, 1);
                    player.TakeDamage(fire, eDamageType.Heat, 10000, 0);
                    GamePlayer target = player;
                    foreach (GamePlayer onlookers in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        if (onlookers != target) //dont send it to the person it happened to, only the other onlookers.
                        {
                            onlookers.Out.SendSpellEffectAnimation(fire, target, 310, 0, false, 1);
                            onlookers.Out.SendMessage("The Wall of Fire burns " + target.Name + " to death!", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
                        }
                    }
                    
                }

            }
            
            //Do I cast a spell effect this round?
            if (Util.Random(1) == 1)
           {
                int spell = Util.Random(2);
                ushort spellid;
                switch(spell)
                {
                    case 0:
                        spellid = 5920;
                        break;
                    case 1:
                        spellid = 6063;
                        break;
                    case 2:
                        spellid = 5980;
                        break;
                    default:
                        spellid = 5920;
                        break;
                }
                if (spellid != null)

                    foreach (GamePlayer player in fire.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        player.Out.SendSpellEffectAnimation(fire, fire, spellid, 0, false, 1);
                    }
            }


            base.Think();
        }
    }


    public class SunBrain : StandardMobBrain
   {
        public SunBrain()
            : base()
        {
            AggroLevel = 100;
            AggroRange = 150;
            ThinkInterval = 3000;

        }
        public override void Think()
        {
            GameSunkaio sunbody = Body as GameSunkaio;
            ushort DDattackrange = 200;
            //attack the player if he's too close, encouraging him to kite me.
            foreach (GamePlayer player in sunbody.GetPlayersInRadius(DDattackrange)) //each player that is in attack distance of the mob
            {

                if (sunbody.IsObjectInFront(player, 180) && sunbody.AttackState)
                {
                    if (GameServer.ServerRules.IsAllowedToAttack(sunbody, player, true))
                    {
                        //This should probably be redone as a spell
                        //1 in 4 chance every think cycle if a player is in front of him and he is attacking to do a Direct Damage.
                        if (Util.Random(3) == 3)
                        {
                            player.Out.SendMessage("Sunkaio sears your flesh for 400 damage!", eChatType.CT_Damaged, eChatLoc.CL_ChatWindow);
                            player.Out.SendSpellEffectAnimation(sunbody, player, 310, 0, false, 1);
                            player.TakeDamage(sunbody, eDamageType.Heat, 400, 0);
                            GamePlayer target = player;
                            foreach (GamePlayer onlookers in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {
                                if (onlookers != target) //dont send it to the person it happened to, only the other onlookers.
                                {
                                    onlookers.Out.SendSpellEffectAnimation(sunbody, target, 310, 0, false, 1);
                                    onlookers.Out.SendMessage("Sunkaio sears the flesh of " + target.Name + "!", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);

                                }
                            }
                        }

                    }
                }

            }

            base.Think();
        }
    }


        public class ZopureoBrain : StandardMobBrain
        {
            public ZopureoBrain()
                : base()
            {
                AggroLevel = 100;
                AggroRange = 0;
                ThinkInterval = 3000;

            }
            public override void Think()
            {
                GameZopureo zbody = Body as GameZopureo;
                //Check if we need to spawn purro's and do it.
                if ((Ianetor.PurrosList.Count < 1) && (zbody.IsAlive))
                {
                    zbody.SpawnPurros();
                }
                    base.Think();
            }


        }

        public class PurroBrain : StandardMobBrain
        {
            bool TimerOn = false;
            public PurroBrain()
                : base()
            {
                AggroLevel = 100;
                AggroRange = 200;
                ThinkInterval = 3000;

            }
            public override void Think()
            {
                //logic of the purros brain goes here.
                GamePurros pbody = Body as GamePurros;
                if (pbody.InCombat && !TimerOn)
                {
                    new RegionTimer(Body, new RegionTimerCallback(PurroExplode), (5 * 1000));
                    TimerOn = true;
                }
                
                
                base.Think();
            }
            public int PurroExplode(RegionTimer timer)
            {
                GamePurros pbody = Body as GamePurros;
                if (pbody.IsAlive && pbody.Health > 1)
                {
                    int damageamount = (int)Math.Round((decimal)(pbody.MaxHealth - pbody.Health) / 2);
                    GameZopureo.ZopureoChangeHealth(damageamount);
                    //AOE Fire - This should probably be redone as a spell.
                    ushort AOErange = 500;
                    foreach (GamePlayer player in pbody.GetPlayersInRadius(AOErange)) //each player that is in attack distance of the mob
                    {
                        if (GameServer.ServerRules.IsAllowedToAttack(pbody, player, true))
                        {
                            player.Out.SendMessage("The purros explodes in your face causing 300 damage!", eChatType.CT_Damaged, eChatLoc.CL_ChatWindow);
                            player.Out.SendSpellEffectAnimation(pbody, player, 310, 0, false, 1);
                            player.TakeDamage(pbody, eDamageType.Heat, 300, 0);
                            GamePlayer target = player;
                            foreach (GamePlayer onlookers in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {        
                                if (onlookers != target) //dont send it to the person it happened to, only the other onlookers.
                                {
                                    onlookers.Out.SendSpellEffectAnimation(pbody, target, 310, 0, false, 1);
                                    onlookers.Out.SendMessage("The purros explodes in " + target.Name + " face!", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
                                }
                            }
                        }
                    }
                    //purros exploded, let's despawn him, first take away the die handler so he dosn't die twice.
                    GameEventMgr.RemoveHandler(pbody, GameNPCEvent.Dying, new DOLEventHandler(Ianetor.PurroHasDied));
                    pbody.Health = 0;
                    pbody.Delete();
                    TimerOn = false;
                    Ianetor.PurrosList.Remove(pbody);
                }
                return 0;
            }

        }

}