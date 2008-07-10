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
#define NOENCRYPTION
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using log4net;
using DOL.Language;
using DOL.GS.Effects;
using DOL.GS.Quests;

namespace DOL.GS.PacketHandler
{
	[PacketLib(190, GameClient.eClientVersion.Version190)]
	public class PacketLib190 : PacketLib189
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.90 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib190(GameClient client)
			: base(client)
		{
		}

		public override void SendUpdatePoints()
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterPointsUpdate));
			pak.WriteInt((uint)m_gameClient.Player.RealmPoints);
			pak.WriteShort(m_gameClient.Player.LevelPermill);
			pak.WriteShort((ushort)m_gameClient.Player.SkillSpecialtyPoints);
			pak.WriteInt((uint)m_gameClient.Player.BountyPoints);
			pak.WriteShort((ushort)m_gameClient.Player.RealmSpecialtyPoints);
			pak.WriteShort(m_gameClient.Player.ChampionLevelPermill);
			pak.WriteLongLowEndian((ulong)m_gameClient.Player.Experience);
			pak.WriteLongLowEndian((ulong)m_gameClient.Player.ExperienceForNextLevel);
			pak.WriteLongLowEndian(0);//champExp
			pak.WriteLongLowEndian(0);//champExpNextLevel
			SendTCP(pak);
		}

		public override void SendStatusUpdate(byte sittingFlag)
		{
			if (m_gameClient.Player == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CharacterStatusUpdate));
			pak.WriteByte(m_gameClient.Player.HealthPercent);
			pak.WriteByte(m_gameClient.Player.ManaPercent);
			pak.WriteByte(sittingFlag);
			pak.WriteByte(m_gameClient.Player.EndurancePercent);
			pak.WriteByte(m_gameClient.Player.ConcentrationPercent);
			//			pak.WriteShort((byte) (m_gameClient.Player.IsAlive ? 0x00 : 0x0f)); // 0x0F if dead ??? where it now ?
			pak.WriteByte(0);// unk
			pak.WriteShort((ushort)m_gameClient.Player.MaxMana);
			pak.WriteShort(100); // MaxEndurance // TODO MaxEndurance when GamePlayer will have +Endurance bonuses
			pak.WriteShort((ushort)m_gameClient.Player.MaxConcentration);
			pak.WriteShort((ushort)m_gameClient.Player.MaxHealth);
			pak.WriteShort((ushort)m_gameClient.Player.Health);
			pak.WriteShort((ushort)m_gameClient.Player.Endurance);
			pak.WriteShort((ushort)m_gameClient.Player.Mana);
			pak.WriteShort((ushort)m_gameClient.Player.Concentration);
			SendTCP(pak);
		}
		// 190c+ SendUpdateIcons
		public override void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.UpdateIcons));
			long initPos = pak.Position;

			int fxcount = 0;
			int entriesCount = 0;
			lock (m_gameClient.Player.EffectList)
			{
				pak.WriteByte(0);	// effects count set in the end
				pak.WriteByte(0);	// unknown
				pak.WriteByte(0);	// unknown
				pak.WriteByte(0);	// unknown
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						fxcount++;
						if (changedEffects != null && !changedEffects.Contains(effect))
							continue;
						//						log.DebugFormat("adding [{0}] '{1}'", fxcount-1, effect.Name);
						pak.WriteByte((byte)(fxcount - 1)); // icon index
						pak.WriteByte((effect is GameSpellEffect) ? (byte)(fxcount - 1) : (byte)0xff);
						byte ImmunByte = 0;
						if (effect is GameSpellAndImmunityEffect)
						{
							GameSpellAndImmunityEffect immunity = (GameSpellAndImmunityEffect)effect;
							if (immunity.ImmunityState) ImmunByte = 1;
						}
						pak.WriteByte(ImmunByte); // new in 1.73; if non zero says "protected by" on right click
						// bit 0x08 adds "more..." to right click info
						pak.WriteShort(effect.Icon);
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
					//					log.DebugFormat("adding [{0}] (empty)", fxcount-1);
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
			return;
		}

		public override void SendMasterLevelWindow(byte ml)
		{
			// If required ML=0 then send current player ML data
			byte mlrequired = (byte)(ml == 0 ? (m_gameClient.Player.MLLevel == 0 ? 1 : m_gameClient.Player.MLLevel) : ml);

			string description = "";
			double MLXPpercent = 0;

			if (m_gameClient.Player.MLLevel < 10)
				MLXPpercent = 100.0 * (double)m_gameClient.Player.MLExperience / (double)m_gameClient.Player.GetMLExperienceForLevel((int)(m_gameClient.Player.MLLevel+1));
			else MLXPpercent = 100.0; // ML10 has no MLXP, so always 100%

			GSTCPPacketOut pak = new GSTCPPacketOut((byte)ePackets.MasterLevelWindow);
			pak.WriteByte((byte)MLXPpercent); // MLXP (displayed in window)
			pak.WriteByte((byte)100);
			pak.WriteByte((byte)(m_gameClient.Player.MLLevel+1)); // ML level + 1
			pak.WriteByte(0);
			pak.WriteShort((ushort)0); // exp1 ? new in 1.90
			pak.WriteShort((ushort)0); // exp2 ? new in 1.90
			pak.WriteByte(ml); // Required ML
			if (mlrequired < 10)
			{
				// ML level completition is displayed client side (Step 11)
				for (int i = 1; i < 11; i++)
				{
					if (!m_gameClient.Player.HasFinishedMLStep((int)mlrequired, i))
						description = i.ToString() + ". " + LanguageMgr.GetTranslation(m_gameClient, String.Format("SendMasterLevelWindow.Uncomplete.ML{0}.Step{1}", mlrequired, i));
					else
						description = i.ToString() + ". " + LanguageMgr.GetTranslation(m_gameClient, String.Format("SendMasterLevelWindow.Complete.ML{0}.Step{1}", mlrequired, i));
					pak.WritePascalString(description);
				}
			}
			else pak.WriteByte(0);

			pak.WriteByte(0);
			SendTCP(pak);
		}
	}
}
