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
using System.Linq;
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
			if (m_gameClient == null || m_gameClient.Player == null)
				return;
			
			GamePlayer player = m_gameClient.Player;

			List<Specialization> specs = m_gameClient.Player.GetSpecList().Where(it => it.Trainable).ToList();
			IList<string> autotrains = player.CharacterClass.GetAutotrainableSkills();
			
			// Send Trainer Window with Trainable Specs
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)specs.Count);
				pak.WriteByte((byte)player.SkillSpecialtyPoints);
				pak.WriteByte(0); // Spec code
				pak.WriteByte(0);

				int i = 0;
				foreach (Specialization spec in specs)
				{
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)Math.Min(player.MaxLevel, spec.Level));
					pak.WriteByte((byte)(Math.Min(player.MaxLevel, spec.Level) + 1)); 
					pak.WritePascalString(spec.Name);
				}
				SendTCP(pak);
			}

			// send RA usable by this class
			var raList = SkillBase.GetClassRealmAbilities(m_gameClient.Player.CharacterClass.ID).Where(ra => !(ra is RR5RealmAbility));
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)raList.Count());
				pak.WriteByte((byte)player.RealmSpecialtyPoints);
				pak.WriteByte(1); // RA Code
				pak.WriteByte(0);

				int i = 0;
				foreach (RealmAbility ra in raList)
				{
					int level = player.GetAbilityLevel(ra.KeyName);
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)level);
					pak.WriteByte((byte)ra.CostForUpgrade(level));
					bool canBeUsed = ra.CheckRequirement(player);
					pak.WritePascalString(canBeUsed ? ra.Name : string.Format("[{0}]", ra.Name));
				}
				SendTCP(pak);
			}
			
			// Send Name Index for each spec.
			// Get ALL skills for player, ordered by spec key.
			List<Tuple<Specialization, List<Tuple<int, int, Skill>>>> skillDictCache = null;
			
			// get from cache
			if (m_gameClient.TrainerSkillCache == null)
			{
				skillDictCache = new List<Tuple<Specialization, List<Tuple<int, int, Skill>>>>();
				
				foreach (Specialization spec in specs)
				{
					var toAdd = new List<Tuple<int, int, Skill>>();
					
					foreach (Ability ab in spec.PretendAbilitiesForLiving(player, player.MaxLevel))
					{
						toAdd.Add(new Tuple<int, int, Skill>(5, ab.InternalID, ab));
					}
					
					foreach (KeyValuePair<SpellLine, List<Skill>> ls in spec.PretendLinesSpellsForLiving(player, player.MaxLevel).Where(k => !k.Key.IsBaseLine))
					{
						foreach (Skill sk in ls.Value)
						{
							toAdd.Add(new Tuple<int, int, Skill>((int)sk.SkillType, sk.InternalID, sk));
						}
					}
					
					foreach (Style st in spec.PretendStylesForLiving(player, player.MaxLevel))
					{
						toAdd.Add(new Tuple<int, int, Skill>((int)st.SkillType, st.InternalID, st));
					}
					
					skillDictCache.Add(new Tuple<Specialization, List<Tuple<int, int, Skill>>>(spec, toAdd.OrderBy(e => (e.Item3 is Ability) ? ((Ability)e.Item3).SpecLevelRequirement : (((e.Item3 is Style) ? ((Style)e.Item3).SpecLevelRequirement : e.Item3.Level)) ).ToList()));
				}
				
				
				
				// save to cache
				m_gameClient.TrainerSkillCache = skillDictCache;
			}
			
			skillDictCache = m_gameClient.TrainerSkillCache;
			
			// Send Names first
			int index = 0;			
			for (int skindex = 0 ; skindex < skillDictCache.Count ; skindex++)
			{
				using(GSTCPPacketOut pakindex = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
				{
					pakindex.WriteByte((byte)skillDictCache[skindex].Item2.Count); //size
					pakindex.WriteByte((byte)player.SkillSpecialtyPoints);
					pakindex.WriteByte(4); // name index code
					pakindex.WriteByte(0);
					pakindex.WriteByte((byte)index); // start index
					
					foreach (Skill sk in skillDictCache[skindex].Item2.Select(e => e.Item3))
					{
						// send name
						pakindex.WritePascalString(sk.Name);
						index++;
					}
					
					SendTCP(pakindex);
				}
			}
			
			// Send Skill Secondly
			using(GSTCPPacketOut pakskill = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				
				pakskill.WriteByte((byte)skillDictCache.Count); //size we send for all specs
				pakskill.WriteByte((byte)player.SkillSpecialtyPoints);
				pakskill.WriteByte(3); // Skill description code
				pakskill.WriteByte(0);
				pakskill.WriteByte((byte)0); // unk ?
				
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
					pakskill.WriteByte((byte)255);
				}
				
				for (int skindex = 0 ; skindex < skillDictCache.Count ; skindex++)
				{				
					
					byte autotrain = 0; 
					if(autotrains.Contains(specs[skindex].KeyName))
					{
						autotrain = (byte)Math.Floor((double)m_gameClient.Player.BaseLevel / 4);
					}

					if (pakskill.Length >= 2045)
						break;

					// Skill Index Header
					pakskill.WriteByte((byte)skindex); // skill index
					pakskill.WriteByte((byte)skillDictCache[skindex].Item2.Count); // Count
					pakskill.WriteByte(autotrain); // autotrain byte
					
					foreach (Tuple<int, int, Skill> sk in skillDictCache[skindex].Item2)
					{
						if (pakskill.Length >= 2040)
							break;
						
						if (sk.Item3 is Ability)
						{
							Ability ab = (Ability)sk.Item3;
							// skill description
							pakskill.WriteByte((byte)ab.SpecLevelRequirement); // level
							// tooltip
							pakskill.WriteShort((ushort)ab.Icon); // icon
							pakskill.WriteByte((byte)sk.Item1); // skill page
							pakskill.WriteByte((byte)0); // 
							pakskill.WriteByte((byte)0xFD); // line
							pakskill.WriteShort((ushort)sk.Item2); // ID
						}
						else if (sk.Item3 is Spell)
						{
							Spell sp = (Spell)sk.Item3;
							// skill description
							pakskill.WriteByte((byte)sp.Level); // level
							// tooltip
							pakskill.WriteShort(sp.InternalIconID > 0 ? sp.InternalIconID : sp.Icon); // icon
							pakskill.WriteByte((byte)sk.Item1); // skill page
							pakskill.WriteByte((byte)0); // 
							pakskill.WriteByte((byte)(sp.SkillType == eSkillPage.Songs ? 0xFF : 0xFE)); // line
							pakskill.WriteShort((ushort)sk.Item2); // ID
						}
						else if (sk.Item3 is Style)
						{
							Style st = (Style)sk.Item3;
							pakskill.WriteByte((byte)Math.Min(player.MaxLevel, st.SpecLevelRequirement));
							// tooltip
							pakskill.WriteShort((ushort)st.Icon);
							pakskill.WriteByte((byte)sk.Item1);
							pakskill.WriteByte((byte)st.OpeningRequirementType);
							pakskill.WriteByte((byte)st.OpeningRequirementValue);
							pakskill.WriteShort((ushort)sk.Item2);
						}
						else
						{
							// ??
							pakskill.WriteByte((byte)sk.Item3.Level);
							// tooltip
							pakskill.WriteShort((ushort)sk.Item3.Icon);
							pakskill.WriteByte((byte)sk.Item1);
							pakskill.WriteByte((byte)0);
							pakskill.WriteByte((byte)0);
							pakskill.WriteShort((ushort)sk.Item2);
						}
					}
				}
				
				SendTCP(pakskill);
			}

			// type 5 (realm abilities)
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)raList.Count());
				pak.WriteByte((byte)player.RealmSpecialtyPoints);
				pak.WriteByte(5);
				pak.WriteByte(0);

				foreach (RealmAbility ra in raList)
				{
					pak.WriteByte((byte)player.GetAbilityLevel(ra.KeyName));
					
					pak.WriteByte(0);
					pak.WriteByte((byte)ra.MaxLevel);

					for (int i = 0; i < ra.MaxLevel; i++)
						pak.WriteByte((byte)ra.CostForUpgrade(i));

					if (ra.CheckRequirement(m_gameClient.Player))
						pak.WritePascalString(ra.KeyName);
					else
						pak.WritePascalString(string.Format("[{0}]", ra.Name));
				}
				SendTCP(pak);
			}
		}
	}
}