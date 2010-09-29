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
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.Commands;
using DOL.GS.Effects;
using DOL.Database;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using log4net;

namespace DOL.GS.PacketHandler
{
	
	[PacketLib(1105, GameClient.eClientVersion.Version1105)]
	public class PacketLib1105 : PacketLib1104
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Client Version 1.105
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1105(GameClient client)
			: base(client)
		{
		}
		
		/// <summary>
		/// SendUpdateIcons method
		/// </summary>
		public override void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.UpdateIcons));
			long initPos = pak.Position;

			int fxcount = 0;
			int entriesCount = 0;
			lock (m_gameClient.Player.EffectList)
			{
				pak.WriteByte(0);	// effects count set in the end
				pak.WriteByte(0);	// unknown
				pak.WriteByte(1);	// new in 1.105
				pak.WriteByte(0);	// unknown
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						fxcount++;
						if (changedEffects != null && !changedEffects.Contains(effect))
							continue;

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
				}

				if (changedEffects != null)
					changedEffects.Clear();

				if (entriesCount == 0)
					return; // nothing changed - no update is needed

				pak.Position = initPos;
				pak.WriteByte((byte)entriesCount);
				pak.Seek(0, System.IO.SeekOrigin.End);

				SendTCP(pak);
			}
			return;
		}

		/// <summary>
		/// Cache trainer windows / characterclass
		/// </summary>
		public static Dictionary<int, IList<byte[]>> TrainerWindows = new Dictionary<int, IList<byte[]>>();

		/// <summary>
		/// SendTrainerWindow method
		/// </summary>
		public override void SendTrainerWindow()
		{
			// type 0 & type 1
			base.SendTrainerWindow();

			if (m_gameClient == null || m_gameClient.Player == null) return;

			lock (TrainerWindows)
			{
				if (!TrainerWindows.ContainsKey(m_gameClient.Player.CharacterClass.ID))
				{
					TrainerWindows.Add(m_gameClient.Player.CharacterClass.ID, new List<byte[]>());
					IList<byte[]> packets = TrainerWindows[m_gameClient.Player.CharacterClass.ID];
					// type 4 (skills) & type 3 (description)
					GSTCPPacketOut paksub = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow));
					long pos = paksub.Position;
					IList spls = m_gameClient.Player.GetSpellLines();
					IList spcls = m_gameClient.Player.GetSpecList();

					paksub.WriteByte(0); //size
					paksub.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
					paksub.WriteByte(3);
					paksub.WriteByte(0);
					paksub.WriteByte(0);

					// Autotrain
					for (byte i = 2; i <= 50; i++)
					{
						int specpoints = 0;
						if (i <= 5)
							specpoints = i;
						if (i > 5)
							specpoints = i * m_gameClient.Player.CharacterClass.SpecPointsMultiplier / 10;
						if (i > 40 && i != 50)
							specpoints += i * m_gameClient.Player.CharacterClass.SpecPointsMultiplier / 20;
						paksub.WriteByte((byte)specpoints);
					}

					byte count = 0;
					int skillindex = 0;
					Dictionary<string, string> Spec2Line = new Dictionary<string, string>();
					foreach (SpellLine line in spls)
					{
						if (line.IsBaseLine) continue;
						if (!Spec2Line.ContainsKey(line.Spec))
							Spec2Line.Add(line.Spec, line.KeyName);
					}

					foreach (Specialization spc in spcls)
					{
						if (Spec2Line.ContainsKey(spc.KeyName)) //spells
						{
							paksub.WriteByte((byte)skillindex);
							skillindex++;

							using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
							{
								List<Spell> lss = SkillBase.GetSpellList(Spec2Line[spc.KeyName]);

								pak.WriteByte((byte)lss.Count);
								pak.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
								pak.WriteByte(4);
								pak.WriteByte(0);
								pak.WriteByte(count);
								count += (byte)lss.Count;

								paksub.WriteByte((byte)lss.Count);
								paksub.WriteByte(0);

								foreach (Spell sp in lss)
								{
									pak.WritePascalString(sp.Name);
									paksub.WriteByte((byte)sp.Level);
									paksub.WriteShort((ushort)sp.Icon);
									paksub.WriteByte((byte)sp.SkillType);
									paksub.WriteByte(0);	// unk
									paksub.WriteByte(255);  // unk
									paksub.WriteShort((ushort)sp.ID);
								}
								pak.WritePacketLength();
								byte[] buf = pak.GetBuffer();
								packets.Add(buf);
							}
						}
						else //styles and other
						{
							paksub.WriteByte((byte)skillindex);
							skillindex++;

							List<Style> lst = SkillBase.GetStyleList(spc.KeyName, m_gameClient.Player.CharacterClass.ID);
							if (lst != null && lst.Count > 0) //styles
							{
								using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
								{
									pak.WriteByte((byte)lst.Count);
									pak.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
									pak.WriteByte(4);
									pak.WriteByte(0);
									pak.WriteByte(count);
									count += (byte)lst.Count;

									paksub.WriteByte((byte)lst.Count);
									paksub.WriteByte(0);

									foreach (var st in lst)
									{
										pak.WritePascalString(st.Name);
										paksub.WriteByte((byte)st.Level);
										paksub.WriteShort((ushort)st.Icon);
										paksub.WriteByte((byte)st.SkillType);
										paksub.WriteByte((byte)st.OpeningRequirementType);
										paksub.WriteByte((byte)st.OpeningRequirementValue);
										paksub.WriteShort((ushort)st.ID);
									}
									pak.WritePacketLength();
									byte[] buf = pak.GetBuffer();
									packets.Add(buf);
								}
							}
							else //other
							{
								paksub.WriteByte(0);
								paksub.WriteByte(0);
							}
						}
					}
					paksub.Seek(pos, System.IO.SeekOrigin.Begin);
					paksub.WriteByte((byte)skillindex); //fix size
					paksub.WritePacketLength();
					byte[] buff = paksub.GetBuffer();
					packets.Add(buff);

				}

				if (TrainerWindows.ContainsKey(m_gameClient.Player.CharacterClass.ID))
				{
					IList<byte[]> packets = TrainerWindows[m_gameClient.Player.CharacterClass.ID];
					if (packets != null && packets.Count > 0)
					{
						foreach (byte[] buf in packets)
							SendTCP(buf);
					}
				}
			}

			// type 5 (realm abilities)
			List<RealmAbility> ras = SkillBase.GetClassRealmAbilities(m_gameClient.Player.CharacterClass.ID);
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)ras.Count);
				pak.WriteByte((byte)m_gameClient.Player.RealmSpecialtyPoints);
				pak.WriteByte(5);
				pak.WriteByte(0);

				foreach (RealmAbility ra in ras)
				{
					if (ra is RR5RealmAbility)
						continue;

					RealmAbility playerRA = (RealmAbility) m_gameClient.Player.GetAbility(ra.KeyName);
					
					if (playerRA != null)
						pak.WriteByte((byte)(playerRA.Level /*- 1*/));
					else
						pak.WriteByte(0);
					
					pak.WriteByte(0);
					pak.WriteByte((byte)ra.MaxLevel);

					for (int i = 0; i < ra.MaxLevel; i++)
						pak.WriteByte((byte)ra.CostForUpgrade(i));

					if (ra.CheckRequirement(m_gameClient.Player))
						pak.WritePascalString(ra.KeyName);
					else
						pak.WritePascalString("[" + ra.Name + "]");
				}
				SendTCP(pak);
			}

		}
	}
}