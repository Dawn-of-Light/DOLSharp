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
using System.Reflection;
using System.Collections;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS.Effects;
using DOL.GS.Styles;
using DOL.GS.PlayerTitles;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(181, GameClient.eClientVersion.Version181)]
	public class PacketLib181 : PacketLib180
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.81 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib181(GameClient client):base(client)
		{
		}

		public override void SendUpdatePureTankSkills()
		{
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styles = m_gameClient.Player.GetStyleList();
			Hashtable m_styleId = new Hashtable();

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));

			lock (skills.SyncRoot)
				lock (styles.SyncRoot)
					lock (specs.SyncRoot)
					{
						pak.WriteByte(0x01); //subcode
						pak.WriteByte((byte) (specs.Count + skills.Count + styles.Count)); //number of entry
						pak.WriteByte(0x03); //subtype
						pak.WriteByte(0x00); //unk

						foreach (Specialization spec in specs)
						{
							pak.WriteByte((byte) spec.Level);
							pak.WriteByte((byte) eSkillPage.Specialization);
							pak.WriteShort(0);
							pak.WriteByte((byte) (m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
							pak.WriteShort(spec.ID);
							pak.WritePascalString(spec.Name);
						}

						int i=0;
						foreach (Skill skill in skills)
						{
							i++;
							pak.WriteByte((byte) 0);
							if(skill.ID < 500) pak.WriteByte((byte) eSkillPage.Abilities);
							else pak.WriteByte((byte) eSkillPage.AbilitiesSpell);
							pak.WriteShort(0);
							pak.WriteByte(0);
							pak.WriteShort(skill.ID);
							pak.WritePascalString(skill.Name);
						}

						foreach (Style style in styles)
						{
							m_styleId[(int)style.ID] = i++;
							//DOLConsole.WriteLine("style sended "+style.Name);
							pak.WriteByte((byte) style.SpecLevelRequirement);
							pak.WriteByte((byte) eSkillPage.Styles);

							int pre = 0;
							switch (style.OpeningRequirementType)
							{
								case Style.eOpening.Offensive:
									pre = 0 + (int) style.AttackResultRequirement; // last result of our attack against enemy
									// hit, miss, target blocked, target parried, ...
									if (style.AttackResultRequirement==Style.eAttackResult.Style)
										pre |= ((100 + (int)m_styleId[style.OpeningRequirementValue]) << 8);
									break;
								case Style.eOpening.Defensive:
									pre = 100 + (int) style.AttackResultRequirement; // last result of enemies attack against us
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

							pak.WriteShort((ushort) pre);
							pak.WriteByte(GlobalConstants.GetSpecToInternalIndex(style.Spec)); // index specialization
							pak.WriteShort(style.ID);
							pak.WritePascalString(style.Name);
						}
					}
			SendTCP(pak);
		}

		public override void SendUpdateHybridSkills()
		{
			if (m_gameClient.Player == null)
				return;
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styles = m_gameClient.Player.GetStyleList();
			IList spelllines = m_gameClient.Player.GetSpellLines();
			Hashtable m_styleId = new Hashtable();
			int maxSkills = 0;
			int firstSkills = 0;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));

			lock (skills.SyncRoot)
				lock (styles.SyncRoot)
					lock (specs.SyncRoot)
						lock (spelllines.SyncRoot)
						{
							int spellscount = m_gameClient.Player.GetAmountOfSpell();
							pak.WriteByte(0x01); //subcode
							pak.WriteByte((byte) (specs.Count + skills.Count + styles.Count + spellscount)); //number of entry
							pak.WriteByte(0x03); //subtype
							pak.WriteByte((byte)firstSkills);

							foreach (Specialization spec in specs)
							{
								CheckLenghtHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								pak.WriteByte((byte) spec.Level);
								pak.WriteByte((byte) eSkillPage.Specialization);
								pak.WriteShort(0);
								pak.WriteByte((byte) (m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
								pak.WriteShort(spec.ID);
								pak.WritePascalString(spec.Name);
							}

							int i=0;
							foreach (Skill skill in skills)
							{
								i++;
								CheckLenghtHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								pak.WriteByte(0);
								if(skill.ID < 500) pak.WriteByte((byte) eSkillPage.Abilities);
								else pak.WriteByte((byte) eSkillPage.AbilitiesSpell);
								pak.WriteShort(0);
								pak.WriteByte(0);
								pak.WriteShort(skill.ID);
								pak.WritePascalString(skill.Name);
							}
							foreach (Style style in styles)
							{
								m_styleId[(int)style.ID] = i++;
								CheckLenghtHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								//DOLConsole.WriteLine("style sended "+style.Name);
								pak.WriteByte((byte) style.SpecLevelRequirement);
								pak.WriteByte((byte) eSkillPage.Styles);

								int pre = 0;
								switch (style.OpeningRequirementType)
								{
									case Style.eOpening.Offensive:
										pre = 0 + (int) style.AttackResultRequirement; // last result of our attack against enemy
										// hit, miss, target blocked, target parried, ...
										if (style.AttackResultRequirement==Style.eAttackResult.Style)
											pre |= ((100 + (int)m_styleId[style.OpeningRequirementValue]) << 8);
										break;
									case Style.eOpening.Defensive:
										pre = 100 + (int) style.AttackResultRequirement; // last result of enemies attack against us
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

								pak.WriteShort((ushort) pre);
								pak.WriteByte(GlobalConstants.GetSpecToInternalIndex(style.Spec)); // index specialization
								pak.WriteShort(style.ID);
								pak.WritePascalString(style.Name);
							}
							foreach (SpellLine spellline in spelllines)
							{
								int spec_index = specs.IndexOf(m_gameClient.Player.GetSpecialization(spellline.Spec));
								if (spec_index == -1)
									spec_index = 0xFE; // Nightshade special value
								IList spells = m_gameClient.Player.GetUsableSpellsOfLine(spellline);
								foreach (Spell spell in spells)
								{
									CheckLenghtHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
									pak.WriteByte((byte) spell.Level);
									if (spell.InstrumentRequirement == 0)
									{
										pak.WriteByte((byte) eSkillPage.Spells);
										pak.WriteByte(0);
										pak.WriteByte((byte) spec_index);
									}
									else
									{
										pak.WriteByte((byte) eSkillPage.Songs);
										pak.WriteByte(0);
										pak.WriteByte(0xFF);
									}
									pak.WriteByte(0);
									pak.WriteShort(spell.Icon);
									pak.WritePascalString(spell.Name);
								}
							}
						}
			if(pak.Length > 7)
			{
				pak.Position = 4;
				pak.WriteByte((byte) (maxSkills - firstSkills)); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)firstSkills);
				SendTCP(pak);
			}
		}

		public override void SendUpdateListCasterSkills()
		{
			if (m_gameClient.Player == null)
				return;
			base.SendUpdateListCasterSkills();
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x02); //subcode
			pak.WriteByte(0x00);
			pak.WriteByte(99); //subtype (new subtype 99 in 1.80e)
			pak.WriteByte(0x00);
			SendTCP(pak);
		}

		public override void SendCustomTextWindow(string caption, IList text)
		{
			if (text == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DetailWindow));

			pak.WriteByte(0); // new in 1.75
			pak.WriteByte(0); // new in 1.81
			pak.WritePascalString(caption); //window caption

			IEnumerator iter = text.GetEnumerator();
			byte line = 1;
			while (iter.MoveNext())
			{
				pak.WriteByte(line++);
				pak.WritePascalString((string) iter.Current);
			}

			//Trailing Zero!
			pak.WriteByte(0);
			SendTCP(pak);
		}

		public override void SendPlayerTitles()
		{
			IList titles = m_gameClient.Player.Titles;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.DetailWindow));

			pak.WriteByte(1); // new in 1.75
			pak.WriteByte(0); // new in 1.81
			pak.WritePascalString("Player Statistics"); //window caption

			byte line = 1;
			foreach (string str in GameServer.ServerRules.FormatPlayerStatistics(m_gameClient.Player))
			{
				pak.WriteByte(line++);
				pak.WritePascalString(str);
			}

			pak.WriteByte(200);
			long titlesCountPos = pak.Position;
			pak.WriteByte(0); // length of all titles part
			pak.WriteByte((byte)titles.Count);
			line = 0;
			foreach (IPlayerTitle title in titles)
			{
				pak.WriteByte(line++);
				pak.WritePascalString(title.GetDescription(m_gameClient.Player));
			}
			long titlesLen = (pak.Position - titlesCountPos - 1); // include titles count
			if (titlesLen > byte.MaxValue)
				log.WarnFormat("Titles block is too long! {0} (player: {1})", titlesLen, m_gameClient.Player);
			//Trailing Zero!
			pak.WriteByte(0);
			//Set titles length
			pak.Position = titlesCountPos;
			pak.WriteByte((byte)titlesLen); // length of all titles part
			SendTCP(pak);
		}

		public override void SendPetWindow(GameLiving pet, ePetWindowAction windowAction, eAggressionState aggroState, eWalkState walkState)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.PetWindow));
			pak.WriteShort((ushort)(pet==null ? 0 : pet.ObjectID));
			pak.WriteByte(0x00); //unused
			pak.WriteByte(0x00); //unused
			switch (windowAction) //0-released, 1-normal, 2-just charmed? | Roach: 0-close window, 1-update window, 2-create window
			{
				case ePetWindowAction.Open  : pak.WriteByte(2); break;
				case ePetWindowAction.Update: pak.WriteByte(1); break;
				default: pak.WriteByte(0); break;
			}
			switch (aggroState) //1-aggressive, 2-defensive, 3-passive
			{
				case eAggressionState.Aggressive: pak.WriteByte(1); break;
				case eAggressionState.Defensive : pak.WriteByte(2); break;
				case eAggressionState.Passive   : pak.WriteByte(3); break;
				default: pak.WriteByte(0); break;
			}
			switch (walkState) //1-follow, 2-stay, 3-goto, 4-here
			{
				case eWalkState.Follow  : pak.WriteByte(1); break;
				case eWalkState.Stay    : pak.WriteByte(2); break;
				case eWalkState.GoTarget: pak.WriteByte(3); break;
				case eWalkState.ComeHere: pak.WriteByte(4); break;
				default: pak.WriteByte(0); break;
			}
			pak.WriteByte(0x00); //unused

			if (pet != null)
			{
				lock (pet.EffectList)
				{
					pak.WriteByte((byte)pet.EffectList.Count); // effect count
					foreach (IGameEffect effect in pet.EffectList)
						pak.WriteShort(effect.Icon); // list of shorts - spell icons on pet
				}
			}

			SendTCP(pak);
		}
	}
}
