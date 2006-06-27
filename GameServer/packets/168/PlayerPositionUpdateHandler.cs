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
			if (client.Player.ObjectState != GameObject.eObjectState.Active)
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

//			if(!GameServer.ServerRules.IsAllowedDebugMode(client) 
//				&& (speed > client.Player.MaxSpeed + SPEED_TOL))


			if ((data & 0x200) != 0)
				speed = -speed;
			client.Player.CurrentSpeed = speed;

			//Don't use the "sit" flag (data&0x1000) because it is
			//useful only for displaying sit status to other clients,
			//but NOT useful at all for changing sit states, use
			//speed instead!
			//Movement can only make players stand, but not sit them down...
			//moved to player.CurrentSpeed:
			//if(client.Player.CurrentSpeed!=0 && client.Player.Sitting)
			//	client.Player.Sit(false);

			client.Player.Strafing = ((data & 0xe000) != 0);


			int realZ = packet.ReadShort();
			ushort xOffsetInZone = packet.ReadShort();
			ushort yOffsetInZone = packet.ReadShort();
			ushort currentZoneID;
			if (packetVersion == 168)
			{
				currentZoneID = (ushort) packet.ReadByte();
				packet.Skip(1); //0x00 padding for zoneID
			}
			else
			{
				currentZoneID = packet.ReadShort();
			}

			//Zone newZone = WorldMgr.GetZone(currentZoneID);
			Zone newZone = client.Player.Region.GetZone(currentZoneID);
			if (newZone == null)
			{
				if (log.IsErrorEnabled)
					log.Error(client.Player.Name + "'s position in unknown zone! => " + currentZoneID);

				// move to bind point if not at it
				Point bind = new Point(
					client.Player.BindX,
					client.Player.BindY,
					client.Player.BindZ);
				
				if (client.Player.RegionId != client.Player.BindRegion
					|| client.Player.Position != bind)
				{
					client.Out.SendMessage("Unknown zone, moving to bind point.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					client.Player.MoveTo(
						(ushort) client.Player.BindRegion,
						bind,
						(ushort) client.Player.BindHeading);
				}

				return 1; // TODO: what should we do? player lost in space
			}

			// move to bind if player fell through the floor
			if (realZ == 0)
			{
				Point bind = new Point(
					client.Player.BindX,
					client.Player.BindY,
					client.Player.BindZ);

				client.Player.MoveTo(
					(ushort) client.Player.BindRegion,
					bind,
					(ushort) client.Player.BindHeading);
				return 1;
			}

			int realX = newZone.XOffset + xOffsetInZone;
			int realY = newZone.YOffset + yOffsetInZone;
			Point realPos = new Point(realX, realY, realZ);
			//DOLConsole.WriteLine("zx="+newZone.XOffset+" zy="+newZone.YOffset+" XOff="+xOffsetInZone+" YOff="+yOffsetInZone+" curZ="+currentZoneID);


			Zone lastPositionUpdateZone = client.Player.LastPositionUpdateZone;
			if (newZone != lastPositionUpdateZone)
			{
				//If the region changes -> make sure we don't take any falling damage
				if (lastPositionUpdateZone != null
					&& newZone.Region.RegionID != lastPositionUpdateZone.Region.RegionID)
				{
					client.Player.MaxLastZ = int.MinValue;
				}
				client.Out.SendMessage("You have entered " + newZone.Description + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.LastPositionUpdateZone = newZone;
			}

			int lastTick = client.Player.TempProperties.getIntProperty(LASTUPDATETICK, 0);
			if (lastTick != 0 && client.Account.PrivLevel == ePrivLevel.Player)
			{
				int tickDiff = Environment.TickCount - lastTick;
				int maxDist = 0;
				if (client.Player.Steed != null)
					maxDist = tickDiff*client.Player.Steed.MaxSpeed/1000; //If mounted, take mount speed
				else
					maxDist = tickDiff*client.Player.MaxSpeed/1000; //else take player speed

				maxDist = (maxDist*DIST_TOLERANCE/100);

				
				int plyDist = client.Player.Position.GetDistance(realPos);

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


			client.Player.Position = realPos;
			// used to predict current position, should be before
			// any calculation (like fall damage)
			client.Player.MovementStartTick = Environment.TickCount;

			// Begin ---------- New Area System -----------					
			if (client.Player.Region.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				IList oldAreas = client.Player.CurrentAreas;
				IList newAreas = AreaMgr.GetAreasOfSpot(newZone, realPos);

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
			//7  6  5  4  3  2  1 0
			//15 14 13 12 11 10 9 8
			//                1 1     

			lock (client.Player.LastUniqueLocations)
			{
				GameLocation[] locations = client.Player.LastUniqueLocations;
				GameLocation loc = locations[0];
				if (loc.Position != realPos || loc.Region != client.Player.Region)
				{
					loc = locations[locations.Length - 1];
					Array.Copy(locations, 0, locations, 1, locations.Length - 1);
					locations[0] = loc;
					loc.Position = realPos;
					loc.Heading = (ushort)client.Player.Heading;
					loc.Region = client.Player.Region;
				}
			}


			//DOLConsole.WriteLine("Player position: X="+realX+" Y="+realY+ "heading="+client.Player.Heading);

			/*
			int flyspeed = (flyingflag&0xFFF);
			if((flyingflag&0x1000)!=0)
				flyspeed = -flyspeed;

			if(flyspeed<0)
			{
				if(client.Player.Z>lastZ)
					lastZ=client.Player.Z;
			}
			else
			{
				lastZ=int.MinValue;
			}

			DOLConsole.WriteLine("Flying Speed="+flyspeed+" rest = "+(flyingflag>>15)+" "+((flyingflag>>14)&0x01)+" "+((flyingflag>>13)&0x01)+"  "+((flyingflag>>12)&0x01));
			*/
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
					int damageDistance = maxLastZ - realPos.Z - minFallDamageDistance - (100 * safeFallLevel);

					if (damageDistance > 0)
					{
						if (safeFallLevel > 0)
							client.Out.SendMessage("The damage was lessened by your safe_fall ability!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

						damageDistance = Math.Min(damageDistance, maxFallDamageDistance);
						// player with 100% health should never die from fall damage
						//					double damagePercent =  (double)damageDistance / maxFallDamageDistance;
						//					damagePercent = Math.Min(0.99, damagePercent);
						//					int damageValue = (int)(client.Player.MaxHealth * damagePercent);

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
						//client.Player.TakeDamage(null, eDamageType.Falling, damageValue, 0);
						client.Player.ChangeHealth(client.Player, GameLiving.eHealthChangeType.Unknown, -damageValue);

						//Update the player's health to all other players around
						//foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						//	player.Out.SendCombatAnimation(null, client.Player, 0, 0, 0, 0, 0, client.Player.HealthPercent);

						client.Out.SendMessage("You lose endurance!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
						client.Player.EndurancePercent -= (byte)damagePercent;
					}
					client.Player.MaxLastZ = realPos.Z;
				}
				else
				{
					// always set Z if on the ground
					if (flyingflag == 0)
						client.Player.MaxLastZ = realPos.Z;
						// set Z if in air and higher than old Z
					else if (maxLastZ < realPos.Z)
						client.Player.MaxLastZ = realPos.Z;
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
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
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
			con168[16] &= 0xFB; //11 11 10 11
			if ((con168[16] & 0x02) != 0x00)
			{
				con168[16] |= 0x04;
			}
			//stealth is set here 
			if (client.Player.IsStealthed)
			{
				con168[16] |= 0x02;
			}

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

			foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
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

			//Moving too far cancels trades
			if (client.Player.TradeWindow != null)
			{
				if (!client.Player.Position.CheckDistance(client.Player.TradeWindow.Partner.Position, WorldMgr.GIVE_ITEM_DISTANCE))
				{
					client.Player.Out.SendMessage("You move too far from your trade partner and cancel the trade!,", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Player.TradeWindow.Partner.Out.SendMessage("Your trade partner moves too far away and cancels the trade!,", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Player.TradeWindow.CloseTrade();
				}
			}

			//Notify the GameEventMgr of the moving player
			GameEventMgr.Notify(GamePlayerEvent.Moving, client.Player);

			//client.Player.NotifyPositionChange();

			return 1;
		}
	}
}