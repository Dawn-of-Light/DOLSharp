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
using System.IO;
using System.Reflection;
using DOL.Database;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.Language;
using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1110, GameClient.eClientVersion.Version1110)]
    public class PacketLib1110 : PacketLib1109
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.110
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1110(GameClient client)
            : base(client)
        {

        }

		/// <summary>
		/// New system in v1.110+ for delve info. delve is cached by client in extra file, stored locally.
		/// </summary>
		/// <param name="info"></param>
		public override void SendDelveInfo(string info)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DelveInfo)))
			{
				pak.WriteString(info, 2048);
				pak.WriteByte(0); // 0-terminated
				SendTCP(pak);
			}
		}

		public override void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null)
			{
				return;
			}
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.UpdateIcons)))
			{
				long initPos = pak.Position;
	
				int fxcount = 0;
				int entriesCount = 0;
	
				pak.WriteByte(0); // effects count set in the end
				pak.WriteByte(0); // unknown
				pak.WriteByte(Icons); // unknown
				pak.WriteByte(0); // unknown
	
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						fxcount++;
						if (changedEffects != null && !changedEffects.Contains(effect))
						{
							continue;
						}
	
						//						log.DebugFormat("adding [{0}] '{1}'", fxcount-1, effect.Name);
						pak.WriteByte((byte)(fxcount - 1)); // icon index
						pak.WriteByte((effect is GameSpellEffect || effect.Icon > 5000) ? (byte)(fxcount - 1) : (byte)0xff);
						byte ImmunByte = 0;
						if (effect is GameSpellEffect)
						{
							//if (((GameSpellEffect)effect).ImmunityState)
							if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
							{
								ImmunByte = 1;
							}
						}
						pak.WriteByte(ImmunByte);
						// bit 0x08 adds "more..." to right click info
						pak.WriteShort(effect.Icon);
						//pak.WriteShort(effect.IsFading ? (ushort)1 : (ushort)(effect.RemainingTime / 1000));
						pak.WriteShort((ushort)(effect.RemainingTime / 1000));
						if (effect is GameSpellEffect)
							pak.WriteShort(((GameSpellEffect)effect).Spell.TooltipId); //v1.110+ send the spell ID for delve info in active icon
						else
							pak.WriteShort(0);//don't override existing tooltip ids
	
						byte flagNegativeEffect = 0;
						if (effect is StaticEffect)
						{
							if (((StaticEffect)effect).HasNegativeEffect)
							{
								flagNegativeEffect = 1;
							}
						}
						else if (effect is GameSpellEffect)
						{
							if (!((GameSpellEffect)effect).SpellHandler.HasPositiveEffect)
							{
								flagNegativeEffect = 1;
							}
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
				{
					changedEffects.Clear();
				}
	
				if (entriesCount == 0)
				{
					return; // nothing changed - no update is needed
				}
	
				pak.Position = initPos;
				pak.WriteByte((byte)entriesCount);
				pak.Seek(0, SeekOrigin.End);
	
				SendTCP(pak);
			}
		}

		/**
		 * 
		 * Need to adjust to dol compatible code 
	
		public override void SendUpdatePlayerSkills()
		{
			if (m_gameClient.Player == null)
				return;
			IList<Specialization> specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList<Style> styles = m_gameClient.Player.GetStyleList();
			List<SpellLine> spelllines = m_gameClient.Player.GetSpellLines();
			var m_styleId = new Hashtable();
			int maxSkills = 0;
			int firstSkills = 0;

			var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
			bool flagSendHybrid = true;
			if (m_gameClient.Player.CharacterClass.ClassType == eClassType.ListCaster)
				flagSendHybrid = false;

			lock (skills.SyncRoot)
			{
				lock (styles)
				{
					lock (m_gameClient.Player.lockSpellLinesList)
					{
						int skillCount = specs.Count + skills.Count + styles.Count;
						if (flagSendHybrid)
							skillCount += m_gameClient.Player.GetSpellCount();

						pak.WriteByte(0x01); //subcode
						pak.WriteByte((byte)skillCount); //number of entry
						pak.WriteByte(0x03); //subtype
						pak.WriteByte((byte)firstSkills);

						foreach (Specialization spec in specs)
						{
							CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
							int baseSpec = m_gameClient.Player.GetBaseSpecLevel(spec.Id);
							int compSpec = m_gameClient.Player.GetSpecLevel(spec.Id);
							pak.WriteByte((byte)baseSpec);//level
							pak.WriteShort((ushort)spec.Id); // index
							pak.WriteByte((byte)eSkillPage.Specialization);  // page --> Type v1.110: delve id?
							pak.WriteShort(0); // stlOpen
							pak.WriteByte((byte)(compSpec - baseSpec)); // bonus
							pak.WriteShort(spec.Icon);// icon
							pak.WritePascalString(spec.Name);
						}

						int i = 0;
						foreach (Skill skill in skills)
						{
							i++;
							CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
							var type = (byte)eSkillPage.Abilities;
							if (skill is RealmAbility)
							{
								type = (byte)eSkillPage.RealmAbilities;
							}
							pak.WriteByte((byte)skill.Level); //level 1.110
							pak.WriteShort(skill.ID); // Index 1.110
							pak.WriteByte(type); // page
							pak.WriteShort(0); // stlOpen
							pak.WriteByte(0); // bonus
							pak.WriteShort(skill.ID); // icon
							string str = "";
							if (m_gameClient.Player.CharacterClass.ID == (int)eCharacterClass.Vampiir)
							{
								if (skill.Name == Abilities.VampiirConstitution ||
									skill.Name == Abilities.VampiirDexterity ||
									skill.Name == Abilities.VampiirStrength)
									str = " +" + ((m_gameClient.Player.Level - 5) * 3);
								else if (skill.Name == Abilities.VampiirQuickness)
									str = " +" + ((m_gameClient.Player.Level - 5) * 2);
							}
							//log.Info("Skill: "+skill.Name+"  "+skill.ID+"  "+skill.SkillType);
							pak.WritePascalString(LanguageMgr.GetTranslation(m_gameClient, skill.Name) + str);
						}

						foreach (Style style in styles)
						{
							m_styleId[(int)style.ID] = i++;
							CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
							//DOLConsole.WriteLine("style sended "+style.Name);
							pak.WriteByte((byte)style.SpecLevelRequirement); //level
							pak.WriteShort(style.ID); // delve id - 1.110
							pak.WriteByte((byte)eSkillPage.Styles); //type

							int pre = 0;
							switch (style.OpeningRequirementType)
							{
								case Style.eOpening.Offensive:
									pre = 0 + (int)style.AttackResultRequirement;
									// last result of our attack against enemy
									// hit, miss, target blocked, target parried, ...
									if (style.AttackResultRequirement == Style.eAttackResult.Style)
										pre |= ((100 + (int)m_styleId[style.OpeningRequirementValue]) << 8);
									break;
								case Style.eOpening.Defensive:
									pre = 100 + (int)style.AttackResultRequirement;
									// last result of enemies attack against us
									// hit, miss, you block, you parry, ...
									break;
								case Style.eOpening.Positional:
									pre = 200 + style.OpeningRequirementValue;
									break;
							}

							// style required?
							if (pre == 0)
							{
								pre = 0x100;
							}

							pak.WriteShort((ushort)pre); //stlOpen
							pak.WriteByte(GlobalConstants.GetSpecToInternalIndex(style.Spec)); // bonus index specialization
							pak.WriteShort((ushort)style.Icon); //icon
							pak.WritePascalString(style.Name);
						}
						if (flagSendHybrid)
						{
							Dictionary<string, KeyValuePair<Spell, SpellLine>> spells =
								m_gameClient.Player.GetUsableSpells(spelllines, false);

							foreach (var spell in spells)
							{
								CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);

								pak.WriteByte((byte)spell.Value.Key.Level); //level delve stuff - 1.110
								pak.WriteShort(spell.Value.Key.ID); //delve id - 1.110

								//pak.WriteByte((byte)eSkillPage.Spells); // page

								int specIndex = specs.IndexOf(SkillBase.GetSpecialization(spell.Value.Value.Spec));
								if (specIndex == -1)
									specIndex = 0xFE; // Nightshade special value
								if (spell.Value.Key.InstrumentRequirement == 0)
								{
									pak.WriteByte((byte)eSkillPage.Spells);
									pak.WriteByte(0);
									pak.WriteByte((byte)specIndex);
								}
								else
								{
									pak.WriteByte((byte)eSkillPage.Songs);
									pak.WriteByte(0);
									pak.WriteByte(0xFF);
								}
								pak.WriteByte(0); //bonus
								pak.WriteShort(spell.Value.Key.Icon); //icon
								pak.WritePascalString(spell.Value.Key.Name);
							}
						}
					}
				}
			}
			if (pak.Length > 7)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - firstSkills)); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)firstSkills);
				SendTCP(pak);
			}

			SendSpellList();

			if (m_gameClient.Player.CharacterClass.ClassType != eClassType.ListCaster)
			{
				pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
				pak.WriteByte(0x02); //subcode
				pak.WriteByte(0x00);
				pak.WriteByte(99); //subtype (new subtype 99 in 1.80e)
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public override void SendSpellList()
		{
			if (m_gameClient.Player == null)
				return;

			GSTCPPacketOut pak; // = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			IList<SpellLine> spelllines = m_gameClient.Player.GetSpellLines();
			byte linenumber = 0;

			bool isHybrid = true;
			if (m_gameClient.Player.CharacterClass.ClassType == eClassType.ListCaster)
				isHybrid = false;

			lock (spelllines)
			{
				foreach (SpellLine line in spelllines)
				{
					// for hybrids only send the advanced spell lines here
					if (isHybrid && m_gameClient.Player.IsAdvancedSpellLine(line) == false)
						continue;

					// make a copy
					var spells = new List<Spell>(SkillBase.GetSpellList(line.KeyName));

					int spellcount = 0;
					for (int i = 0; i < spells.Count; i++)
					{
						if (spells[i].Level <= line.Level)
						{
							spellcount++;
						}
					}
					pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
					pak.WriteByte(0x02); //subcode
					pak.WriteByte((byte)(spellcount + 1)); //number of entry
					pak.WriteByte(0x02); //subtype
					pak.WriteByte(linenumber++); //number of line
					pak.WriteByte(0); // level, not used when spell line
					pak.WriteShort(0); // icon, not used when spell line
					pak.WriteByte(0); //unkown 1.110
					pak.WriteShort(0); // unkown 1.110
					pak.WritePascalString(line.Name);
					foreach (Spell spell in spells)
					{
						if (spell.Level <= line.Level)
						{
							pak.WriteByte((byte)spell.Level);
							pak.WriteByte(0); //unkown 1.110
							pak.WriteShort(spell.ID);//(ushort)((linenumber-1)*100 + spell.Level)); // v1.110: delve id
							pak.WriteShort(spell.Icon);
							pak.WritePascalString(spell.Name);
						}
					}
					SendTCP(pak);
				}
			}

			pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
			pak.WriteByte(0x02); //subcode
			pak.WriteByte(0x00);
			pak.WriteByte(99); //subtype (new subtype 99 in 1.80e)
			pak.WriteByte(0x00);
			SendTCP(pak);
		}
		*/

		/// <summary>
		/// SendTrainerWindow method
		/// </summary>
		public override void SendTrainerWindow()
		{
			if (m_gameClient == null || m_gameClient.Player == null) return;

			// type 0 & type 1

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				IList specs = m_gameClient.Player.GetSpecList();
				pak.WriteByte((byte)specs.Count);
				pak.WriteByte((byte)m_gameClient.Player.SkillSpecialtyPoints);
				pak.WriteByte(0);
				pak.WriteByte(0);

				int i = 0;
				foreach (Specialization spec in specs)
				{
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)Math.Min(50, spec.Level));
					pak.WriteByte((byte)(Math.Min(50, spec.Level) + 1)); 
					pak.WritePascalString(spec.Name);
				}
				SendTCP(pak);
			}

			// realm abilities
			List<RealmAbility> raList = SkillBase.GetClassRealmAbilities(m_gameClient.Player.CharacterClass.ID);
			if (raList != null && raList.Count > 0)
			{
				var offeredRA = new List<RealmAbility>();
				foreach (RealmAbility ra in raList)
				{
					var playerRA = (RealmAbility)m_gameClient.Player.GetAbility(ra.KeyName);
					if (playerRA != null)
					{
						if (playerRA.Level < playerRA.MaxLevel)
						{
							var ab = SkillBase.GetAbility(playerRA.KeyName, playerRA.Level + 1) as RealmAbility;
							if (ab != null)
							{
								offeredRA.Add(ab);
							}
							else
							{
								log.Error("Ability " + ab.Name + " not found!");
							}
						}
					}
					else
					{
						if (ra.Level < ra.MaxLevel)
						{
							offeredRA.Add(ra);
						}
					}
				}

				using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
				{
					pak.WriteByte((byte)offeredRA.Count);
					pak.WriteByte((byte)m_gameClient.Player.RealmSpecialtyPoints);
					pak.WriteByte(1);
					pak.WriteByte(0);

					int i = 0;
					foreach (RealmAbility ra in offeredRA)
					{
						pak.WriteByte((byte)i++);
						pak.WriteByte((byte)ra.Level);
						pak.WriteByte((byte)ra.CostForUpgrade(ra.Level - 1));
						bool canBeUsed = ra.CheckRequirement(m_gameClient.Player);
						pak.WritePascalString((canBeUsed ? "" : "[") + ra.Name + (canBeUsed ? "" : "]"));
					}

					m_gameClient.Player.TempProperties.setProperty("OFFERED_RA", offeredRA);
					SendTCP(pak);
				}
			}

			// type 4 (skills) & type 3 (description)
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
	
				// Fill out an array that tells the client how many spec points are available at each of
				// this characters levels.  This seems to only be used for the 'Minimum Level' display on
				// the new trainer window.  I've changed the calls below to use AdjustedSpecPointsMultiplier
				// to enable servers that allow levels > 50 to train properly by modifying points available per level. - Tolakram
	
				// There is a bug here that is calculating too few spec points and causing level 50 players to 
				// be unable to train RA.  Setting this to max for now to disable 'Minimum Level' feature on train window.
				// I think bug is that auto train points must be added to this calculation.
				// -Tolakram
	
				for (byte i = 2; i <= 50; i++)
				{
					//int specpoints = 0;
	
					//if (i <= 5)
					//    specpoints = i;
	
					//if (i > 5)
					//    specpoints = i * m_gameClient.Player.CharacterClass.AdjustedSpecPointsMultiplier / 10;
	
					//if (i > 40 && i != 50)
					//    specpoints += i * m_gameClient.Player.CharacterClass.AdjustedSpecPointsMultiplier / 20;
	
					//paksub.WriteByte((byte)specpoints);
					paksub.WriteByte((byte)255);
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
								paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.BaseLevel / 4));
							else paksub.WriteByte(0);
	
							foreach (Spell sp in lss)
							{
	
								pak.WritePascalString(sp.Name);
								paksub.WriteByte((byte)Math.Min(50, sp.Level));
								paksub.WriteShort(sp.InternalIconID > 0 ? sp.InternalIconID : sp.Icon);
	                            if (sp.InstrumentRequirement == 0)
	                            {
	                                paksub.WriteByte((byte)eSkillPage.Spells);
	                                paksub.WriteByte(0);
	                            }
	                            else
	                            {
	                                paksub.WriteByte((byte)eSkillPage.Songs);
	                                paksub.WriteByte(0);
	                            }
	                            paksub.WriteByte((byte)((byte)sp.SkillType == 3 ? 254 : 255));
								paksub.WriteShort((ushort)sp.TooltipId);
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
									paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.BaseLevel / 4));
								else paksub.WriteByte(0);
	
								foreach (var st in lst)
								{
									pak.WritePascalString(st.Name);
									paksub.WriteByte((byte)Math.Min(50, st.Level));
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
								paksub.WriteByte((byte)Math.Floor((double)m_gameClient.Player.BaseLevel / 4));
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
