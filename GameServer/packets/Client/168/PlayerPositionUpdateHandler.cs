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
//#define OUTPUT_DEBUG_INFO
using System;
using System.Collections;
using System.Net;
using System.Reflection;
using System.Text;

using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.PositionUpdate, "Handles player position updates", eClientStatus.PlayerInGame)]
	public class PlayerPositionUpdateHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string LASTMOVEMENTTICK = "PLAYERPOSITION_LASTMOVEMENTTICK";
		public const string LASTCPSTICK = "PLAYERPOSITION_LASTCPSTICK";

		/// <summary>
		/// Stores the count of times the player is above speedhack tolerance!
		/// If this value reaches 10 or more, a logfile entry is written.
		/// </summary>
		public const string SPEEDHACKCOUNTER = "SPEEDHACKCOUNTER";
		public const string SHSPEEDCOUNTER = "MYSPEEDHACKCOUNTER";

		//static int lastZ=int.MinValue;
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			//Tiv: in very rare cases client send 0xA9 packet before sending S<=C 0xE8 player world initialize
			if ((client.Player.ObjectState != GameObject.eObjectState.Active) ||
			    (client.ClientState != GameClient.eClientState.Playing))
				return;

			int environmentTick = Environment.TickCount;
			int packetVersion;
			if (client.Version > GameClient.eClientVersion.Version171)
			{
				packetVersion = 172;
			}
			else
			{
				packetVersion = 168;
			}

			int oldSpeed = client.Player.CurrentSpeed;

			//read the state of the player
			packet.Skip(2); //PID
			ushort data = packet.ReadShort();
			int speed = (data & 0x1FF);

			//			if(!GameServer.ServerRules.IsAllowedDebugMode(client)
			//				&& (speed > client.Player.MaxSpeed + SPEED_TOL))


			if ((data & 0x200) != 0)
				speed = -speed;

			if (client.Player.IsMezzed || client.Player.IsStunned)
			{
				// Nidel: updating client.Player.CurrentSpeed instead of speed
				client.Player.CurrentSpeed = 0;
			}
			else
			{
				client.Player.CurrentSpeed = (short)speed;
			}

			client.Player.IsStrafing = ((data & 0xe000) != 0);

			int realZ = packet.ReadShort();
			ushort xOffsetInZone = packet.ReadShort();
			ushort yOffsetInZone = packet.ReadShort();
			ushort currentZoneID;
			if (packetVersion == 168)
			{
				currentZoneID = (ushort)packet.ReadByte();
				packet.Skip(1); //0x00 padding for zoneID
			}
			else
			{
				currentZoneID = packet.ReadShort();
			}


			//Dinberg - Instance considerations.
			//Now this gets complicated, so listen up! We have told the client a lie when it comes to the zoneID.
			//As a result, every movement update, they are sending a lie back to us. Two liars could get confusing!

			//BUT, the lie we sent has a truth to it - the geometry and layout of the zone. As a result, the zones
			//x and y offsets will still actually be relevant to our current zone. And for the clones to have been
			//created, there must have been a real zone to begin with, of id == instanceZone.SkinID.

			//So, although our client is lying to us, and thinks its in another zone, that zone happens to coincide
			//exactly with the zone we are instancing - and so all the positions still ring true.

			//Philosophically speaking, its like looking in a mirror and saying 'Am I a reflected, or reflector?'
			//What it boils down to has no bearing whatsoever on the result of anything, so long as someone sitting
			//outside of the unvierse knows not to listen to whether you say which you are, and knows the truth to the
			//answer. Then, he need only know what you are doing ;)

			Zone newZone = WorldMgr.GetZone(currentZoneID);
			if (newZone == null)
			{
				if(client.Player==null) return;
				if(!client.Player.TempProperties.getProperty("isbeingbanned",false))
				{
					if (log.IsErrorEnabled)
						log.Error(client.Player.Name + "'s position in unknown zone! => " + currentZoneID);
					GamePlayer player=client.Player;
					player.TempProperties.setProperty("isbeingbanned", true);
					player.MoveToBind();
				}

				return; // TODO: what should we do? player lost in space
			}

			// move to bind if player fell through the floor
			if (realZ == 0)
			{
				client.Player.MoveTo(
					(ushort)client.Player.DBCharacter.BindRegion,
					client.Player.DBCharacter.BindXpos,
					client.Player.DBCharacter.BindYpos,
					(ushort)client.Player.DBCharacter.BindZpos,
					(ushort)client.Player.DBCharacter.BindHeading
				);
				return;
			}

			int realX = newZone.XOffset + xOffsetInZone;
			int realY = newZone.YOffset + yOffsetInZone;

			bool zoneChange = newZone != client.Player.LastPositionUpdateZone;
			if (zoneChange)
			{
				//If the region changes -> make sure we don't take any falling damage
				if (client.Player.LastPositionUpdateZone != null && newZone.ZoneRegion.ID != client.Player.LastPositionUpdateZone.ZoneRegion.ID)
					client.Player.MaxLastZ = int.MinValue;

				// Update water level and diving flag for the new zone
				client.Out.SendPlayerPositionAndObjectID();
				zoneChange = true;

				/*
				 * "You have entered Burial Tomb."
				 * "Burial Tomb"
				 * "Current area is adjusted for one level 1 player."
				 * "Current area has a 50% instance bonus."
				 */

                string description = newZone.Description;
                string screenDescription = description;

                LanguageDataObject translation = LanguageMgr.GetTranslation(client.Account.Language, newZone);
                if (translation != null)
                {
                    if (!Util.IsEmpty(((DBLanguageZone)translation).Description))
                        description = ((DBLanguageZone)translation).Description;

                    if (!Util.IsEmpty(((DBLanguageZone)translation).ScreenDescription))
                        screenDescription = ((DBLanguageZone)translation).ScreenDescription;
                }

                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.Entered", description),
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(screenDescription, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);

				client.Player.LastPositionUpdateZone = newZone;
			}

			int coordsPerSec = 0;
			int jumpDetect = 0;
			int timediff = Environment.TickCount - client.Player.LastPositionUpdateTick;
			int distance = 0;

			if (timediff > 0)
			{
				distance = client.Player.LastPositionUpdatePoint.GetDistanceTo(new Point3D(realX, realY, realZ));
				coordsPerSec = distance * 1000 / timediff;

				if (distance < 100 && client.Player.LastPositionUpdatePoint.Z > 0)
				{
					jumpDetect = realZ - client.Player.LastPositionUpdatePoint.Z;
				}
			}

			#region DEBUG
			#if OUTPUT_DEBUG_INFO
			if (client.Player.LastPositionUpdatePoint.X != 0 && client.Player.LastPositionUpdatePoint.Y != 0)
			{
				log.Debug(client.Player.Name + ": distance = " + distance + ", speed = " + oldSpeed + ",  coords/sec=" + coordsPerSec);
			}
			if (jumpDetect > 0)
			{
				log.Debug(client.Player.Name + ": jumpdetect = " + jumpDetect);
			}
			#endif
			#endregion DEBUG

			client.Player.LastPositionUpdateTick = Environment.TickCount;
			client.Player.LastPositionUpdatePoint.X = realX;
			client.Player.LastPositionUpdatePoint.Y = realY;
			client.Player.LastPositionUpdatePoint.Z = realZ;

			int tolerance = ServerProperties.Properties.CPS_TOLERANCE;

			if (client.Player.Steed != null && client.Player.Steed.MaxSpeed > 0)
			{
				tolerance += client.Player.Steed.MaxSpeed;
			}
			else if (client.Player.MaxSpeed > 0)
			{
				tolerance += client.Player.MaxSpeed;
			}

			if (client.Player.IsJumping)
			{
				coordsPerSec = 0;
				jumpDetect = 0;
				client.Player.IsJumping = false;
			}

			if (client.Player.IsAllowedToFly == false && (coordsPerSec > tolerance || jumpDetect > ServerProperties.Properties.JUMP_TOLERANCE))
			{
				bool isHackDetected = true;

				if (coordsPerSec > tolerance)
				{
					// check to see if CPS time tolerance is exceeded
					int lastCPSTick = client.Player.TempProperties.getProperty<int>(LASTCPSTICK, 0);

					if (environmentTick - lastCPSTick > ServerProperties.Properties.CPS_TIME_TOLERANCE)
					{
						isHackDetected = false;
					}
				}

				if (isHackDetected)
				{
					StringBuilder builder = new StringBuilder();
					builder.Append("MOVEHACK_DETECT");
					builder.Append(": CharName=");
					builder.Append(client.Player.Name);
					builder.Append(" Account=");
					builder.Append(client.Account.Name);
					builder.Append(" IP=");
					builder.Append(client.TcpEndpointAddress);
					builder.Append(" CPS:=");
					builder.Append(coordsPerSec);
					builder.Append(" JT=");
					builder.Append(jumpDetect);
					ChatUtil.SendDebugMessage(client, builder.ToString());

					if (client.Account.PrivLevel == 1)
					{
						GameServer.Instance.LogCheatAction(builder.ToString());

						if (ServerProperties.Properties.ENABLE_MOVEDETECT)
						{
							if (ServerProperties.Properties.BAN_HACKERS && false) // banning disabled until this technique is proven accurate
							{
								DBBannedAccount b = new DBBannedAccount();
								b.Author = "SERVER";
								b.Ip = client.TcpEndpointAddress;
								b.Account = client.Account.Name;
								b.DateBan = DateTime.Now;
								b.Type = "B";
								b.Reason = string.Format("Autoban MOVEHACK:(CPS:{0}, JT:{1}) on player:{2}", coordsPerSec, jumpDetect, client.Player.Name);
								GameServer.Database.AddObject(b);
								GameServer.Database.SaveObject(b);

								string message = "";
								
								message = "You have been auto kicked and banned due to movement hack detection!";
								for (int i = 0; i < 8; i++)
								{
									client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
									client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
								}

								client.Out.SendPlayerQuit(true);
								client.Player.SaveIntoDatabase();
								client.Player.Quit(true);
							}
							else
							{
								string message = "";
								
								message = "You have been auto kicked due to movement hack detection!";
								for (int i = 0; i < 8; i++)
								{
									client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
									client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
								}

								client.Out.SendPlayerQuit(true);
								client.Player.SaveIntoDatabase();
								client.Player.Quit(true);
							}
							client.Disconnect();
							return;
						}
					}
				}

				client.Player.TempProperties.setProperty(LASTCPSTICK, environmentTick);
			}

			ushort headingflag = packet.ReadShort();
			client.Player.Heading = (ushort)(headingflag & 0xFFF);
			ushort flyingflag = packet.ReadShort();
			byte flags = (byte)packet.ReadByte();

			if (client.Player.X != realX || client.Player.Y != realY)
			{
				client.Player.TempProperties.setProperty(LASTMOVEMENTTICK, client.Player.CurrentRegion.Time);
			}
			client.Player.X = realX;
			client.Player.Y = realY;
			client.Player.Z = realZ;

			if (zoneChange)
			{
				// update client zone information for waterlevel and diving
				client.Out.SendPlayerPositionAndObjectID();
			}

			// used to predict current position, should be before
			// any calculation (like fall damage)
			client.Player.MovementStartTick = Environment.TickCount;

			// Begin ---------- New Area System -----------
			if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				IList oldAreas = client.Player.CurrentAreas;

				// Because we may be in an instance we need to do the area check from the current region
				// rather than relying on the zone which is in the skinned region.  - Tolakram

				IList newAreas = client.Player.CurrentRegion.GetAreasOfZone(newZone, client.Player);

				// Check for left areas
				if (oldAreas != null)
				{
					foreach (IArea area in oldAreas)
					{
						if (!newAreas.Contains(area))
						{
							area.OnPlayerLeave(client.Player);
						}
					}
				}
				// Check for entered areas
				foreach (IArea area in newAreas)
				{
					if (oldAreas == null || !oldAreas.Contains(area))
					{
						area.OnPlayerEnter(client.Player);
					}
				}
				// set current areas to new one...
				client.Player.CurrentAreas = newAreas;
				client.Player.AreaUpdateTick = client.Player.CurrentRegion.Time + 2000; // update every 2 seconds
			}
			// End ---------- New Area System -----------


			client.Player.TargetInView = ((flags & 0x10) != 0);
			client.Player.GroundTargetInView = ((flags & 0x08) != 0);
			client.Player.IsTorchLighted = ((flags & 0x80) != 0);
			//7  6  5  4  3  2  1 0
			//15 14 13 12 11 10 9 8
			//                1 1

			const string SHLASTUPDATETICK = "SHPLAYERPOSITION_LASTUPDATETICK";
			const string SHLASTFLY = "SHLASTFLY_STRING";
			const string SHLASTSTATUS = "SHLASTSTATUS_STRING";
			int SHlastTick = client.Player.TempProperties.getProperty<int>(SHLASTUPDATETICK);
			int SHlastFly = client.Player.TempProperties.getProperty<int>(SHLASTFLY);
			int SHlastStatus = client.Player.TempProperties.getProperty<int>(SHLASTSTATUS);
			int SHcount = client.Player.TempProperties.getProperty<int>(SHSPEEDCOUNTER);
			int status = (data & 0x1FF ^ data) >> 8;
			int fly = (flyingflag & 0x1FF ^ flyingflag) >> 8;

			if (client.Player.IsJumping)
			{
				SHcount = 0;
			}

			if (SHlastTick != 0 && SHlastTick != environmentTick)
			{
				if (((SHlastStatus == status || (status & 0x8) == 0)) && ((fly & 0x80) != 0x80) && (SHlastFly == fly || (SHlastFly & 0x10) == (fly & 0x10) || !((((SHlastFly & 0x10) == 0x10) && ((fly & 0x10) == 0x0) && (flyingflag & 0x7FF) > 0))))
				{
					if ((environmentTick - SHlastTick) < 400)
					{
						SHcount++;

						if (SHcount > 1 && client.Account.PrivLevel > 1)
						{
							//Apo: ?? no idea how to name the first parameter for language translation: 1: ??, 2: {detected} ?, 3: {count} ?
							client.Out.SendMessage(string.Format("SH: ({0}) detected: {1}, count {2}", 500 / (environmentTick - SHlastTick), environmentTick - SHlastTick, SHcount), eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
						}

						if (SHcount % 5 == 0)
						{
							StringBuilder builder = new StringBuilder();
							builder.Append("TEST_SH_DETECT[");
							builder.Append(SHcount);
							builder.Append("] (");
							builder.Append(environmentTick - SHlastTick);
							builder.Append("): CharName=");
							builder.Append(client.Player.Name);
							builder.Append(" Account=");
							builder.Append(client.Account.Name);
							builder.Append(" IP=");
							builder.Append(client.TcpEndpointAddress);
							GameServer.Instance.LogCheatAction(builder.ToString());

							if (client.Account.PrivLevel > 1)
							{
								client.Out.SendMessage("SH: Logging SH cheat.", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

								if (SHcount >= ServerProperties.Properties.SPEEDHACK_TOLERANCE)
									client.Out.SendMessage("SH: Player would have been banned!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
							}

							if ((client.Account.PrivLevel == 1) && SHcount >= ServerProperties.Properties.SPEEDHACK_TOLERANCE)
							{
								if (ServerProperties.Properties.BAN_HACKERS)
								{
									DBBannedAccount b = new DBBannedAccount();
									b.Author = "SERVER";
									b.Ip = client.TcpEndpointAddress;
									b.Account = client.Account.Name;
									b.DateBan = DateTime.Now;
									b.Type = "B";
									b.Reason = string.Format("Autoban SH:({0},{1}) on player:{2}", SHcount, environmentTick - SHlastTick, client.Player.Name);
									GameServer.Database.AddObject(b);
									GameServer.Database.SaveObject(b);

									string message = "";
									
									message = "You have been auto kicked and banned for speed hacking!";
									for (int i = 0; i < 8; i++)
									{
										client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
										client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
									}

									client.Out.SendPlayerQuit(true);
									client.Player.SaveIntoDatabase();
									client.Player.Quit(true);
								}
								else
								{
									string message = "";
									
									message = "You have been auto kicked for speed hacking!";
									for (int i = 0; i < 8; i++)
									{
										client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
										client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
									}

									client.Out.SendPlayerQuit(true);
									client.Player.SaveIntoDatabase();
									client.Player.Quit(true);
								}
								client.Disconnect();
								return;
							}
						}
					}
					else
					{
						SHcount = 0;
					}

					SHlastTick = environmentTick;
				}
			}
			else
			{
				SHlastTick = environmentTick;
			}

			int state = ((data >> 10) & 7);
			client.Player.IsClimbing = (state == 7);
			client.Player.IsSwimming = (state == 1);
			if (state == 3 && client.Player.TempProperties.getProperty<bool>(GamePlayer.DEBUG_MODE_PROPERTY, false) == false && client.Player.IsAllowedToFly == false) //debugFly on, but player not do /debug on (hack)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("HACK_FLY");
				builder.Append(": CharName=");
				builder.Append(client.Player.Name);
				builder.Append(" Account=");
				builder.Append(client.Account.Name);
				builder.Append(" IP=");
				builder.Append(client.TcpEndpointAddress);
				GameServer.Instance.LogCheatAction(builder.ToString());
				{
					if (ServerProperties.Properties.BAN_HACKERS)
					{
						DBBannedAccount b = new DBBannedAccount();
						b.Author = "SERVER";
						b.Ip = client.TcpEndpointAddress;
						b.Account = client.Account.Name;
						b.DateBan = DateTime.Now;
						b.Type = "B";
						b.Reason = string.Format("Autoban flying hack: on player:{0}", client.Player.Name);
						GameServer.Database.AddObject(b);
						GameServer.Database.SaveObject(b);
					}
					string message = "";
					
					message = "Client Hack Detected!";
					for (int i = 0; i < 6; i++)
					{
						client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_ChatWindow);
					}
					client.Out.SendPlayerQuit(true);
					client.Disconnect();
					return;
				}
			}

			SHlastFly = fly;
			SHlastStatus = status;
			client.Player.TempProperties.setProperty(SHLASTUPDATETICK, SHlastTick);
			client.Player.TempProperties.setProperty(SHLASTFLY, SHlastFly);
			client.Player.TempProperties.setProperty(SHLASTSTATUS, SHlastStatus);
			client.Player.TempProperties.setProperty(SHSPEEDCOUNTER, SHcount);
			lock (client.Player.LastUniqueLocations)
			{
				GameLocation[] locations = client.Player.LastUniqueLocations;
				GameLocation loc = locations[0];
				if (loc.X != realX || loc.Y != realY || loc.Z != realZ || loc.RegionID != client.Player.CurrentRegionID)
				{
					loc = locations[locations.Length - 1];
					Array.Copy(locations, 0, locations, 1, locations.Length - 1);
					locations[0] = loc;
					loc.X = realX;
					loc.Y = realY;
					loc.Z = realZ;
					loc.Heading = client.Player.Heading;
					loc.RegionID = client.Player.CurrentRegionID;
				}
			}

			//**************//
			//FALLING DAMAGE//
			//**************//
			if (GameServer.ServerRules.CanTakeFallDamage(client.Player) && client.Player.IsSwimming == false)
			{
				int maxLastZ = client.Player.MaxLastZ;
				/* Are we on the ground? */
				if ((flyingflag >> 15) != 0)
				{
					int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);
					int fallSpeed = (flyingflag & 0xFFF) - 100 * safeFallLevel; // 0x7FF fall speed and 0x800 bit = fall speed overcaped
					int fallMinSpeed = 400;
					int fallDivide = 6;
					if (client.Version >= GameClient.eClientVersion.Version188)
					{
						fallMinSpeed = 500;
						fallDivide = 15;
					}

                    int fallPercent = Math.Min(99, (fallSpeed - (fallMinSpeed + 1)) / fallDivide);

                    if (fallSpeed > fallMinSpeed)
                    {
                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.FallingDamage"),
                        eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        client.Player.CalcFallDamage(fallPercent);
                    }

					client.Player.MaxLastZ = client.Player.Z;
				}

				else
				{
					// always set Z if on the ground
					if (flyingflag == 0)
						client.Player.MaxLastZ = client.Player.Z;
					// set Z if in air and higher than old Z
					else if (maxLastZ < client.Player.Z)
						client.Player.MaxLastZ = client.Player.Z;
				}
			}
			//**************//

			byte[] con168 = packet.ToArray();
			//Riding is set here!
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				client.Player.Heading = client.Player.Steed.Heading;

				con168[2] = 0x18; // Set ride flag 00011000
				con168[3] = 0; // player speed = 0 while ride
				con168[12] = (byte)(client.Player.Steed.ObjectID >> 8); //heading = steed ID
				con168[13] = (byte)(client.Player.Steed.ObjectID & 0xFF);
				con168[14] = (byte)0;
				con168[15] = (byte)(client.Player.Steed.RiderSlot(client.Player)); // there rider slot this player
			}
			else if (!client.Player.IsAlive)
			{
				con168[2] &= 0xE3; //11100011
				con168[2] |= 0x14; //Set dead flag 00010100
			}
			//diving is set here
			con168[16] &= 0xFB; //11 11 10 11
			if ((con168[16] & 0x02) != 0x00)
			{
				client.Player.IsDiving = true;
				con168[16] |= 0x04;
			}
			else
				client.Player.IsDiving = false;

			con168[16] &= 0xFC; //11 11 11 00 cleared Wireframe & Stealth bits
			if (client.Player.IsWireframe)
			{
				con168[16] |= 0x01;
			}
			//stealth is set here
			if (client.Player.IsStealthed)
			{
				con168[16] |= 0x02;
			}

			con168[17] = (byte)((con168[17] & 0x80) | client.Player.HealthPercent);
			// zone ID has changed in 1.72, fix bytes 11 and 12
			byte[] con172 = (byte[])con168.Clone();
			if (packetVersion == 168)
			{
				// client sent v168 pos update packet, fix 172 version
				con172[10] = 0;
				con172[11] = con168[10];
			}
			else
			{
				// client sent v172 pos update packet, fix 168 version
				con168[10] = con172[11];
				con168[11] = 0;
			}

			GSUDPPacketOut outpak168 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			//Now copy the whole content of the packet
			outpak168.Write(con168, 0, 18/*con168.Length*/);
			outpak168.WritePacketLength();

			GSUDPPacketOut outpak172 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			//Now copy the whole content of the packet
			outpak172.Write(con172, 0, 18/*con172.Length*/);
			outpak172.WritePacketLength();

			//			byte[] pak168 = outpak168.GetBuffer();
			//			byte[] pak172 = outpak172.GetBuffer();
			//			outpak168 = null;
			//			outpak172 = null;
			GSUDPPacketOut outpak190 = null;

			foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null)
					continue;
				//No position updates for ourselves
				if (player == client.Player)
				{
					// Update Player Cache (Client sending Packet is admitting he's already having it)
					player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = GameTimer.GetTickCount();
					continue;
				}
				//no position updates in different houses
				if ((client.Player.InHouse || player.InHouse) && player.CurrentHouse != client.Player.CurrentHouse)
					continue;

				if (client.Player.MinotaurRelic != null)
				{
					MinotaurRelic relic = client.Player.MinotaurRelic;
					if (!relic.Playerlist.Contains(player) && player != client.Player)
					{
						relic.Playerlist.Add(player);
						player.Out.SendMinotaurRelicWindow(client.Player, client.Player.MinotaurRelic.Effect, true);
					}
				}

				if (!client.Player.IsStealthed || player.CanDetect(client.Player))
				{
					// Update Player Cache
					player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = GameTimer.GetTickCount();
					
					//forward the position packet like normal!
					if (player.Client.Version >= GameClient.eClientVersion.Version190)
					{
						if (outpak190 == null)
						{
							outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
							outpak190.Write(con172, 0, 18/*con172.Length*/);
							outpak190.WriteByte(client.Player.ManaPercent);
							outpak190.WriteByte(client.Player.EndurancePercent);
							outpak190.FillString(client.Player.CharacterClass.Name, 32);
							// roleplay flag, if == 1, show name (RP) with gray color
							if (client.Player.RPFlag)
								outpak190.WriteByte(1);
							else outpak190.WriteByte(0);
							outpak190.WriteByte((con168.Length == 54) ? con168[53] : (byte) 0); // send last byte for 190+ packets
							outpak190.WritePacketLength();
						}
						player.Out.SendUDPRaw(outpak190);
					}
					else if (player.Client.Version >= GameClient.eClientVersion.Version172)
						player.Out.SendUDPRaw(outpak172);
					else
						player.Out.SendUDPRaw(outpak168);
				}
				else
					player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
			}

			if (client.Player.CharacterClass.ID == (int)eCharacterClass.Warlock)
			{
				//Send Chamber effect
				client.Player.Out.SendWarlockChamberEffect(client.Player);
			}

			//handle closing of windows
			//trade window
			if (client.Player.TradeWindow != null)
			{
				if (client.Player.TradeWindow.Partner != null)
				{
					if (!client.Player.IsWithinRadius(client.Player.TradeWindow.Partner, WorldMgr.GIVE_ITEM_DISTANCE))
						client.Player.TradeWindow.CloseTrade();
				}
			}
		}
	}
}
