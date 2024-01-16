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
using System.IO;
using System.Reflection;

using DOL.Language;
using DOL.GS.Effects;
using DOL.GS.Quests;
using log4net;
using DOL.GS.Finance;

namespace DOL.GS.PacketHandler
{
	[PacketLib(190, GameClient.eClientVersion.Version190)]
	public class PacketLib190 : PacketLib189
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected byte icons;
		public byte Icons {
			get { return icons; }
		}

		/// <summary>
		/// Constructs a new PacketLib for Version 1.90 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib190(GameClient client)
			: base(client)
		{
			// SendUpdateIcons
			icons = 0;
		}

		public override void SendUpdatePoints()
		{
			if (m_gameClient.Player == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterPointsUpdate)))
			{
				pak.WriteInt((uint)m_gameClient.Player.RealmPoints);
				pak.WriteShort(m_gameClient.Player.LevelPermill);
				pak.WriteShort((ushort)m_gameClient.Player.SkillSpecialtyPoints);
				pak.WriteInt((uint)m_gameClient.Player.BountyPointBalance);
				pak.WriteShort((ushort)m_gameClient.Player.RealmSpecialtyPoints);
				pak.WriteShort(m_gameClient.Player.ChampionLevelPermill);
				pak.WriteLongLowEndian((ulong)m_gameClient.Player.Experience);
				pak.WriteLongLowEndian((ulong)m_gameClient.Player.ExperienceForNextLevel);
				pak.WriteLongLowEndian(0);//champExp
				pak.WriteLongLowEndian(0);//champExpNextLevel
				SendTCP(pak);
			}
		}

		public override void SendStatusUpdate(byte sittingFlag)
		{
			if (m_gameClient.Player == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterStatusUpdate)))
			{
				pak.WriteByte(m_gameClient.Player.HealthPercent);
				pak.WriteByte(m_gameClient.Player.ManaPercent);
				pak.WriteByte(sittingFlag);
				pak.WriteByte(m_gameClient.Player.EndurancePercent);
				pak.WriteByte(m_gameClient.Player.ConcentrationPercent);
				//			pak.WriteShort((byte) (m_gameClient.Player.IsAlive ? 0x00 : 0x0f)); // 0x0F if dead ??? where it now ?
				pak.WriteByte(0);// unk
				pak.WriteShort((ushort)m_gameClient.Player.MaxMana);
				pak.WriteShort((ushort)m_gameClient.Player.MaxEndurance);
				pak.WriteShort((ushort)m_gameClient.Player.MaxConcentration);
				pak.WriteShort((ushort)m_gameClient.Player.MaxHealth);
				pak.WriteShort((ushort)m_gameClient.Player.Health);
				pak.WriteShort((ushort)m_gameClient.Player.Endurance);
				pak.WriteShort((ushort)m_gameClient.Player.Mana);
				pak.WriteShort((ushort)m_gameClient.Player.Concentration);
				SendTCP(pak);
			}
		}
		// 190c+ SendUpdateIcons
		public override void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null)
				return;
			
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.UpdateIcons)))
			{
				long initPos = pak.Position;
	
				int fxcount = 0;
				int entriesCount = 0;
				lock (m_gameClient.Player.EffectList)
				{
					pak.WriteByte(0);	// effects count set in the end
					pak.WriteByte(0);	// unknown
					pak.WriteByte(Icons);	// unknown
					pak.WriteByte(0);	// unknown
					foreach (IGameEffect effect in m_gameClient.Player.EffectList)
					{
						if (effect.Icon != 0)
						{
							fxcount++;
							if (changedEffects != null && !changedEffects.Contains(effect))
								continue;
							//						Log.DebugFormat("adding [{0}] '{1}'", fxcount-1, effect.Name);
							pak.WriteByte((byte)(fxcount - 1)); // icon index
							pak.WriteByte((effect is GameSpellEffect) ? (byte)(fxcount - 1) : (byte)0xff);

							byte ImmunByte = 0;
							var gsp = effect as GameSpellEffect;
							if (gsp != null && gsp.IsDisabled)
								ImmunByte = 1;
							pak.WriteByte(ImmunByte); // new in 1.73; if non zero says "protected by" on right click

							// bit 0x08 adds "more..." to right click info
							pak.WriteShort(effect.Icon);
							//pak.WriteShort(effect.IsFading ? (ushort)1 : (ushort)(effect.RemainingTime / 1000));
							pak.WriteShort((ushort)(effect.RemainingTime / 1000));
							pak.WriteShort(effect.InternalID);      // reference for shift+i or cancel spell
							byte flagNegativeEffect = 0;
							if (effect is StaticEffect)
							{
								if (((StaticEffect)effect).HasNegativeEffect)
									flagNegativeEffect = 1;
							}
							else if (effect is GameSpellEffect)
							{
								if (!((GameSpellEffect)effect).SpellHandler.HasPositiveEffect)
									flagNegativeEffect = 1;
							}
							pak.WriteByte(flagNegativeEffect);
							pak.WritePascalString(effect.Name);
							entriesCount++;
						}
					}
	
					int oldCount = lastUpdateEffectsCount;
					lastUpdateEffectsCount = fxcount;
					while (oldCount > fxcount)
					{
						pak.WriteByte((byte)(fxcount++));
						pak.Fill(0, 10);
						entriesCount++;
						//					Log.DebugFormat("adding [{0}] (empty)", fxcount-1);
					}
	
					if (changedEffects != null)
						changedEffects.Clear();
	
					if (entriesCount == 0)
						return; // nothing changed - no update is needed
	
					pak.Position = initPos;
					pak.WriteByte((byte)entriesCount);
					pak.Seek(0, SeekOrigin.End);
	
					SendTCP(pak);
				}
			}
			return;
		}

		public override void SendMasterLevelWindow(byte ml)
		{
			if (m_gameClient == null || m_gameClient.Player == null)
				return;

			// If required ML=0 then send current player ML data
			byte mlToSend = (byte)(ml == 0 ? (m_gameClient.Player.MLLevel == 0 ? 1 : m_gameClient.Player.MLLevel) : ml);

			if (mlToSend > GamePlayer.ML_MAX_LEVEL)
				mlToSend = GamePlayer.ML_MAX_LEVEL;

			double mlXPPercent = 0;
			double mlStepPercent = 0;

			if (m_gameClient.Player.MLLevel < 10)
			{
				mlXPPercent = 100.0 * (double)m_gameClient.Player.MLExperience / (double)m_gameClient.Player.GetMLExperienceForLevel((int)(m_gameClient.Player.MLLevel + 1));
				if (m_gameClient.Player.GetStepCountForML((byte)(m_gameClient.Player.MLLevel + 1)) > 0)
				{
					mlStepPercent = 100.0 * (double)m_gameClient.Player.GetCountMLStepsCompleted((byte)(m_gameClient.Player.MLLevel + 1)) / (double)m_gameClient.Player.GetStepCountForML((byte)(m_gameClient.Player.MLLevel + 1));
				}
			}
			else
			{
				mlXPPercent = 100.0; // ML10 has no MLXP, so always 100%
			}

			using (GSTCPPacketOut pak = new GSTCPPacketOut((byte)eServerPackets.MasterLevelWindow))
			{
				pak.WriteByte((byte)mlXPPercent); // MLXP (blue bar)
				pak.WriteByte((byte)Math.Min(mlStepPercent, 100)); // Step percent (red bar)
				pak.WriteByte((byte)(m_gameClient.Player.MLLevel + 1)); // ML level + 1
				pak.WriteByte(0);
				pak.WriteShort((ushort)0); // exp1 ? new in 1.90
				pak.WriteShort((ushort)0); // exp2 ? new in 1.90
				pak.WriteByte(ml); 
	
				// ML level completion is displayed client side for Step 11
				for (int i = 1; i < 11; i++)
				{
					string description = m_gameClient.Player.GetMLStepDescription(mlToSend, i);
					pak.WritePascalString(description);
				}
	
				pak.WriteByte(0);
				SendTCP(pak);
			}
		}
		
		/// <summary>
		/// This is used to build a server side "Position Object"
		/// Usually Position Packet Should only be relayed
		/// The only purpose of this method is refreshing postion when there is Lag
		/// </summary>
		/// <param name="player"></param>
		public override void SendPlayerForgedPosition(GamePlayer player)
		{
			using (GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.PlayerPosition)))
			{
				// PID
				pak.WriteShort((ushort)player.Client.SessionID);
				
				// Write Speed
				if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
				{
					player.Orientation = player.Steed.Orientation;
					pak.WriteShort(0x1800);
				}
				else
				{
					short rSpeed = player.CurrentSpeed;
					if (player.IsIncapacitated)
						rSpeed = 0;
					
					ushort content;
					if (rSpeed < 0)
					{
						content = (ushort)((Math.Abs(rSpeed) > 511 ? 511 : Math.Abs(rSpeed)) + 0x200);
					}
					else
					{
						content = (ushort)(rSpeed > 511 ? 511 : rSpeed);
					}
					
					if (!player.IsAlive)
					{
						content += 5 << 10;
					}
					else
					{
						ushort state = 0;
						
						if (player.IsSwimming)
							state = 1;
						
						if (player.IsClimbing)
							state = 7;
						
						if (player.IsSitting)
							state = 4;
						
						content += (ushort)(state << 10);
					}
					
					content += (ushort)(player.IsStrafing ? 1 << 13 : 0 << 13);
					
					pak.WriteShort(content);
				}

                var zoneCoord = player.Coordinate - player.CurrentZone.Offset;
				pak.WriteShort((ushort)zoneCoord.Z);
				pak.WriteShort((ushort)zoneCoord.X);
				pak.WriteShort((ushort)zoneCoord.Y);
				
				// Write Zone
				pak.WriteShort(player.CurrentZone.ZoneSkinID);

				// Copy Heading && Falling or Write Steed
				if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
				{
					pak.WriteShort((ushort)player.Steed.ObjectID);
					pak.WriteShort((ushort)player.Steed.RiderSlot(player));
				}
				else
				{
					// Set Player always on ground, this is an "anti lag" packet
					ushort contenthead = (ushort)(player.Orientation.InHeading + (true ? 0x1000 : 0));
					pak.WriteShort(contenthead);
					// No Fall Speed.
					pak.WriteShort(0);
				}
				
				// Write Flags
				byte flagcontent = 0;
			
				if (player.IsDiving)
				{
					flagcontent += 0x04;
				}
				
				if (player.IsWireframe)
				{
					flagcontent += 0x01;
				}
				
				if (player.IsStealthed)
				{
					flagcontent += 0x02;
				}
				
				if (player.IsTorchLighted)
				{
					flagcontent += 0x80;
				}
				
				pak.WriteByte(flagcontent);

				// Write health + Attack
				byte healthcontent = (byte)(player.HealthPercent + (player.AttackState ? 0x80 : 0));
			
				pak.WriteByte(healthcontent);
				
				// Write Remainings.
				pak.WriteByte(player.ManaPercent);
				pak.WriteByte(player.EndurancePercent);
				pak.FillString(player.Salutation, 32);
				pak.WriteByte((byte)(player.RPFlag ? 1 : 0));
				pak.WriteByte(0); // send last byte for 190+ packets
	
				SendUDP(pak);
			}
			
			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(player.CurrentRegionID, (ushort)player.ObjectID)] = GameTimer.GetTickCount();
		}

	}
}
