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
		// Packet Doku (max use 18) :
		// 0-1   Short = Player ID
		// 2-3   Short = (bit strafe left) (bit strafe right) (bit strafe moves) (3 bit state, 0== standing, 1 == swimming, 2 == jump down, 3 == jump up, 1 == sit, 5 == dead, 6 == riding, 7 == climbing) (1 bit sign) (9 bit speed)
		// 4-5   Short = Z
		// 6-7   Short = Xoff
		// 8-9   Short = Yoff
		// 10-11 Short = ZoneID (<172 Byte ZoneID + Ignored Byte)
		// 12-13 Short = (3bit counter) (1 bit floor) (12 bit Heading) or (16 bit steed)
		// 14-15 Short = (1bit hit ground) (2bit empty) (1 bit falling) (1bit fall cap) (11 fall speed) or (8bit empty) (8bit riding slot)
		// 16    Byte  = (1 bit torch) (1bit unknown) (1 bit target visible) (1bit gt visible) (1 bit unknown) (1 bit diving) (1 bit shadow) (1bit wireframe)
		// 17    Byte  = (1 bit attack) (7 bit health)
		
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string LASTMOVEMENTTICK = "PLAYERPOSITION_LASTMOVEMENTTICK";

		public const string CURRENT_POSITION_PACKET = "CURRENT_POSITION_PACKET";
		public const string CURRENT_POSITION_UPDATE = "CURRENT_POSITION_UPDATE";
		
		//static int lastZ=int.MinValue;
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			long startingTime = GameTimer.GetTickCount();
			//Tiv: in very rare cases client send 0xA9 packet before sending S<=C 0xE8 player world initialize
			if ((client.Player.ObjectState != GameObject.eObjectState.Active) ||
			    (client.ClientState != GameClient.eClientState.Playing))
				return;
						
			// Get Packet Data.
			PositionPacketData posPacket = new PositionPacketData(packet, client);
			
			if (posPacket.playerId == 1)
			{
				//log.ErrorFormat("   {0}", packet.ToHumanReadable());
				//log.ErrorFormat("   {0}", posPacket);
			}
			
			// Change what needed
			
			// Get Coords from Zone
			Zone currentZone = WorldMgr.GetZone(posPacket.zoneId);
			if (currentZone == null)
			{
				if (client.Player == null)
					return;
				
				if (!client.Player.TempProperties.getProperty("isbeingoutofzone", false))
				{
					if (log.IsErrorEnabled)
						log.Error(client.Player.Name + "'s position in unknown zone! => " + posPacket.zoneId);
					
					client.Player.TempProperties.setProperty("isbeingoutofzone", true);
					client.Player.MoveToBind();
				}

				return;
			}
			
			// Get Real Coord
			int realX = currentZone.XOffset + posPacket.Xoff;
			int realY = currentZone.YOffset + posPacket.Yoff;

			if (client.Player.X != realX || client.Player.Y != realY)
				client.Player.MovementStartTick = startingTime;
			
			// Don't update if incapacited ?
			// Should use some "vector" algorithm to stuck someone after being incapacited ?
			// Throttle speed ?
			client.Player.X = realX;
			client.Player.Y = realY;
			client.Player.Z = posPacket.Z;
			client.Player.IsTorchLighted = posPacket.torch;
			
			// Get Zone Region Change
			bool zoneChange = currentZone != client.Player.LastPositionUpdateZone;
			bool regionChange = (client.Player.LastPositionUpdateZone == null || client.Player.LastPositionUpdateZone.ZoneRegion != currentZone.ZoneRegion);


			// Check CPS
			int coordsPerSec = 0;
			int jumpDistance = 0;
			long timediff = startingTime - client.LastPositionUpdateTick;
			int distance = 0;
			Point3D newPoint = new Point3D(realX, realY, posPacket.Z);

			// We have to be careful to calculate CPS.
			if (timediff > 0 && client.LastPositionUpdateTick > 0 && !client.Player.IsJumping && !regionChange)
			{
				distance = client.LastPositionUpdatePoint.GetDistanceTo(newPoint, (posPacket.fallSpeed != 0) ? 0 : 1);
				coordsPerSec = (int)(distance * 1000 / timediff);

				if (distance < 100 && client.LastPositionUpdatePoint.Z > 0)
				{
					jumpDistance = posPacket.Z - client.LastPositionUpdatePoint.Z;
				}
			}

			int realSpeed = posPacket.speed;
			int maxSpeed = client.Player.MaxSpeed;
			
			// Check speed to match overcapped. (do not check if straffing)
			if (coordsPerSec > 511 && maxSpeed > 511 && posPacket.strafe == 0)
			{
				if (coordsPerSec >= maxSpeed)
				{
					realSpeed = realSpeed < 0 ? -1 * maxSpeed : maxSpeed;
				}
				else
				{
					int ratio = coordsPerSec / 512;
					if (realSpeed < 0)
					{
						realSpeed = -1 * (ratio * 512) + realSpeed;
					}
					else
					{
						realSpeed = ratio * 512 + realSpeed;
					}
				}
			}
			
			// If player can't move disable speed and strafe.
			if (client.Player.IsIncapacitated)
			{
				realSpeed = 0;
				posPacket.strafe = 0;
			}
			
			// if player can't turn disable heading update
			if (!client.Player.IsTurningDisabled && !(posPacket.state == 6))
			{
				client.Player.Heading = posPacket.heading;
			}
			else
			{
				posPacket.heading = client.Player.Heading;
			}
			
			// Update Player and Client History
			client.Player.CurrentSpeed = (short)realSpeed;
			client.Player.IsStrafing = posPacket.strafe != 0;
			client.Player.GroundTargetInView = posPacket.losGround;
			client.Player.TargetInView = posPacket.losTarget;
			client.Player.IsDiving = posPacket.diving;
			
			client.LastCoordsPerSec = coordsPerSec;
			client.LastPositionUpdateTick = startingTime;
			client.LastPositionUpdatePoint = newPoint;

			// Build Other Packet Data
			GSUDPPacketOut outpak168 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			GSUDPPacketOut outpak172 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			GSUDPPacketOut outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			GSUDPPacketOut outpak1112 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
			
			// Copy PID
			outpak168.WriteShort(posPacket.playerId);
			outpak172.WriteShort(posPacket.playerId);
			outpak190.WriteShort(posPacket.playerId);
			outpak1112.WriteShort(posPacket.playerId);
			
			// Write Speed
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				client.Player.Heading = client.Player.Steed.Heading;
				outpak168.WriteShort(0x1800);
				outpak172.WriteShort(0x1800);
				outpak190.WriteShort(0x1800);
				outpak1112.WriteShort(0x1800);
				
			}
			else
			{
				ushort content;
				if (realSpeed < 0)
				{
					content = (ushort)((Math.Abs(realSpeed) > 511 ? 511 : Math.Abs(realSpeed)) + 0x200);
				}
				else
				{
					content = (ushort)(realSpeed > 511 ? 511 : realSpeed);
				}
				
				if (!client.Player.IsAlive)
				{
					content += 5 << 10;
				}
				else
				{
					content += (ushort)(posPacket.state << 10);
				}
				
				content += (ushort)(posPacket.strafe << 13);
				
				outpak168.WriteShort(content);
				outpak172.WriteShort(content);
				outpak190.WriteShort(content);
				outpak1112.WriteShort(content);
			}
			
			// Copy Position
			outpak168.WriteShort(posPacket.Z);
			outpak172.WriteShort(posPacket.Z);
			outpak190.WriteShort(posPacket.Z);
			outpak1112.WriteShort(posPacket.Z);
			
			outpak168.WriteShort(posPacket.Xoff);
			outpak172.WriteShort(posPacket.Xoff);
			outpak190.WriteShort(posPacket.Xoff);
			outpak1112.WriteShort(posPacket.Xoff);
			
			outpak168.WriteShort(posPacket.Yoff);
			outpak172.WriteShort(posPacket.Yoff);
			outpak190.WriteShort(posPacket.Yoff);
			outpak1112.WriteShort(posPacket.Yoff);
			
			// Write Zone
			outpak168.WriteByte((byte)posPacket.zoneId);
			outpak168.WriteByte(0);
			outpak172.WriteShort(posPacket.zoneId);
			outpak190.WriteShort(posPacket.zoneId);
			outpak1112.WriteShort(posPacket.zoneId);
			
			// Copy Heading && Falling or Write Steed
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				outpak168.WriteShort((ushort)client.Player.Steed.ObjectID);
				outpak172.WriteShort((ushort)client.Player.Steed.ObjectID);
				outpak190.WriteShort((ushort)client.Player.Steed.ObjectID);
				outpak1112.WriteShort((ushort)client.Player.Steed.ObjectID);

				outpak168.WriteShort((ushort)client.Player.Steed.RiderSlot(client.Player));
				outpak172.WriteShort((ushort)client.Player.Steed.RiderSlot(client.Player));
				outpak190.WriteShort((ushort)client.Player.Steed.RiderSlot(client.Player));
				outpak1112.WriteShort((ushort)client.Player.Steed.RiderSlot(client.Player));
			}
			else
			{
				ushort contenthead = (ushort)(posPacket.heading + (posPacket.onGround ? 0x1000 : 0) + (posPacket.headingCount << 13));
				outpak168.WriteShort(contenthead);
				outpak172.WriteShort(contenthead);
				outpak190.WriteShort(contenthead);
				outpak1112.WriteShort(contenthead);
				
				ushort contentfall = (ushort)(posPacket.fallSpeed + (posPacket.fallOvercap ? 0x800 : 0) + (posPacket.fallDown ? 0x1000 : 0) + (posPacket.fallHitGround ? 0x8000 : 0));
				outpak168.WriteShort(contentfall);
				outpak172.WriteShort(contentfall);
				outpak190.WriteShort(contentfall);
				outpak1112.WriteShort(contentfall);
			}
			
			// Write Flags
			byte flagcontent = 0;
			
			if (posPacket.diving)
			{
				flagcontent += 0x04;
			}
			
			if (client.Player.IsWireframe)
			{
				flagcontent += 0x01;
			}
			
			if (client.Player.IsStealthed)
			{
				flagcontent += 0x02;
			}
			
			if (posPacket.torch)
			{
				flagcontent += 0x80;
			}

			outpak168.WriteByte(flagcontent);
			outpak172.WriteByte(flagcontent);
			outpak190.WriteByte(flagcontent);
			outpak1112.WriteByte(flagcontent);

			// Write health
			byte healthcontent = (byte)(client.Player.HealthPercent + (posPacket.attack ? 0x80 : 0));
			
			outpak168.WriteByte(healthcontent);
			outpak172.WriteByte(healthcontent);
			outpak190.WriteByte(healthcontent);
			outpak1112.WriteByte(healthcontent);
			
			outpak168.WritePacketLength();
			outpak172.WritePacketLength();
			
			// Write Remainings.
			outpak190.WriteByte(client.Player.ManaPercent);
			outpak190.WriteByte(client.Player.EndurancePercent);
			outpak190.FillString(client.Player.CharacterClass.Name, 32);
			outpak190.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak190.WriteByte(0); // send last byte for 190+ packets
			outpak190.WritePacketLength();			

			outpak1112.WriteByte(client.Player.ManaPercent);
			outpak1112.WriteByte(client.Player.EndurancePercent);
			outpak1112.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
			outpak1112.WriteByte(0);
			
			outpak1112.WritePacketLength();
						
			// TODO Save Packets
			client.Player.TempProperties.setProperty(CURRENT_POSITION_PACKET, outpak1112);
			client.Player.TempProperties.setProperty(CURRENT_POSITION_UPDATE, startingTime);
			
			foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				// No position update for null or ourself
				if (player == null || player == client.Player)
					continue;

				//no position updates in different houses
				if ((client.Player.InHouse || player.InHouse) && player.CurrentHouse != client.Player.CurrentHouse)
					continue;

				// Send MinotaurRelic update
				if (client.Player.MinotaurRelic != null)
				{
					MinotaurRelic relic = client.Player.MinotaurRelic;
					if (!relic.Playerlist.Contains(player) && player != client.Player)
					{
						relic.Playerlist.Add(player);
						player.Out.SendMinotaurRelicWindow(client.Player, client.Player.MinotaurRelic.Effect, true);
					}
				}

				// Check For Stealth before Sending Packets
				if (!client.Player.IsStealthed || player.CanDetect(client.Player))
				{
					// TODO update even if they should not receive the packet !??? Track Stealthed in Cache ??
					player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = startingTime;
					//forward the position packet like normal!
					if (player.Client.Version >= GameClient.eClientVersion.Version1112)
					{
						player.Out.SendUDPRaw(outpak1112);
					}
					else if (player.Client.Version >= GameClient.eClientVersion.Version190)
					{
						player.Out.SendUDPRaw(outpak190);
					}
					else if (player.Client.Version >= GameClient.eClientVersion.Version172)
					{
						player.Out.SendUDPRaw(outpak172);
					}
					else
					{
						player.Out.SendUDPRaw(outpak168);
					}
				}
				else
				{
					player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
				}
			}
			
			//Send Chamber effect
			if (client.Player.CharacterClass.ID == (int)eCharacterClass.Warlock)
			{
				client.Player.Out.SendWarlockChamberEffect(client.Player);
			}

			//handle closing of trade windows
			if (client.Player.TradeWindow != null)
			{
				if (client.Player.TradeWindow.Partner != null)
				{
					if (!client.Player.IsWithinRadius(client.Player.TradeWindow.Partner, WorldMgr.GIVE_ITEM_DISTANCE))
						client.Player.TradeWindow.CloseTrade();
				}
			}
			
			// Make Checks
			
			// Fell through the floor
			if (posPacket.Z == 0)
			{
				client.Player.MoveToBind();
				return;
			}

			// Zone Change
			if (zoneChange)
			{
				//If the region changes -> make sure we don't take any falling damage
				if (regionChange)
					client.LastMaxHeight = int.MinValue;

				// Update water level and diving flag for the new zone
				client.Out.SendPlayerPositionAndObjectID();

				/*
				 * "You have entered Burial Tomb."
				 * "Burial Tomb"
				 * "Current area is adjusted for one level 1 player."
				 * "Current area has a 50% instance bonus."
				 */

                string description = currentZone.Description;
                string screenDescription = description;

                LanguageDataObject translation = LanguageMgr.GetTranslation(client.Account.Language, currentZone);
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

				client.Player.LastPositionUpdateZone = currentZone;
			}
			
			// Falling Damage
			ApplyFallingDamage(client, posPacket.fallHitGround, posPacket.fallSpeed, posPacket.fallOvercap);
			
			// Area System
			CheckForNewArea(client, currentZone);
		}

		/// <summary>
		/// Compute Falling Values to Damage Player.
		/// </summary>
		private void ApplyFallingDamage(GameClient client, bool hitGround, ushort speed, bool overcap)
		{
			if (GameServer.ServerRules.CanTakeFallDamage(client.Player) && client.Player.IsSwimming == false)
			{
				// Are we on the ground
				if (hitGround)
				{
					int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);
					int fallSpeed = speed - 100 * safeFallLevel;
					
					// 1.68-1.87
					int fallMinSpeed = 400;
					int fallDivide = 6;
					// 1.88+
					if (client.Version >= GameClient.eClientVersion.Version188)
					{
						fallMinSpeed = 500;
						fallDivide = 15;
					}

                    int fallPercent;
                    if (overcap)
                    {
						fallPercent = 99;
                    }
                    else
                    {
                    	fallPercent = Math.Min(99, (fallSpeed - (fallMinSpeed + 1)) / fallDivide);
                    }

                    if (fallSpeed > fallMinSpeed)
                    {
                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.FallingDamage"),
                        eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        client.Player.CalcFallDamage(fallPercent);
                    }
				}
			}
		}
		
		/// <summary>
		/// Check For Area Change around player
		/// </summary>
		/// <param name="client"></param>
		/// <param name="currentZone"></param>
		private void CheckForNewArea(GameClient client, Zone currentZone)
		{			
			if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				IList oldAreas = client.Player.CurrentAreas;

				// Because we may be in an instance we need to do the area check from the current region
				// rather than relying on the zone which is in the skinned region.  - Tolakram

				IList newAreas = client.Player.CurrentRegion.GetAreasOfZone(currentZone, client.Player);

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
		}
		
		/// <summary>
		/// Class to Read a position packet into an easy to use/print class struct
		/// </summary>
		private class PositionPacketData
		{
			public ushort playerId;
			public byte strafe;
			public byte state;
			public short speed;
			public ushort Z;
			public ushort Xoff;
			public ushort Yoff;
			public ushort zoneId;
			public bool onGround;
			public ushort heading;
			public byte headingCount;
			public bool fallHitGround;
			public bool fallOvercap;
			public ushort fallSpeed;
			public bool fallDown;
			public byte ridingSlot;
			public bool torch;
			public bool unkLos;
			public bool losTarget;
			public bool losGround;
			public bool unkLosTarget;
			public bool unkflag;
			public bool diving;
			public bool wireframe;
			public bool attack;
			public byte health;
			
			public PositionPacketData(GSPacketIn packet, GameClient client)
			{
				playerId = packet.ReadShort();
				ushort data = packet.ReadShort();
				strafe = (byte)(data >> 13);
				state = (byte)((data >> 10) & 7);
				
				speed = (short)(data & 0x1FF);
				
				if ((data & 0x200) != 0)
					speed = (short)(speed * -1);
				
				Z = packet.ReadShort();
				Xoff = packet.ReadShort();
				Yoff = packet.ReadShort();
				
				zoneId = packet.ReadShort();
				// Move Zone ID for pre-1.72
				if (client.Version < GameClient.eClientVersion.Version172)
					zoneId = (ushort)(zoneId >> 8);
				
				heading = packet.ReadShort();
				// if riding, heading is steed id
				if (state != 6)
				{
					onGround = ((heading >> 12) & 1) == 1;
					headingCount = (byte)(heading >> 13);
					heading &= 0xFFF;
				}
				
				ushort fall = packet.ReadShort();
				
				if (state != 6)
				{
					fallHitGround = (fall & 0x8000) != 0;
					fallOvercap = (fall & 0x800) != 0;
					fallSpeed = (ushort)(fall & 0x7FF);
					fallDown = (fall & 0x1000) != 0;
				}
				else
				{
					ridingSlot = (byte)(fall & 0xFF);
				}
				
				byte flags = (byte)packet.ReadByte();
				
				torch = (flags & 0x80) != 0;
				unkLos = (flags & 0x40) != 0;
				unkLosTarget = (flags & 0x20) != 0;
				losTarget = (flags & 0x10) != 0;
				losGround = (flags & 0x8) != 0;
				unkflag = (flags & 0x4) != 0;
				diving = (flags & 0x2) != 0;
				wireframe = (flags & 0x1) != 0;
					
				
				byte healthflag = (byte)packet.ReadByte();
				
				attack = (healthflag & 0x80) != 0;
				health = (byte)(healthflag & 0x7F);
				
				packet.Position = 0;
			}
			
			public override string ToString()
			{
				return string.Format("[PositionPacketData PID={0}, Strafe={1}, State={2}, Speed={3}, onGround={8}, Heading={9}, FallHit={10}, FallOvercap={11}, FallSpeed={12}, FallDown={24},  RidingSlot={13}, Torch={14}, UnkLos={15}, LosTarget={16}, LosGround={17}, UnkLos2={18}, unkflag={19}, Diving={20}, Wireframe={21}, Attack={22}, Health={23}]",
				                     playerId, strafe, state, speed, Z, Xoff, Yoff, zoneId, onGround,
									 heading, fallHitGround, fallOvercap, fallSpeed, ridingSlot, torch, unkLos, losTarget, losGround, unkLosTarget,
									 unkflag, diving, wireframe, attack, health, fallDown);
			}

		}
	}
}
