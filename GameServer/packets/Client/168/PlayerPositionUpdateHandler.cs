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
using System.Linq;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.PositionUpdate, "Handles player position updates for client 1.124+", eClientStatus.PlayerInGame)]
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
			int oldSpeed = client.Player.CurrentSpeed;
			
			int newPlayerX = (int)packet.ReadFloatLowEndian();
			int newPlayerY = (int)packet.ReadFloatLowEndian();
			int newPlayerZ = (int)packet.ReadFloatLowEndian();
			int newPlayerSpeed = (int)packet.ReadFloatLowEndian();
			int newPlayerZSpeed = (int)packet.ReadFloatLowEndian();
			ushort sessionID = packet.ReadShort();
			ushort currentZoneID = packet.ReadShort();
			ushort playerState = packet.ReadShort();
            ushort fallingDMG = packet.ReadShort();            
			ushort newHeading = packet.ReadShort();
			byte playerAction = (byte)packet.ReadByte();
			packet.Skip(2); // unknown bytes x2
			byte playerHealth = (byte)packet.ReadByte();
			// two trailing bytes, no data
			
			//int speed = (newPlayerSpeed & 0x1FF);
			//Flags1 = (eFlags1)playerState;
			//Flags2 = (eFlags2)playerAction;                        

            if (client.Player.IsMezzed || client.Player.IsStunned)
			{
				// Nidel: updating client.Player.CurrentSpeed instead of speed
				client.Player.CurrentSpeed = 0;
			}
			else
			{
				client.Player.CurrentSpeed = (short)newPlayerSpeed;
			}
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
			if (newPlayerZ == 0)
			{
				client.Player.MoveTo(
					(ushort)client.Player.BindRegion,
					client.Player.BindXpos,
					client.Player.BindYpos,
					(ushort)client.Player.BindZpos,
					(ushort)client.Player.BindHeading
				);
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

                var translation = client.GetTranslation(newZone) as DBLanguageZone;
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
			int timediff = Environment.TickCount - client.Player.LastPositionUpdateTick;
			int distance = 0;

			if (timediff > 0)
			{
				distance = client.Player.LastPositionUpdatePoint.GetDistanceTo(new Point3D(newPlayerX, newPlayerY, newPlayerZ));
				coordsPerSec = distance * 1000 / timediff;

				if (distance < 100 && client.Player.LastPositionUpdatePoint.Z > 0)
				{
					jumpDetect = newPlayerZ - client.Player.LastPositionUpdatePoint.Z;
				}
			}			

			client.Player.LastPositionUpdateTick = Environment.TickCount;
			client.Player.LastPositionUpdatePoint.X = newPlayerX;
			client.Player.LastPositionUpdatePoint.Y = newPlayerY;
			client.Player.LastPositionUpdatePoint.Z = newPlayerZ;

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

			if (client.Player.X != newPlayerX || client.Player.Y != newPlayerY)
			{
				client.Player.TempProperties.setProperty(LASTMOVEMENTTICK, client.Player.CurrentRegion.Time);
			}
            //patch 0024 expermental
            //client.Player.X = realX;
            //client.Player.Y = realY;
            //client.Player.Z = realZ;

            client.Player.SetCoords(newPlayerX, newPlayerY, newPlayerZ, (ushort)(newHeading & 0xFFF)); //patch 0024 expermental
            if (zoneChange)
			{
				// update client zone information for waterlevel and diving
				client.Out.SendPlayerPositionAndObjectID();
			}

			// used to predict current position, should be before
			// any calculation (like fall damage)
			//client.Player.MovementStartTick = Environment.TickCount; experimental 0024

			// Begin ---------- New Area System -----------
			if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				var oldAreas = client.Player.CurrentAreas;

				// Because we may be in an instance we need to do the area check from the current region
				// rather than relying on the zone which is in the skinned region.  - Tolakram

				var newAreas = client.Player.CurrentRegion.GetAreasOfZone(newZone, client.Player);

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
            lock (client.Player.LastUniqueLocations)
			{
				GameLocation[] locations = client.Player.LastUniqueLocations;
				GameLocation loc = locations[0];
				if (loc.X != newPlayerX || loc.Y != newPlayerY || loc.Z != newPlayerZ || loc.RegionID != client.Player.CurrentRegionID)
				{
					loc = locations[locations.Length - 1];
					Array.Copy(locations, 0, locations, 1, locations.Length - 1);
					locations[0] = loc;
					loc.X = newPlayerX;
					loc.Y = newPlayerY;
					loc.Z = newPlayerZ;
					loc.Heading = client.Player.Heading;
					loc.RegionID = client.Player.CurrentRegionID;
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
                        
                        int fallSpeed = (newPlayerZSpeed * -1) - (100 * safeFallLevel);
                        
                        int fallDivide = 15;

                        int fallPercent = Math.Min(99, (fallSpeed - (501)) / fallDivide);
                        
                        if (fallSpeed > 500)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.FallingDamage"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage(string.Format("You take {0}% of you max hits in damage.", fallPercent), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("You lose endurance", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            client.Player.CalcFallDamage(fallPercent);
                        }

                        client.Player.MaxLastZ = client.Player.Z;
                    }
                    else
                    {
                        if (maxLastZ < client.Player.Z || client.Player.IsRiding || newPlayerZSpeed > -150) // is riding, for dragonflys
                        {
                            client.Player.MaxLastZ = client.Player.Z;
                        }
                    }
                }
                catch
                {
                    log.Warn("error when attempting to calculate fall damage");
                }
            }

            if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
            {
                client.Player.Heading = client.Player.Steed.Heading;
                newHeading = (ushort)client.Player.Steed.ObjectID; // test patch 0064
                //playerState |= 0x1800;
            }
            else if ((playerState >> 10) == 4) // patch 0062 fix bug on release preventing players from receiving res sickness
            {
                client.Player.IsSitting = true;
            }
            /*else if ((playerState & 0x1000) != 0) //player sitting when releasing on death, dead = 0x1400
            {
                client.Player.IsSitting = true;
            }*/
            GSUDPPacketOut outpak = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));                      
            
            //patch 0069 test to fix player swim out byte flag
            byte playerOutAction = 0x00;
            if (client.Player.IsDiving)
            {
                playerOutAction |= 0x04;
            }
            if (client.Player.TargetInView)
            {
                playerOutAction |= 0x30;
            }
            if (client.Player.GroundTargetInView)
            {
                playerOutAction |= 0x08;
            }
            if (client.Player.IsTorchLighted)
            {
                playerOutAction |= 0x80;
            }
            //stealth is set here
            if (client.Player.IsStealthed)
			{
				playerState |= 0x0200;
			}
            
            outpak.WriteFloatLowEndian(newPlayerX);
            outpak.WriteFloatLowEndian(newPlayerY);
            outpak.WriteFloatLowEndian(newPlayerZ);
            outpak.WriteFloatLowEndian(newPlayerSpeed);
            outpak.WriteFloatLowEndian(newPlayerZSpeed);
            outpak.WriteShort(sessionID);
            outpak.WriteShort(currentZoneID);
            outpak.WriteShort(playerState);
            outpak.WriteShort(0); // fall damage flag, dont need to send it
            outpak.WriteShort(newHeading);
            outpak.WriteByte(playerOutAction);
            outpak.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
            outpak.WriteByte(0);
            outpak.WriteByte((byte)(client.Player.HealthPercent + (client.Player.AttackState ? 0x80 : 0)));            
            outpak.WriteByte(client.Player.ManaPercent);
            outpak.WriteByte(client.Player.EndurancePercent);
            //Now copy the whole content of the packet
            //outpak1119.Write(pak1119, 0, 36);
            outpak.WritePacketLength();

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
                /* patch 0068 no minotaur logic
				if (client.Player.MinotaurRelic != null)
				{
					MinotaurRelic relic = client.Player.MinotaurRelic;
					if (!relic.Playerlist.Contains(player) && player != client.Player)
					{
						relic.Playerlist.Add(player);
						player.Out.SendMinotaurRelicWindow(client.Player, client.Player.MinotaurRelic.Effect, true);
					}
				}*/

				if (!client.Player.IsStealthed || player.CanDetect(client.Player))
				{
					// Update Player Cache // test patch remove players in cache 0065
					//player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = GameTimer.GetTickCount();
									
					player.Out.SendUDPRaw(outpak);					
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
	}
}
