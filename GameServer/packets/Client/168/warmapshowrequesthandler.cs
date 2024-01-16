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
using System;
using System.Collections;
using DOL.GS.Geometry;
using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.ShowWarmapRequest, "Show Warmap", eClientStatus.PlayerInGame)]
	public class WarmapShowRequestHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int code = packet.ReadByte();
			int RealmMap = packet.ReadByte();
			int keepId = packet.ReadByte();

			if (client == null || client.Player == null)
				return;

			//hack fix new keep ids
			else if ((int)client.Version >= (int)GameClient.eClientVersion.Version190)
			{
				if (keepId >= 82)
					keepId -= 7;
				else if (keepId >= 62)
					keepId -= 12;
			}

			switch (code)
			{
				//warmap open
				//warmap update
				case 0:
				{
					client.Player.WarMapPage = (byte)RealmMap;
					break;
				}
				case 1:
				{
					client.Out.SendWarmapUpdate(GameServer.KeepManager.GetKeepsByRealmMap(client.Player.WarMapPage));
					WarMapMgr.SendFightInfo(client);
					break;
				}
				//teleport
				case 2:
					{
						client.Out.SendWarmapUpdate(GameServer.KeepManager.GetKeepsByRealmMap(client.Player.WarMapPage));
						WarMapMgr.SendFightInfo(client);

						if (client.Account.PrivLevel == (int)ePrivLevel.Player &&
							(client.Player.InCombat || client.Player.CurrentRegionID != 163 || GameRelic.IsPlayerCarryingRelic(client.Player)))
						{
							return;
						}

						AbstractGameKeep keep = null;

						if (keepId > 6)
						{
							keep = GameServer.KeepManager.GetKeepByID(keepId);
						}

						if (keep == null && keepId > 6)
						{
							return;
						}

						if (client.Account.PrivLevel == (int)ePrivLevel.Player)
						{
							bool found = false;

							if (keep != null)
							{
								// if we are requesting to teleport to a keep we need to check that keeps requirements first

								if (keep.Realm != client.Player.Realm)
								{
									return;
								}

								if (keep is GameKeep && ((keep as GameKeep).OwnsAllTowers == false || keep.InCombat))
								{
									return;
								}

								// Missing: Supply line check
							}

							if (client.Player.CurrentRegionID == 163)
							{
								// We are in the frontiers and all keep requirements are met or we are not near a keep
								// this may be a portal stone in the RvR village, for example

								foreach (GameStaticItem item in client.Player.GetItemsInRadius(WorldMgr.INTERACT_DISTANCE))
								{
									if (item is FrontiersPortalStone)
									{
										found = true;
										break;
									}
								}
							}

							if (!found)
							{
								client.Player.Out.SendMessage("You cannot teleport unless you are near a valid portal stone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
						}

                        var portPosition = Position.Zero;
						switch (keepId)
						{
							
							case 1: //sauvage
							case 2: //snowdonia
							case 3: //svas
							case 4: //vind
							case 5: //ligen
							case 6: //cain
								{
									portPosition = GameServer.KeepManager.GetBorderKeepPosition(keepId);
									break;
								}
							default:
								{
									if (keep != null && keep is GameKeep)
									{
										var stone = keep.TeleportStone;
										if (stone != null) 
										{
                                            var distance = Util.Random(50, 150);
                                            var direction = stone.Orientation + Angle.Heading(Util.Random(- 500, 500));
                                            portPosition = stone.Position + Vector.Create(direction, distance);
										}
										else
										{
											portPosition = Position.Create(regionID: 163, keep.X, keep.Y, keep.Z+150, keep.Orientation);
										}
									}
									break;
								}
						}

						if (portPosition != Position.Zero)
						{
							client.Player.MoveTo(portPosition);
						}

						break;
					}
			}
		}
	}

}
