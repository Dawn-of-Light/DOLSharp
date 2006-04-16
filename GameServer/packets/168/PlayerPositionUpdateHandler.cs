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
using System.Reflection;
using System.Text;
using DOL.GS;
using DOL.Events;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x01 ^ 168, "Handles player position updates")]
	public class PlayerPositionUpdateHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

#if OUTPUT_DEBUG_INFO
		private static int lastX = 0;
		private static int lastY = 0;
		private static int lastUpdateTick = 0;
#endif
		/// <summary>
		/// The tolerance for speedhack etc. ... 180% of max speed
		/// This might sound much, but actually it is not because even
		/// without speedhack the distance varies very much ...
		/// It seems the client lets you walk curves FASTER than straight ahead!
		/// Very funny ;-)
		/// </summary>
		public const int DIST_TOLERANCE = 180;
		/// <summary>
		/// Stores the last update tick inside the player
		/// </summary>
		public const string LASTUPDATETICK = "PLAYERPOSITION_LASTUPDATETICK";
		/// <summary>
		/// Stores the count of times the player is above speedhack tolerance!
		/// If this value reaches 10 or more, a logfile entry is written.
		/// </summary>
		public const string SPEEDHACKCOUNTER = "SPEEDHACKCOUNTER";

		//static int lastZ=int.MinValue;
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player.ObjectState != eObjectState.Active)
				return 1;

			int packetVersion;
			if (client.Version > GameClient.eClientVersion.Version171)
			{
				packetVersion = 172;
			}
			else
			{
				packetVersion = 168;
			}

#if OUTPUT_DEBUG_INFO
			int oldSpeed = client.Player.CurrentSpeed;
#endif
			//read the state of the player
			packet.Skip(2); //PID
			ushort data = packet.ReadShort();
			int speed = (data & 0x1FF);

			if ((data & 0x200) != 0)
				speed = -speed;
			client.Player.CurrentSpeed = speed;

			client.Player.Strafing = ((data & 0xe000) != 0);
			client.Player.IsSwimming = ((data & 0x400) != 0);

			//Don't use the "sit" flag because it is useful only
			//for displaying sit status to other clients,
			//but NOT useful at all for changing sit states, use
			//speed instead!
			// jump ((data & 0x800) != 0))
			// sit ((data & 0x1000) != 0))
			
			int zOffsetInZone = packet.ReadShort();
			ushort xOffsetInZone = packet.ReadShort();
			ushort yOffsetInZone = packet.ReadShort();
			ushort newZoneID;
			if (packetVersion == 168)
			{
				newZoneID = (ushort) packet.ReadByte();
				packet.Skip(1); //0x00 padding for zoneID
			}
			else
			{
				newZoneID = packet.ReadShort();
			}

			Region currentRegion = client.Player.Region;
			Point startingPos = client.Player.Position;
			Zone startingZone = currentRegion.GetZone(startingPos);
			Zone targetZone = currentRegion.GetZone(newZoneID);
			Point targetPos = Point.Zero;
			
			//Geometry engine position update
			if(startingZone != null && targetZone != null && zOffsetInZone >= 0)
			{
				targetPos = new Point(targetZone.XOffset + xOffsetInZone, targetZone.YOffset + yOffsetInZone, zOffsetInZone);
			
				if(targetPos != startingPos)
				{
					#region ANTI SPEED HACK

					int lastTick = client.Player.TempProperties.getIntProperty(LASTUPDATETICK, 0);
					if (lastTick != 0 && client.Account.PrivLevel == ePrivLevel.Player)
					{
						int tickDiff = Environment.TickCount - lastTick;
						int maxDist = 0;
						if (client.Player.Steed != null)
							maxDist = tickDiff*client.Player.Steed.CurrentSpeed/1000; //If mounted, take mount speed
						else
							maxDist = tickDiff*client.Player.MaxSpeed/1000; //else take player speed

						maxDist = (maxDist*DIST_TOLERANCE/100);

				
						int plyDist = client.Player.Position.GetDistance(targetPos);

						//We ignore distances below 100 coordinates because below that the toleranze
						//is nonexistent... eg. sometimes bumping into a wall would cause you to move
						//more than you actually should etc...
						if (plyDist > 100 && plyDist > maxDist)
						{
							int counter = client.Player.TempProperties.getIntProperty(SPEEDHACKCOUNTER, 0);
							counter++;
							if (counter%10 == 0)
							{
								StringBuilder builder = new StringBuilder();
								builder.Append("SPEEDHACK[");
								builder.Append(counter);
								builder.Append("]: ");
								builder.Append("CharName=");
								builder.Append(client.Player.Name);
								builder.Append(" Account=");
								builder.Append(client.Account.AccountName);
								builder.Append(" IP=");
								builder.Append(client.TcpEndpoint);
								GameServer.Instance.LogCheatAction(builder.ToString());
							}
							client.Player.TempProperties.setProperty(SPEEDHACKCOUNTER, counter);
						}
					}
					client.Player.TempProperties.setProperty(LASTUPDATETICK, Environment.TickCount);

					#endregion

					SubZone fromSubZone = startingZone.GetSubZone(startingPos);
					SubZone toSubZone = targetZone.GetSubZone(targetPos);
					if(toSubZone != null && fromSubZone != null && toSubZone != fromSubZone)
					{
						System.Console.WriteLine("(Zone "+startingZone.ZoneID+") Remove player from SubZone ("+client.Player.Position.X+","+client.Player.Position.Y+")");
						startingZone.ObjectExitSubZone(client.Player, fromSubZone);

						System.Console.WriteLine("(Zone "+targetZone.ZoneID+") Add player to SubZone ("+targetPos.X+","+targetPos.Y+")");
						targetZone.ObjectEnterSubZone(client.Player, toSubZone);
					}

					client.Player.UpdateInternalPosition(targetPos);
				}
			}
			else
			{
				// move to bind point if not at it
				client.Out.SendMessage("You try to move from or to a invalid position => moving to bind point.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				client.Player.MoveTo(client.Player.BindRegion, client.Player.BindPosition, (ushort) client.Player.BindHeading);
				return 1; 
			}

			// Begin ---------- New Area System -----------					
			if (currentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				IList oldAreas = client.Player.CurrentAreas;
				IList newAreas = AreaMgr.GetAreasOfSpot(targetZone, targetPos);

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
				client.Player.AreaUpdateTick = client.Player.Region.Time + 1500; // update every 1.5 seconds
			}
			// End ---------- New Area System -----------


			ushort headingflag = packet.ReadShort();
			client.Player.Heading = (ushort) (headingflag & 0xFFF);
			ushort flyingflag = packet.ReadShort();
			byte flags = (byte) packet.ReadByte();
			client.Player.TargetInView = ((flags & 0x10) != 0);
			client.Player.GroundTargetInView = ((flags & 0x08) != 0);
			client.Player.IsDiving = ((flags & 0x02) != 0);
			
			// pet in view ((flags & 0x04) != 0))
			// target in view ((flags & 0x10) != 0) or ((flags & 0x20) != 0))
		
		//	DOLConsole.WriteLine("Flying Speed="+flyspeed+" rest = "+(flyingflag>>15)+" "+((flyingflag>>14)&0x01)+" "+((flyingflag>>13)&0x01)+"  "+((flyingflag>>12)&0x01));
			
			//**************//
			//FALLING DAMAGE//
			//**************//
			if (GameServer.ServerRules.CanTakeFallDamage(client.Player))
			{
				int maxLastZ = client.Player.MaxLastZ;
				/* Are we on the ground? */
				if ((flyingflag >> 15) != 0)
				{
					const int minFallDamageDistance = 180;
					const int maxFallDamageDistance = 1024 - minFallDamageDistance;
					int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);
					int damageDistance = maxLastZ - targetPos.Z - minFallDamageDistance - (100 * safeFallLevel);

					if (damageDistance > 0)
					{
						if (safeFallLevel > 0)
							client.Out.SendMessage("The damage was lessened by your safe_fall ability!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

						damageDistance = Math.Min(damageDistance, maxFallDamageDistance);
						
						// player with 100% health should never die from fall damage
						int damageValue = (client.Player.MaxHealth - 1)*damageDistance/maxFallDamageDistance;
						int damagePercent = damageValue*100/client.Player.MaxHealth;

						client.Out.SendMessage("You take falling damage!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

						if (damagePercent > 0)
						{
							// on live servers this message is not sent sometimes; others are always shown
							client.Out.SendMessage("You take " + damagePercent + "% of your max hits in damage.", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
						}

						//Damage player before sending update
						client.Player.ChangeHealth(null, -damageValue, false);

						//Update the player's health to all other players around
						foreach (GamePlayer player in client.Player.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
							player.Out.SendCombatAnimation(null, client.Player, 0, 0, 0, 0, 0, client.Player.HealthPercent);

						client.Out.SendMessage("You lose endurance!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
						client.Player.EndurancePercent -= (byte)damagePercent;
					}
					client.Player.MaxLastZ = targetPos.Z;
				}
				else if(flyingflag == 0 || maxLastZ < targetPos.Z)
				{
					// always set Z if on the ground or if new z is bigger than old z
					client.Player.MaxLastZ = targetPos.Z;
				}
			}
			//**************//

#if OUTPUT_DEBUG_INFO
			if (lastX != 0 && lastY != 0)
			{
				if (log.IsDebugEnabled)
				{
					int timediff = Environment.TickCount - lastUpdateTick;
					int distance = WorldMgr.GetDistance(lastX, lastY, 0, realX, realY, 0);
					log.Debug("Distanze = " + distance + "Speed=" + oldSpeed + " coords/sec=" + (distance*1000/timediff));
				}
			}
			lastX = realX;
			lastY = realY;
			lastUpdateTick = Environment.TickCount;
#endif


			byte[] con168 = packet.ToArray();

			//Riding is set here!
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == eObjectState.Active)
			{
				client.Player.Heading = client.Player.Steed.Heading;

				con168[2] |= 24; //Set ride flag 00011000
				con168[12] = (byte) (client.Player.Steed.ObjectID >> 8); //heading = steed ID
				con168[13] = (byte) (client.Player.Steed.ObjectID & 0xFF);
			}
			else if (!client.Player.Alive)
			{
				con168[2] &= 0xE3; //11100011
				con168[2] |= 0x14; //Set dead flag 00010100
			}

			//diving is set here
			con168[16] &= 0xF0; //11 11 00 00
			if (client.Player.IsDiving)
			{
				con168[16] |= 0x04;
			}

			//stealth is set here 
			if (client.Player.IsStealthed)
			{
				con168[16] |= 0x02;
			}

			con168[17] = (byte)((con168[17] & 0x80) | client.Player.HealthPercent); 

			// zone ID has changed in 1.72, fix bytes 11 and 12
			byte[] con172 = (byte[]) con168.Clone();
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

			GSUDPPacketOut outpak168 = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerPosition));
			//Now copy the whole content of the packet
			outpak168.Write(con168, 0, con168.Length);
			outpak168.WritePacketLength();

			GSUDPPacketOut outpak172 = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerPosition));
			//Now copy the whole content of the packet
			outpak172.Write(con172, 0, con172.Length);
			outpak172.WritePacketLength();
			
			byte[] pak168 = outpak168.GetBuffer();
			byte[] pak172 = outpak172.GetBuffer();
			outpak168 = null;
			outpak172 = null;

			foreach (GamePlayer player in client.Player.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				//No position updates for ourselves
				if (player == client.Player)
					continue;

				//Check stealth
				if (!client.Player.IsStealthed || player.CanDetect(client.Player))
				{
					//forward the position packet like normal!
					if (player.Client.Version > GameClient.eClientVersion.Version171)
						player.Out.SendUDP(pak172);
					else
						player.Out.SendUDP(pak168);
				}
				else
					player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
			}

			//TODO moving too far from other things like siege weapons to release control...

			//Notify the GameEventMgr of the moving player
			GameEventMgr.Notify(GamePlayerEvent.Moving, client.Player);

			return 1;
		}
	}
}