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

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0xE0 ^ 168, "Show warmap")]
	public class WarmapShowRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int code = packet.ReadByte();
			int RealmMap = packet.ReadByte();
			int keepId = packet.ReadByte();

			switch (code)
			{
				//warmap open
				//warmap update
				case 0:
				case 1:
					{
						client.Out.SendWarmapUpdate(KeepMgr.getKeepsByRealmMap(RealmMap));
						break;
					}
				//teleport
				case 2:
					{
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
									AbstractGameKeep keep = KeepMgr.getKeepByID(keepId);
									if (keep == null) return 1;

									//we redo our checks here
									bool good = true;
									if (client.Account.PrivLevel == 1)
									{
										//check realm
										if (keep.Realm != client.Player.Realm)
											return 0;
										bool found = false;
										foreach (GameStaticItem item in client.Player.GetItemsInRadius(WorldMgr.INTERACT_DISTANCE))
										{
											if (item is FrontiersPortalStone)
											{
												found = true;
												break;
											}
										}
										if (!found)
										{
											client.Player.Out.SendMessage("You cannot teleport unless you are near a valid portal stone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 0;
										}
										//does keep have all towers intact?
										GameKeep theKeep = keep as GameKeep;
										foreach (GameKeepTower tower in theKeep.Towers)
										{
											if (tower.Realm != theKeep.Realm)
											{
												good = false;
												break;
											}
										}
									}
									//todo 5 second teleport
									if (good)
									{
										FrontiersPortalStone stone = keep.TeleportStone;
										heading = stone.Heading;
										z = stone.Z;
										stone.GetTeleportLocation(out x, out y);
									}
									break;
								}
						}
						if (x!= 0)
							client.Player.MoveTo(163, x, y, z, heading);
						break;
					}
			}
			return 1;
		}
	}

}