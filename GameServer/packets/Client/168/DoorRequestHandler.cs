/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System.Linq;
using System.Collections.Generic;

using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.DoorRequest, "Door Interact Request Handler", eClientStatus.PlayerInGame)]
    public class DoorRequestHandler : IPacketHandler
    {
        public static int HandlerDoorId { get; set; }

        /// <summary>
        /// door index which is unique
        /// </summary>
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            var doorId = (int)packet.ReadInt();
            HandlerDoorId = doorId;
            var doorState = (byte)packet.ReadByte();
            int doorType = doorId / 100000000;

            int radius = Properties.WORLD_PICKUP_DISTANCE * 2;
            int zoneDoor = doorId / 1000000;

            string debugText = string.Empty;

            // For ToA the client always sends the same ID so we need to construct an id using the current zone
            if (client.Player.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
            {
                debugText = $"ToA DoorID: {doorId} ";

                doorId -= zoneDoor * 1000000;
                zoneDoor = client.Player.CurrentZone.ID;
                doorId += zoneDoor * 1000000;
                HandlerDoorId = doorId;

                // experimental to handle a few odd TOA door issues
                if (client.Player.CurrentRegion.IsDungeon)
                {
                    radius *= 4;
                }
            }

            // debug text
            if (client.Account.PrivLevel > 1 || Properties.ENABLE_DEBUG)
            {
                if (doorType == 7)
                {
                    int ownerKeepId = (doorId / 100000) % 1000;
                    int towerNum = (doorId / 10000) % 10;
                    int keepId = ownerKeepId + towerNum * 256;
                    int componentId = (doorId / 100) % 100;
                    int doorIndex = doorId % 10;
                    client.Out.SendDebugMessage($"Keep Door ID: {doorId} state:{doorState} (Owner Keep: {ownerKeepId} KeepID:{keepId} ComponentID:{componentId} DoorIndex:{doorIndex} TowerNumber:{towerNum})");

                    if (keepId > 255 && ownerKeepId < 10)
                    {
                        ChatUtil.SendDebugMessage(client, "Warning: Towers with an Owner Keep ID < 10 will have untargetable doors!");
                    }
                }
                else if (doorType == 9)
                {
                    int doorIndex = doorId - doorType * 10000000;
                    client.Out.SendDebugMessage($"House DoorID:{doorId} state:{doorState} (doorType:{doorType} doorIndex:{doorIndex})");
                }
                else
                {
                    int fixture = doorId - zoneDoor * 1000000;
                    int fixturePiece = fixture;
                    fixture /= 100;
                    fixturePiece = fixturePiece - fixture * 100;

                    client.Out.SendDebugMessage($"{debugText}DoorID:{doorId} state:{doorState} zone:{zoneDoor} fixture:{fixture} fixturePiece:{fixturePiece} Type:{doorType}");
                }
            }

            if (client.Player.TargetObject is GameDoor target && !client.Player.IsWithinRadius(target, radius))
            {
                client.Player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }

            var door = GameServer.Database.SelectObjects<DBDoor>("`InternalID` = @InternalID", new QueryParameter("@InternalID", doorId)).FirstOrDefault();

            if (door != null)
            {
                if (doorType == 7 || doorType == 9)
                {
                    new ChangeDoorAction(client.Player, doorId, doorState, radius).Start(1);
                    return;
                }

                if (client.Account.PrivLevel == 1)
                {
                    if (door.Locked == 0)
                    {
                        if (door.Health == 0)
                        {
                            new ChangeDoorAction(client.Player, doorId, doorState, radius).Start(1);
                            return;
                        }

                        if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
                        {
                            if (door.Realm != 0)
                            {
                                new ChangeDoorAction(client.Player, doorId, doorState, radius).Start(1);
                                return;
                            }
                        }

                        if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
                        {
                            if (client.Player.Realm == (eRealm)door.Realm || door.Realm == 6)
                            {
                                new ChangeDoorAction(client.Player, doorId, doorState, radius).Start(1);
                                return;
                            }
                        }
                    }
                }

                if (client.Account.PrivLevel > 1)
                {
                    client.Out.SendDebugMessage("GM: Forcing locked door open.");
                    new ChangeDoorAction(client.Player, doorId, doorState, radius).Start(1);
                    return;
                }
            }

            if (door == null)
            {
                if (doorType != 9 && client.Account.PrivLevel > 1 && client.Player.CurrentRegion.IsInstance == false)
                {
                    if (client.Player.TempProperties.getProperty(DoorMgr.WANT_TO_ADD_DOORS, false))
                    {
                        client.Player.Out.SendCustomDialog("This door is not in the database. Place yourself nearest to this door and click Accept to add it.", AddingDoor); 
                    }
                    else
                    {
                        client.Player.Out.SendMessage("This door is not in the database. Use '/door show' to enable the add door dialog when targeting doors.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                }

                new ChangeDoorAction(client.Player, doorId, doorState, radius).Start(1);
            }
        }

        public void AddingDoor(GamePlayer player, byte response)
        {
            if (response != 0x01)
            {
                return;
            }

            int doorType = HandlerDoorId / 100000000;
            if (doorType == 7)
            {
                PositionMgr.CreateDoor(HandlerDoorId, player);
            }
            else
            {
                var door = new DBDoor
                {
                    ObjectId = null,
                    InternalID = HandlerDoorId,
                    Name = "door",
                    Type = HandlerDoorId / 100000000,
                    Level = 20,
                    Realm = 6,
                    MaxHealth = 2545,
                    Health = 2545,
                    Locked = 0,
                    X = player.X,
                    Y = player.Y,
                    Z = player.Z,
                    Heading = player.Heading
                };

                GameServer.Database.AddObject(door);

                player.Out.SendMessage($"Added door {HandlerDoorId} to the database!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                DoorMgr.Init();
            }
        }

        /// <summary>
        /// Handles the door state change actions
        /// </summary>
        protected class ChangeDoorAction : RegionAction
        {
            /// <summary>
            /// The target door Id
            /// </summary>
            private readonly int _doorId;

            /// <summary>
            /// The door state
            /// </summary>
            private readonly int _doorState;

            /// <summary>
            /// allowed distance to door
            /// </summary>
            private readonly int _radius;

            /// <summary>
            /// Constructs a new ChangeDoorAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="doorId">The target door Id</param>
            /// <param name="doorState">The door state</param>
            public ChangeDoorAction(GamePlayer actionSource, int doorId, int doorState, int radius)
                : base(actionSource)
            {
                _doorId = doorId;
                _doorState = doorState;
                _radius = radius;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;
                List<IDoor> doorList = DoorMgr.getDoorByID(_doorId);

                if (doorList.Count > 0)
                {
                    bool success = false;
                    foreach (IDoor mydoor in doorList)
                    {
                        if (success)
                        {
                            break;
                        }

                        if (mydoor is GameKeepDoor)
                        {
                            var door = mydoor as GameKeepDoor;

                            // portal keeps left click = right click
                            if (door.Component.AbstractKeep is GameKeepTower && door.Component.AbstractKeep.KeepComponents.Count > 1)
                            {
                                door.Interact(player);
                            }

                            success = true;
                        }
                        else
                        {
                            if (player.IsWithinRadius(mydoor, _radius))
                            {
                                if (_doorState == 0x01)
                                {
                                    mydoor.Open(player);
                                }
                                else
                                {
                                    mydoor.Close(player);
                                }

                                success = true;
                            }
                        }
                    }

                    if (!success)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "DoorRequestHandler.OnTick.TooFarAway", doorList[0].Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
                else
                {
                    // new frontiers we don't want this, i.e. relic gates etc
                    if (player.CurrentRegionID == 163 && player.Client.Account.PrivLevel == 1)
                    {
                        return;
                    }

                    player.Out.SendDebugMessage($"Door {_doorId} not found in door list, opening via GM door hack.");

                    // else basic quick hack
                    var door = new GameDoor
                    {
                        DoorID = _doorId,
                        X = player.X,
                        Y = player.Y,
                        Z = player.Z,
                        Realm = eRealm.Door,
                        CurrentRegion = player.CurrentRegion
                    };

                    door.Open(player);
                }
            }
        }
    }
}