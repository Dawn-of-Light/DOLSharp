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
using DOL.GS.Geometry;

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

			if (client.Version >= GameClient.eClientVersion.Version1124)
			{
				_HandlePacket1124(client, packet);
				return;
			}

			int environmentTick = Environment.TickCount;
			int oldSpeed = client.Player.CurrentSpeed;

			//read the state of the player
			packet.Skip(2); //PID
			ushort speedData = packet.ReadShort();
			int speed = (speedData & 0x1FF);

			//			if(!GameServer.ServerRules.IsAllowedDebugMode(client)
			//				&& (speed > client.Player.MaxSpeed + SPEED_TOL))


			if ((speedData & 0x200) != 0)
				speed = -speed;

			if (client.Player.IsMezzed || client.Player.IsStunned)
				// Nidel: updating client.Player.CurrentSpeed instead of speed
				client.Player.CurrentSpeed = 0;
			else
				client.Player.CurrentSpeed = (short)speed;

			client.Player.IsStrafing = ((speedData & 0xe000) != 0);

			int realZ = packet.ReadShort();
			ushort xOffsetInZone = packet.ReadShort();
			ushort yOffsetInZone = packet.ReadShort();
			ushort currentZoneID = packet.ReadShort();

			var headingflag = packet.ReadShort();
			var flyingflag = packet.ReadShort();
			var flags = (byte)packet.ReadByte();


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
                client.Player.MoveTo(client.Player.BindPosition);
				return;
			}

            var newCoordinate = Coordinate.Create(
                x: newZone.Offset.X + xOffsetInZone,
                y: newZone.Offset.Y + yOffsetInZone,
                z: realZ);

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

                var translation = LanguageMgr.GetTranslation(client, newZone) as DBLanguageZone;
                if (translation != null)
                {
                    if (!Util.IsEmpty(translation.Description))
                        description = translation.Description;

                    if (!Util.IsEmpty(translation.ScreenDescription))
                        screenDescription = translation.ScreenDescription;
                }

                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.Entered", description),
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage(screenDescription, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);

				client.Player.LastPositionUpdateZone = newZone;
			}

			int coordsPerSec = 0;
			int jumpDetect = 0;
			int timediff = Environment.TickCount - client.Player.MovementStartTick;
			int distance = 0;

			if (timediff > 0)
			{
				distance = (int)client.Player.LastUpdateCoordinate.DistanceTo(newCoordinate);
				coordsPerSec = distance * 1000 / timediff;

				if (distance < 100 && client.Player.LastUpdateCoordinate.Z > 0)
				{
					jumpDetect = realZ - client.Player.LastUpdateCoordinate.Z;
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

			int tolerance = ServerProperties.Properties.CPS_TOLERANCE;

			if (client.Player.Steed != null && client.Player.Steed.MaxSpeed > 0)
				tolerance += client.Player.Steed.MaxSpeed;
			else if (client.Player.MaxSpeed > 0)
				tolerance += client.Player.MaxSpeed;

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

			if (client.Player.Coordinate.X != newCoordinate.X || client.Player.Coordinate.Y != newCoordinate.Y)
            {
				client.Player.TempProperties.setProperty(LASTMOVEMENTTICK, client.Player.CurrentRegion.Time);
            }
            client.Player.Position = Position.Create(client.Player.Position.RegionID, coordinate: newCoordinate, heading: (ushort)(headingflag & 0xFFF));

			// update client zone information for waterlevel and diving
			if (zoneChange)
				client.Out.SendPlayerPositionAndObjectID();

			// Begin ---------- New Area System -----------
			if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				var oldAreas = client.Player.CurrentAreas;

				// Because we may be in an instance we need to do the area check from the current region
				// rather than relying on the zone which is in the skinned region.  - Tolakram

				var newAreas = client.Player.CurrentRegion.GetAreasOfZone(newZone, client.Player.Coordinate);

				// Check for left areas
				if (oldAreas != null)
					foreach (IArea area in oldAreas)
						if (!newAreas.Contains(area))
							area.OnPlayerLeave(client.Player);
				// Check for entered areas
				foreach (IArea area in newAreas)
					if (oldAreas == null || !oldAreas.Contains(area))
						area.OnPlayerEnter(client.Player);
				// set current areas to new one...
				client.Player.CurrentAreas = newAreas;
				client.Player.AreaUpdateTick = client.Player.CurrentRegion.Time + 2000; // update every 2 seconds
			}
			// End ---------- New Area System -----------


			client.Player.TargetInView = (flags & 0x10) != 0;
			client.Player.GroundTargetInView = (flags & 0x08) != 0;
			client.Player.IsTorchLighted = (flags & 0x80) != 0;
			client.Player.IsDiving = (flags & 0x02) != 0x00;
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
			int status = (speedData & 0x1FF ^ speedData) >> 8;
			int fly = (flyingflag & 0x1FF ^ flyingflag) >> 8;

			if (client.Player.IsJumping)
				SHcount = 0;
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
				SHlastTick = environmentTick;

			int state = ((speedData >> 10) & 7);
			client.Player.IsClimbing = (state == 7);
			client.Player.IsSwimming = (state == 1);
			// debugFly on, but player not do /debug on (hack)
			if (state == 3 && !client.Player.TempProperties.getProperty(GamePlayer.DEBUG_MODE_PROPERTY, false) && !client.Player.IsAllowedToFly)
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
			lock (client.Player.LastUniquePositions)
			{
				var positions = client.Player.LastUniquePositions;
				var pos = positions[0];
                var newPosition = client.Player.Position.With(coordinate: newCoordinate);
                if (pos.Coordinate != newPosition.Coordinate)
                {
                    pos = positions[positions.Length - 1];
                    Array.Copy(positions, 0, positions, 1, positions.Length - 1);
                    positions[0] = newPosition;
                }
			}

			//**************//
			//FALLING DAMAGE//
			//**************//
			int fallSpeed = 0;
			double fallDamage = 0;
			if (GameServer.ServerRules.CanTakeFallDamage(client.Player) && !client.Player.IsSwimming)
			{
				int maxLastZ = client.Player.MaxLastZ;
				/* Are we on the ground? */
				if ((flyingflag >> 15) != 0)
				{
					int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);
					fallSpeed = (flyingflag & 0xFFF) - 100 * safeFallLevel; // 0x7FF fall speed and 0x800 bit = fall speed overcaped
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
						fallDamage = client.Player.CalcFallDamage(fallPercent);
					}

					client.Player.MaxLastZ = client.Player.Position.Z;
				}

				else
				{
					// always set Z if on the ground
					if (flyingflag == 0)
						client.Player.MaxLastZ = client.Player.Position.Z;
					// set Z if in air and higher than old Z
					else if (maxLastZ < client.Player.Position.Z)
						client.Player.MaxLastZ = client.Player.Position.Z;
				}
			}
			//**************//

            //Riding is set here!
            if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
				client.Player.Orientation = client.Player.Steed.Orientation;

			var outpak = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			outpak.WriteShort((ushort)client.SessionID);
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
				outpak.WriteShort(0x1800);
			else
			{
				var rSpeed = client.Player.IsIncapacitated ? 0 : client.Player.CurrentSpeed;
				ushort content;
				if (rSpeed < 0)
					content = (ushort)((rSpeed < -511 ? 511 : -rSpeed) + 0x200);
				else
					content = (ushort)(rSpeed > 511 ? 511 : rSpeed);

				if (!client.Player.IsAlive)
					content |= 5 << 10;
				else
				{
					ushort pState = 0;
					if (client.Player.IsSwimming)
						pState = 1;
					if (client.Player.IsClimbing)
						pState = 7;
					if (client.Player.IsSitting)
						pState = 4;
					if (client.Player.IsStrafing)
						pState |= 8;
					content |= (ushort)(pState << 10);
				}
				outpak.WriteShort(content);
			}
            var zoneCoord = client.Player.Coordinate - client.Player.CurrentZone.Offset;
			outpak.WriteShort((ushort)zoneCoord.Z);
			outpak.WriteShort((ushort)zoneCoord.X);
			outpak.WriteShort((ushort)zoneCoord.Y);
			// Write Zone
			outpak.WriteShort(client.Player.CurrentZone.ZoneSkinID);

			// Copy Heading && Falling or Write Steed
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				outpak.WriteShort((ushort)client.Player.Steed.ObjectID);
				outpak.WriteShort((ushort)client.Player.Steed.RiderSlot(client.Player));
			}
			else
			{
				// Set Player always on ground, this is an "anti lag" packet
				ushort contenthead = (ushort)(client.Player.Orientation.InHeading + (true ? 0x1000 : 0));
				outpak.WriteShort(contenthead);
				outpak.WriteShort(0); // No Fall Speed.
			}

			// Write Flags
			byte flagcontent = 0;
			if (client.Player.IsWireframe)
				flagcontent |= 0x01;
			if (client.Player.IsStealthed)
				flagcontent |= 0x02;
			if (client.Player.IsDiving)
				flagcontent |= 0x04;
			if (client.Player.IsTorchLighted)
				flagcontent |= 0x80;
			outpak.WriteByte(flagcontent);

			// Write health + Attack
			outpak.WriteByte((byte)(client.Player.HealthPercent + (client.Player.AttackState ? 0x80 : 0)));

			// Write Remainings.
			outpak.WriteByte(client.Player.ManaPercent);
			outpak.WriteByte(client.Player.EndurancePercent);
			var outpakArr = outpak.ToArray();

			var outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			outpak190.Write(outpakArr, 5, outpakArr.Length - 5);
			outpak190.FillString(client.Player.Salutation, 32);
			outpak190.WriteByte((byte)(client.Player.RPFlag ? 1 : 0)); // roleplay flag, if == 1, show name (RP) with gray color
			outpak190.WriteByte(0); // send last byte for 190+ packets
			outpak190.WritePacketLength();

			GSUDPPacketOut outpak1112 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			outpak1112.Write(outpakArr, 5, outpakArr.Length - 5);
			outpak1112.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak1112.WriteByte(0); //outpak1112.WriteByte((con168.Length == 22) ? con168[21] : (byte)0);
			outpak1112.WritePacketLength();

			GSUDPPacketOut outpak1124 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			byte playerAction = 0x00;
			if (client.Player.IsDiving)
				playerAction |= 0x04;
			if (client.Player.TargetInView)
				playerAction |= 0x30;
			if (client.Player.GroundTargetInView)
				playerAction |= 0x08;
			if (client.Player.IsTorchLighted)
				playerAction |= 0x80;
			if (client.Player.IsStealthed)
				playerAction |= 0x02;
			ushort playerState = 0;
			outpak1124.WriteFloatLowEndian(client.Player.Position.X);
			outpak1124.WriteFloatLowEndian(client.Player.Position.Y);
			outpak1124.WriteFloatLowEndian(client.Player.Position.Z);
			outpak1124.WriteFloatLowEndian(client.Player.CurrentSpeed);
			outpak1124.WriteFloatLowEndian(fallSpeed);
			outpak1124.WriteShort((ushort)client.SessionID);
			outpak1124.WriteShort(currentZoneID);
			outpak1124.WriteShort(playerState);
			outpak1124.WriteShort((ushort)(client.Player.Steed?.RiderSlot(client.Player) ?? 0)); // fall damage flag coming in, steed seat position going out
			outpak1124.WriteShort(client.Player.Orientation.InHeading);
			outpak1124.WriteByte(playerAction);
			outpak1124.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak1124.WriteByte(0);
			outpak1124.WriteByte((byte)(client.Player.HealthPercent + (client.Player.AttackState ? 0x80 : 0)));
			outpak1124.WriteByte(client.Player.ManaPercent);
			outpak1124.WriteByte(client.Player.EndurancePercent);
			outpak1124.WritePacketLength();


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
					if (player.Client.Version >= GameClient.eClientVersion.Version1124)
						player.Out.SendUDP(outpak1124);
					else if (player.Client.Version >= GameClient.eClientVersion.Version1112)
						player.Out.SendUDP(outpak1112);
					else if (player.Client.Version >= GameClient.eClientVersion.Version190)
						player.Out.SendUDP(outpak190);
				}
				else
					player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
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

		private void _HandlePacket1124(GameClient client, GSPacketIn packet)
		{
			//Tiv: in very rare cases client send 0xA9 packet before sending S<=C 0xE8 player world initialize
			if ((client.Player.ObjectState != GameObject.eObjectState.Active) ||
				(client.ClientState != GameClient.eClientState.Playing))
				return;

			int environmentTick = Environment.TickCount;
			int oldSpeed = client.Player.CurrentSpeed;

			var newPlayerX = packet.ReadFloatLowEndian();
			var newPlayerY = packet.ReadFloatLowEndian();
			var newPlayerZ = packet.ReadFloatLowEndian();
			var newPlayerSpeed = packet.ReadFloatLowEndian();
			var newPlayerZSpeed = packet.ReadFloatLowEndian();
			ushort sessionID = packet.ReadShort();
			if (client.Version >= GameClient.eClientVersion.Version1127)
				packet.ReadShort(); // object ID
			ushort currentZoneID = packet.ReadShort();
			ushort playerState = packet.ReadShort();
			ushort fallingDMG = packet.ReadShort();
			ushort newHeading = packet.ReadShort();
			byte playerAction = (byte)packet.ReadByte();
			packet.Skip(2); // unknown bytes x2
			byte playerHealth = (byte)packet.ReadByte();
			// two trailing bytes, no data + 2 more for 1.127+

			//int speed = (newPlayerSpeed & 0x1FF);
			//Flags1 = (eFlags1)playerState;
			//Flags2 = (eFlags2)playerAction;                        

			if (client.Player.IsMezzed || client.Player.IsStunned)
				client.Player.CurrentSpeed = 0;
			else
				client.Player.CurrentSpeed = (short)newPlayerSpeed;
			/*
			client.Player.IsStrafing = Flags1 == eFlags1.STRAFELEFT || Flags1 == eFlags1.STRAFERIGHT;
			client.Player.IsDiving = Flags2 == eFlags2.DIVING ? true : false;
			client.Player.IsSwimming = Flags1 == eFlags1.SWIMMING ? true : false;
			if (client.Player.IsRiding)
				Flags1 = eFlags1.RIDING;
			client.Player.IsClimbing = Flags1 == eFlags1.CLIMBING ? true : false;
			if (!client.Player.IsAlive)
				Flags1 = eFlags1.DEAD;*/

			client.Player.IsJumping = ((playerAction & 0x40) != 0);
			client.Player.IsStrafing = ((playerState & 0xe000) != 0);

			Zone newZone = WorldMgr.GetZone(currentZoneID);
			if (newZone == null)
			{
				if (!client.Player.TempProperties.getProperty("isbeingbanned", false))
				{
					log.Error(client.Player.Name + "'s position in unknown zone! => " + currentZoneID);
					GamePlayer player = client.Player;
					player.TempProperties.setProperty("isbeingbanned", true);
					player.MoveToBind();
				}

				return; // TODO: what should we do? player lost in space
			}

			// move to bind if player fell through the floor
			if (newPlayerZ == 0)
			{
				client.Player.MoveToBind();
				return;
			}

			//int realX = newPlayerX;
			//int realY = newPlayerY;
			//int realZ = newPlayerZ;
			bool zoneChange = newZone != client.Player.LastPositionUpdateZone;
			if (zoneChange)
			{
				//If the region changes -> make sure we don't take any falling damage
				if (client.Player.LastPositionUpdateZone != null && newZone.ZoneRegion.ID != client.Player.LastPositionUpdateZone.ZoneRegion.ID)
				{
					client.Player.MaxLastZ = int.MinValue;
				}
				// Update water level and diving flag for the new zone
				client.Out.SendPlayerPositionAndObjectID();

				/*
				 * "You have entered Burial Tomb."
				 * "Burial Tomb"
				 * "Current area is adjusted for one level 1 player."
				 * "Current area has a 50% instance bonus."
				 */

				string description = newZone.Description;
				string screenDescription = description;

				var translation = LanguageMgr.GetTranslation(client, newZone) as DBLanguageZone;
				if (translation != null)
				{
					if (!Util.IsEmpty(translation.Description))
						description = translation.Description;

					if (!Util.IsEmpty(translation.ScreenDescription))
						screenDescription = translation.ScreenDescription;
				}

				client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.Entered", description),
									   eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(screenDescription, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);

				client.Player.LastPositionUpdateZone = newZone;
			}

            var newPosition = Position.Create(
                regionID: newZone.ZoneRegion.ID,
                x: (int)newPlayerX,
                y: (int)newPlayerY,
                z: (int)newPlayerZ,
                heading: (ushort)(newHeading & 0xFFF)
            );

			int coordsPerSec = 0;
			int jumpDetect = 0;
			int timediff = Environment.TickCount - client.Player.MovementStartTick;
			int distance = 0;

			if (timediff > 0)
			{
				distance = (int)client.Player.LastUpdateCoordinate.DistanceTo(newPosition.Coordinate);
				coordsPerSec = distance * 1000 / timediff;

				if (distance < 100 && client.Player.LastUpdateCoordinate.Z > 0)
				{
					jumpDetect = (int)newPlayerZ - client.Player.LastUpdateCoordinate.Z;
				}
			}

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

			if (!client.Player.IsAllowedToFly && (coordsPerSec > tolerance || jumpDetect > ServerProperties.Properties.JUMP_TOLERANCE))
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
			//client.Player.Heading = (ushort)(newHeading & 0xFFF); //patch 0024 expermental

			if (client.Player.Position.X != newPosition.X || client.Player.Position.Y != newPosition.Y)
			{
				client.Player.TempProperties.setProperty(LASTMOVEMENTTICK, client.Player.CurrentRegion.Time);
			}

            client.Player.Position = newPosition;

			// Begin ---------- New Area System -----------
			if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				var oldAreas = client.Player.CurrentAreas;

				// Because we may be in an instance we need to do the area check from the current region
				// rather than relying on the zone which is in the skinned region.  - Tolakram

				var newAreas = client.Player.CurrentRegion.GetAreasOfZone(newZone, client.Player.Coordinate);

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


			//client.Player.TargetInView = ((flags & 0x10) != 0);
			//client.Player.IsDiving = ((playerAction & 0x02) != 0);
			client.Player.TargetInView = ((playerAction & 0x30) != 0);
			client.Player.GroundTargetInView = ((playerAction & 0x08) != 0);
			client.Player.IsTorchLighted = ((playerAction & 0x80) != 0);
			// patch 0069 player diving is 0x02, but will broadcast to other players as 0x04
			// if player has a pet summoned, player action is sent by client as 0x04, but sending to other players this is skipped
			client.Player.IsDiving = ((playerAction & 0x02) != 0);

			int state = ((playerState >> 10) & 7);
			client.Player.IsClimbing = (state == 7);
			client.Player.IsSwimming = (state == 1);

			//int status = (data & 0x1FF ^ data) >> 8;
			//int fly = (flyingflag & 0x1FF ^ flyingflag) >> 8;
			if (state == 3 && client.Player.TempProperties.getProperty<bool>(GamePlayer.DEBUG_MODE_PROPERTY, false) == false && !client.Player.IsAllowedToFly) //debugFly on, but player not do /debug on (hack)
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
			lock (client.Player.LastUniquePositions)
			{
				var positions = client.Player.LastUniquePositions;
				var pos = positions[0];
				if (pos.Coordinate != newPosition.Coordinate)
				{
					pos = positions[positions.Length - 1];
					Array.Copy(positions, 0, positions, 1, positions.Length - 1);
					positions[0] = newPosition;
				}
			}

			//FALLING DAMAGE

			if (GameServer.ServerRules.CanTakeFallDamage(client.Player) && !client.Player.IsSwimming)
			{
				try
				{
					int maxLastZ = client.Player.MaxLastZ;

					// Are we on the ground?
					if ((fallingDMG >> 15) != 0)
					{
						int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);

						var fallSpeed = (newPlayerZSpeed * -1) - (100 * safeFallLevel);

						int fallDivide = 15;

						var fallPercent = (int)Math.Min(99, (fallSpeed - 501) / fallDivide);

						if (fallSpeed > 500)
						{
							client.Player.CalcFallDamage(fallPercent);
						}

						client.Player.MaxLastZ = client.Player.Position.Z;
					}
					else if (maxLastZ < client.Player.Position.Z || client.Player.IsRiding || newPlayerZSpeed > -150) // is riding, for dragonflys
						client.Player.MaxLastZ = client.Player.Position.Z;
				}
				catch
				{
					log.Warn("error when attempting to calculate fall damage");
				}
			}

			ushort steedSeatPosition = 0;

			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				client.Player.Orientation = client.Player.Steed.Orientation;
				newHeading = (ushort)client.Player.Steed.ObjectID;
			}
			else if ((playerState >> 10) == 4) // patch 0062 fix bug on release preventing players from receiving res sickness
				client.Player.IsSitting = true;

			GSUDPPacketOut outpak1124 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			//patch 0069 test to fix player swim out byte flag
			byte playerOutAction = 0x00;
			if (client.Player.IsDiving)
				playerOutAction |= 0x04;
			if (client.Player.TargetInView)
				playerOutAction |= 0x30;
			if (client.Player.GroundTargetInView)
				playerOutAction |= 0x08;
			if (client.Player.IsTorchLighted)
				playerOutAction |= 0x80;
			if (client.Player.IsStealthed)
				playerOutAction |= 0x02;

			outpak1124.WriteFloatLowEndian(newPlayerX);
			outpak1124.WriteFloatLowEndian(newPlayerY);
			outpak1124.WriteFloatLowEndian(newPlayerZ);
			outpak1124.WriteFloatLowEndian(newPlayerSpeed);
			outpak1124.WriteFloatLowEndian(newPlayerZSpeed);
			outpak1124.WriteShort(sessionID);
			outpak1124.WriteShort(currentZoneID);
			outpak1124.WriteShort(playerState);
			outpak1124.WriteShort(steedSeatPosition); // fall damage flag coming in, steed seat position going out
			outpak1124.WriteShort(newHeading);
			outpak1124.WriteByte(playerOutAction);
			outpak1124.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak1124.WriteByte(0);
			outpak1124.WriteByte((byte)(client.Player.HealthPercent + (client.Player.AttackState ? 0x80 : 0)));
			outpak1124.WriteByte(client.Player.ManaPercent);
			outpak1124.WriteByte(client.Player.EndurancePercent);
			outpak1124.WritePacketLength();

			var outpak1127 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			outpak1127.Write(outpak1124.GetBuffer(), 5, 22); // from position X to sessionID
			outpak1127.WriteShort((ushort)client.Player.ObjectID);
			outpak1127.WriteShort(currentZoneID);
			outpak1127.WriteShort(playerState);
			outpak1127.WriteShort(steedSeatPosition); // fall damage flag coming in, steed seat position going out
			outpak1127.WriteShort(newHeading);
			outpak1127.WriteByte(playerOutAction);
			outpak1127.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak1127.WriteByte(0);
			outpak1127.WriteByte((byte)(client.Player.HealthPercent + (client.Player.AttackState ? 0x80 : 0)));
			outpak1127.WriteByte(client.Player.ManaPercent);
			outpak1127.WriteByte(client.Player.EndurancePercent);
			outpak1127.WriteShort(0);
			outpak1127.WritePacketLength();

			var outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			outpak190.WriteShort((ushort)client.SessionID);
			outpak190.WriteShort((ushort)(client.Player.CurrentSpeed & 0x1FF));
			outpak190.WriteShort((ushort)newPlayerZ);
			var xoff = (ushort)(newPlayerX - (client.Player.CurrentZone?.Offset.X ?? 0));
			outpak190.WriteShort(xoff);
			var yoff = (ushort)(newPlayerY - (client.Player.CurrentZone?.Offset.Y ?? 0));
			outpak190.WriteShort(yoff);
			outpak190.WriteShort(currentZoneID);
			outpak190.WriteShort(newHeading);
			outpak190.WriteShort(steedSeatPosition);
			outpak190.WriteByte(playerAction);
			outpak190.WriteByte((byte)(client.Player.HealthPercent + (client.Player.AttackState ? 0x80 : 0)));
			outpak190.WriteByte(client.Player.ManaPercent);
			outpak190.WriteByte(client.Player.EndurancePercent);

			var outpak1112 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			outpak1112.Write(outpak190.GetBuffer(), 5, (int)outpak190.Length - 5);
			outpak1112.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak1112.WriteByte(0);
			outpak1112.WritePacketLength();

			outpak190.FillString(client.Player.Salutation, 32);
			outpak190.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak190.WriteByte(0);
			outpak190.WritePacketLength();

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

				if (!client.Player.IsStealthed || player.CanDetect(client.Player))
				{
					// Update Player Cache
					player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = GameTimer.GetTickCount();

					if (player.Client.Version >= GameClient.eClientVersion.Version1127)
						player.Out.SendUDP(outpak1127);
					else if (player.Client.Version >= GameClient.eClientVersion.Version1124)
						player.Out.SendUDP(outpak1124);
					else if (player.Client.Version >= GameClient.eClientVersion.Version1112)
						player.Out.SendUDP(outpak1112);
					else
						player.Out.SendUDP(outpak190);
				}
				else
					player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
			}

			//handle closing of windows
			//trade window
			if (client.Player.TradeWindow?.Partner != null && !client.Player.IsWithinRadius(client.Player.TradeWindow.Partner, WorldMgr.GIVE_ITEM_DISTANCE))
				client.Player.TradeWindow.CloseTrade();
		}
	}
}
