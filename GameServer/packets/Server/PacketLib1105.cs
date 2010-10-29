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
			// type 0 & type 1
			base.SendTrainerWindow();

			if (m_gameClient == null || m_gameClient.Player == null) return;

			// type 4 (skills) & type 3 (description)
			GSTCPPacketOut paksub = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow));
			long pos = paksub.Position;
			IList spls = m_gameClient.Player.GetSpellLines();
			IList spcls = m_gameClient.Player.GetSpecList();
			IList<string> autotrains = m_gameClient.Player.CharacterClass.GetAutotrainableSkills();

			paksub.WriteByte(0); //size
			paksub.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
			paksub.WriteByte(3);
			paksub.WriteByte(0);
			paksub.WriteByte(0);

			// Fill out an array that tells the client how many spec points are available at each of
			// this characters levels.  This seems to only be used for the 'Level Required' display on
			// the new trainer window.  I've changed the calls below to use AdjustedSpecPointsMultiplier
			// to enable servers that allow levels > 50 to train properly by modifying points available per level. - Tolakram

			for (byte i = 2; i <= 50; i++)
			{
				int specpoints = 0;

				if (i <= 5)
					specpoints = i;

				if (i > 5)
					specpoints = i * m_gameClient.Player.CharacterClass.AdjustedSpecPointsMultiplier / 10;

				if (i > 40 && i != 50)
					specpoints += i * m_gameClient.Player.CharacterClass.AdjustedSpecPointsMultiplier / 20;

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
						if(autotrains.Contains(spc.KeyName))
							paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.Level / 4));
						else paksub.WriteByte(0);

						foreach (Spell sp in lss)
						{
							pak.WritePascalString(sp.Name);
							paksub.WriteByte((byte)sp.Level);
							paksub.WriteShort((ushort)sp.Icon);
							paksub.WriteByte((byte)sp.SkillType);
							paksub.WriteByte(0);	// unk
							paksub.WriteByte((byte)((byte)sp.SkillType == 3 ? 254 : 255));
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
							if(autotrains.Contains(spc.KeyName))
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
						pak.WriteByte((byte)(playerRA.Level));

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