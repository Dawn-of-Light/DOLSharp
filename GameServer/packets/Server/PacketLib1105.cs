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
			// SendUpdateIcons
			icons = 1;
		}

		/// <summary>
		/// SendTrainerWindow method
		/// </summary>
		public override void SendTrainerWindow()
		{
			if (m_gameClient == null || m_gameClient.Player == null) return;

			// type 0 & type 1

			int i = 0;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				IList specs = m_gameClient.Player.GetSpecList();
				pak.WriteByte((byte)specs.Count);
				pak.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
				pak.WriteByte((byte)0);
				pak.WriteByte((byte)0);

				foreach (Specialization spec in specs)
				{
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)spec.Level);
					pak.WriteByte((byte)(spec.Level + 1));
					pak.WritePascalString(spec.Name);
				}
				SendTCP(pak);
			}

			// realm abilities
			List<RealmAbility> raList = SkillBase.GetClassRealmAbilities(m_gameClient.Player.CharacterClass.ID);
			if (raList != null && raList.Count > 0)
			{
				RealmAbility remove = null;
				foreach (RealmAbility ab in raList)
				{
					if (ab is RR5RealmAbility)
					{
						remove = ab;
						break;
					}
				}
				if (remove != null)
					raList.Remove(remove);

				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
				{
					pak.WriteByte((byte)raList.Count);
					pak.WriteByte((byte)m_gameClient.Player.RealmSpecialtyPoints);
					pak.WriteByte((byte)1);
					pak.WriteByte((byte)0);

					i = 0;

					List<RealmAbility> offeredRA = new List<RealmAbility>();
					foreach (RealmAbility ra in raList)
					{
						RealmAbility playerRA = (RealmAbility)m_gameClient.Player.GetAbility(ra.KeyName);
						if (playerRA != null)
						{
							if (playerRA.Level < playerRA.MaxLevel)
							{
								RealmAbility ab = SkillBase.GetAbility(playerRA.KeyName, playerRA.Level + 1) as RealmAbility;
								if (ab != null)
								{
									offeredRA.Add(ab);

									pak.WriteByte((byte)i);
									pak.WriteByte((byte)ab.Level);
									pak.WriteByte((byte)ab.CostForUpgrade(ab.Level - 1));
									bool canBeUsed = ab.CheckRequirement(m_gameClient.Player);
									pak.WritePascalString((canBeUsed ? "" : "[") + ab.Name + (canBeUsed ? "" : "]"));
								}
								else
								{
									log.Error("Ability " + ab.Name + " not found unexpectly");
								}
							}
							else
							{
								offeredRA.Add(playerRA);

								pak.WriteByte((byte)i);
								pak.WriteByte(1);
								pak.WriteByte(0);
								pak.WritePascalString("          ");
							}
						}
						else
						{
							offeredRA.Add(ra);

							pak.WriteByte((byte)i);
							pak.WriteByte((byte)ra.Level);
							pak.WriteByte((byte)ra.CostForUpgrade(ra.Level - 1));
							bool canBeUsed = ra.CheckRequirement(m_gameClient.Player);
							pak.WritePascalString((canBeUsed ? "" : "[") + ra.Name + (canBeUsed ? "" : "]"));
						}
						i++;
					}

					m_gameClient.Player.TempProperties.setProperty("OFFERED_RA", offeredRA);
					SendTCP(pak);
				}
			}


			using (GSTCPPacketOut paksub = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				long pos = paksub.Position;
				IList spls = m_gameClient.Player.GetSpellLines();
				IList spcls = m_gameClient.Player.GetSpecList();
				IList<string> autotrains = m_gameClient.Player.CharacterClass.GetAutotrainableSkills();

				paksub.WriteByte(0); //size
				paksub.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
				paksub.WriteByte(3);
				paksub.WriteByte(0);
				paksub.WriteByte(0);

				for (byte a = 2; a <= 50; a++)
				{
					int specpoints = 0;
					if (a <= 5)
						specpoints = a;
					if (a > 5)
						specpoints = a * m_gameClient.Player.CharacterClass.SpecPointsMultiplier / 10;
					if (a >= 40 && a != 50)
						specpoints += a * m_gameClient.Player.CharacterClass.SpecPointsMultiplier / 20;
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
							if (autotrains.Contains(spc.KeyName))
								paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.Level / 4));
							else paksub.WriteByte(0);

							foreach (Spell sp in lss)
							{
								pak.WritePascalString(sp.Name);
								paksub.WriteByte((byte)sp.Level);
								paksub.WriteShort((ushort)sp.Icon);
								paksub.WriteByte((byte)sp.SkillType);
								paksub.WriteByte(0);	// unk
								paksub.WriteByte((byte)((byte)sp.SkillType == 3 ? 254 : 255));  // unk
								paksub.WriteShort((ushort)sp.ID);
							}

							SendTCP(pak);
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
								if (autotrains.Contains(spc.KeyName))
									paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.Level / 4));
								else paksub.WriteByte(0);

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

								SendTCP(pak);
							}
						}
						else //other
						{
							using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
							{
								pak.WriteByte(0);
								pak.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
								pak.WriteByte(4);
								pak.WriteByte(0);
								pak.WriteByte(count);

								SendTCP(pak);
							}

							paksub.WriteByte(0);
							if (autotrains.Contains(spc.KeyName))
								paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.Level / 4));
							else paksub.WriteByte(0);
						}
					}
				}
				paksub.Seek(pos, System.IO.SeekOrigin.Begin);
				paksub.WriteByte((byte)skillindex); //fix size
				paksub.Seek(0, System.IO.SeekOrigin.End);

				SendTCP(paksub);
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

					RealmAbility playerRA = (RealmAbility)m_gameClient.Player.GetAbility(ra.KeyName);
					if (playerRA != null)
						pak.WriteByte((byte)(playerRA.Level));
					else pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte((byte)ra.MaxLevel);

					for (int a = 0; a < ra.MaxLevel; a++)
						pak.WriteByte((byte)ra.CostForUpgrade(a));

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