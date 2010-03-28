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

using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0xE0 ^ 168, "Show warmap")]
	public class WarmapShowRequestHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int code = packet.ReadByte();
			int RealmMap = packet.ReadByte();
			int keepId = packet.ReadByte();

			if (client == null || client.Player == null)
				return 1;

			//hack fix new keep ids
			if ((int)client.Version >= (int)GameClient.eClientVersion.Version190)
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
					client.Out.SendWarmapUpdate(KeepMgr.getKeepsByRealmMap(client.Player.WarMapPage));
					WarMapMgr.SendFightInfo(client);
					break;
				}
				//teleport
				case 2:
					{
						client.Out.SendWarmapUpdate(KeepMgr.getKeepsByRealmMap(client.Player.WarMapPage));
						WarMapMgr.SendFightInfo(client);

						if (client.Account.PrivLevel == (int)ePrivLevel.Player &&
							(client.Player.InCombat || client.Player.CurrentRegionID != 163 || GameRelic.IsPlayerCarryingRelic(client.Player)))
						{
							return 0;
						}

						AbstractGameKeep keep = null;

						if (keepId > 6)
						{
							keep = KeepMgr.getKeepByID(keepId);
						}

						if (keep == null && keepId > 6)
						{
							return 1;
						}

						if (client.Account.PrivLevel == (int)ePrivLevel.Player)
						{
							bool found = false;

							if (keep != null)
							{
								// if we are requesting to teleport to a keep we need to check that keeps requirements first

								if (keep.Realm != client.Player.Realm)
								{
									return 0;
								}

								if (keep is GameKeep && ((keep as GameKeep).OwnsAllTowers == false || keep.InCombat))
								{
									return 0;
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
								return 0;
							}
						}

						int x = 0;
						int y = 0;
						int z = 0;
						ushort heading = 0;
						switch (keepId)
						{
							//sauvage
							case 1:
							//snowdonia
							case 2:
							//svas
							case 3:
							//vind
							case 4:
							//ligen
							case 5:
							//cain
							case 6:
								{
									KeepMgr.GetBorderKeepLocation(keepId, out x, out y, out z, out heading);
									break;
								}
							default:
								{
									if (keep != null && keep is GameKeep)
									{
										FrontiersPortalStone stone = keep.TeleportStone;
										heading = stone.Heading;
										z = stone.Z;
										stone.GetTeleportLocation(out x, out y);
									}
									break;
								}
						}

						if (x != 0)
						{
							client.Player.MoveTo(163, x, y, z, heading);
						}

						break;
					}
			}
			return 1;
		}
	}

}
